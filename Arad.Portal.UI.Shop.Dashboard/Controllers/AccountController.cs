using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.User;
using Arad.Portal.DataLayer.Repositories.General.User.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
using Microsoft.AspNetCore.Authentication;
using Arad.Portal.DataLayer.Contracts.General.Role;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Arad.Portal.DataLayer.Contracts.General.Permission;
using System.Security.Claims;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Role;
using Microsoft.Extensions.Options;
using Arad.Portal.DataLayer.Contracts.General.Notification;
using Arad.Portal.DataLayer.Models.Notification;
using Arad.Portal.DataLayer.Contracts.General.MessageTemplate;
using Arad.Portal.GeneralLibrary.Utilities;
using System.Collections.Specialized;
using System.Web;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using PhoneNumbers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Arad.Portal.DataLayer.Helpers;
using Arad.Portal.DataLayer.Entities.General.Error;
using Arad.Portal.DataLayer.Contracts.General.Error;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserExtensions _userExtension;
        private readonly IConfiguration _configuration;
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IPermissionView _permissionViewManager;
        private readonly IOptions<MessageCenter> _smsSettings;
        private readonly IMessageTemplateRepository _messageTemplateRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDomainRepository _domainRepository;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly CreateNotification _createNotification;
        
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IPermissionView permissionView,
            UserExtensions userExtension,
            IRoleRepository roleRepository,
            CreateNotification createNotification,
            IPermissionRepository permissionRepository,
            ILanguageRepository languageRepository,
            IConfiguration configuration,
            IOptions<MessageCenter> smsSettings,
            IHttpContextAccessor httpContextAccessor,
            INotificationRepository notificationRepository,
            ICurrencyRepository currencyRepository,
            IDomainRepository domainRepository,
            IErrorLogRepository errorLogRepository,
            IMessageTemplateRepository messageTemplateRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userExtension = userExtension;
            _configuration = configuration;
            _permissionViewManager = permissionView;
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _languageRepository = languageRepository;
            _smsSettings = smsSettings;
            _currencyRepository = currencyRepository;
            _notificationRepository = notificationRepository;
            _messageTemplateRepository = messageTemplateRepository;
            _httpContextAccessor = httpContextAccessor;
            _domainRepository = domainRepository;
            _createNotification = createNotification;
            _errorLogRepository = errorLogRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);

            return View(dicKey);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            if (HttpContext.User.Identity != null &&
                HttpContext.User.Identity.IsAuthenticated)
            {
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
            }
            var viewModel = new LoginViewModel
            {
                ReturnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl,
                RememberMe = false
            };
            ViewBag.Message = string.Empty;
            return View(viewModel);
        }


        [Authorize(Policy ="Role")]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginViewModel model)
        {
            //if (!HttpContext.Session.ValidateCaptcha(model.Captcha))
            //{
            //    ModelState.AddModelError("Captcha", Language.GetString("AlertAndMessage_CaptchaIsExpired"));
            //}

            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

            //await HttpContext.SignOutAsync();
            //ApplicationUser user = await _userManager.FindByNameAsync(model.Username);

            //if (user == null || await _userManager.CheckPasswordAsync(user, model.Password) != true)
            //{
            //    ViewBag.Message = Language.GetString("AlertAndMessage_InvalidUsernameOrPassword");
            //    return View(model);
            //}

            //if (!user.IsActive)
            //{
            //    ViewBag.Message = Language.GetString("AlertAndMessage_InActiveUserAccount");
            //    return View(model);
            //}
            ApplicationUser user = await _userManager.FindByNameAsync(model.Username);
            await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            user.LastLoginDate = DateTime.Now;
            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && model.ReturnUrl != "/")
            {
                return Redirect(model.ReturnUrl);
            }

            TempData["LoginUser"] = $"{user.Profile.FirstName} {user.Profile.LastName} {Language.GetString("AlertAndMessage_Welcome")}";
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult ChangeLang([FromQuery] string langId)
        {
            var domainName = $"{HttpContext.Request.Host}";
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
                CultureInfo.CurrentCulture.DateTimeFormat = new CultureInfo("en-US").DateTimeFormat;
                return Ok(true);
            }
            return Ok(false);
        }


        [HttpGet]
        public async Task<IActionResult> List()
        {
            var pageSize = 10;
            var page = 1;
            var result = new PagedItems<UserListView>();


            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            ViewBag.Permissions = dicKey;

            try
            {
                var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var query = _userManager.Users.Where(c => c.Id != currentUserId);

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
                    query = query.Where(c => c.UserRoleId == queryParams["role"]);
                }

                if (!string.IsNullOrWhiteSpace(queryParams["name"]))
                {
                    query = query.Where(c => c.Profile.FullName.ToLower().Contains(queryParams["name"].ToLower()));
                }

                if (!string.IsNullOrWhiteSpace(queryParams["userName"]))
                {
                    query = query.Where(c => c.UserName.ToLower().Contains(queryParams["userName"].ToLower()));
                }


                long count = query.Count();

                var lst = query.OrderBy(x => x.CreationDate).ThenBy(x => x.Profile.LastName)
                    .Skip((page - 1) * pageSize).Take(pageSize).ToList();

                result = new PagedItems<UserListView>()
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    ItemsCount = count,
                    Items = lst.Select(_ => new UserListView()
                    {
                        Id = _.Id,
                        PhoneNumber = _.PhoneNumber,
                        IsActive = _.IsActive,
                        Name = _.Profile.FirstName,
                        IsSystem = _.IsSystemAccount,
                        LastName = _.Profile.LastName,
                        UserName = _.UserName,
                        UserRoleId = _.UserRoleId,
                        RoleName = _roleRepository.FetchRole(_.UserRoleId).Result.RoleName,
                        CreateDate = _.CreationDate,
                        //persianCreateDate = _.CreationDate,
                        IsDelete = _.IsDeleted
                    }).ToList()
                };


                var list = await _roleRepository.List("");
                ViewBag.Roles = list.Items.Select(_ => new RoleListView()
                {
                    Title = _.RoleName,
                    Id = _.RoleId,
                }).ToList();
            }
            catch (Exception e)
            {

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

        //private List<RoleListView>  RoleList()
        //{
        //    var result = new List<RoleListView>();
        //    try
        //    {
        //        var pagedItems =_roleRepository.List("");
        //        result = pagedItems.Result.Items.Select(_ => new RoleListView()
        //        {
        //            Title = _.RoleName,
        //            Id = _.RoleId,
        //            IsActive = _.IsActive
        //        }).ToList();

        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    return result;
        //}

        public async Task<IActionResult> AddUser()
        {
            var model = new RegisterUserModel();
            var list = await _roleRepository.List("");
            ViewBag.LangList = _languageRepository.GetAllActiveLanguage();
            var currentUserId = _httpContextAccessor.HttpContext.User
                .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if(currentUser.IsSystemAccount)
            {
                ViewBag.IsSystem = true;
                ViewBag.DomainList = _domainRepository.GetAllActiveDomains();

            }else
            {
                ViewBag.IsSystem = false;
            }
            model.Roles = list.Items.Select(_ => new RoleListView()
            {
                Title = _.RoleName,
                Id = _.RoleId,
            }).ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] RegisterUserModel model)
        {
            List<ClientValidationErrorModel> errors = new List<ClientValidationErrorModel>();
            JsonResult result;

            if (!ModelState.IsValid)
            {

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
                return new JsonResult(new
                {
                    Status = "error",
                    Message = Language.GetString("AlertAndMessage_FillEssentialFields"),
                    ModelStateErrors = errors
                });
            }
            try
            {
                var existUser = _userExtension.GetUsersByPhoneNumber(model.FullMobile.Replace("+", ""));

                #region Fetch currency from language
                var lan = _languageRepository.FetchLanguage(model.DefaultLanguageId);
                CultureInfo userCultureInfo = new CultureInfo(lan.Symbol, false);
                var ri = new RegionInfo(userCultureInfo.LCID);
                var currencyPrefix = ri.ISOCurrencySymbol;
                var currencyDto = _currencyRepository.GetCurrencyByItsPrefix(currencyPrefix);
                #endregion Fetch currency from language

                model.DefaultCurrencyId = currencyDto.CurrencyId;
                model.DefaultCurrencyName = currencyDto.CurrencyName;
                var currentDomain = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
                string associatedDomainId = "";
                if (!string.IsNullOrWhiteSpace(model.DomainId))
                {
                    associatedDomainId = model.DomainId;
                }
                else
                {
                    associatedDomainId = _domainRepository.FetchByName(currentDomain).ReturnValue.DomainId;
                }
                if (existUser == null)
                {
                    var user = new ApplicationUser()
                    {
                        Profile = new Profile()
                        {
                            FirstName = model.Name,
                            LastName = model.LastName,
                            FullName = $"{model.Name} {model.LastName}",
                            DefaultCurrencyId = model.DefaultCurrencyId,
                            DefaultCurrencyName = model.DefaultCurrencyName,
                            DefaultLanguageId = model.DefaultLanguageId,
                            DefaultLanguageName = model.DefaultLanguageName
                        },
                        Id = Guid.NewGuid().ToString(),
                        PhoneNumber = model.FullMobile.Replace("+", ""),
                        UserName = model.UserName,
                        IsActive = model.IsActive,
                        UserRoleId = model.UserRoleId,
                        CreationDate = DateTime.UtcNow,
                        CreatorId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    };
                    if(model.IsVendor)
                    {
                        user.Claims.Add(new IdentityUserClaim<string>
                        {
                            ClaimType = "AppRole",
                            ClaimValue = true.ToString()
                        });
                    }
                    //add domain to its claims
                    user.Claims.Add(new IdentityUserClaim<string> 
                    {
                        ClaimType = "RelatedDomain",
                        ClaimValue = associatedDomainId
                    });
                    var res = await _userManager.CreateAsync(user, model.Password);

                    if (!res.Succeeded)
                    {
                        var errorsIdentity = res.Errors.ToList();

                        foreach (var error in errorsIdentity)
                        {
                            if (error.Code == "DuplicateUserName")
                            {
                                var obj = new ClientValidationErrorModel
                                {
                                    Key = "UserName",
                                    ErrorMessage = Language.GetString("AlertAndMessage_DuplicateUsername"),
                                };
                                errors.Add(obj);
                            }
                        }

                        result = new JsonResult(new { Status = "error", Message = "", ModelStateErrors = errors });
                    }
                    else
                    {
                        result = new JsonResult(new { Status = "success", Message = Language.GetString("AlertAndMessage_UserCreatedSuccessfully") });
                    }
                }
                else
                {
                    result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_DuplicatePhoneNumber"), ModelStateErrors = errors });
                }
            }
            catch (Exception e)
            {

                result = new JsonResult(new
                {
                    Status = "error",
                    Message = Language.GetString("AlertAndMessage_TryLator"),
                    ModelStateErrors = errors
                });
            }
            return result;
        }

        
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var model = new UserEdit();
            try
            {
                var userDb = await _userManager.FindByIdAsync(id);

                model = new UserEdit()
                {
                    Id = userDb.Id.ToString(),
                    PhoneNumber = userDb.PhoneNumber.Substring(2, 10),
                    FirstName = userDb.Profile.FirstName,
                    LastName = userDb.Profile.LastName,
                    UserRoleId = userDb.UserRoleId
                };

                var list = await _roleRepository.List("");
                ViewBag.Roles = list.Items.Select(_ => new RoleListView()
                {
                    Title = _.RoleName,
                    Id = _.RoleId
                }).ToList();
            }
            catch (Exception e)
            {

            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] UserEdit model)
        {
            List<ClientValidationErrorModel> errors = new List<ClientValidationErrorModel>();
            var user = new ApplicationUser();

       
            var result = new JsonResult(new { Status = "success", Message = Language.GetString("AlertAndMessage_EditionDoneSuccessfully") });
            if (model.Id != null)
            {
                user = await _userManager.FindByIdAsync(model.Id);

                if (user == null)
                {
                    ModelState.AddModelError("Name", Language.GetString("AlertAndMessage_NoUserWasFound"));
                }

                var state = _userExtension.IsPhoneNumberUnique(model.FullMobile.Replace("+", ""), user.PhoneNumber);

                if (!state)
                {
                    ModelState.AddModelError("PhoneNumber", Language.GetString("AlertAndMessage_DuplicatePhoneNumber"));
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (currentUserId == model.Id)
                    {
                        result = new JsonResult(new
                        {
                            Status = "error",
                            Message = Language.GetString("AlertAndMessage_DontAccess"),
                            ModelStateErrors = errors
                        });
                    }
                    else
                    {
                        user.Profile.FirstName = model.FirstName;
                        user.Profile.LastName = model.LastName;
                        user.Profile.FullName = $"{model.FirstName} {model.LastName}";

                        #region Fetch currency from language
                        var lan = _languageRepository.FetchLanguage(model.DefaultLanguageId);
                        CultureInfo userCultureInfo = new CultureInfo(lan.Symbol, false);
                        var ri = new RegionInfo(userCultureInfo.LCID);
                        var currencyPrefix = ri.ISOCurrencySymbol;
                        var currencyDto = _currencyRepository.GetCurrencyByItsPrefix(currencyPrefix);
                        #endregion Fetch currency from language


                        user.Profile.DefaultLanguageId = model.DefaultLanguageId;
                        //user.Profile.DefaultLanguageName
                        user.PhoneNumber = model.FullMobile.Replace("+", "");
                        user.UserRoleId = model.UserRoleId;

                        if (model.IsVendor)
                        {
                            user.Claims.Add(new IdentityUserClaim<string>
                            {
                                ClaimType = "AppRole",
                                ClaimValue = true.ToString()
                            });
                        }
                        else
                        {
                            var vendorClaim = user.Claims.Find(c => c.ClaimType == "AppRole");
                            if (vendorClaim != null)
                            {
                                user.Claims.Remove(vendorClaim);
                            }
                        }
                        var res = await _userManager.UpdateAsync(user);
                        if (!res.Succeeded)
                        {
                            var identityErrors = res.Errors.ToList();

                            foreach (var error in identityErrors)
                            {
                                if (error.Code == "DuplicateUserName")
                                {
                                    var obj = new ClientValidationErrorModel
                                    {
                                        Key = "UserName",
                                        ErrorMessage = Language.GetString("AlertAndMessage_DuplicateUsername"),
                                    };

                                    errors.Add(obj);

                                    result = new JsonResult(new { Status = "error", Message = "", ModelStateErrors = errors });
                                }
                            }
                            return View("Index");
                        }
                    }
                }
                catch (Exception e)
                {

                    result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator"), ModelStateErrors = errors });
                }
            }
            else
            {

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
                result = new JsonResult(new
                {
                    Status = "error",
                    Message = Language.GetString("AlertAndMessage_FillEssentialFields"),
                    ModelStateErrors = errors
                });
            }
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> Restore(string id)
        {
            JsonResult result;
            var userCurrentId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userCurrentId == id)
            {
                result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_NoAccess") });
            }
            else
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(id);
                    if (user == null)
                    {
                        result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_NoUserWasFound") });
                    }
                    else
                    {
                        user.IsDeleted = false;

                        var res = await _userManager.UpdateAsync(user);

                        if (res.Succeeded)
                        {
                            result = new JsonResult(new { Status = "success", Message = Language.GetString("AlertAndMessage_EditionDoneSuccessfully") });
                        }
                        else
                        {
                            result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
                        }
                    }

                }
                catch (Exception e)
                {
                    result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
                }
            }
            return result;

        }

        [HttpGet]
        public async Task<IActionResult> ChangeActivation(string id)
        {
            JsonResult result;
            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_NoUserWasFound") });
                }

                user.IsActive = !user.IsActive;

                var res = await _userManager.UpdateAsync(user);

                result = new JsonResult(new { Status = "success", result = user.IsActive, Message = Language.GetString("AlertAndMessage_EditionDoneSuccessfully") });
            }
            catch (Exception e)
            {
                return new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }
            return result;

        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            JsonResult result = new JsonResult(new
            {
                Status = "error",
                Message = Language.GetString("AlertAndMessage_NoAccess")
            });
            var userCurrentId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userCurrentId != id)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(id);
                    if (user == null)
                    {
                        result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_NoUserWasFound") });
                    }

                    user.IsDeleted = true;

                    var res = await _userManager.UpdateAsync(user);

                    if (res.Succeeded)
                    {
                        result = new JsonResult(new { Status = "success", Message = Language.GetString("AlertAndMessage_DeletionDoneSuccessfully") });
                    }
                    else
                    {
                        result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
                    }
                }
                catch (Exception e)
                {
                    result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
                }
            }
            return result;
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
                    errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel 
                    { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
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

            string otp = Utilities.GenerateOtp();
            HttpContext.Session.SetString("Otp", otp);
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("OtpTime", DateTime.Now.ToString(CultureInfo.CurrentCulture));

            Result result = await _createNotification.SendOtp("SendOtpForResetPassword", user, otp);

            if (!result.Succeeded)
            {
                ErrorLog errorLog = new() { Error = result.Message, Source = @"Account\ResetPassword", Ip = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() };
                await _errorLogRepository.Add(errorLog);
            }

            return Ok(result.Succeeded ? new { Status = "Success", result.Message } : new { Status = "Error", result.Message });
        }

        public IActionResult UnAuthorize()
        {
            return View();
        }
    }
}


