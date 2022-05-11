using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Contracts.General.Permission;
using Arad.Portal.DataLayer.Contracts.General.User;
using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models;
using Arad.Portal.DataLayer.Models.Permission;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.General.Permission;
using Arad.Portal.DataLayer.Repositories.General.Permission.Mongo;
using Arad.Portal.DataLayer.Repositories.General.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using MongoDB.Driver;
using Arad.Portal.DataLayer.Entities.General.Permission;

namespace Arad.Portal.UI.Shop.Dashboard.Authorization
{
    //public class PermissionView : IPermissionView
    //{
        //private readonly IUserRepository _userRepository;
        //private readonly PermissionContext _permissionContext;
        //private readonly UserManager<ApplicationUser> _userManager;

        //public PermissionView(IUserRepository userRepository,
        //    UserManager<ApplicationUser> userManager,
        //    PermissionContext permissionContext)
        //{
        //    _userRepository = userRepository;
        //    _userManager = userManager;
        //    _permissionContext = permissionContext;
        //}

        //public async Task<Dictionary<string, bool>> PermissionsViewGet(HttpContext HttpContext)
        //{
            //string userId = HttpContext.User.Claims
            //    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            //var user = _userManager.FindByIdAsync(userId).Result;

            //Dictionary<string, bool> dicKey = new Dictionary<string, bool>();
            //string[] trans = Enum.GetNames(typeof(Enums.PermissionMethod));

           
            //if(user.IsSystemAccount)
            //{
            //    dicKey = trans.Select((value, key) =>
            //      new { value, key }).ToDictionary(x => x.value, x => true);
            //}
            //else
            //{
                //var route = HttpContext.GetRouteData();
                //var controller = route.Values["controller"].ToString();
                //var action = route.Values["action"].ToString();
                //string address = $"{controller}/{action}".ToLower();

                //var page = _permissionContext.Collection
                //    .AsQueryable()
                //    .FirstOrDefault(p => p.Type == Enums.PermissionType.Menu
                //    && p.ClientAddress.Equals(address));

                //var pageModules = _permissionContext.Collection.AsQueryable()
                //    .Where(a => a.Type == Enums.PermissionType.Module 
                //           && a.MenuIdOfModule == page.PermissionId).ToList();

                //List<Permission> persUser = _userRepository.GetPermissionsOfUser(user)
                //    .Where(_=>_.Type == Enums.PermissionType.Module 
                //                && _.MenuIdOfModule == page.PermissionId).Distinct().ToList();
                //foreach (var item in trans)
                //{
                //    if(persUser.Any(_=>_.Method.ToString() == item))
                //    {
                //        dicKey.Add(item, true);
                //    }else
                //    {
                //        dicKey.Add(item, false);
                //    }
                //}
        //    }

        //    return dicKey;
        //}
   // }
}
