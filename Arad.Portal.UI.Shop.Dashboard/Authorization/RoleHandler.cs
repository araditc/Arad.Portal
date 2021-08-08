using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Contracts.General.Role;
using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Entities.General.Role;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Repositories.General.Permission;
using Arad.Portal.DataLayer.Repositories.General.Permission.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Role;
using Arad.Portal.DataLayer.Repositories.General.Role.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Arad.Portal.UI.Shop.Dashboard.Authorization
{
    public class RoleHandler : AuthorizationHandler<RoleRequirement>
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleContext _roleContext;
        private readonly PermissionContext _permissionContext;

        public RoleHandler(IHttpContextAccessor accessor, 
            IConfiguration configuration, UserManager<ApplicationUser> userManager,
            RoleContext roleContext, PermissionContext permissionContext)
        {
            _accessor = accessor;
            _configuration = configuration;
            _userManager = userManager;
            _roleContext = roleContext;
            _permissionContext = permissionContext;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, 
            RoleRequirement requirement)
        {
            try
            {
                var accessor = _accessor.HttpContext.GetRouteData();

                if (accessor == null)
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                var controller = accessor.Values["controller"].ToString();
                var action = accessor.Values["action"].ToString();

                string userId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                var user = _userManager.FindByIdAsync(userId).Result;
                if (user == null)
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                if (!user.IsActive)
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                if (user.IsSystemAccount)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                string address = $"{controller}/{action}".ToLower();


                Role userRoleEntity = _roleContext.Collection.Find(_ => _.RoleId == user.UserRoleId).FirstOrDefault();
                   

                var permissions = _permissionContext.Collection.AsQueryable().Where(p => p.IsActive).ToList();
                List<DataLayer.Entities.General.Permission.Permission> permissionList = new();
                var toLowerRoutesPers = new List<string>();

                permissionList = _permissionContext.Collection.AsQueryable()
                        .Where(_ => _.IsActive && userRoleEntity.PermissionIds.Contains(_.PermissionId)).ToList();
                   
                toLowerRoutesPers = permissionList.SelectMany(_ => _.Routes).ToList();

                //string ch = toLowerRoutesPers.FirstOrDefault(w => w == address);

                if (!toLowerRoutesPers.Any(_=>_.ToLower() == address))
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                context.Fail();
                return Task.CompletedTask;
            }
        }
    }
}
