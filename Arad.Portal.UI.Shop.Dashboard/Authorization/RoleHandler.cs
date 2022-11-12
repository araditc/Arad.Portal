using Arad.Portal.DataLayer.Contracts.General.Permission;
using Arad.Portal.DataLayer.Contracts.General.Role;
using Arad.Portal.DataLayer.Entities.General.Permission;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Authorization
{
    public class RoleHandler : AuthorizationHandler<RoleRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRoleRepository _roleRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPermissionRepository _permissionRepository;

        public RoleHandler(UserManager<ApplicationUser> userManager,
            IPermissionRepository permissionRepository,
            IRoleRepository roleRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _permissionRepository = permissionRepository;
            _roleRepository = roleRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            RoleRequirement requirement)
        {
            try
            {
                RouteData route = _httpContextAccessor.HttpContext?.GetRouteData();

                if (route == null)
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                string userId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                var user = _userManager.FindByIdAsync(userId).Result;

                if (user == null || !user.IsActive)
                {
                    context.Fail();
                    return Task.CompletedTask;
                }
                else
                if (user.IsSystemAccount)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
                
                string path = $"{route?.Values["controller"]}/{route?.Values["action"]}".ToLower();


                var roleDto = _roleRepository.FetchRole(user.UserRoleId);
                List<Permission> roots = _permissionRepository.GetAllListViewCustom().Result;

                if(path.Contains("home/Index", StringComparison.OrdinalIgnoreCase))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                if (!CheckAccess(roots))
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                context.Succeed(requirement);
                return Task.CompletedTask;

                bool CheckAccess(List<Permission> permissions)
                {
                    foreach (Permission permission in permissions)
                    {
                        List<string> accessUrl = new() { permission.ClientAddress.ToLower() };
                        if (permission.Urls != null)
                        {
                            accessUrl.AddRange(permission.Urls.Select(u => u.ToLower()));
                        }

                        foreach (DataLayer.Entities.General.Permission.Action permissionAction in permission.Actions)
                        {
                            accessUrl.Add(permissionAction.ClientAddress.ToLower());
                            accessUrl.AddRange(permissionAction.Urls.Select(u => u.ToLower()));
                        }

                        if (roleDto.PermissionIds.Split(",").Any(r => r.Equals(permission.PermissionId)) && accessUrl.Any(r => r.Equals($"/{path}")))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                context.Fail();
                return Task.CompletedTask;
            }
        }
    }
}
