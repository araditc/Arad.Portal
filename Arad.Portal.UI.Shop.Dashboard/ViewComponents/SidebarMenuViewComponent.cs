using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Contracts.General.Permission;
using Arad.Portal.DataLayer.Models.Shared;
using System.Security.Claims;
using System.Text;
using AspNetCore.Identity.Mongo.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.AspNetCore.Routing;




namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
    public class SidebarMenu : ViewComponent
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPermissionRepository _permissionRepository;

        public SidebarMenu(
            IHttpContextAccessor accessor,
            IPermissionRepository permissionRepository,
            UserManager<ApplicationUser> userManager)
        {
            _accessor = accessor;
            _permissionRepository = permissionRepository;
            _userManager = userManager;
        }
        //public async Task<IViewComponentResult> InvokeAsync()
        //{
        //    var menues = new List<MenuLinkModel>();
        //    try
        //    {
        //        string userId = _accessor.HttpContext.User.Claims
        //            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        //        var route = _accessor.HttpContext.GetRouteData();
        //        var controller = route.Values["controller"].ToString();
        //        var action = route.Values["action"].ToString();
        //        string address = $"{controller}/{action}";

        //        if (userId != null)
        //        {
        //            menues = await _permissionRepository.ListOfMenues(userId, address);
        //        }
        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    return View(menues);
        //}

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menus = new List<MenuLinkModel>();

            try
            {
                string sub = _accessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var route = _accessor.HttpContext.GetRouteData();
                var controller = route.Values["controller"].ToString();
                var action = route.Values["action"].ToString();
                string address = $"{controller}/{action}";

                if (sub != null)
                {
                    menus = await _permissionRepository.ListOfMenues(sub, address);
                }

                return View(menus);
            }
            catch (Exception e)
            {
                return View(menus);
            }
        }
    }
}
