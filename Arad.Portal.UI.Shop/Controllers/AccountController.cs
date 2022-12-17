using Arad.Portal.DataLayer.Contracts.General.Content;
using Arad.Portal.DataLayer.Contracts.General.CountryParts;
using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Error;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.Notification;
using Arad.Portal.DataLayer.Contracts.General.User;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Contracts.Shop.Transaction;
using Arad.Portal.DataLayer.Entities.General.Error;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Helpers;
using Arad.Portal.DataLayer.Models.Notification;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Transaction;
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
using Microsoft.Extensions.Configuration;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Serilog;
using AspNetCore.Identity.MongoDbCore.Models;

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
        private readonly ITransactionRepository _traRepository;
        private readonly ICurrencyRepository _curRepository;
        private readonly IConfiguration _configuration;
        private readonly IProductRepository _productRepository;
        private readonly IContentRepository _contentRepository;
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
            ICurrencyRepository curRepo,
            IConfiguration configuration, 
            ILanguageRepository lanRepo,
            IProductRepository productRepository,
            IContentRepository contentRepository,
            ITransactionRepository transactionRepository,
            SignInManager<ApplicationUser> signInManager):base(accessor, domainRepository)
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
            _traRepository = transactionRepository;
            _mapper = mapper;
            _contentRepository = contentRepository;
            _productRepository = productRepository;
            _configuration = configuration;
            _countryRepository = countryRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

       
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                await _signInManager.SignOutAsync();
                var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
                return Redirect(HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.ToString() + $"/{lanIcon}/Account/Login");
            }
            else
                return null;
          
        }

        [HttpGet]
        public IActionResult Address()
        {
            var returnUrl = HttpContext.Request.Headers["Referer"].ToString();
            var lst = _userRepository.GetAddressTypes();
            ViewBag.CountryList = _countryRepository.GetAllCountries();
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
            model.FullUserName = model.FullUserName.Replace("+", "");
           
            ApplicationUser user = await _userManager.FindByNameAsync(model.FullUserName.Trim());
            
            if(user == null)
            {
                user = _userManager.Users.FirstOrDefault(_ => _.PhoneNumber == model.FullUserName.Trim());
                
            }
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
            }
            else
            {
                ModelState.AddModelError("Username", Language.GetString("AlertAndMessage_UsernameIncorrect"));
            }

            if (!ModelState.IsValid || user == null)
            {
                return View(model);
            }
          
            var res = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
            if(res.Succeeded)
            {
                //check whether this domain added to userDomains if not added then add

                var domainEntity = _domainRepository.FetchByName(this.DomainName, false).ReturnValue;
                if(!user.Domains.Any(_=>_.DomainId == domainEntity.DomainId))
                {
                    user.Domains.Add(new UserDomain()
                    {
                        DomainId = domainEntity.DomainId,
                        DomainName = domainEntity.DomainName,
                        IsOwner = false
                    });
                }
                Log.Fatal("end of passwordsigninAsync successfully");
            }
            
            user.LastLoginDate = DateTime.Now;
            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && model.ReturnUrl != "/")
            {
                return Redirect(HttpContext.Request.Scheme + "://"+ HttpContext.Request.Host.ToString() + model.ReturnUrl);
            }

            
            if (CultureInfo.CurrentCulture.Name != null)
            {
                Response.Cookies
                    .Append(CookieRequestCultureProvider.DefaultCookieName, CookieRequestCultureProvider.MakeCookieValue(new(CultureInfo.CurrentCulture.Name)), new() { Expires = DateTimeOffset.Now.AddDays(5) });
            }
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];

            // return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}:/{lanIcon}");
            return Redirect("/");
        }

        [HttpGet]
        public IActionResult Register()
        {
            RegisterDTO registerDto = new();

            return View(registerDto);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            ChangePassDto registerDto = new();

            return View(registerDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePassDto model)
        {
            #region Validate
            if (!HttpContext.Session.ValidateCaptcha(model.Captcha))
            {
                ModelState.AddModelError("Captcha", Language.GetString("AlertAndMessage_CaptchaIncorrectOrExpired"));
            }

            model.FullCellPhoneNumber = model.FullCellPhoneNumber.Replace("+", "").Trim();
           

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
                if (string.IsNullOrWhiteSpace(model.Username))
                {
                    ModelState.AddModelError("Username", Language.GetString("Validation_EnterUsername"));
                }else
                {
                    var user = await _userManager.FindByNameAsync(model.Username);
                    if(user.PhoneNumber != model.FullCellPhoneNumber)
                    {
                        ModelState.AddModelError("Username", Language.GetString("Validation_InvalidUserNameOrPhoneNumber"));
                    }
                }
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

            var currentUser = await _userManager.FindByNameAsync(model.FullCellPhoneNumber);
            if(currentUser == null)
            {
                currentUser = await _userManager.FindByNameAsync(model.Username);
            }

            string pass = Helpers.Utilities.GenerateRandomPassword(new()
            {
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true,
                RequiredLength = 7,
                RequiredUniqueChars = 0
            });
            while (!Password.PasswordIsValid(true, true, true,
                   true, false, pass))
            {
                pass = Password.GeneratePassword(true, true, true,
                    true, false, 10);
            }

            Result result = await _createNotification.Send("AutomatedPasswordReset", currentUser, pass);
            if (result.Succeeded)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(currentUser);
                var changePassResult =
                    await _userManager.ResetPasswordAsync(currentUser, token, pass);

                return Ok(changePassResult.Succeeded ?
                    new { Status = "Success", Message = Language.GetString("AlertAndMessage_OperationSuccess") } :
                    new { Status = "Error", Message = Language.GetString("AlertAndMessage_Error") });
            }
            else
                return Ok(new { Status = "Error", Message = Language.GetString("AlertAndMessage_Error") });
        }

        [HttpGet]
        public IActionResult ChangeKnownPassword()
        {
            RegisterDTO registerDto = new();

            return View(registerDto);
        }

        [HttpGet]
        public ActionResult CheckCaptcha(string captcha)
        {
            return Ok(HttpContext.Session.ValidateCaptcha(captcha) ? new { Status = "success" } : new { Status = "error" });
        }

        [HttpGet]
        public IActionResult GetUserOrders()
        {
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            ViewBag.LanIcon = lanIcon;
            if (User != null && User.Identity.IsAuthenticated)
            {
                var domainName = base.DomainName;
                var currentUserId = User.GetUserId();
                var res = _domainRepository.FetchByName(domainName, false);
                if (res.Succeeded)
                {
                    var model = _traRepository.GetUserOrderHistory(currentUserId);
                    return View(model);
                }
                else
                {
                    var model = new List<TransactionDTO>();
                    return View(model);
                }

            }
            else
            {
                return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + $"/{lanIcon}/Account/Login?returnUr=/{lanIcon}/basket/get");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] RegisterVM model)
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

            if (_userManager.Users.Any(c => c.PhoneNumber == model.FullCellPhoneNumber || c.UserName == model.CellPhoneNumber))
            {
                ModelState.AddModelError("CellPhoneNumber", Language.GetString("Validation_MobileNumberAlreadyRegistered"));
            }

            if (string.IsNullOrWhiteSpace(model.SecurityCode))
            {
                ModelState.AddModelError("SecurityCode", Language.GetString("AlertAndMessage_SecurityCodeIsInvalid"));
            }

            OTP otp = OtpHelper.Get(model.FullCellPhoneNumber);

            if (otp == null)
            {
                ModelState.AddModelError("SecurityCode", Language.GetString("AlertAndMessage_SecurityCodeIsInvalid"));
            }
            else
            {
                if (otp.ExpirationDate >= DateTime.Now.AddMinutes(3))
                {
                    ModelState.AddModelError("SecurityCode", Language.GetString("AlertAndMessage_ProfileConfirmPhoneTimeOut"));
                }

                if (!string.IsNullOrWhiteSpace(model.SecurityCode) && !model.SecurityCode.Equals(otp.Code))
                {
                    ModelState.AddModelError("SecurityCode", Language.GetString("AlertAndMessage_SecurityCodeIsInvalid"));
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
            List<MongoClaim> claims = new()
            {
                new() { Type = ClaimTypes.GivenName, Value = model.FullCellPhoneNumber },
                new() { Type = "IsActive", Value = true.ToString() },
                new() { Type = "IsSystemAccount", Value = false.ToString() }
            };
            #endregion

            var domainId = _domainRepository.FetchByName(DomainName, false).ReturnValue.DomainId;
            ApplicationUser user = new()
            {
                UserName = model.FullCellPhoneNumber,
                IsSystemAccount = false,
                Id = id,
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
                IsSiteUser = true
            };
            user.Domains.Add(new() { DomainId = domainId, IsOwner = false, DomainName = DomainName });
            string pass = Helpers.Utilities.GenerateRandomPassword(new() { RequireDigit = true, RequireLowercase = true, 
                RequireNonAlphanumeric = false, RequireUppercase = true, RequiredLength = 7, RequiredUniqueChars = 0 });
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeKnownPassword([FromForm] RegisterDTO model)
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

            var currentUser = await _userManager.FindByNameAsync(model.FullCellPhoneNumber);
            var checkPass = await _userManager.CheckPasswordAsync(currentUser, model.CurrentPass);
            if(!checkPass)
            {
                ModelState.AddModelError("CurrentPass", Language.GetString("AlertAndMessage_InCorrectPassword"));
            }

            if(model.NewPass != model.ReNewPass)
            {
                ModelState.AddModelError("ReNewPass", Language.GetString("AlertAndMessage_PassWordAndRePassNotEqual"));
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
                
            var token = await _userManager.GeneratePasswordResetTokenAsync(currentUser);
            var changePass = await _userManager.ResetPasswordAsync(currentUser, token, model.NewPass);
            return Ok(changePass.Succeeded ?
                new { Status = "Success", Message = Language.GetString("AlertAndMessage_OperationSuccess") } :
                new { Status = "Error", Message = Language.GetString("AlertAndMessage_Error") });
           
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
                return Json(new { status = "error", message = Language.GetString(ConstMessages.InternalServerErrorMessage) });
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
                return Json(new { status = "error", message = Language.GetString(ConstMessages.InternalServerErrorMessage) });
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddAddress(AddressDto address)
        {
            ApplicationUser user;
            string userId;
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
                   
                    return Json(new { Status = "error", message = Language.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
                }


                if (User.Identity.IsAuthenticated)
                {
                    userId = User.GetUserId();
                    user = await _userManager.FindByIdAsync(userId);



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
                }
                return Json(new { status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }
            catch (Exception x)
            {
                return Json(new { status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Favorites(string type, string keyword = "")
        {
            var List = new List<UserFavoritesDTO>();
            var domainName = this.DomainName;
            var domainRes = _domainRepository.FetchByName(domainName, false);
            var images = new List<Image>();
            FavoriteType favType = type.ToLower() == "product" ? FavoriteType.Product : FavoriteType.Content;
            var userId =User.GetUserId();
            var lst = _userRepository.GetUserFavoriteList(userId, favType);
            string lanSymbol = string.Empty;
            string lanId = string.Empty;
            if (CultureInfo.CurrentCulture.Name != null)
            {
                lanSymbol = CultureInfo.CurrentCulture.Name;
                lanId = _lanRepository.FetchBySymbol(lanSymbol);
            }
            else
            {
                lanId = domainRes.ReturnValue.DefaultLanguageId;
            }
            ViewBag.Type = type;
            foreach (var item in lst)
                {
                    var obj = new UserFavoritesDTO();
                    obj = _mapper.Map<UserFavoritesDTO>(item);
                    
                    if(type.ToLower() == "product")
                    {
                        var res =await _productRepository.ProductFetch(item.EntityId);
                        images = res.Images;
                        obj.Name = res.MultiLingualProperties.Any(_ => _.LanguageId == lanId) ? 
                        res.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).Name : res.MultiLingualProperties.FirstOrDefault().Name;
                    }
                    else
                    {
                        var res = await _contentRepository.ContentFetch(item.EntityId);
                        images = res.Images;
                        obj.Name = res.Title;
                    }
                    if(!string.IsNullOrWhiteSpace(keyword) && !obj.Name.Contains(keyword))
                    {
                         continue;
                    }
                    var mainImage = images.FirstOrDefault(_ => _.IsMain);
                    if (mainImage == null)
                    {
                        mainImage = images.FirstOrDefault(_ => _.ImageRatio == ImageRatio.Square);
                    }
                    if (mainImage == null)
                    {
                       obj.ImagePath = "";
                    obj.NoImage = true;
                    }
                    else
                    {
                        obj.ImagePath = mainImage.Url;
                    }
                    List.Add(obj);
                }

            ViewBag.Type = type;
            return View(List);
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
            dto.UserName = user.UserName;
            dto.UserID = user.Id;
           
            var countries = _countryRepository.GetAllCountries();
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
                var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                var path = "Images/UserProfiles";
                var img = new Image()
                {
                    Content = dto.FileContent,
                    Title = dto.FileName
                };
                

                var res = ImageFunctions.SaveImageModel(img, path, localStaticFileStorageURL);
                if (res.Key != Guid.Empty.ToString())
                {
                    pro.ProfilePhoto.ImageId = res.Key;
                    pro.ProfilePhoto.Url = res.Value;
                    pro.ProfilePhoto.Content = "";
                }

                pro.FullName = dto.FirstName + " " + dto.LastName;
               user.UserName = dto.UserName;
                user.Profile = pro;
               
                var updateRes = await _userManager.UpdateAsync(user);
               
                    result = Json(updateRes.Succeeded ? new { Status = "Success", Message = Language.GetString("AlertAndMessage_OperationSuccess") }
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
               
             return Ok(new { Status = "Success" , Message = Language.GetString("AlertAndMessage_OperationSuccess") });
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
            if (CultureInfo.CurrentCulture.Name.ToLower() != langId.ToLower())
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


        [HttpGet]
        public IActionResult RequestResetPassword()
        {
            var isAuthenticated = HttpContext.User.Identity.IsAuthenticated;
            if (!isAuthenticated)
            {
                var captcha = HttpContext.Session.GenerateCaptchaImageString(2);
                ViewBag.Captcha = $"data:image/png;base64,{captcha}";
                return View();
            }

            return LocalRedirect("/");
        }

        [HttpPost]
        public async Task<IActionResult> SendOtpResetPassword(RegisterDTO model)
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

            ApplicationUser user = await _userManager.FindByNameAsync(model.FullCellPhoneNumber);

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
            HttpContext.Session.SetString("Phone", model.FullCellPhoneNumber);
            HttpContext.Session.SetString("OtpTime", DateTime.Now.ToString(CultureInfo.CurrentCulture));

            Result result = await _createNotification.SendOtp("SendOtpForResetPassword", user, otp);

            return Ok(result.Succeeded ? new { Status = "Success", result.Message } : new { Status = "Error", result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetOtpResetPassword()
        {
            var phone = HttpContext.Session.GetString("Phone");
            if (phone == null)
            {
                return RedirectToAction("RequestResetPassword");
            }
            ApplicationUser existUser = await _userManager.FindByNameAsync(phone);
            if (existUser != null)
            {
                ViewBag.Error = Language.GetString("AlertAndMessage_NoUserWasFound");
            }
            return View();
        }

     

        [HttpGet]
        public IActionResult EnterOtp()
        {
            var model = new EnterOtpModel();
            return View(model);
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
            if (model.FullCellPhoneNumber.StartsWith("+"))
                model.FullCellPhoneNumber = model.FullCellPhoneNumber.Substring(1);
            if (user.PhoneNumber != model.FullCellPhoneNumber)
            {
                return Ok(new { Status = "Error", Message = Language.GetString("AlertAndMessage_NotFoundUser") });
            }

            string otp = DataLayer.Helpers.Utilities.GenerateOtp();
            HttpContext.Session.SetString("Otp", otp);
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("OtpTime", DateTime.Now.ToString(CultureInfo.CurrentCulture));

            Result result = await _createNotification.SendOtp("AutomatedPasswordReset", user, otp);

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
