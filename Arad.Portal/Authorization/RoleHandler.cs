using Arad.Portal.DataLayer.Entities.General.Permission;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Permission;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Role;
using Arad.Portal.Models.Shared.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Arad.Portal.Areas.Admin.Controllers.Account;
using Arad.Portal.DataLayer.Entities.General.ApplicationRole;
using Serilog;

namespace Arad.Portal.Authorization;

public class RoleHandler(
    UserManager<ApplicationUser> userManager,
    IPermissionRepository permissionRepository,
    IRoleRepository roleRepository,
    IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   RoleRequirement requirement)
    {
        try
        {
            Log.Information($"Entering in {nameof(RoleHandler) }");

            RouteData? route = httpContextAccessor.HttpContext?.GetRouteData();

            if (route == null)
            {
                Log.Warning($"Route is null. {nameof(RoleHandler)}");
                context.Fail();
                return Task.CompletedTask;
            }

            string? userId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                Log.Warning($"UserId is null. {nameof(RoleHandler)}");
                context.Fail();
                return Task.CompletedTask;
            }

            ApplicationUser? user = userManager.FindByIdAsync(userId).Result;

            if (user is not { IsActive: true })
            {
                Log.Warning($"user is not active. {nameof(RoleHandler)}");
                context.Fail();
                return Task.CompletedTask;
            }

            if (user.IsSystemAccount)
            {
                Log.Information($"IsSystemAccount is true. {nameof(RoleHandler)}");
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            string path = $"{route?.Values["controller"]}/{route?.Values["action"]}".ToLower();
            //string sa = $"{ route?.Values["referenceSource"]}/{route?.Values["action"]}".ToLower();
            //if (!string.IsNullOrEmpty(route?.Values["referenceSource"]?.ToString()))
            //{
            //    if (route?.Values["referenceSource"]?.ToString() == "ContentComments" || route?.Values["referenceSource"]?.ToString() == "ProductComments")
            //    {
            //        path = $"{route?.Values["referenceSource"]}/{route?.Values["action"]}".ToLower();
            //    }
            //}
            
            RoleDto result = new();
            ApplicationRole roleDto = roleRepository.FirstOrDefault(c => c.Id == user.UserRoleId);
            if (roleDto != null)
            {
                result = new() { Name = roleDto.Name, Id = roleDto.Id, IsActive = roleDto.IsActive, PermissionIds = string.Join(',', roleDto.PermissionIds) };
            }
            List<Permission> res = [];
            List<Permission> a = permissionRepository.GetAll();
            List<Permission> permissions = a.ToList();
            GenerateList(permissions, "");

            if (path.Contains("home/Index", StringComparison.OrdinalIgnoreCase) || path.Contains("account/changeknownpassword") || path.Contains("account/profile") || path.Contains("account/getstates") || path.Contains("account/getcities") || path.Contains("account/favorites") || path.Contains("home/getlogo") || path.Contains("home/getfavicon"))
            {
                Log.Information($"Allow anonymous urls. {nameof(RoleHandler)}");
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (!CheckAccess(res))
            {
                Log.Warning($"This url is not valid for role {roleDto?.Name} of user {user.Id} , {user.UserName} , stacktrace: {nameof(RoleHandler)}");
                context.Fail();
                return Task.CompletedTask;
            }
            Log.Information($"user {user.Id} , {user.UserName} with role {roleDto?.Name} entered successfully. stack trace: {nameof(RoleHandler)}");
            context.Succeed(requirement);
            return Task.CompletedTask;

            void GenerateList(List<Permission> tmPermissions, string parentName)
            {
                foreach (Permission permission in tmPermissions)
                {
                    Permission dto = new()
                                     {
                                         Id = permission.Id,
                                         Urls = permission.Urls,
                                         ClientAddress = permission.ClientAddress,
                                         IsActive = permission.IsActive,
                                         Actions = permission.Actions
                                     };
                    res.Add(dto);

                    GenerateList(permission.Children, $"{parentName}{(string.IsNullOrWhiteSpace(parentName) ? "" : "-")}{dto.Title}");
                }
            }

            bool CheckAccess(List<Permission> permissions)
            {
                foreach (Permission permission in permissions)
                {
                    List<string> accessUrl = [permission.ClientAddress.ToLower()];
                    if (permission.Urls != null)
                    {
                        accessUrl.AddRange(permission.Urls.Select(u => u.ToLower()));
                    }

                    foreach (DataLayer.Entities.General.Permission.Action permissionAction in permission.Actions)
                    {
                        accessUrl.Add(permissionAction.ClientAddress.ToLower());
                        accessUrl.AddRange(permissionAction.Urls.Select(u => u.ToLower()));
                    }

                    if (roleDto != null && roleDto.PermissionIds.Contains(permission.Id) && accessUrl.Any(r => r.Equals($"/admin/{path}")))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Exception occured.  {ex.Message} , stack trace: {nameof(RoleHandler)}");
            context.Fail();
            return Task.CompletedTask;
        }
    }
}
