using Arad.Portal.DataLayer.Contracts.General.CountryParts;
using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Error;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.User;
using Arad.Portal.DataLayer.Entities.General.Error;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Helpers;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.User;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly CreateNotification _createNotification;
        private readonly IDomainRepository _domainRepository;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILanguageRepository _lanRepository;
        private readonly ICurrencyRepository _curRepository;
        private readonly string _domainName;
        private readonly IMapper _mapper;
        
        public AccountController(UserManager<ApplicationUser> userManager,
            CreateNotification createNotification,
            IMapper mapper,
            IUserRepository userRepo,
            IErrorLogRepository errorLogRepository,
            IDomainRepository domainRepository,
            IHttpContextAccessor accessor,
            ICountryRepository countryRepository,
            IWebHostEnvironment env,
            ICurrencyRepository curRepo,
            ILanguageRepository lanRepo,
            SignInManager<ApplicationUser> signInManager):base(accessor, env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _createNotification = createNotification;
            _errorLogRepository = errorLogRepository;
            _domainRepository = domainRepository;
            _userRepository = userRepo;
            _domainName = DomainName;
            _lanRepository = lanRepo;
            _curRepository = curRepo;
            _mapper = mapper;
            _countryRepository = countryRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "Role")]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            return Redirect($"/{lanIcon}/Account/Login");
        }

        [HttpGet]
        public IActionResult Address()
        {
            var returnUrl = HttpContext.Request.Headers["Referer"].ToString();
            var lst = _userRepository.GetAddressTypes();
            ViewBag.AddressTypes = lst;
            TempData["ReturnUrl"] = returnUrl;
            return View();
        }


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
            var captcha = HttpContext.Session.GenerateCaptchaImageString(2);
            ViewBag.Captcha = $"data:image/png;base64,{captcha}";
            LoginDTO loginDto = new() { ReturnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl, RememberMe = false };

            return View(loginDto);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginDTO model)
        {
            if (!HttpContext.Session.ValidateCaptcha(model.Captcha))
            {
                ModelState.AddModelError("Captcha", Language.GetString("AlertAndMessage_CaptchaIncorrectOrExpired"));
            }

            if (string.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError("Username", Language.GetString("Validation_Username"));
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", Language.GetString("Validation_Password"));
            }

            await HttpContext.SignOutAsync();
            ApplicationUser user = await _userManager.FindByNameAsync(model.Username);

            if (user != null)
            {
                if (await _userManager.CheckPasswordAsync(user, model.Password) != true)
                {
                    ModelState.AddModelError("Password", Language.GetString("AlertAndMessage_PasswordIncorrect"));
                }

                if (!user.IsActive)
                {
                    ModelState.AddModelError("Username", Language.GetString("AlertAndMessage_AccountHasDeactivated"));
                }

                //List<User> upAdmins = await _userExtension.GetUpUsers(user.Id);

                //if (upAdmins.Any(x => !x.IsActive))
                //{
                //    ModelState.AddModelError("Username", Language.GetString("AlertAndMessage_AccountHasDeactivated"));
                //}
            }
            else
            {
                ModelState.AddModelError("Username", Language.GetString("AlertAndMessage_UsernameIncorrect"));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (user == null)
            {
                return View(model);
            }

           
            await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            user.LastLoginDate = DateTime.Now;
            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && model.ReturnUrl != "/")
            {
                return Redirect(model.ReturnUrl);
            }

            //???
            //if (CultureInfo.CurrentCulture.Name != user.DefaultLanguage)
            //{
            //    Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, CookieRequestCultureProvider.MakeCookieValue(new(user.DefaultLanguage)), new() { Expires = DateTimeOffset.Now.AddYears(1) });
            //}

            //TempData["LoginUser"] = string.Format(Language.GetString("AlertAndMessage_WelcomeUser"), user.FullName);
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
           
            return Redirect($"/{lanIcon}/Home/Index");
        }

        [HttpGet]
        public IActionResult Register()
        {
            RegisterDTO registerDto = new();

            return View(registerDto);
        }

        [HttpGet]
        public ActionResult CheckCaptcha(string captcha)
        {
            return Ok(HttpContext.Session.ValidateCaptcha(captcha) ? new { Status = "success" } : new { Status = "error" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] RegisterDTO model)
        {
            #region Validate
            if (!HttpContext.Session.ValidateCaptcha(model.Captcha))
            {
                ModelState.AddModelError("Captcha", Language.GetString("AlertAndMessage_CaptchaIncorrectOrExpired"));
            }

            model.FullCellPhoneNumber = model.FullCellPhoneNumber.Replace("+", "");
            model.FullCellPhoneNumber = model.FullCellPhoneNumber.Replace(" ", "");

            if (string.IsNullOrWhiteSpace(model.FullCellPhoneNumber))
            {
                ModelState.AddModelError("CellPhoneNumber", Language.GetString("Validation_EnterMobileNumber"));
            }
            else
            {
                PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

                PhoneNumber phoneNumber = phoneUtil.Parse(model.FullCellPhoneNumber, "IR");

                if (!phoneUtil.IsValidNumber(phoneNumber))
                {
                    ModelState.AddModelError("CellPhoneNumber", Language.GetString("Validation_MobileNumberInvalid1"));
                }
                else
                {
                    PhoneNumberType numberType = phoneUtil.GetNumberType(phoneNumber); // Produces Mobile , FIXED_LINE 

                    if (numberType != PhoneNumberType.MOBILE)
                    {
                        ModelState.AddModelError("CellPhoneNumber", Language.GetString("Validation_MobileNumberInvalid2"));
                    }
                }
            }

            if (_userManager.Users.Any(c => c.PhoneNumber == model.FullCellPhoneNumber))
            {
                ModelState.AddModelError("CellPhoneNumber", Language.GetString("Validation_MobileNumberAlreadyRegistered"));
            }

            if (string.IsNullOrWhiteSpace(model.SecurityCode))
            {
                ModelState.AddModelError("SecurityCode", Language.GetString("AlertAndMessage_ProfileConfirmPhoneError"));
            }

            OTP otp = OtpHelper.Get(model.FullCellPhoneNumber);

            if (otp == null)
            {
                ModelState.AddModelError("SecurityCode", Language.GetString("AlertAndMessage_ProfileConfirmPhoneError"));
            }
            else
            {
                if (otp.ExpirationDate >= DateTime.Now.AddMinutes(3))
                {
                    ModelState.AddModelError("SecurityCode", Language.GetString("AlertAndMessage_ProfileConfirmPhoneTimeOut"));
                }

                if (!string.IsNullOrWhiteSpace(model.SecurityCode) && !model.SecurityCode.Equals(otp.Code))
                {
                    ModelState.AddModelError("SecurityCode", Language.GetString("AlertAndMessage_ProfileConfirmPhoneTimeOut"));
                }
            }

            if (!ModelState.IsValid)
            {
                List<AjaxValidationErrorModel> errors = new();

                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors
                        .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }

                return Ok(new { Status = "ModelError", ModelStateErrors = errors });
            }
            #endregion

            string id = Guid.NewGuid().ToString();

            #region Set claim
            List<IdentityUserClaim<string>> claims = new()
            {
                new() { ClaimType = ClaimTypes.GivenName, ClaimValue = model.FullCellPhoneNumber },
                new() { ClaimType = "IsActive", ClaimValue = true.ToString() },
                new() { ClaimType = "IsSystemAccount", ClaimValue = false.ToString() }
            };
            #endregion

            var domainId = _domainRepository.FetchByName(DomainName, false).ReturnValue.DomainId;
            ApplicationUser user = new()
            {
                UserName = model.FullCellPhoneNumber,
                IsSystemAccount = false,
                Id = id,
                IsDomainAdmin = false,
                Profile = new DataLayer.Models.User.Profile() { UserType = UserType.Customer},
                IsDeleted = false ,
                CreatorId = id,
                CreatorUserName = model.FullCellPhoneNumber,
                CreationDate = DateTime.Now,
                IsActive = true,
                PhoneNumber = model.FullCellPhoneNumber,
                PhoneNumberConfirmed = true,
                Modifications = new(),
                Claims = claims,
            };
            user.DomainId.Add(domainId);
            string pass = Shop.Helpers.Utilities.GenerateRandomPassword(new() { RequireDigit = true, RequireLowercase = true, 
                RequireNonAlphanumeric = false, RequireUppercase = true, RequiredLength = 6, RequiredUniqueChars = 0 });
            IdentityResult insertResult = await _userManager.CreateAsync(user, pass);

            if (insertResult.Succeeded)
            {
                Result result = await _createNotification.ClientSignUp("ClientSignupEmail", user, pass);

                if (!result.Succeeded)
                {
                    ErrorLog errorLog = new() { Error = result.Message, Source = @"Account\Register",
                        Ip = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() };
                    await _errorLogRepository.Add(errorLog);
                }

                await _signInManager.PasswordSignInAsync(user, pass, false, false);
            }

            return Ok(insertResult.Succeeded ?
                new { Status = "Success", Message = Language.GetString("AlertAndMessage_OperationSuccess") } : 
                new { Status = "Error", Message = insertResult.Errors.FirstOrDefault().Description });
        }
        [HttpGet]
        public IActionResult GetCountries()
        {
            var countries = _countryRepository.GetAllCountries();
            return Json(new { Status = "success", Data = countries });
        }

        [HttpGet]
        public IActionResult GetStates(string countryName)
        {
            try
            {
                var states = _countryRepository.GetStates(countryName);
                var st = states.Select(c => new CountryPartView()
                {
                    Id = c.Value,
                    Name = c.Text
                }).ToList();
                return Json(new { Status = "success", Data = st });
            }
            catch (Exception e)
            {
                return Json(new { status = "error", data = new List<CountryPartView>() });
            }
        }

        [HttpGet]
        public IActionResult GetCities(string stateId)
        {
            try
            {
                var cities = _countryRepository.GetCities(stateId);
                var ci = cities.Select(c => new CountryPartView()
                {
                    Id = c.Value,
                    Name = c.Text
                }).ToList();

                return Json(new { status = "success", data = ci });
            }

            catch (Exception e)
            {
                return Json(new { status = "error", data = new List<CountryPartView>() });
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddAddress(AddressDto address)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    double i;
                    if (string.IsNullOrEmpty(address.AddressTypeId))
                    {
                        ModelState.AddModelError("AddressType", Language.GetString("AlertAndMessage_AddressType"));
                    }
                    if (string.IsNullOrEmpty(address.CountryId))
                    {
                        ModelState.AddModelError("CountryId", Language.GetString("AlertAndMessage_CountryId"));
                    }
                    if (string.IsNullOrEmpty(address.ProvinceId))
                    {
                        ModelState.AddModelError("ProvinceId", Language.GetString("AlertAndMessage_ProvinceId"));
                    }
                    if (string.IsNullOrEmpty(address.CityId))
                    {
                        ModelState.AddModelError("CityId", Language.GetString("AlertAndMessage_CityId"));
                    }
                    if (!double.TryParse(address.PostalCode, out i))
                    {
                        ModelState.AddModelError("PostalCode", Language.GetString("AlertAndMessage_EnterPostalCode"));
                    }
                  
                    if (string.IsNullOrEmpty(address.Address1))
                    {
                        ModelState.AddModelError("Address1", Language.GetString("AlertAndMessage_Address1"));
                    }

                    List<ClientValidationErrorModel> errors = new List<ClientValidationErrorModel>();

                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        var modelStateVal = ModelState[modelStateKey];
                        foreach (var error in modelStateVal.Errors)
                        {
                            var obj = new ClientValidationErrorModel
                            {
                                Key = modelStateKey,
                                ErrorMessage = error.ErrorMessage,
                            };

                            errors.Add(obj);
                        }
                    }
                   
                    return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
                }

                //var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                //var userName = User.FindFirstValue(ClaimTypes.Name);

                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = await _userManager.FindByIdAsync(userId);

                address.Id = Guid.NewGuid().ToString();

                var provinceId = address.ProvinceId;
                var add = _mapper.Map<Address>(address);
                user.Profile.Addresses.Add(add);

                var updateAsync = await _userManager.UpdateAsync(user);

                if (updateAsync.Succeeded)
                {
                    var returnUrl = TempData["ReturnUrl"];
                    return Json(new { status = "success", Message = Language.GetString("AlertAndMessage_AddressAddedSuccessfully"), Url = returnUrl });
                }

                return Json(new { status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }
            catch (Exception x)
            {
                return Json(new { status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }
        }


        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
            JsonResult result;
            if (!ModelState.IsValid)
            {
                List<ClientValidationErrorModel> errors = new List<ClientValidationErrorModel>();

                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    foreach (var error in modelStateVal.Errors)
                    {
                        var obj = new ClientValidationErrorModel
                        {
                            Key = modelStateKey,
                            ErrorMessage = error.ErrorMessage,
                        };

                        errors.Add(obj);
                    }
                }
                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }

            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = await _userManager.FindByIdAsync(userId);

                var res = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);

                if (res.Succeeded)
                {
                    result = Json(new { status = "success", Message = Language.GetString("AlertAndMessage_PasswordChangeSuccessfully") });
                }
               result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = new List<string>() });
            }
            catch (Exception e)
            {
                result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = new List<string>() });
            }
            return result;
        }

       
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            string userId = User.GetUserId();
            ApplicationUser user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("PageOrItemNotFound", "Account");
            }

            //await SetViewBag();
            var lst = _userRepository.GetAddressTypes();
            ViewBag.AddressTypes = lst;
            UserProfileDTO dto = _mapper.Map<UserProfileDTO>(user.Profile);
            var countries = this.GetCountries();
            ViewBag.Countries = countries;
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            ViewBag.LanIcon = lanIcon;
            var currencyList = _curRepository.GetAllActiveCurrency();
            ViewBag.CurrencyList = currencyList;

            //await SetViewBag();

            return View(dto);
        }


        [HttpPost]
        public async Task<IActionResult> Profile([FromBody] UserProfileDTO dto)
        {
            JsonResult result;
            var errors = new List<AjaxValidationErrorModel>();
            if (!ModelState.IsValid)
            {

                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel
                    { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }else
            {
                var user =await _userManager.FindByIdAsync(dto.UserID);
                var pro = _mapper.Map<DataLayer.Models.User.Profile>(dto);
                user.Profile = pro;
                var res = await _userManager.UpdateAsync(user);
               
                    result = Json(res.Succeeded ? new { Status = "Success", Message = Language.GetString("AlertAndMessage_OperationSuccess") }
                   : new { Status = "Error", Message = Language.GetString("AlertAndMessage_OperationFailed") });
               
            }
            return result;

        }

        [HttpGet]
        public IActionResult PageOrItemNotFound()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AlreadyRegisteredCellPhoneNumber(string cellPhoneNumber)
        {
            #region validate
            cellPhoneNumber = cellPhoneNumber.Replace("+", "");
            cellPhoneNumber = cellPhoneNumber.Replace(" ", "");

            if (string.IsNullOrWhiteSpace(cellPhoneNumber))
            {
                return Ok(new { Status = "Error", Message = Language.GetString("Validation_EnterMobileNumber") });
            }

            PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

            PhoneNumber phoneNumber = phoneUtil.Parse(cellPhoneNumber, "IR");

            if (!phoneUtil.IsValidNumber(phoneNumber))
            {
                return Ok(new { Status = "Error", Message = Language.GetString("Validation_MobileNumberInvalid1") });
            }

            PhoneNumberType numberType = phoneUtil.GetNumberType(phoneNumber); // Produces Mobile , FIXED_LINE 

            if (numberType != PhoneNumberType.MOBILE)
            {
                return Ok(new { Status = "Error", Message = Language.GetString("Validation_MobileNumberInvalid2") });
            }

            if (_userManager.Users.Any(c => c.PhoneNumber == cellPhoneNumber))
            {
                return Ok(new { Status = "Error", Message = Language.GetString("Validation_MobileNumberAlreadyRegistered") });
            }
            #endregion

            return Ok(new { Status = "Success", Message = "" });
        }

        [HttpGet]
        public async Task<IActionResult> SendSecurityCode(string cellPhoneNumber)
        {
            #region validate
            cellPhoneNumber = cellPhoneNumber.Replace("+", "");
            cellPhoneNumber = cellPhoneNumber.Replace(" ", "");

            if (string.IsNullOrWhiteSpace(cellPhoneNumber))
            {
                return Ok(new { Status = "Error", Message = Language.GetString("Validation_EnterMobileNumber") });
            }

            PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

            PhoneNumber phoneNumber = phoneUtil.Parse(cellPhoneNumber, "IR");

            if (!phoneUtil.IsValidNumber(phoneNumber))
            {
                return Ok(new { Status = "Error", Message = Language.GetString("Validation_MobileNumberInvalid1") });
            }

            PhoneNumberType numberType = phoneUtil.GetNumberType(phoneNumber); // Produces Mobile , FIXED_LINE 

            if (numberType != PhoneNumberType.MOBILE)
            {
                return Ok(new { Status = "Error", Message = Language.GetString("Validation_MobileNumberInvalid2") });
            }

            OTP process = OtpHelper.Process(new() { Mobile = cellPhoneNumber, Code = DataLayer.Helpers.Utilities.GenerateOtp(),
                ExpirationDate = DateTime.Now.AddMinutes(3), IsSent = false });

            if (process.IsSent)
            {
                return Ok(new { Status = "Error", Message = Language.GetString("AlertAndMessage_ProfileConfirmPhoneSending") });
            }
            #endregion

            Result result = await _createNotification.SendOtp("SendOtp", cellPhoneNumber, process.Code);

            if (!result.Succeeded)
            {
                ErrorLog errorLog = new() { Error = result.Message, Source = @"Account\SendSecurityCode", Ip = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() };
                await _errorLogRepository.Add(errorLog);

                return Ok(new { Status = "Error", Message = Language.GetString("AlertAndMessage_SubmitOtpPleaseTryAgainLater") });
            }

            return Ok(new { Status = "Success", result.Message });
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Login([FromForm] LoginViewModel model)
        //{
        //    if (!HttpContext.Session.ValidateCaptcha(model.Captcha))
        //    {
        //        ModelState.AddModelError("Captcha", Language.GetString("AlertAndMessage_CaptchaIsExpired"));
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    await HttpContext.SignOutAsync();
        //    ApplicationUser user = await _userManager.FindByNameAsync(model.Username);

        //    if (user == null || await _userManager.CheckPasswordAsync(user, model.Password) != true)
        //    {
        //        ViewBag.Message = Language.GetString("AlertAndMessage_InvalidUsernameOrPassword");
        //        return View(model);
        //    }

        //    if (!user.IsActive)
        //    {
        //        ViewBag.Message = Language.GetString("AlertAndMessage_InActiveUserAccount");
        //        return View(model);
        //    }
        //    await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

        //    user.LastLoginDate = DateTime.Now;
        //    await _userManager.UpdateAsync(user);

        //    if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && model.ReturnUrl != "/")
        //    {
        //        return Redirect(model.ReturnUrl);
        //    }

        //    TempData["LoginUser"] = $"{user.Profile.FirstName} {user.Profile.LastName} {Language.GetString("AlertAndMessage_Welcome")}";
        //    return RedirectToAction("Index", "Home");
        //}


        [HttpGet]
        public IActionResult ChangeLang(string langId)
        {
            //var domainName = this.DomainName;
            string domainName = $"{HttpContext.Request.Host}";
            if (domainName.ToString().ToLower().StartsWith("localhost"))
            {
                //prevent port of localhost
                domainName = HttpContext.Request.Host.ToString().Substring(0, 9);
            }
            if (CultureInfo.CurrentCulture.Name != langId)
            {
                Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(langId))
                    , new CookieOptions()
                    {
                        Expires = DateTimeOffset.Now.AddYears(1),
                        Domain = domainName
                    });
                //CultureInfo.CurrentCulture.DateTimeFormat = new CultureInfo("en-US").DateTimeFormat;
                return Ok(true);
            }
            return Ok(false);
        }

        


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword()
        {
            ResetPassword model = new();
            return View("ResetPassword", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPassword model)
        {
            #region validate
            if (!HttpContext.Session.ValidateCaptcha(model.Captcha))
            {
                ModelState.AddModelError("Captcha", Language.GetString("AlertAndMessage_CaptchaIncorrectOrExpired"));
            }

            if (string.IsNullOrWhiteSpace(model.UserName))
            {
                ModelState.AddModelError("UserName", Language.GetString("Validation_EnterUsername"));
            }

            if (string.IsNullOrWhiteSpace(model.FullCellPhoneNumber))
            {
                ModelState.AddModelError("CellPhoneNumber", Language.GetString("Validation_EnterMobileNumber"));
            }
            else
            {
                PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

                PhoneNumber phoneNumber = phoneUtil.Parse(model.FullCellPhoneNumber, "IR");

                if (!phoneUtil.IsValidNumber(phoneNumber))
                {
                    ModelState.AddModelError("CellPhoneNumber", Language.GetString("Validation_MobileNumberInvalid1"));
                }
                else
                {
                    PhoneNumberType numberType = phoneUtil.GetNumberType(phoneNumber); // Produces Mobile , FIXED_LINE 

                    if (numberType != PhoneNumberType.MOBILE)
                    {
                        ModelState.AddModelError("CellPhoneNumber", Language.GetString("Validation_MobileNumberInvalid2"));
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                List<AjaxValidationErrorModel> errors = new();

                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors.Select(error => 
                    new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }

                return Ok(new { Status = "ModelError", ModelStateErrors = errors });
            }

            #endregion

            ApplicationUser user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null)
            {
                return Ok(new { Status = "Error", Message = Language.GetString("AlertAndMessage_NotFoundUser") });
            }

            if (user.PhoneNumber != model.FullCellPhoneNumber)
            {
                return Ok(new { Status = "Error", Message = Language.GetString("AlertAndMessage_NotFoundUser") });
            }

            string otp = DataLayer.Helpers.Utilities.GenerateOtp();
            HttpContext.Session.SetString("Otp", otp);
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("OtpTime", DateTime.Now.ToString(CultureInfo.CurrentCulture));

            Result result = await _createNotification.SendOtp("SendOtpForResetPassword", user, otp);

            if (!result.Succeeded)
            {
                ErrorLog errorLog = new() { Error = result.Message, Source = @"Account\ResetPassword", 
                    Ip = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() };
                await _errorLogRepository.Add(errorLog);
            }

            return Ok(result.Succeeded ? new { Status = "Success", result.Message } : new { Status = "Error", result.Message });
        }

    }
}
