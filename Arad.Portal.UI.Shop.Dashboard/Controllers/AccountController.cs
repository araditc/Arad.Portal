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
        private readonly IPermissionView _permissionViewManager;
        private readonly IOptions<MessageCenter> _smsSettings;
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IPermissionView permissionView,
            UserExtensions userExtension,
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            IConfiguration configuration,
            IOptions<MessageCenter> smsSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userExtension = userExtension;
            _configuration = configuration;
            _permissionViewManager = permissionView;
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _smsSettings = smsSettings;
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

            if (!HttpContext.Session.ValidateCaptcha(model.Captcha))
            {
                ModelState.AddModelError("Captcha", "عبارت امنیتی صحیح نیست یا منقضی شده است.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await HttpContext.SignOutAsync();
            ApplicationUser user = await _userManager.FindByNameAsync(model.Username);

            if (user == null || await _userManager.CheckPasswordAsync(user, model.Password) != true)
            {
                ViewBag.Message = "نام کاربری یا رمز عبور صحیح نیست.";
                return View(model);
            }

            if (!user.IsActive)
            {
                ViewBag.Message = "حساب کاربری توسط ادمین مربوطه غیر فعال شده است.";
                return View(model);
            }

            await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            user.LastLoginDate = DateTime.Now;
            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && model.ReturnUrl != "/")
            {
                return Redirect(model.ReturnUrl);
            }

            TempData["LoginUser"] = $"کاربر {user.Profile.FirstName} {user.Profile.LastName} خوش آمدید.";
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


        [HttpPost]
        public async Task<IActionResult> List([FromForm] UserSearchParams searchParam)
        {
            var result = new PagedItems<UserListView>();
            if (ModelState.IsValid)
            {
                var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);

                try
                {
                    var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    PagedItems<UserDTO> data = _userExtension.UserFilters(searchParam, currentUserId);
                    result = new PagedItems<UserListView>()
                    {
                        CurrentPage = data.CurrentPage,
                        PageSize = data.PageSize,
                        ItemsCount = data.ItemsCount,
                        Items = data.Items.Select(_ => new UserListView()
                        {
                            Id = _.UserId,
                            PhoneNumber = _.PhoneNumber,
                            IsActive = _.IsActive,
                            Name = _.UserProfile.FirstName,
                            IsSystem = _.IsSystemAccount,
                            LastName = _.UserProfile.LastName,
                            UserName = _.UserName,
                            persianCreateDate = _.CreationDate.ToUniversalTime().ToPersianDdate(),
                            IsDelete = _.IsDeleted
                        }).ToList()
                    };


                    ViewBag.Permissions = dicKey;
                }
                catch (Exception e)
                {

                }
            }
            return View(result);
        }


        [HttpGet]
        public async Task<IActionResult> RoleList()
        {
            JsonResult result;
            try
            {
                var pagedItems = await _roleRepository.List("");
                var roles = pagedItems.Items.Select(_ => new RoleListView()
                {
                    Title = _.RoleName,
                    Id = _.RoleId,
                    IsSelected = false
                }).ToList();
                result = new JsonResult(new { Status = "success", Data = roles });
            }
            catch (Exception e)
            {
                result = new JsonResult(new { Status = "error" });
            }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromForm] RegisterUserModel model)
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
                    Message = "فیلدهای ضروری تکمیل گردد.",
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
                        PhoneNumber = model.FullMobile.Replace("+", ""),
                        UserName = model.UserName,
                        IsActive = model.IsActive,
                        UserRoles = model.UserRoles,
                        CreationDate = DateTime.UtcNow,
                        CreatorId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    };

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
                                    ErrorMessage = "نام کاربری تکراری می باشد.",
                                };

                                errors.Add(obj);
                            }
                        }

                        result = new JsonResult(new { Status = "error", Message = "", ModelStateErrors = errors });
                    }

                    result = new JsonResult(new { Status = "success", Message = "کاربر با موفقیت ایجاد شد." });
                }

                result = new JsonResult(new { Status = "error", Message = "با این شماره موبایل قبلا کاربری ثبت نام شده است.", ModelStateErrors = errors });

            }
            catch (Exception e)
            {

                result = new JsonResult(new
                {
                    Status = "error",
                    Message = "لطفا مجددا تلاش کنید.",
                    ModelStateErrors = errors
                });
            }
            return result;
        }


        [HttpGet]
        public async Task<IActionResult> GetUser(string id)
        {
            JsonResult result;
            try
            {
                var userDb = await _userManager.FindByIdAsync(id);

                if (userDb == null)
                {
                   result = new JsonResult(new { Status = "error", Message = "کاربر مورد نظر یافت نشد." });
                }
                var user = new UserDTO()
                {
                    UserId = userDb.Id.ToString(),
                    PhoneNumber = userDb.PhoneNumber.Substring(2, 10),
                    UserProfile = new Profile()
                    {
                        FirstName = userDb.Profile.FirstName,
                        LastName = userDb.Profile.LastName
                    },
                    UserRoles = userDb.UserRoles
                };

                var list = await _roleRepository.List("");
                var roles = list.Items.Select(_ => new RoleListView()
                {
                    Title = _.RoleName,
                    Id = _.RoleId,
                    IsSelected = user.UserRoles.Any(c => c == _.RoleId)
                }).ToList();

                var data = new { User = user, Roles = roles };

               result = new JsonResult(new { Status = "success", Data = data });
            }
            catch (Exception e)
            {
               result =new  JsonResult(new { Status = "error", Message = "لطفا مجددا سعی نمایید." });
            }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserDTO model)
        {
            List<ClientValidationErrorModel> errors = new List<ClientValidationErrorModel>();
            var user = new ApplicationUser();
            JsonResult result;
            if (model.UserId != null)
            {
                user = await _userManager.FindByIdAsync(model.UserId);

                if (user == null)
                {
                    ModelState.AddModelError("Name", "کاربر یافت نشد");
                }

                var state = _userExtension.IsPhoneNumberUnique(model.PhoneNumber.Replace("+", ""), user.PhoneNumber);

                if (!state)
                {
                    ModelState.AddModelError("PhoneNumber", "شماره تلفن تکراری می باشد.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (currentUserId == model.UserId)
                    {
                        result = new JsonResult(new 
                        {   Status = "error",
                            Message = "شما دسترسی ویرایش کاربر مورد نظر را ندارید.", 
                            ModelStateErrors = errors 
                        });
                    }


                    user.Profile.FirstName = model.UserProfile.FirstName;
                    user.Profile.LastName = model.UserProfile.LastName;
                    user.PhoneNumber = model.PhoneNumber.Replace("+", "");
                    user.UserRoles = model.UserRoles;


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
                                    ErrorMessage = "نام کاربری تکراری می باشد.",
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

                    result = new JsonResult(new { Status = "error", Message = "لطفا مجددا تلاش کنید.", ModelStateErrors = errors });
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
               result =  new JsonResult(new
               { Status = "error", 
                 Message = "فیلدهای ضروری تکمیل گردد.", 
                 ModelStateErrors = errors 
               });
            }

            result = new JsonResult(new { Status = "success", Message = "کاربر با موفقیت ویرایش شد." });

            return result;
        }

        [HttpGet]
        public async Task<IActionResult> ChangeActivation(string id, bool IsActive)
        {
            JsonResult result;
            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    result =  new JsonResult(new { Status = "error", Message = "کاربر یافت نشد." });
                }

                user.IsActive = IsActive;

                var res = await _userManager.UpdateAsync(user);

                result = new JsonResult(new { Status = "success", Message = "تغییر وضعیت کاربر با موفقیت انجام شد" });
            }
            catch (Exception e)
            {
                return new JsonResult(new { Status = "error", Message = "لطفا مجددا سعی نمایید." });
            }
            return result;

        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            JsonResult result = new JsonResult(new
            {
                Status = "error",
                Message = "دسترسی حذف کاربر مورد نظر را ندارید."
            });
            var userCurrentId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userCurrentId != id)
            {
                try
                {



                    var user = await _userManager.FindByIdAsync(id);
                    if (user == null)
                    {
                        result = new JsonResult(new { Status = "error", Message = "کاربر یافت نشد." });
                    }

                    user.IsDeleted = true;

                    var res = await _userManager.UpdateAsync(user);

                    if (res.Succeeded)
                    {
                        result = new JsonResult(new { Status = "success", Message = "  کاربر با موفقیت حذف گردید " });
                    }
                    result = new JsonResult(new { Status = "error", Message = "لطفا مجددا سعی نمایید." });

                }
                catch (Exception e)
                {
                    result = new JsonResult(new { Status = "error", Message = "لطفا مجددا سعی نمایید." });
                }
            }
            return result;
        }
        //????? 
        [HttpGet]
        public async Task<IActionResult> ChangePassword(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new { Status = "error", Message = "کاربر یافت نشد." });
                }

                var pass = Password.GeneratePassword(true, true, true,
                    true, false, 8);
                while (!Password.PasswordIsValid(true, true, true,
                    true, false, pass))
                {
                    pass = Password.GeneratePassword(true, true, true,
                        true, false, 8);
                }

                //???
                //var resultSendPass = SendPassSms(user.PhoneNumber, pass);

                //if (resultSendPass)
                //{
                //    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                //    var changePassResult =
                //        await _userManager.ResetPasswordAsync(user, token, pass);

                //    if (changePassResult.Succeeded)
                //    {
                //        return Json(new { Status = "success", Message = "کلمه عبور با موفقیت تغییر یافت." });
                //    }
                //}

                return Json(new { Status = "error", Message = "لطفا مجددا امتحان نمایید." });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        

    }
}


