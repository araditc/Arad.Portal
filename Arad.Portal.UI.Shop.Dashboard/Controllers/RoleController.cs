using Arad.Portal.DataLayer.Contracts.General.Permission;
using Arad.Portal.DataLayer.Contracts.General.Role;
using Arad.Portal.DataLayer.Models.Permission;
using Arad.Portal.DataLayer.Models.Role;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class RoleController : Controller
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IPermissionView _permissionViewManager;

        public RoleController(
            IRoleRepository roleRepository,
            IPermissionRepository repository,
            IPermissionView permissionViewManager)
        {
            _roleRepository = roleRepository;
            _permissionRepository = repository;
            _permissionViewManager = permissionViewManager;
        }
        public async Task<IActionResult> Index()
        {
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            return View(dicKey);
        }

        [HttpPost]
        public async Task<IActionResult> Add(RoleDTO role)
        {
            JsonResult result;
            List<ClientValidationErrorModel> errors = new List<ClientValidationErrorModel>();

            if (!ModelState.IsValid)
            {
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    foreach (var error in modelStateVal.Errors)
                    {
                        var obj = new ClientValidationErrorModel
                        {
                            Key = modelStateKey,
                            ErrorMessage = error.ErrorMessage
                        };
                        errors.Add(obj);
                    }
                }
                result = new  JsonResult(new { Status = "error", Message = "فیلدهای ضروری تکمیل گردد.", ModelStateErrors = errors });
            }
            else
            {
                try
                {
                    var res = await _roleRepository.Add(role);

                    if (!res.Succeeded)
                    {
                        result = new JsonResult(new { Status = "error", Message = res.Message, ModelStateErrors = errors });
                    }else
                    {
                        result = new JsonResult(new { Status = "success", Message = res.Message, ModelStateErrors = errors });
                    }
                    
                }
                catch (Exception e)
                {
                    result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
                }
            }
            return result;
        }


        [HttpGet]
        public async Task<IActionResult> GetRole(string id)
        {
            try
            {
                var role = await _roleRepository.FetchRole(id);

                if (role == null)
                {
                    return new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_DataWasNotFound") });
                }

                //this part should be changed based on our menu preview
                //???
                var allPers = _permissionRepository.GetAllPermissions();
                //var pers = allPers.Select(c => new PermissionListView
                //{
                //    Title = c.Title,
                //    Id = c.PermissionId,
                //    IsActive = c.IsActive,
                //    pers = c.Type == Enums.PermissionType.baseMenu ? c.Select(d => new PermissionListView()
                //    {
                //        Id = d.Id,
                //        Title = d.Title,
                //        IsSelected = role.Permissions.Any(f => f == d.Id),
                //        IsActive = d.IsActive,
                //        pers = d.Modules.Select(m => new PermissionListView()
                //        {
                //            Id = m.Id,
                //            Title = m.Title,
                //            IsSelected = role.Permissions.Any(h => h == m.Id),
                //            IsActive = m.IsActive
                //        }).ToList(),
                //    }).ToList() : c.Modules != null ? c.Modules.Select(m => new PermissionListView()
                //    {
                //        Id = m.Id,
                //        Title = m.Title,
                //        IsSelected = role.Permissions.Any(h => h == m.Id),
                //        IsActive = m.IsActive,
                //        pers = new List<PermissionListView>()
                //    }).ToList() : new List<PermissionListView>()
                //}).ToList();
                

                var data = new { Role = role, AllPers = pers };

                return Json(new { Status = "success", Data = data });
            }
            catch (Exception e)
            {
                return Json(new { Status = "error", Message = "لطفا مجددا امتحان نمایید." });
            }
        }
    }
}
