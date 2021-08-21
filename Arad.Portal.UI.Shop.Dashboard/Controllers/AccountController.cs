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
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IPermissionView permissionView,
            UserExtensions userExtension,
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            IConfiguration configuration,
            IOptions<MessageCenter> smsSettings,
            INotificationRepository notificationRepository,
            IMessageTemplateRepository messageTemplateRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userExtension = userExtension;
            _configuration = configuration;
            _permissionViewManager = permissionView;
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _smsSettings = smsSettings;
            _notificationRepository = notificationRepository;
            _messageTemplateRepository = messageTemplateRepository;
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
            ApplicationUser user = await _userManager.FindByNameAsync("superAdmin");
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
            if (CultureInfo.CurrentCulture.Name != langId)
            {
                Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(langId))
                    , new CookieOptions()
                    {
                        Expires = DateTimeOffset.Now.AddYears(1)
                    });

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

                if (existUser == null)
                {
                    var user = new ApplicationUser()
                    {
                        Profile = new Profile()
                        {
                            FirstName = model.Name,
                            LastName = model.LastName
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
                    result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_DuplicateMobilePhone"), ModelStateErrors = errors });
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


                    user.Profile.FirstName = model.FirstName;
                    user.Profile.LastName = model.LastName;
                    user.PhoneNumber = model.FullMobile.Replace("+", "");
                    user.UserRoleId = model.UserRoleId;
                    if(model.IsVendor)
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
        public async Task<IActionResult> ChangePassword(string id)
        {
            JsonResult result;
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_NoUserWasFound") });
                }

                var pass = Password.GeneratePassword(true, true, true,
                    true, false, 8);
                while (!Password.PasswordIsValid(true, true, true,
                    true, false, pass))
                {
                    pass = Password.GeneratePassword(true, true, true,
                        true, false, 8);
                }
                //add new record in notify table and declare user that I send the codes
                var template = _messageTemplateRepository.FetchTemplateByName("ChangePassword");
                var body = template.Body.Replace("[0]", pass);
                var notify = new NotificationDTO()
                {
                    NotificationId = Guid.NewGuid().ToString(),
                    CreationDate = DateTime.UtcNow,
                    CreatorUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
                    CreatorUserName = HttpContext.User.FindFirstValue(ClaimTypes.Name),
                    MessageText = body,
                    To = user.PhoneNumber,
                    TemplateName = "ChangePassword",
                    Type = Enums.NotificationType.Sms
                };
                var res = _notificationRepository.AddNewNotification(notify);
                if (res.Succeeded)
                {
                    result = Json(new { Status = "success", Message = Language.GetString("AlertAndMessage_SendNewPassword") });
                }
                else
                {
                    result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return result;
        }
    }
}


