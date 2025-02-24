using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Permission;
using System.Security.Claims;
using AutoMapper;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Role;
using Arad.Portal.Models.Shared.Permission;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Admin;

using Serilog;

namespace Arad.Portal.Areas.Admin.ViewComponents;

public class SidebarMenu(
    IHttpContextAccessor accessor,
    IPermissionRepository permissionRepository,
    IConfiguration configuration,
    UserManager<ApplicationUser> userManager,
    IMapper mapper,
    IRoleRepository roleRepository,
    ControllerHelper controllerHelper)
    : ViewComponent
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IMapper _mapper = mapper;
    private readonly IRoleRepository _roleRepository = roleRepository;
    private readonly IPermissionRepository _permissionRepository = permissionRepository;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        List<PermissionTreeViewDto> menues = new List<PermissionTreeViewDto>();
        try
        {
            //PermissionTreeViewDto permissionTreeViewDto = new()
            //{
            //    Id = "729f278f-2839-4d9a-84c4-d7765c6d7a9f",
            //    Title = "WebSiteManagement",
            //    LevelNo = 1,
            //    Priority = 1,
            //    Icon = "fas fa-window-restore",
            //    ClientAddress = "/Home/Index",
            //    Children = []
            //};
            //menues.Add(permissionTreeViewDto);
            if (accessor.HttpContext != null)
            {
                string userId = accessor.HttpContext.User.Claims
                                         .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                RouteData route = accessor.HttpContext.GetRouteData();

                string? baseAddress = configuration["BaseAddress"];
                //if(string.IsNullOrWhiteSpace(baseAddress))
                //{
                //    baseAddress = "/Admin";
                //}
                if (baseAddress != null)
                {
                    ViewBag.BasePath = baseAddress;
                }

                RequestMenuModel obj = new()
                                       {
                                           PathString = $"/{route?.Values["controller"]}/{route?.Values["action"]}".ToLower(),
                                           Domain = $"{accessor.HttpContext.Request.Host}"
                                       };
                if (route?.Values["controller"].ToString() == "Comment")
                {
                    obj.PathString = accessor.HttpContext.Request.Path;
                }

                if (userId != null)
                {
                    string defLangSymbol = controllerHelper.GetDefaultLanguage().Symbol.ToLower();
                    menues = await controllerHelper.GetMenus(userId, obj.PathString);
                    foreach (PermissionTreeViewDto menu in menues)
                    {
                        menu.ClientAddress = $"/{defLangSymbol}" + menu.ClientAddress;
                        foreach (PermissionTreeViewDto item in menu.Children)
                        {
                            item.ClientAddress = $"/{defLangSymbol}" + item.ClientAddress;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Error($"Exception {e.Message} occured. stack trace: {nameof(SidebarMenu)}");
        }

        return View("~/Areas/Admin/Views/Shared/Components/SidebarMenu/Default.cshtml", menues);
    }
}