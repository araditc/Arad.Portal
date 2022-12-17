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

using Arad.Portal.DataLayer.Contracts.General.Permission;
using System.Security.Claims;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Role;
using Microsoft.Extensions.Options;
using Arad.Portal.DataLayer.Contracts.General.Notification;

using Arad.Portal.DataLayer.Contracts.General.MessageTemplate;
using Arad.Portal.GeneralLibrary.Utilities;
using System.Collections.Specialized;
using System.Web;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using PhoneNumbers;

using Arad.Portal.DataLayer.Helpers;
using Arad.Portal.DataLayer.Entities.General.Error;
using Arad.Portal.DataLayer.Contracts.General.Error;
using AutoMapper;
using MongoDB.Driver;
using MongoDB.Bson;
using Arad.Portal.DataLayer.Contracts.General.User;
using Arad.Portal.DataLayer.Contracts.General.CountryParts;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SixLabors.ImageSharp.Formats;
using System.IO;
using SixLabors.ImageSharp;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserExtensions _userExtension;
        private readonly IConfiguration _configuration;
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IOptions<MessageCenter> _smsSettings;
        private readonly IMessageTemplateRepository _messageTemplateRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDomainRepository _domainRepository;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly CreateNotification _createNotification;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICountryRepository _countryRepository;
        


        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
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
            IMapper mapper,
            ICountryRepository countryRepository,
            IUserRepository userRepository,
            IMessageTemplateRepository messageTemplateRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userExtension = userExtension;
            _configuration = configuration;
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
            _mapper = mapper;
            _userRepository = userRepository;
            _countryRepository = countryRepository;
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
            ViewBag.Captcha = captcha;
            var viewModel = new LoginViewModel
            {
                ReturnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl,
                RememberMe = false
            };
            ViewBag.Message = string.Empty;
            return View(viewModel);
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginViewModel model)
        {
            if (!HttpContext.Session.ValidateCaptcha(model.Captcha))
            {
                ModelState.AddModelError("Captcha", Language.GetString("AlertAndMessage_CaptchaIsExpired"));
            }
            if(string.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError("Username", Language.GetString("AlertAndMessage_UserNameRequired"));
            }

            if(string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", Language.GetString("AlertAndMessage_PasswordRequired"));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await HttpContext.SignOutAsync();
            ApplicationUser user = await _userManager.FindByNameAsync(model.Username);

            //only users who isSiteUser = false can login in site admin
            //only owner of this domain can login in this part
            var domainName = HttpContext.Request.Host.ToString();
            var domainEntity = _domainRepository.FetchByName(domainName, false).ReturnValue;
            
           if(user == null || !user.IsSystemAccount)
            {
                if (user == null || await _userManager.CheckPasswordAsync(user, model.Password) != true || !user.Domains.Any(_=>_.IsOwner)
                || (user != null && user.IsSiteUser) || 
                (user != null && user.Domains.Any(a => a.IsOwner) && user.Domains.FirstOrDefault(a => a.IsOwner).DomainId != domainEntity.DomainId) )
                {
                    ViewBag.Message = Language.GetString("AlertAndMessage_InvalidUsernameOrPassword");
                    return View(model);
                }

                if (!user.IsActive)
                {
                    ViewBag.Message = Language.GetString("AlertAndMessage_InActiveUserAccount");
                    return View(model);
                }
            }
            
            var res = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            if(res.Succeeded)
            {
                user.LastLoginDate = DateTime.Now;
                await _userManager.UpdateAsync(user);

                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && model.ReturnUrl != "/")
                {
                    return Redirect(model.ReturnUrl);
                }

                TempData["LoginUser"] = $"{user.Profile.FirstName} {user.Profile.LastName} {Language.GetString("AlertAndMessage_Welcome")}";
                return RedirectToAction("Index", "Home");
            }else
            {
                ViewBag.Message = Language.GetString("AlertAndMessage_InvalidUsernameOrPassword");
                return View(model);
            }
            
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult CheckCaptcha(string captcha)
        {
            return Ok(HttpContext.Session.ValidateCaptcha(captcha) ? new { Status = "success" } : new { Status = "error" });
        }


        [HttpGet]
        [AllowAnonymous]
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
                //CultureInfo.CurrentCulture.DateTimeFormat = new CultureInfo("en-US").DateTimeFormat;
                return Ok(true);
            }
            return Ok(false);
        }

        [HttpGet]
        public IActionResult CreateRandomPass()
        {
            string pass = Helpers.Utilities.GenerateRandomPassword(new()
            {
                RequireDigit = false,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true,
                RequiredLength = 10,
                RequiredUniqueChars = 1
            });

            return Ok(new { Status = "Success", Pass = pass });
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var pageSize = 10;
            var page = 1;
            var result = new PagedItems<UserListView>();

           
            FilterDefinitionBuilder<ApplicationUser> builder = new();
            FilterDefinition<ApplicationUser> filterDef = builder.Empty; 
            try
            {
                var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userEntity = _userManager.Users.FirstOrDefault(_ => _.Id == currentUserId);
                // filterDef = builder.Ne(nameof(ApplicationUser.Id), currentUserId);
                IQueryable<ApplicationUser> users;
                if(userEntity.IsSystemAccount)
                {
                    users = _userManager.Users.Where(_ => true);
                }else
                {
                    users = _userManager.Users.Where(_ => _.Id != currentUserId);
                }

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
                    //filterDef = builder.And(filterDef, builder.Eq(nameof(ApplicationUser.UserRoleId), queryParams["role"]));
                    users = users.Where(_ => _.UserRoleId == queryParams["role"]);
                }

                if (!string.IsNullOrWhiteSpace(queryParams["name"]))
                {

                    //filterDef = builder.And(filterDef, builder.Regex(_=>_.Profile.FirstName, new(queryParams["name"].ToLower())));
                    users = users.Where(_ => _.Profile.FirstName.ToLower().Contains(queryParams["name"].ToLower()));
                }

                if (!string.IsNullOrWhiteSpace(queryParams["userName"]))
                {
                    //query = query.Where(c => c.UserName.ToLower().Contains(queryParams["userName"].ToLower()));
                    //filterDef = builder.And(filterDef, builder.Regex(_ => _.UserName, new BsonRegularExpression(queryParams["userName"].ToLower())));
                    users = users.Where(_ => _.UserName.ToLower().Contains(queryParams["userName"].ToLower()));
                }


                long count = users.Count();

                //var lst = _userManager.Users.fin.OrderBy(x => x.CreationDate).ThenBy(x => x.Profile.LastName)
                //    .Skip((page - 1) * pageSize).Take(pageSize).ToList();

                
                var lst = users.Select(_=> new UserListView()
                {
                    Id = _.Id,
                    PhoneNumber = _.PhoneNumber,
                    IsActive = _.IsActive,
                    Name = _.Profile.FirstName,
                    IsSystem = _.IsSystemAccount,
                    LastName = _.Profile.LastName,
                    UserName = _.UserName,
                    UserRoleId = _.UserRoleId,
                    //RoleName = _.UserRoleId != null ? _roleRepository.FetchRole(_.UserRoleId).RoleName : "",
                    CreateDate = _.CreationDate,
                    //persianCreateDate = _.CreationDate,
                    IsDelete = _.IsDeleted
                }).ToList().OrderByDescending(_=>_.CreateDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();
                foreach (var user in lst)
                {
                    user.RoleName = user.UserRoleId != null ? _roleRepository.FetchRole(user.UserRoleId).RoleName : "";
                }

                result = new PagedItems<UserListView>()
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    ItemsCount = count,
                    Items = lst
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

        public async Task<IActionResult> AddUser()
        {
            var model = new RegisterUserModel();
           
            ViewBag.LangList = _languageRepository.GetAllActiveLanguage();
            var currentUserId = _httpContextAccessor.HttpContext.User
                .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            var lan = _languageRepository.GetDefaultLanguage(currentUserId);
            ViewBag.LangId = lan.LanguageId;
            if (currentUser.IsSystemAccount)
            {
                ViewBag.IsSystem = true;
                ViewBag.DomainList = _domainRepository.GetAllActiveDomains();

            }
            else
            {
                ViewBag.IsSystem = false;
            }
            var roles = _roleRepository.GetActiveRoles();
            roles.Insert(0, new RoleListView()
            {
                Id = "",
                Title = Language.GetString("Choose")
            });
            model.Roles = roles;
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
                            ErrorMessage = error.ErrorMessage == "AlertAndMessage_MinLength" ? 
                            (modelStateKey == "UserName" ? 
                            Language.GetString(error.ErrorMessage).Replace("0", Language.GetString("AlertAndMessage_Username")).Replace("1", "3") :
                            Language.GetString(error.ErrorMessage).Replace("0", Language.GetString("AlertAndMessage_Password")).Replace("1", "6") ) : 
                            Language.GetString(error.ErrorMessage)
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
                #region check mobile uniqueness
                if(existUser == null)
                {
                    //maybe this user register with its mobile in website
                    existUser = await _userManager.FindByNameAsync(model.FullMobile);
                }
                #endregion

                #region Fetch currency from language
                var lan = _languageRepository.FetchLanguage(model.DefaultLanguageId);
                CultureInfo userCultureInfo = new(lan.Symbol, false);
                var ri = new RegionInfo(userCultureInfo.LCID);
                var currencyPrefix = ri.ISOCurrencySymbol;
                var currencyDto = _currencyRepository.GetCurrencyByItsPrefix(currencyPrefix);
                #endregion Fetch currency from language

                model.DefaultCurrencyId = currencyDto.CurrencyId;
                model.DefaultCurrencyName = currencyDto.CurrencyName;
                var currentDomain = $"{_httpContextAccessor.HttpContext.Request.Host}";
                
                if (existUser == null)
                {
                    var user = new ApplicationUser()
                    {
                        Profile = new DataLayer.Models.User.Profile()
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
                        IsSiteUser = model.IsSiteUser,
                        UserRoleId = model.UserRoleId,
                        CreationDate = DateTime.UtcNow,
                        CreatorId = User.GetUserId(),
                        CreatorUserName = User.GetUserName(),
                        Email = model.Email
                       
                    };
                    if(model.IsVendor)
                    {
                        user.Claims.Add(new AspNetCore.Identity.MongoDbCore.Models.MongoClaim() {Type = "Vendor", Value = true.ToString() });
                       
                    }
                    
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
                            else if(error.Code == "PasswordRequiresLower")
                            {
                                var obj = new ClientValidationErrorModel
                                {
                                    Key = "Password",
                                    ErrorMessage = Language.GetString("AlertAndMessage_PasswordRequiresLower"),
                                };
                                errors.Add(obj);
                            }
                            else if (error.Code == "PasswordRequiresUpper")
                            {
                                var obj = new ClientValidationErrorModel
                                {
                                    Key = "Password",
                                    ErrorMessage = Language.GetString("AlertAndMessage_PasswordRequiresUpper"),
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
                bool isVendor;
                if (userDb.Claims.Any(_ => _.Type == "Vendor"))
                    isVendor = true;
                else isVendor = false;
                ViewBag.IsSystem = userDb.IsSystemAccount;
                ViewBag.LangList = _languageRepository.GetAllActiveLanguage();
                var currentUserId = _httpContextAccessor.HttpContext.User
                .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                if (currentUser.IsSystemAccount)
                {
                    ViewBag.IsSystem = true;
                    ViewBag.DomainList = _domainRepository.GetAllActiveDomains();

                }
                else
                {
                    ViewBag.IsSystem = false;
                }
                model = new UserEdit()
                {
                    Id = userDb.Id.ToString(),
                    PhoneNumber = userDb.PhoneNumber.Substring(2, 10),
                    FirstName = userDb.Profile.FirstName,
                    LastName = userDb.Profile.LastName,
                    UserRoleId = userDb.UserRoleId,
                    DefaultLanguageId = userDb.Profile.DefaultLanguageId,
                    DefaultCurrencyId = userDb.Profile.DefaultCurrencyId,
                    IsSiteUser = userDb.IsSiteUser,
                    Email = userDb.Email,
                    IsVendor = isVendor
                };

                var roles = _roleRepository.GetActiveRoles();
                roles.Insert(0, new RoleListView()
                {
                    Id = "-1",
                    Title = Language.GetString("Choose")
                });
                ViewBag.Roles = roles;
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
                    ModelState.AddModelError("FirstName", Language.GetString("AlertAndMessage_NoUserWasFound"));
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
                        CultureInfo userCultureInfo = new(lan.Symbol, false);
                        var ri = new RegionInfo(userCultureInfo.LCID);
                        var currencyPrefix = ri.ISOCurrencySymbol;
                        var currencyDto = _currencyRepository.GetCurrencyByItsPrefix(currencyPrefix);
                        #endregion Fetch currency from language


                        user.Profile.DefaultLanguageId = model.DefaultLanguageId;
                        //user.Profile.DefaultLanguageName
                        user.PhoneNumber = model.FullMobile.Replace("+", "");
                        user.UserRoleId = model.UserRoleId;
                        user.IsSiteUser = model.IsSiteUser;
                       
                        user.Email = model.Email;
                       
                        
                        if (model.IsVendor)
                        {
                            user.Claims.Add(new AspNetCore.Identity.MongoDbCore.Models.MongoClaim() { Type = "Vendor", Value = true.ToString() });
                        }
                        else
                        {
                            var vendorClaim = user.Claims.Find(c => c.Type == "Vendor");
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
                            ErrorMessage = Language.GetString(error.ErrorMessage),
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
        public async Task<IActionResult> CreatePasswordNotification(string userId, string password)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            var useId = User.GetUserId();
            var currentUser = await _userManager.FindByIdAsync(userId);
            var domain = _domainRepository.FetchDomain(currentUser.Domains.FirstOrDefault(_ => _.IsOwner).DomainId);
            Result result = await _createNotification.Send("AutomatedPasswordReset", user, password);

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
        public async Task<IActionResult> ResetPassword([FromForm] ChangePassDto model)
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
            }

            if (string.IsNullOrWhiteSpace(model.SecurityCode))
            {
                ModelState.AddModelError("SecurityCode", Language.GetString("AlertAndMessage_ProfileConfirmPhoneError"));
            }
            if(string.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError("UserName", Language.GetString("Validation_EnterUsername"));
            }else
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if(user.PhoneNumber != model.FullCellPhoneNumber)
                {
                    ModelState.AddModelError("UserName", Language.GetString("Validation_InvalidUserNameOrPhoneNumber"));
                }
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

            var currentUser = await _userManager.FindByNameAsync(model.Username);

            string pass = Helpers.Utilities.GenerateRandomPassword(new()
            {
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true,
                RequiredLength = 10,
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeKnownPassword([FromForm] RegisterDTO model)
        {
            #region Validate
            if (!HttpContext.Session.ValidateCaptcha(model.Captcha))
            {
                ModelState.AddModelError("Captcha", Language.GetString("AlertAndMessage_CaptchaIncorrectOrExpired"));
            }

            var currentUser = await _userManager.FindByNameAsync(model.Username);
            var checkPass = await _userManager.CheckPasswordAsync(currentUser, model.CurrentPass);
            if (!checkPass)
            {
                ModelState.AddModelError("CurrentPass", Language.GetString("AlertAndMessage_InCorrectPassword"));
            }

            if (model.NewPass != model.ReNewPass)
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

        [HttpPut]
        public async Task<IActionResult> ChangePassword(string id)
        {
            string currentUserId = User.GetUserId();

            string pass = Helpers.Utilities.GenerateRandomPassword(new()
            {
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true,
                RequiredLength = 10,
                RequiredUniqueChars = 0
            });
            while (!Password.PasswordIsValid(true, true, true, false, false, pass))
            {
                pass = Helpers.Utilities.GenerateRandomPassword(new()
                {
                    RequireDigit = true,
                    RequireLowercase = true,
                    RequireNonAlphanumeric = true,
                    RequireUppercase = true,
                    RequiredLength = 10,
                    RequiredUniqueChars = 0
                });
            }

            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult result = await _userManager.ResetPasswordAsync(user, token, pass);
            if (!result.Succeeded)
            {
                return Ok(new { Result = result.Succeeded });
            }

            user.Modifications.Add(new() { DateTime = DateTime.Now, ModificationReason = "user pass changes by admin", UserName = User.GetUserName(), UserId = currentUserId });

            result = await _userManager.UpdateAsync(user);

            return Ok(new { Result = result.Succeeded, Data = pass });
        }

        [HttpGet]
        [AllowAnonymous]
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

            OTP process = OtpHelper.Process(new()
            {
                Mobile = cellPhoneNumber,
                Code = DataLayer.Helpers.Utilities.GenerateOtp(),
                ExpirationDate = DateTime.Now.AddMinutes(3),
                IsSent = false
            });

            if (process.IsSent)
            {
                return Ok(new { Status = "Error", Message = Language.GetString("AlertAndMessage_ProfileConfirmPhoneSending") });
            }
            #endregion
            var userId = User.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            var domain = _domainRepository.FetchDomain(user.Domains.FirstOrDefault(_ => _.IsOwner).DomainId).ReturnValue;

            Result result = await _createNotification.SendOtp("SendOtp", cellPhoneNumber, process.Code);

            if (!result.Succeeded)
            {
                ErrorLog errorLog = new() { Error = result.Message, Source = @"Account\SendSecurityCode", Ip = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() };
                await _errorLogRepository.Add(errorLog);

                return Ok(new { Status = "Error", Message = Language.GetString("AlertAndMessage_SubmitOtpPleaseTryAgainLater") });
            }

            return Ok(new { Status = "Success", result.Message });
        }

        [AllowAnonymous]
        public IActionResult UnAuthorize()
        {
            return View();
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
           if(CultureInfo.CurrentCulture.Name =="fa-IR" && user.Profile.BirthDate != null)
            {
                dto.PersianBirthDate = DateHelper.ToPersianDdate(user.Profile.BirthDate.Value);
            }
            dto.UserName = user.UserName;
            dto.UserID = user.Id;
            var staticFileStorageURL = _configuration["LocalStaticFileStorage"];
            if (user.Profile.ProfilePhoto != null)
            {
                if (!string.IsNullOrWhiteSpace(user.Profile.ProfilePhoto.Url))
                {
                    user.Profile.ProfilePhoto.Url = user.Profile.ProfilePhoto.Url.Replace("\\", "/");
                    IImageFormat format;
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(Path.Combine(staticFileStorageURL, user.Profile.ProfilePhoto.Url), out format))
                    {
                        var imageEncoder = Configuration.Default.ImageFormatsManager.FindEncoder(format);
                        using (MemoryStream m = new MemoryStream())
                        {
                            image.Save(m, imageEncoder);
                            byte[] imageBytes = m.ToArray();

                            // Convert byte[] to Base64 String
                            string base64String = Convert.ToBase64String(imageBytes);
                            user.Profile.ProfilePhoto.Content = $"data:image/png;base64, {base64String}";
                        }
                    }
                }
            }

            var countries = _countryRepository.GetAllCountries();
            ViewBag.Countries = countries;
            ViewBag.LangList = _languageRepository.GetAllActiveLanguage();
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            ViewBag.LanIcon = lanIcon;
            var currencyList = _currencyRepository.GetAllActiveCurrency();
            ViewBag.CurrencyList = currencyList;

            //await SetViewBag();

            return View(dto);
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
            }
            else
            {
                var user = await _userManager.FindByIdAsync(dto.UserID);
                var pro = _mapper.Map<DataLayer.Models.User.Profile>(dto);
                if(!string.IsNullOrWhiteSpace(dto.PersianBirthDate))
                {
                    pro.BirthDate = DateHelper.ToEnglishDate(dto.PersianBirthDate.Split(" ")[0]);
                    pro.FullName = dto.FileContent + " " + dto.LastName;
                }
                var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                var path = "Images/UserProfiles";
                var img = new DataLayer.Models.Shared.Image()
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

    }
}


