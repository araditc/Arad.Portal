using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.SliderModule;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Serilog;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class LoginProfile : ViewComponent
    {
        private readonly IDomainRepository _domainRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public LoginProfile(IDomainRepository domainRepository,UserManager<ApplicationUser> userManager)
        {
            _domainRepository = domainRepository;
            _userManager = userManager;
        }
        public async Task<IViewComponentResult> InvokeAsync(string domainId)
        {
            Log.Fatal($"Hit ViewComponent with domainId ={domainId}");
            var domainObj = _domainRepository.FetchDomain(domainId).ReturnValue;
            Log.Fatal("is user authenticated :" + User.Identity.IsAuthenticated);
            if (User.Identity.IsAuthenticated)
            {
                
                var currentUserId = HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value;
                var user = await _userManager.FindByIdAsync(currentUserId);
                //ViewBag.UserName = user.UserName;
                ViewBag.FullName = !string.IsNullOrEmpty(user.Profile.FullName) ? user.Profile.FullName : user.UserName;
                if (!string.IsNullOrWhiteSpace(user.Profile.ProfilePhoto.Url))
                {
                    ViewBag.ProfileUrl = user.Profile.ProfilePhoto.Url;
                }
            }
            Log.Fatal("Going to View Of LoginProfile with domainObject");
            return View("Default", domainObj.IsShop);
        }
    }
}
