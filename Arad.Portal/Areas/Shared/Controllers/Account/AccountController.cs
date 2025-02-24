using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.ApplicationRole;
using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Error;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.Transaction;
using Arad.Portal.DataLayer.Helpers;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Shared.User;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.BasicData;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.CountryParts;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Role;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SendMessage;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Transaction;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.Domain;
using Arad.Portal.Models.Shared.Role;
using Arad.Portal.Models.Shared.Transaction;
using Arad.Portal.Models.Shared.User;
using Arad.Portal.Models.UI;

using AutoMapper;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using PhoneNumbers;

using Serilog;

using SixLabors.ImageSharp;

using Image = Arad.Portal.DataLayer.Models.Shared.Image;
using Profile = Arad.Portal.DataLayer.Models.Shared.User.Profile;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Utilities = Arad.Portal.Helpers.UI.Utilities;

namespace Arad.Portal.Areas.Shared.Controllers.Account;

[Authorize(Policy = "Role")]
[Area("Shared")]
public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IRoleRepository roleRepository,
    IMapper mapper,
    ICountryRepository countryRepository,
    IModificationRepository modificationRepository,
    IUserRepository userRepository,
    IBasicDataRepository basicDataRepository,
    ITransactionRepository transactionRepository,
    IDomainRepository domainRepository,
    ControllerHelper controllerHelper,
    CreateNotification createNotification,
    ISendMessageRepository sendMessageRepository,
    MinioHelper minioHelper,
    ILogger logger)
    : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string returnUrl)
    {
        if (HttpContext.User.Identity is { IsAuthenticated: true })
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
        }

        string captcha = HttpContext.Session.GenerateCaptchaImageString(2);
        ViewBag.Captcha = captcha;
        LoginViewModel viewModel = new() { ReturnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl, RememberMe = false };
        ViewBag.Message = "";

        return View(viewModel);
    }

    [AllowAnonymous]
    [HttpGet]
    public async ValueTask<IActionResult> Logout()
    {
        ApplicationUser user = await controllerHelper.GetCurrentUser();
        await signInManager.SignOutAsync();
        logger.Information($"userId: {user.Id} , userName: {user.UserName} signed out");

        return RedirectToAction("Login");
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> Login([FromForm] LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.ValidateCaptcha(model.Captcha))
        {
            ModelState.AddModelError("Captcha", UtilityLanguage.GetString("AlertAndMessage_CaptchaIsExpired"));
        }

        if (string.IsNullOrWhiteSpace(model.Username))
        {
            ModelState.AddModelError("Username", UtilityLanguage.GetString("AlertAndMessage_UserNameRequired"));
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError("Password", UtilityLanguage.GetString("AlertAndMessage_PasswordRequired"));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await HttpContext.SignOutAsync();

        if (model.Username == null)
        {
            return View(model);
        }

        ApplicationUser user = await userManager.FindByNameAsync(model.Username);

        //only users who isSiteUser = false can log in site admin
        //only owner of this domain can log in this part
        Result<DomainDto> result = new();

        try
        {
            string domainName = controllerHelper.GetCurrentDomainName();
            Domain dbEntity = await domainRepository.FirstOrDefaultAsync(c => c.DomainName == "https://" + domainName, cancellationToken);

            if (dbEntity != null)
            {
                DomainDto dto = mapper.Map<DomainDto>(dbEntity);
                dto.SupportedLangId = (await basicDataRepository.GetListAsync(c => c.AssociatedDomainId == dbEntity.Id && c.GroupKey == "SupportedCultures", cancellationToken))
                                      .Select(c => new string(c.Value))
                                      .ToList();
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
                result.ReturnValue = dto;
            }
            else
            {
                result.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");

                if (user != null)
                {
                    logger.Debug($"userId: {user.Id} , userName: {user.UserName} error occurred: {result.Message}");
                }

                result.ReturnValue = new();
            }

            if (user is not { IsSystemAccount: true })
            {
                bool check = user != null && !string.IsNullOrEmpty(model.Password) && user.Domains.Any(c => c.DomainId == dbEntity?.Id) && await userManager.CheckPasswordAsync(user, model.Password);

                if (check != true)
                {
                    ViewBag.Message = UtilityLanguage.GetString("AlertAndMessage_InvalidUsernameOrPassword");

                    if (user != null)
                    {
                        logger.Debug($"userId: {user.Id} , userName: {user.UserName} error occurred: {ViewBag.Message}");
                    }

                    return View(model);
                }

                if (user is { IsActive: false })
                {
                    ViewBag.Message = UtilityLanguage.GetString("AlertAndMessage_InActiveUserAccount");
                    logger.Debug($"userId: {user.Id} , userName: {user.UserName} error occurred: {ViewBag.Message}");

                    return View(model);
                }
            }

            if (model.Password != null)
            {
                SignInResult res = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                if (res.Succeeded)
                {
                    user.LastLoginDate = DateTime.Now;
                    await userRepository.UpdateAsync(c => c.Id == user.Id, m => m.LastLoginDate, user.LastLoginDate, cancellationToken);
                    logger.Information($"userId: {user.Id} , userName: {user.UserName} signed in");

                    if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && model.ReturnUrl != "/")
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    TempData["LoginUser"] = $"{user.Profile.FirstName} {user.Profile.LastName} {UtilityLanguage.GetString("AlertAndMessage_Welcome")}";

                    return Redirect("~/Home/Index");
                }

                ViewBag.Message = UtilityLanguage.GetString("AlertAndMessage_InvalidUsernameOrPassword");
                logger.Debug($"userId: {user.Id} , userName: {user.UserName} error occurred: {ViewBag.Message}");

                return View(model);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);

            if (user != null)
            {
                logger.Error($"userId: {user.Id} , userName: {user.UserName} error occurred: {ex.Message} stack trace: {nameof(AccountController)}/{nameof(Login)}");
            }
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult CheckCaptcha(string captcha)
    {
        return Ok(HttpContext.Session.ValidateCaptcha(captcha) ? new { Status = "success" } : new { Status = "error" });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        RegisterDto registerDto = new();

        return View(registerDto);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> Register([FromForm] RegisterVm model, CancellationToken cancellationToken)
    {
        string domainName = controllerHelper.GetCurrentDomainName();
        Domain domainEntity = await domainRepository.FirstOrDefaultAsync(c => c.DomainName == "https://" + domainName, cancellationToken);

        #region Validate
        if (!HttpContext.Session.ValidateCaptcha(model.Captcha))
        {
            ModelState.AddModelError("Captcha", UtilityLanguage.GetString("AlertAndMessage_CaptchaIncorrectOrExpired"));
        }

        if (!ModelState.IsValid)
        {
            List<AjaxValidationErrorModel> errors = [];

            foreach (string modelStateKey in ModelState.Keys)
            {
                ModelStateEntry modelStateVal = ModelState[modelStateKey];

                if (modelStateVal != null)
                {
                    errors.AddRange(modelStateVal.Errors
                                                 .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
            }

            return Ok(new { Status = "ModelError", ModelStateErrors = errors });
        }

        model.FullCellPhoneNumber = model.FullCellPhoneNumber.Replace("+", "");
        model.FullCellPhoneNumber = model.FullCellPhoneNumber.Replace(" ", "");

        if (string.IsNullOrWhiteSpace(model.FullCellPhoneNumber))
        {
            ModelState.AddModelError("CellPhoneNumber", UtilityLanguage.GetString("Validation_EnterMobileNumber"));
        }
        else
        {
            PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

            PhoneNumber phoneNumber = phoneUtil.Parse(model.FullCellPhoneNumber, "IR");

            if (!phoneUtil.IsValidNumber(phoneNumber))
            {
                ModelState.AddModelError("CellPhoneNumber", UtilityLanguage.GetString("Validation_MobileNumberInvalid1"));
            }
            else
            {
                PhoneNumberType numberType = phoneUtil.GetNumberType(phoneNumber); // Produces Mobile , FIXED_LINE 

                if (numberType != PhoneNumberType.MOBILE)
                {
                    ModelState.AddModelError("CellPhoneNumber", UtilityLanguage.GetString("Validation_MobileNumberInvalid2"));
                }
            }
        }

        if (userManager.Users.Any(c => c.PhoneNumber == model.FullCellPhoneNumber || c.UserName == model.CellPhoneNumber))
        {
            ModelState.AddModelError("CellPhoneNumber", UtilityLanguage.GetString("Validation_MobileNumberAlreadyRegistered"));
        }

        if (string.IsNullOrWhiteSpace(model.SecurityCode))
        {
            ModelState.AddModelError("SecurityCode", UtilityLanguage.GetString("AlertAndMessage_SecurityCodeIsInvalid"));
        }

        OTP otp = OtpHelper.Get(model.FullCellPhoneNumber);

        if (otp == null)
        {
            ModelState.AddModelError("SecurityCode", UtilityLanguage.GetString("AlertAndMessage_SecurityCodeIsInvalid"));
        }
        else
        {
            if (otp.ExpirationDate >= DateTime.Now.AddMinutes(3))
            {
                ModelState.AddModelError("SecurityCode", UtilityLanguage.GetString("AlertAndMessage_ProfileConfirmPhoneTimeOut"));
            }

            if (!string.IsNullOrWhiteSpace(model.SecurityCode) && !model.SecurityCode.Equals(otp.Code))
            {
                ModelState.AddModelError("SecurityCode", UtilityLanguage.GetString("AlertAndMessage_SecurityCodeIsInvalid"));
            }
        }
        #endregion

        string id = Guid.NewGuid().ToString();

        ApplicationUser user = new()
        {
            UserName = model.FullCellPhoneNumber,
            IsSystemAccount = false,
            Id = id,
            Profile = new() { UserType = UserType.Customer },
            IsDeleted = false,
            CreatorId = id,
            CreatorUserName = model.FullCellPhoneNumber,
            CreationDate = DateTime.Now,
            IsActive = true,
            PhoneNumber = model.FullCellPhoneNumber,
            PhoneNumberConfirmed = true,
            IsSiteUser = true
        };

        user.Domains.Add(new() { DomainId = domainEntity.Id, IsOwner = false, DomainName = domainName });
        string pass = Utilities.GenerateRandomPassword(new() { RequireDigit = true, RequireLowercase = true, RequireNonAlphanumeric = true, RequireUppercase = true, RequiredLength = 7, RequiredUniqueChars = 0 });
        await sendMessageRepository.FirstOrDefaultAsync(cancellationToken);

        IdentityResult insertResult = await userManager.CreateAsync(user, pass);
        user.IsSystemAccount = true;
        user.IsActive = true;
        user.Id = id;
        user.CreatorId = id;
        user.IsDeleted = false;
        user.CreationDate = DateTime.Now;
        user.Profile = new() { UserType = UserType.Customer };
        user.PhoneNumberConfirmed = true;
        user.IsSiteUser = true;

        if (!insertResult.Succeeded)
        {
            return Ok(insertResult.Succeeded ? new { Status = "Success", Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") } : new { Status = "Error", Message = insertResult.Errors.FirstOrDefault()?.Description });
        }

        try
        {
            await createNotification.ClientSignUp("ClientSignupEmail", user, pass, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            // Log detailed error information
            logger.Error(e, $"HTTP request failed  |  stack trace: {nameof(AccountController)}/{nameof(ResetPassword)}");

            return StatusCode(500, new { Status = "Error", Message = "Failed to send SMS. Please try again later." });
        }
        catch (Exception e)
        {
            // General exception handling
            logger.Error($"{e.Message}  |  stack trace: {nameof(AccountController)}/{nameof(ResetPassword)}");

            return StatusCode(500, new { Status = "Error", Message = "An unexpected error occurred. Please try again later." });
        }

        await signInManager.PasswordSignInAsync(user, pass, false, false);




        return Ok(insertResult.Succeeded ? new { Status = "Success", Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") } : new { Status = "Error", Message = insertResult.Errors.FirstOrDefault()?.Description });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ChangeLang([FromQuery] string langId)
    {
        string domainName = $"{HttpContext.Request.Host}";

        if (domainName.ToLower().StartsWith("localhost"))
        {
            //prevent port of localhost
            domainName = HttpContext.Request.Host.ToString()[..9];
        }

        if (CultureInfo.CurrentCulture.Name == langId)
        {
            return Ok(false);
        }

        Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                                CookieRequestCultureProvider.MakeCookieValue(new(langId)),
                                new() { Expires = DateTimeOffset.Now.AddYears(1), Domain = domainName });

        return Ok(true);
    }

    [HttpGet]
    public async ValueTask<IActionResult> CreateRandomPass(CancellationToken cancellationToken)
    {
        string pass = Utilities.GenerateRandomPassword(new() { RequireDigit = false, RequireLowercase = true, RequireNonAlphanumeric = true, RequireUppercase = true, RequiredLength = 10, RequiredUniqueChars = 1 });
        ApplicationUser user = await controllerHelper.GetCurrentUser(cancellationToken);
        logger.Information($"userId: {user.Id}, userName: {user.UserName} create random password successfully. Password: {pass}");

        return Ok(new { Status = "Success", Pass = pass });
    }

    [HttpGet]
    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        int pageSize = 10;
        int page = 1;
        PagedItems<UserListView> result = new();

        try
        {
            ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
            IQueryable<ApplicationUser> users = userDb.IsSystemAccount ? userManager.Users.Where(_ => true) : userManager.Users.Where(c => c.Id != userDb.Id);

            NameValueCollection queryParams = HttpUtility.ParseQueryString(Request.QueryString.ToString());

            if (!string.IsNullOrWhiteSpace(queryParams["page"]))
            {
                page = Convert.ToInt32(queryParams["page"]);
            }

            if (!string.IsNullOrWhiteSpace(queryParams["pageSize"]))
            {
                pageSize = Convert.ToInt32(queryParams["pageSize"]);
            }

            if (!string.IsNullOrWhiteSpace(queryParams["role"]))
            {
                users = users.Where(c => c.UserRoleId == queryParams["role"]);
            }

            if (!string.IsNullOrWhiteSpace(queryParams["name"]))
            {
                users = users.Where(c => c.Profile.FirstName.ToLower().Contains(queryParams["name"].ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(queryParams["userName"]))
            {
                users = users.Where(c => c.UserName.ToLower().Contains(queryParams["userName"].ToLower()));
            }

            long count = users.Count();

            List<UserListView> usersDto = mapper.Map<List<UserListView>>(users).OrderByDescending(c => c.CreationDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();

            foreach (UserListView user in usersDto.Where(user => !string.IsNullOrWhiteSpace(user.UserRoleId)))
            {
                ApplicationRole role = await roleRepository.FirstOrDefaultAsync(c => c.Id == user.UserRoleId, cancellationToken);
                user.RoleName = role != null ? role.Name : "";
            }

            result = new() { CurrentPage = page, PageSize = pageSize, ItemsCount = count, Items = usersDto };

            List<ApplicationRole> roleListModel = await roleRepository.GetAllAsync(cancellationToken);
            List<RoleListView> roleListDto = mapper.Map<List<RoleListView>>(roleListModel);
            ViewBag.Roles = roleListDto;
        }
        catch (Exception)
        {
            // ignored
        }

        return View(result);
    }

    [HttpGet]
    public IActionResult AccessDenied(string returnUrl)
    {
        return View();
    }

    [HttpGet]
    public IActionResult PageOrItemNotFound()
    {
        return View();
    }

    [HttpGet]
    public async ValueTask<IActionResult> CreatePasswordNotification(string userId, string password, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        string useId = User.GetUserId();
        await userManager.FindByIdAsync(userId);
        _ = controllerHelper.GetDefaultLanguage().Id;
        Result result = await createNotification.Send("AutomatedPasswordReset", user, password, cancellationToken);
        logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName} creating password notification done successfully.");

        return Ok(new { Result = result });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ChangePassword()
    {
        ChangePassDto registerDto = new();

        return View(registerDto);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> ResetPassword([FromForm] ChangePassDto dto, CancellationToken cancellationToken)
    {
        #region Validate
        if (!HttpContext.Session.ValidateCaptcha(dto.Captcha))
        {
            ModelState.AddModelError("Captcha", UtilityLanguage.GetString("AlertAndMessage_CaptchaIncorrectOrExpired"));
        }

        dto.FullCellPhoneNumber = dto.FullCellPhoneNumber.Replace("+", "").Trim();

        if (string.IsNullOrWhiteSpace(dto.FullCellPhoneNumber))
        {
            ModelState.AddModelError("CellPhoneNumber", UtilityLanguage.GetString("Validation_EnterMobileNumber"));
        }
        else
        {
            PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

            PhoneNumber phoneNumber = phoneUtil.Parse(dto.FullCellPhoneNumber, "IR");

            if (!phoneUtil.IsValidNumber(phoneNumber))
            {
                ModelState.AddModelError("CellPhoneNumber", UtilityLanguage.GetString("Validation_MobileNumberInvalid1"));
            }
            else
            {
                PhoneNumberType numberType = phoneUtil.GetNumberType(phoneNumber); // Produces Mobile , FIXED_LINE 

                if (numberType != PhoneNumberType.MOBILE)
                {
                    ModelState.AddModelError("CellPhoneNumber", UtilityLanguage.GetString("Validation_MobileNumberInvalid2"));
                }
            }
        }

        if (string.IsNullOrWhiteSpace(dto.SecurityCode))
        {
            ModelState.AddModelError("SecurityCode", UtilityLanguage.GetString("AlertAndMessage_ProfileConfirmPhoneError"));
        }

        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            ModelState.AddModelError("UserName", UtilityLanguage.GetString("Validation_EnterUsername"));
        }
        else
        {
            ApplicationUser user = await userManager.FindByNameAsync(dto.Username);

            if (user != null && user.PhoneNumber != dto.FullCellPhoneNumber)
            {
                ModelState.AddModelError("UserName", UtilityLanguage.GetString("Validation_InvalidUserNameOrPhoneNumber"));
            }
        }

        OTP otp = OtpHelper.Get(dto.FullCellPhoneNumber);

        if (otp == null)
        {
            ModelState.AddModelError("SecurityCode", UtilityLanguage.GetString("AlertAndMessage_ProfileConfirmPhoneError"));
        }
        else
        {
            if (otp.ExpirationDate >= DateTime.Now.AddMinutes(3))
            {
                ModelState.AddModelError("SecurityCode", UtilityLanguage.GetString("AlertAndMessage_ProfileConfirmPhoneTimeOut"));
            }

            if (!string.IsNullOrWhiteSpace(dto.SecurityCode) && !dto.SecurityCode.Equals(otp.Code))
            {
                ModelState.AddModelError("SecurityCode", UtilityLanguage.GetString("AlertAndMessage_ProfileConfirmPhoneTimeOut"));
            }
        }

        if (!ModelState.IsValid)
        {
            List<AjaxValidationErrorModel> errors = [];

            foreach (string modelStateKey in ModelState.Keys)
            {
                ModelStateEntry modelStateVal = ModelState[modelStateKey];

                if (modelStateVal != null)
                {
                    errors.AddRange(modelStateVal.Errors
                                                 .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
            }

            return Ok(new { Status = "ModelError", ModelStateErrors = errors });
        }
        #endregion

        if (dto.Username == null)
        {
            return null;
        }

        ApplicationUser currentUser = await userManager.FindByNameAsync(dto.Username);

        string pass = Helpers.Admin.Utilities.GenerateRandomPassword(new()
        {
            RequireDigit = true,
            RequireLowercase = true,
            RequireNonAlphanumeric = true,
            RequireUppercase = true,
            RequiredLength = 10,
            RequiredUniqueChars = 0
        });

        while (!Password.PasswordIsValid(true,
                                         true,
                                         true,
                                         true,
                                         false,
                                         pass))
        {
            pass = Password.GeneratePassword(true,
                                             true,
                                             true,
                                             true,
                                             false,
                                             10);
        }

        Result result = await createNotification.Send("AutomatedPasswordReset", currentUser, pass, cancellationToken);

        if (!result.Succeeded)
        {
            return Ok(new { Status = "Error", Message = UtilityLanguage.GetString("AlertAndMessage_Error") });
        }

        if (currentUser == null)
        {
            return null;
        }

        string token = await userManager.GeneratePasswordResetTokenAsync(currentUser);
        IdentityResult changePassResult =
            await userManager.ResetPasswordAsync(currentUser, token, pass);

        return Ok(changePassResult.Succeeded
                      ? new { Status = "Success", Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") }
                      : new { Status = "Error", Message = UtilityLanguage.GetString("AlertAndMessage_Error") });

    }

    [HttpGet]
    public IActionResult ChangeKnownPassword()
    {
        RegisterDto registerDto = new();

        return View(registerDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> ChangeKnownPassword([FromForm] RegisterDto dto)
    {
        #region Validate
        if (!HttpContext.Session.ValidateCaptcha(dto.Captcha))
        {
            ModelState.AddModelError("Captcha", UtilityLanguage.GetString("AlertAndMessage_CaptchaIncorrectOrExpired"));
        }

        ApplicationUser currentUser = await userManager.FindByNameAsync(dto.Username);
        bool checkPass = currentUser != null && await userManager.CheckPasswordAsync(currentUser, dto.CurrentPass);

        if (!checkPass)
        {
            ModelState.AddModelError("CurrentPass", UtilityLanguage.GetString("AlertAndMessage_InCorrectPassword"));
        }

        if (dto.NewPass != dto.ReNewPass)
        {
            ModelState.AddModelError("ReNewPass", UtilityLanguage.GetString("AlertAndMessage_PassWordAndRePassNotEqual"));
        }

        if (!ModelState.IsValid)
        {
            List<AjaxValidationErrorModel> errors = [];

            foreach (string modelStateKey in ModelState.Keys)
            {
                ModelStateEntry modelStateVal = ModelState[modelStateKey];

                if (modelStateVal != null)
                {
                    errors.AddRange(modelStateVal.Errors
                                                 .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
            }

            return Ok(new { Status = "ModelError", ModelStateErrors = errors });
        }
        #endregion

        if (currentUser == null)
        {
            return BadRequest();
        }

        string token = await userManager.GeneratePasswordResetTokenAsync(currentUser);
        IdentityResult changePass = await userManager.ResetPasswordAsync(currentUser, token, dto.NewPass);

        return Ok(changePass.Succeeded ? new { Status = "Success", Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") } : new { Status = "Error", Message = UtilityLanguage.GetString("AlertAndMessage_Error") });

    }

    [HttpGet]
    [AllowAnonymous]
    public async ValueTask<IActionResult> SendSecurityCode(string cellPhoneNumber, CancellationToken cancellationToken)
    {
        #region Validation
        if (string.IsNullOrWhiteSpace(cellPhoneNumber))
        {
            return Ok(new { Status = "Error", Message = UtilityLanguage.GetString("Validation_EnterMobileNumber") });
        }

        // Clean up the cell phone number using a regex to remove spaces and "+"
        cellPhoneNumber = Regex.Replace(cellPhoneNumber, @"[\s+]", "");

        PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

        try
        {
            PhoneNumber phoneNumber = phoneUtil.Parse(cellPhoneNumber, "IR");

            if (!phoneUtil.IsValidNumber(phoneNumber))
            {
                return Ok(new { Status = "Error", Message = UtilityLanguage.GetString("Validation_MobileNumberInvalid1") });
            }

            PhoneNumberType numberType = phoneUtil.GetNumberType(phoneNumber);

            if (numberType != PhoneNumberType.MOBILE)
            {
                return Ok(new { Status = "Error", Message = UtilityLanguage.GetString("Validation_MobileNumberInvalid2") });
            }
        }
        catch (NumberParseException ex)
        {
            return Ok(new { Status = "Error", Message = UtilityLanguage.GetString("Validation_MobileNumberInvalid2"), ErrorDetails = ex.Message });
        }

        // Check if OTP is already sent
        OTP process = OtpHelper.Process(new() { Mobile = cellPhoneNumber, Code = DataLayer.Helpers.Utilities.GenerateOtp(), ExpirationDate = DateTime.Now.AddMinutes(3), IsSent = false });

        if (process.IsSent)
        {
            return Ok(new { Status = "Error", Message = UtilityLanguage.GetString("AlertAndMessage_ProfileConfirmPhoneSending") });
        }
        #endregion

        // Fetch user information
        try
        {
            Result result = await createNotification.SendOtp("SendOtp", cellPhoneNumber, process.Code, cancellationToken);

            if (result.Succeeded)
            {
                return Ok(new { Status = "Success", result.Message });
            }

            // Log the error and return a friendly error message
            await LogError(result.Message, @"Account\SendSecurityCode");

            return Ok(new { Status = "Error", Message = UtilityLanguage.GetString("AlertAndMessage_SubmitOtpPleaseTryAgainLater") });

        }
        catch (Exception ex)
        {
            await LogError(ex.Message, @"Account\SendSecurityCode");

            return Ok(new { Status = "Error", Message = UtilityLanguage.GetString("AlertAndMessage_SubmitOtpPleaseTryAgainLater") });
        }
    }

    // Error Logging Helper Method
    private async ValueTask LogError(string errorMessage, string source)
    {
        ErrorLog errorLog = new() { Error = errorMessage, Source = source, Ip = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() };
        await controllerHelper.AddErrorLog(errorLog);
    }

    [AllowAnonymous]
    public IActionResult UnAuthorize()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetStates(string countryName)
    {
        try
        {
            List<SelectListModel> states = countryRepository.GetAll()
                                                            .Where(c => c.Name == countryName)
                                                            .SelectMany(c => c.States)
                                                            .OrderBy(c => c.Name)
                                                            .Select(c => new SelectListModel { Text = c.Name, Value = c.Id })
                                                            .ToList();
            states.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });

            List<CountryPartView> st = states.Select(c => new CountryPartView { Id = c.Value, Name = c.Text }).ToList();

            return Json(new { Status = "success", Data = st });
        }
        catch (Exception)
        {
            return Json(new { status = "error", message = UtilityLanguage.GetString(ConstMessages.InternalServerErrorMessage) });
        }
    }

    [HttpGet]
    public IActionResult GetCities(string stateId)
    {
        try
        {
            List<SelectListModel> cities = countryRepository.GetAll()
                                                            .Where(c => c.States.Any(c => c.Id.Equals(stateId)))
                                                            .SelectMany(c => c.States)
                                                            .Where(c => c.Id.Equals(stateId))
                                                            .SelectMany(c => c.Cities)
                                                            .OrderBy(c => c.Name)
                                                            .Select(c => new SelectListModel { Text = c.Name, Value = c.Id })
                                                            .ToList();
            cities.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });
            List<CountryPartView> ci = cities.Select(c => new CountryPartView { Id = c.Value, Name = c.Text }).ToList();

            return Json(new { status = "success", data = ci });
        }

        catch (Exception e)
        {
            logger.Error($"error {e.Message} occured. stack trace: {nameof(AccountController)}/{nameof(GetCities)}");

            return Json(new { status = "error", message = UtilityLanguage.GetString(ConstMessages.InternalServerErrorMessage) });
        }
    }

    [HttpGet]
    public async ValueTask<IActionResult> Profile()
    {
        string userId = User.GetUserId();
        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return RedirectToAction("PageOrItemNotFound", "Account");
        }

        List<SelectListModel> lst = controllerHelper.GetAddressTypes();
        ViewBag.AddressTypes = lst;

        // Map ApplicationUser to UserProfileDTO
        UserProfileDto dto = mapper.Map<UserProfileDto>(user.Profile);

        // Convert birthdate to Persian date if culture is Persian
        if (CultureInfo.CurrentCulture.Name == "fa-IR" && user.Profile.BirthDate != null)
        {
            dto.PersianBirthDate = user.Profile.BirthDate.Value.ToPersianDdate();
        }

        // Set other properties in DTO
        dto.UserName = user.UserName;
        dto.UserId = user.Id;

        if (!string.IsNullOrWhiteSpace(dto.ProfilePhoto.ImageId))
        {
            CultureInfo current = new("en-US") { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
            Thread.CurrentThread.CurrentCulture = current;
            Domain domain = controllerHelper.GetCurrentUserDomain();
            string objectName = $"{domain.Id}/{dto.ProfilePhoto.ImageId}/{dto.ProfilePhoto.FileName.Replace(':', '-')}";
            (bool success, byte[] imageData) = await minioHelper.GetObject("accountimage", objectName);

            if (success)
            {
                dto.ProfilePhoto.Content = Convert.ToBase64String(imageData);
            }
        }

        DataLayer.Entities.General.Language.Language defLang = controllerHelper.GetDefaultLanguage();
        CultureInfo current2 = new(defLang.Symbol) { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
        Thread.CurrentThread.CurrentCulture = current2;

        // Get countries and set them in ViewBag
        List<SelectListModel> countries = controllerHelper.GetAllCountries();

        if (countries is null)
        {
            ViewBag.Countries = null;
        }

        ViewBag.Countries = countries;

        // Set language list in ViewBag
        ViewBag.LangList = controllerHelper.GetAllActiveLanguage();

        // Get currency list and set them in ViewBag          
        ViewBag.CurrencyList = controllerHelper.GetAllActiveCurrency();

        return View(dto);
    }

    [HttpPost]
    public async ValueTask<IActionResult> Profile([FromBody] UserProfileDto dto, CancellationToken cancellationToken)
    {
        JsonResult result;
        List<AjaxValidationErrorModel> errors = [];

        if (!ModelState.IsValid)
        {
            foreach (string modelStateKey in ModelState.Keys)
            {
                ModelStateEntry modelStateVal = ModelState[modelStateKey];

                if (modelStateVal != null)
                {
                    errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
            }

            result = Json(new { Status = "ModelError", ModelStateErrors = errors });
        }
        else
        {
            Result<Currency> currency = new();
            if (dto.DefaultCurrencyId != "-1")
            {
                currency = controllerHelper.FetchCurrency(dto.DefaultCurrencyId);
            }

            ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
            DataLayer.Entities.General.Language.Language lan = controllerHelper.FetchLanguage(dto.DefaultLanguageId);
            dto.DefaultLanguageName = lan.LanguageName;
            dto.DefaultCurrencyName = currency.ReturnValue.CurrencyName;
            ApplicationUser user = await userManager.FindByIdAsync(dto.UserId);
            Profile pro = mapper.Map<Profile>(dto);

            if (!string.IsNullOrWhiteSpace(dto.PersianBirthDate))
            {
                pro.BirthDate = dto.PersianBirthDate.Split(" ")[0].ToEnglishDate();
                pro.FullName = dto.FileContent + " " + dto.LastName;
            }

            Image img = new() { Content = dto.FileContent, FileName = dto.FileName };

            if (!string.IsNullOrEmpty(img.Content))
            {
                CultureInfo current = new("en-US") { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
                Thread.CurrentThread.CurrentCulture = current;

                if (string.IsNullOrEmpty(img.ImageId))
                {
                    img.ImageId = Guid.NewGuid().ToString();
                }

                byte[] bytes = Convert.FromBase64String(img.Content.Replace("data:image/jpeg;base64,", ""));
                SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(bytes);
                MemoryStream ms = new();
                await image.SaveAsJpegAsync(ms, cancellationToken);
                ms.Seek(0, SeekOrigin.Begin);
                bool isBucket = await minioHelper.MakeBucket("accountimage");

                if (isBucket)
                {
                    Domain domain = controllerHelper.GetCurrentUserDomain();
                    string objectName = $"{domain.Id}/{img.ImageId}/{img.FileName.Replace(':', '-')}";
                    bool isDelete = await minioHelper.RemoveObject("accountimage", objectName);
                    bool isSave = await minioHelper.Upload("accountimage", objectName, ms, "image/jpeg", ms.Length);

                    if (isSave)
                    {
                        logger.Information($"image with {img.ImageId} id in account with {dto.UserId} id saved correctly");
                    }
                    else
                    {
                        logger.Error($"image not save correctly. stack trace: {nameof(AccountController)}/{nameof(Profile)}");
                    }
                }

                DataLayer.Entities.General.Language.Language defLang = controllerHelper.GetDefaultLanguage();
                CultureInfo current2 = new(defLang.Symbol) { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
                Thread.CurrentThread.CurrentCulture = current2;
                pro.ProfilePhoto = img;
            }
            else
            {
                if (user != null)
                {
                    pro.ProfilePhoto = user.Profile.ProfilePhoto;
                }
            }

            pro.FullName = dto.FirstName + " " + dto.LastName;

            if (user == null)
            {
                //   return result;
            }

            user.Profile = pro;

            Result<ApplicationUser> updateRes = await userRepository.UpdateAsync(c => c.Id == user.Id, m => m.Profile, user.Profile, cancellationToken);

            if (user.UserName != dto.UserName)
            {
                user.UserName = dto.UserName;
                updateRes = await userRepository.UpdateAsync(c => c.Id == user.Id, m => m.UserName, user.UserName, cancellationToken);
            }

            if (updateRes.Succeeded)
            {
                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName} user profile {user.Id} updated successfully.");
                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Update,
                    CollectionType = CollectionType.ApplicationUser,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = user.Id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
            }

            result = Json(updateRes.Succeeded
                              ? new { Status = "Success", Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") }
                              : new { Status = "Error", Message = UtilityLanguage.GetString("AlertAndMessage_OperationFailed") });
        }

        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> SendOtpResetPassword(RegisterDto model, CancellationToken cancellationToken)
    {
        Domain domain = controllerHelper.GetCurrentUserDomain();

        #region Validate
        if (!HttpContext.Session.ValidateCaptcha(model.Captcha))
        {
            ModelState.AddModelError("Captcha", UtilityLanguage.GetString("AlertAndMessage_CaptchaIncorrectOrExpired"));
        }

        model.FullCellPhoneNumber = model.FullCellPhoneNumber.Replace("+", "");
        model.FullCellPhoneNumber = model.FullCellPhoneNumber.Replace(" ", "");

        if (string.IsNullOrWhiteSpace(model.FullCellPhoneNumber))
        {
            ModelState.AddModelError("CellPhoneNumber", UtilityLanguage.GetString("Validation_EnterMobileNumber"));
        }
        else
        {
            PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

            PhoneNumber phoneNumber = phoneUtil.Parse(model.FullCellPhoneNumber, "IR");

            if (!phoneUtil.IsValidNumber(phoneNumber))
            {
                ModelState.AddModelError("CellPhoneNumber", UtilityLanguage.GetString("Validation_MobileNumberInvalid1"));
            }
            else
            {
                PhoneNumberType numberType = phoneUtil.GetNumberType(phoneNumber); // Produces Mobile , FIXED_LINE 

                if (numberType != PhoneNumberType.MOBILE)
                {
                    ModelState.AddModelError("CellPhoneNumber", UtilityLanguage.GetString("Validation_MobileNumberInvalid2"));
                }
            }
        }

        if (userManager.Users.Any(c => c.PhoneNumber == model.FullCellPhoneNumber))
        {
            ModelState.AddModelError("CellPhoneNumber", UtilityLanguage.GetString("Validation_MobileNumberAlreadyRegistered"));
        }

        if (string.IsNullOrWhiteSpace(model.SecurityCode))
        {
            ModelState.AddModelError("SecurityCode", UtilityLanguage.GetString("AlertAndMessage_ProfileConfirmPhoneError"));
        }

        if (!ModelState.IsValid)
        {
            List<AjaxValidationErrorModel> errors = [];

            foreach (string modelStateKey in ModelState.Keys)
            {
                ModelStateEntry modelStateVal = ModelState[modelStateKey];

                if (modelStateVal != null)
                {
                    errors.AddRange(modelStateVal.Errors
                                                 .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
            }

            return Ok(new { Status = "ModelError", ModelStateErrors = errors });
        }
        #endregion

        ApplicationUser user = await userManager.FindByNameAsync(model.FullCellPhoneNumber);

        if (user == null)
        {
            return Ok(new { Status = "Error", Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundUser") });
        }

        if (user.PhoneNumber != model.FullCellPhoneNumber)
        {
            return Ok(new { Status = "Error", Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundUser") });
        }

        string otp = DataLayer.Helpers.Utilities.GenerateOtp();
        HttpContext.Session.SetString("Otp", otp);
        HttpContext.Session.SetString("Phone", model.FullCellPhoneNumber);
        HttpContext.Session.SetString("OtpTime", DateTime.Now.ToString(CultureInfo.CurrentCulture));
        _ = controllerHelper.FetchDomainByName(domain.DomainName, false).ReturnValue;
        Result result = await createNotification.SendOtp("SendOtpForResetPassword", user, otp, cancellationToken);

        return Ok(result.Succeeded ? new { Status = "Success", result.Message } : new { Status = "Error", result.Message });
    }

    [HttpGet]
    public async ValueTask<IActionResult> GetOtpResetPassword()
    {
        string phone = HttpContext.Session.GetString("Phone");

        if (phone == null)
        {
            return RedirectToAction("RequestResetPassword");
        }

        ApplicationUser existUser = await userManager.FindByNameAsync(phone);

        if (existUser != null)
        {
            ViewBag.Error = UtilityLanguage.GetString("AlertAndMessage_NoUserWasFound");
        }

        return View();
    }

    [HttpGet]
    public IActionResult EnterOtp()
    {
        EnterOtpModel model = new();

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AlreadyRegisteredCellPhoneNumber(string cellPhoneNumber)
    {
        #region validate
        cellPhoneNumber = cellPhoneNumber.Replace("+", "");
        cellPhoneNumber = cellPhoneNumber.Replace(" ", "");

        if (string.IsNullOrWhiteSpace(cellPhoneNumber))
        {
            return Ok(new { Status = "Error", Message = UtilityLanguage.GetString("Validation_EnterMobileNumber") });
        }

        PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

        PhoneNumber phoneNumber = phoneUtil.Parse(cellPhoneNumber, "IR");

        if (!phoneUtil.IsValidNumber(phoneNumber))
        {
            return Ok(new { Status = "Error", Message = UtilityLanguage.GetString("Validation_MobileNumberInvalid1") });
        }

        PhoneNumberType numberType = phoneUtil.GetNumberType(phoneNumber);

        return numberType != PhoneNumberType.MOBILE ? Ok(new { Status = "Error", Message = UtilityLanguage.GetString("Validation_MobileNumberInvalid2") }) : Ok(userManager.Users.Any(c => c.PhoneNumber == cellPhoneNumber) ? new { Status = "Error", Message = UtilityLanguage.GetString("Validation_MobileNumberAlreadyRegistered") } : new { Status = "Success", Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") });
        #endregion
    }

    [HttpPost]
    public async ValueTask<IActionResult> AddAddress(AddressDto address, CancellationToken cancellationToken)
    {
        ApplicationUser user;

        try
        {
            if (!ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(address.AddressTypeId))
                {
                    ModelState.AddModelError("AddressType", UtilityLanguage.GetString("AlertAndMessage_AddressType"));
                }

                if (string.IsNullOrEmpty(address.CountryId))
                {
                    ModelState.AddModelError("CountryId", UtilityLanguage.GetString("AlertAndMessage_CountryId"));
                }

                if (string.IsNullOrEmpty(address.ProvinceId))
                {
                    ModelState.AddModelError("ProvinceId", UtilityLanguage.GetString("AlertAndMessage_ProvinceId"));
                }

                if (string.IsNullOrEmpty(address.CityId))
                {
                    ModelState.AddModelError("CityId", UtilityLanguage.GetString("AlertAndMessage_CityId"));
                }

                if (!double.TryParse(address.PostalCode, out double _))
                {
                    ModelState.AddModelError("PostalCode", UtilityLanguage.GetString("AlertAndMessage_EnterPostalCode"));
                }

                if (string.IsNullOrEmpty(address.Address1))
                {
                    ModelState.AddModelError("Address1", UtilityLanguage.GetString("AlertAndMessage_Address1"));
                }

                List<ClientValidationErrorModel> errors = [];

                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry modelStateVal = ModelState[modelStateKey];

                    if (modelStateVal == null)
                    {
                        continue;
                    }

                    errors.AddRange(modelStateVal.Errors.Select(error => new ClientValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }

                return Json(new { Status = "error", message = UtilityLanguage.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
            }

            if (User.Identity is not { IsAuthenticated: true })
            {
                return Json(new { status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
            }

            string userId = User.GetUserId();
            user = await userManager.FindByIdAsync(userId);

            address.Id = Guid.NewGuid().ToString();

            Address add = mapper.Map<Address>(address);

            if (user != null)
            {
                user.Profile.Addresses.Add(add);

                Result<ApplicationUser> updateAsync = await userRepository.UpdateAsync(c => c.Id == user.Id, m => m.Profile.Addresses, user.Profile.Addresses, cancellationToken);

                if (!updateAsync.Succeeded)
                {
                    return Json(new { status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
                }
            }

            object returnUrl = TempData["ReturnUrl"];

            return Json(new { status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_AddressAddedSuccessfully"), Url = returnUrl });

        }
        catch (Exception ex)
        {
            Log.Error($"exception with {ex.Message} occured. stack trace: {nameof(AccountController)}/{nameof(AddAddress)}");
            return Json(new { status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
        }
    }

    [HttpGet]
    public IActionResult Address()
    {
        string returnUrl = HttpContext.Request.Headers["Referer"].ToString();

        ViewBag.CountryList = controllerHelper.GetAllCountries();

        ViewBag.AddressTypes = controllerHelper.GetAddressTypes();

        TempData["ReturnUrl"] = returnUrl;

        return View();
    }

    [HttpGet]
    public async ValueTask<IActionResult> GetUserOrders(CancellationToken cancellationToken)
    {
        if (HttpContext.Request.Path.Value == null)
        {
            return View();
        }

        string lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
        ViewBag.LanIcon = lanIcon;

        if (User.Identity is not { IsAuthenticated: true })
        {
            return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + $"/{lanIcon}/Account/Login?returnUr=/{lanIcon}/basket/get");
        }

        string domain = controllerHelper.GetCurrentUserDomain().DomainName ?? throw new ArgumentNullException($"{nameof(controllerHelper)}.FetchDefaultDomain().ReturnValue.DomainName");
        string currentUserId = User.GetUserId();
        Result<Domain> res = controllerHelper.FetchDomainByName(domain, false);

        if (res != null)
        {
            List<Transaction> transactionsModel = await transactionRepository.GetListAsync(c => c.CreatorUserId == currentUserId, cancellationToken);
            List<TransactionDto> transactionsDto = mapper.Map<List<TransactionDto>>(transactionsModel);

            return View(transactionsDto);
        }

        List<TransactionDto> model = [];

        return View(model);

    }

    [HttpGet]
    public IActionResult RequestResetPassword()
    {
        bool isAuthenticated = HttpContext.User.Identity is { IsAuthenticated: true };

        if (isAuthenticated)
        {
            return LocalRedirect("/");
        }

        string captcha = HttpContext.Session.GenerateCaptchaImageString(2);
        ViewBag.Captcha = $"data:image/png;base64,{captcha}";

        return View();

    }
}