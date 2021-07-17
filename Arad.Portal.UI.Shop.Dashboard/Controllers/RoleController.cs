using Arad.Portal.DataLayer.Contracts.General.Permission;
using Arad.Portal.DataLayer.Contracts.General.Role;
using Arad.Portal.DataLayer.Entities.General.Role;
using Arad.Portal.DataLayer.Models.Permission;
using Arad.Portal.DataLayer.Models.Role;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
                result = new  JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
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
            JsonResult result; 
            try
            {
                var role = await _roleRepository.FetchRole(id);

                if (role == null)
                {
                    result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_DataWasNotFound") });
                }
                
                //var permissions = await _permissionRepository.ListPermissions(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
              
                var data = new { Role = role };

                result = Json(new { Status = "success", Data = data });
            }
            catch (Exception e)
            {
                result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RoleDTO role)
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
                            ErrorMessage = error.ErrorMessage,
                        };
                        errors.Add(obj);
                    }
                }
                result =  new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
            }
            else
            {
                try
                {
                    var res = await _roleRepository.Update(role);

                    if (!res.Succeeded)
                    {
                        result = new JsonResult(new { Status = "error", Message = res.Message, ModelStateErrors = errors });
                    }
                    else
                    {
                        result = new JsonResult(new { Status = "success", Message = res.Message, ModelStateErrors = errors });
                    }

                }
                catch
                {
                    result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
                }
            }
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id, string modificationReason)
        {
            JsonResult result;
            try
            {
                var res = await _roleRepository.Delete(id, modificationReason);
                if (res.Succeeded)
                {
                    result =  Json(new { Status = "success", Message = res.Message });
                }
                else
                {
                    result = Json(new { Status = "error", Message = res.Message });
                }
            }
            catch (Exception e)
            {
                result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }

            return result;
        }

        [HttpGet]
        public async Task<IActionResult> ChangeActivation(string id, bool IsActive, string modificationReason)
        {
            JsonResult result;
            try
            {
                var res = await _roleRepository.ChangeActivation(id, IsActive, modificationReason);
                if (res.Succeeded)
                {
                    result = Json(new { Status = "success", Message = res.Message });
                }
                else
                {
                    result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
                }
            }
            catch (Exception e)
            {
                result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> List(int pageSize, int currentPage)
        {
            var res = new PagedItems<RoleDTO>();
            try
            {
                var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
                ViewBag.Permissions = dicKey;
                res = await _roleRepository.List($"?pageSize={pageSize}&currentPage={currentPage}");
                
            }
            catch(Exception ex)
            {
               
            }
            return View(res);
        }

        [HttpGet]
        public async Task<IActionResult> ListPermissions()
        {
            try
            {
                var list = await _permissionRepository.ListPermissions(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                return View(list);
            }
            catch (Exception e)
            {
                return View(new List<PermissionListView>());
            }

        }
    }
}
