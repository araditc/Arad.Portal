using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Serilog;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;

using System.Globalization;
using System.Threading;
using System;

using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.Helpers.Shared;

using Microsoft.Extensions.Configuration;

namespace Arad.Portal.ViewComponents;

public class LoginProfile1(IDomainRepository domainRepository, UserManager<ApplicationUser> userManager, ControllerHelper controllerHelper, IConfiguration configuration, MinioHelper minioHelper)
    : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(string domainId)
    {
        Domain domainObj = await domainRepository.FirstOrDefaultAsync(c => c.Id == domainId);
        if (User.Identity.IsAuthenticated)
        {

            string currentUserId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser user = await userManager.FindByIdAsync(currentUserId);
            //ViewBag.UserName = user.UserName;
            ViewBag.FullName = !string.IsNullOrEmpty(user.Profile.FullName) ? user.Profile.FullName : user.UserName;
            if (!string.IsNullOrWhiteSpace(user.Profile.ProfilePhoto.ImageId))
            {
                CultureInfo current = new("en-US")
                {
                    DateTimeFormat = new()
                    {
                        Calendar = new GregorianCalendar()
                    }
                };
                Thread.CurrentThread.CurrentCulture = current;
                Domain domain = controllerHelper.GetCurrentUserDomain();
                string objectName = $"{domain.Id}/{user.Profile.ProfilePhoto.ImageId}/{user.Profile.ProfilePhoto.FileName.Replace(':', '-')}";
                (bool success, byte[] imageData) = minioHelper.GetObject("accountimage", objectName).Result;
                if (success)
                {
                    user.Profile.ProfilePhoto.Content = Convert.ToBase64String(imageData);

                }
                ViewBag.ProfileUrl = user.Profile.ProfilePhoto.Content;
            }
            else
            {
                ViewBag.ProfileUrl = "";
            }
            Language defLang = controllerHelper.GetDefaultLanguage();
            CultureInfo current2 = new(defLang.Symbol)
            {
                DateTimeFormat = new()
                {
                    Calendar = new GregorianCalendar()
                }
            };
            Thread.CurrentThread.CurrentCulture = current2;
            ViewBag.FontFamily = configuration["SiteSettings:FontFamily"] ?? "Arial, sans-serif";
        }
        return View("Default", domainObj.IsShop);
    }
}