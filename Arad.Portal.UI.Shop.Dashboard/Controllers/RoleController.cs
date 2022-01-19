using Arad.Portal.DataLayer.Contracts.General.Permission;
using Arad.Portal.DataLayer.Contracts.General.Role;
using Arad.Portal.DataLayer.Entities.General.Role;
using Arad.Portal.DataLayer.Models.Permission;
using Arad.Portal.DataLayer.Models.Role;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.General.User.Mongo;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Arad.Portal.DataLayer.Entities;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class RoleController : Controller
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IPermissionView _permissionViewManager;
        private readonly UserExtensions _userExtensions;

        public RoleController(IRoleRepository repository, IPermissionRepository permissionRepository,
            UserExtensions userExtensions, IPermissionView permissionViewManager)
        {
            _roleRepository = repository;
            _permissionRepository = permissionRepository;
            _userExtensions = userExtensions;
            _permissionViewManager = permissionViewManager;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            //var claims = User.Claims;
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            ViewBag.Permissions = dicKey;
            PagedItems<RoleListViewModel> list = await _roleRepository.RoleList(Request.QueryString.ToString());
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> AddEdit(string id)
        {
            var model = new RoleDTO();
           
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if(!string.IsNullOrWhiteSpace(id))
            {
                model = await _roleRepository.FetchRole(id);
            }
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromForm]RoleDTO model)
        {
            if (!model.PermissionIds.Any())
            {
                ModelState.AddModelError("PermissionIds", Language.GetString("AlertAndMessage_PermissionSelectLimitation"));
            }else
            {
                model.PermissionIds = model.PermissionIds[0].Split(',').ToList();
            }
            
            if (!string.IsNullOrWhiteSpace(model.RoleId))
            {
                if (string.IsNullOrWhiteSpace(model.ModificationReason))
                {
                    ModelState.AddModelError("ModificationReason", Language.GetString("AlertAndMessage_ModificationReason"));
                }
            }

            if (!ModelState.IsValid)
            {
                var errors = new List<AjaxValidationErrorModel>();

                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }

                return Json(new { Status = "ModelError", ModelStateErrors = errors });
            }

            List<string> currentUserPers = await _permissionRepository
                .GetUserPermissionsAsync();

            if (model.PermissionIds.Any(item => !currentUserPers.Contains(item)))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (!string.IsNullOrWhiteSpace(model.RoleId))
            {
                var role = await _roleRepository.FetchRole(model.RoleId);
                if (role == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }
            }
            Result saveResult;
            if (!string.IsNullOrWhiteSpace(model.RoleId))
            {
                saveResult = await _roleRepository.Update(model);
            }else
            {
                saveResult = await _roleRepository.Add(model);
            }

            
            return Ok(saveResult.Succeeded ? new { Status = "Success", saveResult.Message } : new { Status = "Error", saveResult.Message });
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
        public async Task<IActionResult> Details(string roleId)
        {
            JsonResult result;
            try
            {
                var role = await _roleRepository.FetchRole(roleId);

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

        //[HttpPost]
        //public async Task<IActionResult> Edit(RoleDTO role)
        //{
        //    JsonResult result;
        //    List<ClientValidationErrorModel> errors = new List<ClientValidationErrorModel>();

        //    if (!ModelState.IsValid)
        //    {
        //        foreach (var modelStateKey in ModelState.Keys)
        //        {
        //            var modelStateVal = ModelState[modelStateKey];
        //            foreach (var error in modelStateVal.Errors)
        //            {
        //                var obj = new ClientValidationErrorModel
        //                {
        //                    Key = modelStateKey,
        //                    ErrorMessage = error.ErrorMessage,
        //                };
        //                errors.Add(obj);
        //            }
        //        }
        //        result =  new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
        //    }
        //    else
        //    {
        //        try
        //        {

        //            var oldRole =await _roleRepository.FetchRole(role.RoleId);
        //            oldRole.RoleName = role.RoleName;
        //            oldRole.PermissionIds = role.PermissionIds;

        //            var res = await _roleRepository.Update(role);

        //            if (!res.Succeeded)
        //            {
        //                result = new JsonResult(new { Status = "error", Message = res.Message, ModelStateErrors = errors });
        //            }
        //            else
        //            {
        //                result = new JsonResult(new { Status = "success", Message = res.Message, ModelStateErrors = errors });
        //            }

        //        }
        //        catch
        //        {
        //            result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
        //        }
        //    }
        //    return result;
        //}

        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            JsonResult result;
            try
            {
                var res = await _roleRepository.Delete(id, "");
                if (res.Succeeded)
                {
                    result = Json(new { Status = "success", Message = res.Message });
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
        public async Task<IActionResult> ChangeActivation(string id)
        {
            JsonResult result;
            try
            {
                var res = await _roleRepository.ChangeActivation(id);
                if (res.Succeeded)
                {
                    var role = await _roleRepository.FetchRole(id);
                    result = Json(new { Status = "success", Message = res.Message, result = role.IsActive.ToString()});
                }
                else
                {
                    result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator")});
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
        public async Task<IActionResult> GetLatestModifications(string id)
        {
            var result = new List<Modification>();
            var entity = await _roleRepository.FetchRoleEntity(id);
            if(entity != null)
            {
                result = entity.Modifications.OrderByDescending(_ => _.DateTime).Take(10).ToList();
            }
           
            return View("~/Views/Shared/GetLatestModifications.cshtml", result);
        }

        
        [HttpGet]
        public IActionResult ListPermissions(string id = "")
        {
            var result = new List<TreeviewModel>();
            try
            {
                result = _permissionRepository
                    .ListPermissions(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), id).Result;
               
            }
            catch (Exception e)
            {
            }
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> Restore(string id)
        {
            JsonResult result;

            try
            {
                var roleDto = await _roleRepository.FetchRole(id);
                if (roleDto == null)
                {
                    result = new JsonResult(new { Status = "error", 
                        Message = Language.GetString("AlertAndMessage_EntityNotFound") });
                }
                else
                {
                    roleDto.IsDeleted = false;
                    roleDto.ModificationReason = "restore role";
                    var res = await _roleRepository.Update(roleDto);
                    if (res.Succeeded)
                    {
                        result = new JsonResult(new { Status = "success", 
                            Message = Language.GetString("AlertAndMessage_EditionDoneSuccessfully") });
                    }
                    else
                    {
                        result = new JsonResult(new { Status = "error", 
                            Message = Language.GetString("AlertAndMessage_TryLator") });
                    }
                }
            }
            catch (Exception e)
            {
                result = new JsonResult(new { Status = "error", 
                    Message = Language.GetString("AlertAndMessage_TryLator") });
            }

            return result;
        }

    }
}
