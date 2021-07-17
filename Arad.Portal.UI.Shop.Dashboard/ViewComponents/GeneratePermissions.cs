using Arad.Portal.DataLayer.Contracts.General.Permission;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
    public class GeneratePermissions : ViewComponent
    {
        private readonly IPermissionRepository _permissionRepository;
        public GeneratePermissions(IPermissionRepository repository)
        {
            _permissionRepository = repository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var permissions = await _permissionRepository.ListPermissions(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            return View(permissions);
        }
    }
}
