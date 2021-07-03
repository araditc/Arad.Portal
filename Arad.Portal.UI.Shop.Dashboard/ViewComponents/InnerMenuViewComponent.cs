using Arad.Portal.DataLayer.Contracts.General.Permission;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.General.Permission;
using Arad.Portal.DataLayer.Repositories.General.User.Mongo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
    public class InnerMenuViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPermissionRepository _permissionRepository;
        private readonly UserRepository _userRepository;

        public InnerMenuViewComponent(
            IHttpContextAccessor accessor,
            IPermissionRepository permissionRepository,
            UserManager<ApplicationUser> userManager,
            UserRepository userRepository)
        {
            _accessor = accessor;
            _permissionRepository = permissionRepository;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(string menuId)
        {
            var result = new MenuLinkModel();
           

            try
            {
                string currentUserId = _accessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var route = _accessor.HttpContext.GetRouteData();
                var controller = route.Values["controller"].ToString();
                var action = route.Values["action"].ToString();
                string address = $"{controller}/{action}";
                var applicationUser = await _userManager.FindByIdAsync(currentUserId);

                var pers = _userRepository.GetPermissionsOfUser(applicationUser);
                var context = pers.Where(p => p.Type == Enums.PermissionType.Menu).ToList();
                var per =await _permissionRepository.FetchPermission(menuId);
                if(per != null)
                {
                    result.MenuId = per.PermissionId;
                    result.MenuTitle = GeneralLibrary.Utilities.Language.GetString("PermissionTitle_" + per.Title);
                    result.Icon = per.Icon;
                    result.Priority = per.Priority;
                    result.IsActive = per.ClientAddress != null && per.Routes.Contains(address);
                }

                if (applicationUser != null)
                {
                    result.Children = _permissionRepository.GetChildren(context, menuId, address);
                }
            }
            catch (Exception e)
            {
               
            }


            return View(result);
        }
    }
}
