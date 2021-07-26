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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class RoleController : Controller
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly UserExtensions _userExtensions;

        public RoleController(IRoleRepository repository, IPermissionRepository permissionRepository,
            UserExtensions userExtensions)
        {
            _roleRepository = repository;
            _permissionRepository = permissionRepository;
            _userExtensions = userExtensions;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var claims = User.Claims;
            PagedItems<RoleListViewModel> list = await _roleRepository.RoleList(Request.QueryString.ToString());
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> New()
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var model = new UserRoleViewModel()
            {
                AllAllowedPermissions = await _roleRepository.GetAllPermissions(currentUserId),
                SelectedPermissions = _roleRepository.GetSelectedPermissions(string.Empty),
                IsEditView = false,
            };

            return View(model);
        }


        [HttpGet]
        public IActionResult GetCitiesForAddress(string id)
        {
            SelectList cities = _roleRepository.GetCities(id);
            return View("GetCitiesForAddress", cities);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromForm] UserRoleViewModel model)
        {
            if (!model.PermissionIds.Any())
            {
                ModelState.AddModelError("PermissionIds", "حداقل یک دسترسی باید انتخاب شود.");
            }

            if (model.IsEditView)
            {
                if (string.IsNullOrWhiteSpace(model.ModificationReason))
                {
                    ModelState.AddModelError("ModificationReason", "دلیل ویرایش را به طور مختصر ذکر نمایید.");
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

                return Ok(new { Status = "ModelError", ModelStateErrors = errors });
            }

            List<string> currentUserPers = await _permissionRepository
                .GetUserPermissionsAsync();

            if (model.PermissionIds.Any(item => !currentUserPers.Contains(item)))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (model.IsEditView)
            {
                var role = await _roleRepository.GetRoleByRoleId(model.Id);
                if (role == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }

                if (User.GetUserId() != role.CreatorId)
                {
                    if (await _userExtension.CheckIfIsSubUserAsync(User.GetUserId(), role.CreatorId) == false)
                    {
                        return RedirectToAction("AccessDenied", "Account");
                    }
                }
            }

            RepositoryOperationResult saveResult = await _roleRepository.Save(model);
            return Ok(saveResult.Succeeded ? new { Status = "Success", saveResult.Message } : new { Status = "Error", saveResult.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleRepository.GetRoleByRoleId(id);

            if (role == null)
            {
                return RedirectToAction("PageOrItemNotFound", "Account");
            }

            if (User.GetUserId() != role.CreatorId)
            {
                if (await _userExtension.CheckIfIsSubUserAsync(User.GetUserId(), role.CreatorId) == false)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
            }

            var model = new UserRoleViewModel()
            {
                AllAllowedPermissions = await _roleRepository.GetAllPermissions(role.CreatorId),
                SelectedPermissions = _roleRepository.GetSelectedPermissions(id),
                IsEditView = true,
                Color = role.Color,
                FaTitle = role.FaTitle,
                Id = role.Id
            };

            return View("New", model);
        }


        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleRepository.GetRoleByRoleId(id);
            if (role == null)
            {
                return NotFound();
            }

            if (!User.IsSystemAccount())
            {
                if (User.GetUserId() != role.CreatorId)
                {
                    if (await _userExtension.CheckIfIsSubUserAsync(User.GetUserId(), role.CreatorId) == false)
                    {
                        return Forbid();
                    }
                }
            }

            RepositoryOperationResult opResult = await _roleRepository.DeleteRole(id);
            return Ok(opResult.Succeeded ? new { Status = "Success", opResult.Message } : new { Status = "Error", opResult.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestModifications(string id)
        {
            var entity = await _roleRepository.GetRoleByRoleId(id);
            if (entity == null)
            {
                return NotFound();
            }

            if (!User.IsSystemAccount())
            {
                if (User.GetUserId() != entity.CreatorId)
                {
                    if (await _userExtension.CheckIfIsSubUserAsync(User.GetUserId(), entity.CreatorId) == false)
                    {
                        return Forbid();
                    }
                }
            }

            return View("~/Views/Permission/GetLatestModifications.cshtml", entity.Modifications.Take(10).ToList());
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

      
        //[HttpGet]
        //public async Task<IActionResult> GetRole(string id)
        //{
        //    JsonResult result;
        //    try
        //    {
        //        var role = await _roleRepository.FetchRole(id);

        //        if (role == null)
        //        {
        //            result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_DataWasNotFound") });
        //        }

        //        //var permissions = await _permissionRepository.ListPermissions(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        //        var data = new { Role = role };

        //        result = Json(new { Status = "success", Data = data });
        //    }
        //    catch (Exception e)
        //    {
        //        result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
        //    }
        //    return result;
        //}

        [HttpGet]
        public async Task<IActionResult> GetRole(string id, bool editPermission)
        {
            RoleDTO role = new RoleDTO();
           
            try
            {
                ViewBag.Edit = editPermission;
                role = await _roleRepository.FetchRole(id);
            }
            catch (Exception e)
            {
            }
            return View(role);
        }

        public async Task<IActionResult> GeneratePermissions(string roleId)
        {
            var permissions = await _permissionRepository.ListPermissions(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), roleId);
            return PartialView(permissions);
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
                   
                    var oldRole =await _roleRepository.FetchRole(role.RoleId);
                    oldRole.RoleName = role.RoleName;
                    oldRole.PermissionIds = role.PermissionIds;

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
        public async Task<IActionResult> Delete(string id)
        {
            JsonResult result;
            try
            {
                var res = await _roleRepository.Delete(id, "");
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
        public IActionResult ListPermissions(string currentRoleId = "")
        {
            var result = new List<ListPermissions>();
            try
            {
                result =  _permissionRepository.ListPermissions(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), currentRoleId).Result;
               
            }
            catch (Exception e)
            {
            }
            return View(result);
        }



    }
}
