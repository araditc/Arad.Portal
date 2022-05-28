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
using AutoMapper;
using MongoDB.Driver;
using MongoDB.Bson;

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
        private readonly UserContext _userContext;
        private readonly IMapper _mapper;

        
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
            UserContext userContext,
            IMapper mapper,
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
            _userContext = userContext;
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


        [Authorize(Policy ="Role")]
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

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await HttpContext.SignOutAsync();
            ApplicationUser user = await _userManager.FindByNameAsync(model.Username);

            if (user == null || await _userManager.CheckPasswordAsync(user, model.Password) != true)
            {
                ViewBag.Message = Language.GetString("AlertAndMessage_InvalidUsernameOrPassword");
                return View(model);
            }

            if (!user.IsActive)
            {
                ViewBag.Message = Language.GetString("AlertAndMessage_InActiveUserAccount");
                return View(model);
            }
            
            user = await _userManager.FindByNameAsync(model.Username);
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
                //CultureInfo.CurrentCulture.DateTimeFormat = new CultureInfo("en-US").DateTimeFormat;
                return Ok(true);
            }
            return Ok(false);
        }

        [HttpPut]
        public async Task<IActionResult> ChangePassword(string id)
        {
            string currentUserId = User.GetUserId();

            string pass = Helpers.Utilities.GenerateRandomPassword
                (new() { RequireDigit = false, RequireLowercase = true, RequireNonAlphanumeric = true, RequireUppercase = true, 
                    RequiredLength = 10, RequiredUniqueChars = 1 });

            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!user.IsSystemAccount && user.Id != user.CreatorId )
            {
                return Forbid();
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult result = await _userManager.ResetPasswordAsync(user, token, pass);
            if (!result.Succeeded)
            {
                return Ok(new { Result = result.Succeeded });
            }

            user.Modifications.Add(new() { DateTime = DateTime.Now, 
                ModificationReason = Language.GetString("User_ModificationPasswordByAdmin"), 
                UserName = User.GetUserName(), UserId = currentUserId });

            result = await _userManager.UpdateAsync(user);

            return Ok(new { Result = result.Succeeded, Data = pass });
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
                filterDef = builder.Ne(nameof(ApplicationUser.Id), currentUserId);
                    

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
                    filterDef = builder.And(filterDef, builder.Eq(nameof(ApplicationUser.UserRoleId), queryParams["role"]));
                }

                if (!string.IsNullOrWhiteSpace(queryParams["name"]))
                {
                    
                    filterDef = builder.And(filterDef, builder.Regex(_=>_.Profile.FirstName, new(queryParams["name"].ToLower())));
                }

                if (!string.IsNullOrWhiteSpace(queryParams["userName"]))
                {
                    //query = query.Where(c => c.UserName.ToLower().Contains(queryParams["userName"].ToLower()));
                    filterDef = builder.And(filterDef, builder.Regex(_ => _.UserName, new BsonRegularExpression(queryParams["userName"].ToLower())));
                }


                long count = _userContext.Collection.Find(filterDef).CountDocuments();

                //var lst = _userManager.Users.fin.OrderBy(x => x.CreationDate).ThenBy(x => x.Profile.LastName)
                //    .Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var lst = _userContext.Collection
                 .Find(filterDef)
                   .Project(_ =>
                       new UserListView()
                       {
                           Id = _.Id,
                           PhoneNumber = _.PhoneNumber,
                           IsActive = _.IsActive,
                           Name = _.Profile.FirstName,
                           IsSystem = _.IsSystemAccount,
                           LastName = _.Profile.LastName,
                           UserName = _.UserName,
                           UserRoleId = _.UserRoleId,
                           RoleName = !string.IsNullOrWhiteSpace(_.UserRoleId) ? _roleRepository.FetchRole(_.UserRoleId).Result.RoleName : "",
                           CreateDate = _.CreationDate,
                           //persianCreateDate = _.CreationDate,
                           IsDelete = _.IsDeleted
                       }).Sort(Builders<ApplicationUser>.Sort.Descending(a => a.CreationDate)).Skip((page - 1) * pageSize).Limit(pageSize).ToList();

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
            //var superRole = list.Items.FirstOrDefault(_ => _.RoleName == "سوپر ادمین");
            //list.Items.Remove(superRole);
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
                CultureInfo userCultureInfo = new(lan.Symbol, false);
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
                    associatedDomainId = _domainRepository.FetchByName(currentDomain, false).ReturnValue.DomainId;
                }
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
                            else if(error.Code == "PasswordRequiresLower")
                            {
                                var obj = new ClientValidationErrorModel
                                {
                                    Key = "Password",
                                    ErrorMessage = Language.GetString("AlertAndMessage_PasswordRequiresLower"),
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
                if (userDb.Claims.Any(_ => _.ClaimType == "AppRole"))
                    isVendor = true;
                else isVendor = false;
                ViewBag.IsSystem = userDb.IsSystemAccount;
                ViewBag.LangList = _languageRepository.GetAllActiveLanguage();
                model = new UserEdit()
                {
                    Id = userDb.Id.ToString(),
                    PhoneNumber = userDb.PhoneNumber.Substring(2, 10),
                    FirstName = userDb.Profile.FirstName,
                    LastName = userDb.Profile.LastName,
                    UserRoleId = userDb.UserRoleId,
                    DefaultLanguageId = userDb.Profile.DefaultLanguageId,
                    IsVendor = isVendor
                };

                var list = await _roleRepository.List("");
                //var superRole = list.Items.FirstOrDefault(_ => _.RoleName == "سوپر ادمین");
                //list.Items.Remove(superRole);
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
                        CultureInfo userCultureInfo = new(lan.Symbol, false);
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

            if (string.IsNullOrWhiteSpace(model.CellPhoneNumber))
            {
                ModelState.AddModelError("CellPhoneNumber", Language.GetString("Validation_EnterMobileNumber"));
            }
            else
            {
                PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

                PhoneNumber phoneNumber = phoneUtil.Parse(model.CellPhoneNumber, "IR");

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

            if (user.PhoneNumber != model.FullCellPhoneNumber.Replace("+",""))
            {
                return Ok(new { Status = "Error", Message = Language.GetString("AlertAndMessage_NotFoundUser") });
            }

            string otp = Arad.Portal.DataLayer.Helpers.Utilities.GenerateOtp();
            HttpContext.Session.SetString("Otp", otp);
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("OtpTime", DateTime.Now.ToString(CultureInfo.CurrentCulture));

            Result result = await _createNotification.SendOtp("SendOtpForResetPassword", user, otp);

            return Ok(result.Succeeded ? new { Status = "Success", result.Message } : new { Status = "Error", result.Message });
        }

        public IActionResult UnAuthorize()
        {
            return View();
        }

        
        //private async Task SetViewBag()
        //{
            //List<Country> countries = await _countryRepository.GetAll();
            //string iranCountryId = countries.Any(c => c.Iso.Equals("IR")) ? countries.First(c => c.Iso.Equals("IR")).Id : "";

            //ViewBag.IranCountryId = iranCountryId;
            //ViewBag.Countries = new SelectList(countries, "Id", "Name", iranCountryId);
            //ViewBag.Roles = new SelectList(await _roleRepository.GetAll(), "Id", "Title");
       // }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile([FromForm] UserProfileDTO model)
        {
            if (!ModelState.IsValid)
            {
                //await SetViewBag();
                return View(model);
            }

            //if (_userManager.Users.Any(_ => _.PhoneNumber == model.PhoneNumber && !_.UserName.Equals(model.UserName)))
            //{
            //    ViewBag.OperationResult = new OperationResult { Message = Language.GetString("Validation_MobileNumberAlreadyRegistered"), Succeeded = false };
            //    //await SetViewBag();
            //    return View(model);
            //}

            ApplicationUser user = await _userManager.FindByIdAsync(User.GetUserId());
            if (user == null)
            {
                return RedirectToAction("PageOrItemNotFound", "Account");
            }

            //updating claims
            List<IdentityUserClaim<string>> claims = user.Claims;
            IdentityUserClaim<string> claim = new() { ClaimType = ClaimTypes.GivenName, ClaimValue = user.Profile.FullName };
            IdentityUserClaim<string> foundClaim = claims.Find(c => c.ClaimType == ClaimTypes.GivenName);
            if (foundClaim != null)
            {
                user.Claims.Remove(foundClaim);
            }

            string userName = user.UserName;
            _mapper.Map(model, user);
            user.UserName = userName;
            model.UserName = userName;
            user.Claims.Add(claim);

            // saving image
            string result = SaveImage(user.Id.ToString());
            if (!string.IsNullOrWhiteSpace(result))
            {
                ViewBag.OperationResult = new OperationResult { Message = result, Succeeded = false };
                //await SetViewBag();
                return View(model);
            }

            IdentityResult updateResult = await _userManager.UpdateAsync(user);
            ViewBag.OperationResult = new OperationResult
            {
                Message = Language.GetString($"AlertAndMessages_{(updateResult.Succeeded ? "OperationDoneSuccessfully" : "OperationFailed")}"),
                Succeeded = updateResult.Succeeded,
                Url = Url.Action("Index", "Home", new { Area = "" })
            };
            //await SetViewBag();
            return View(model);

            string SaveImage(string id)
            {
                //if (!string.IsNullOrWhiteSpace(model.FileContent) && !string.IsNullOrWhiteSpace(model.FileName) && model.FileContent != "undefined" && model.FileName != "undefined")
                //{
                //    UploadPicResult uploadResult = ProfilePic.UploadProfilePic(new() { FileName = model.FileName, ImageBase64 = model.FileContent, Id = id, _env = _env });

                //    if (uploadResult.UploadResult)
                //    {
                //        user.Profile.Avatar = uploadResult.UploadedAddress;
                //        model.ProfileDto.Avatar = uploadResult.UploadedAddress;
                //    }
                //    else
                //    {
                //        return uploadResult.ErrorMessage;
                //    }
                //}

                return "";
            }
        }
    }
}


