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

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserExtensions _userExtension;
        private readonly IConfiguration _configuration;
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            //UserExtensions userExtension,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            //_userExtension = userExtension;
            _configuration = configuration;

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
    }
}
