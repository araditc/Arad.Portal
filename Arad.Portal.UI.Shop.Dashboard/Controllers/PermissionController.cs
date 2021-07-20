using Arad.Portal.DataLayer.Contracts.General.Permission;
using Arad.Portal.DataLayer.Entities.General.Permission;
using Arad.Portal.DataLayer.Models.Permission;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
using Arad.Portal.GeneralLibrary.Utilities;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class PermissionController : Controller
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IPermissionView _permissionViewManager;

        public PermissionController(IPermissionRepository repository,
            IPermissionView permissionViewManager)
        {
            _permissionRepository = repository;
            _permissionViewManager = permissionViewManager;
        }

        public async Task<IActionResult> Index()
        {
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            return View(dicKey);
        }


        [HttpPost]
        public async Task<IActionResult> List(int pageSize, int currentPage)
        {
            try
            {
                var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
                ViewBag.Permissions = dicKey;
                PagedItems<ListPermissionViewModel> data = await _permissionRepository.List($"?CurrentPage={currentPage}&PageSize={pageSize}");
                return View(data);
            }
            catch
            {
                return View(new PagedItems<ListPermissionViewModel>());
            }

        }

        [HttpGet]
        public async Task<IActionResult> ChangeActivation(string id, bool IsActive, string modificationReason)
        {
            JsonResult result;
            try
            {
                var res = await _permissionRepository.ChangeActivation(id, IsActive, modificationReason);
                if (res.Succeeded)
                {
                    result =  Json(new { Status = "success", Message = res.Message });
                }else
                {
                    result =  Json(new { Status = "error", Message = res.Message });
                }
            }
            catch (Exception e)
            {
                result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }
            return result;
        }


        [HttpPost]
        public IActionResult AddPermission(PermissionDTO permission)
        {
            JsonResult finalResult;
            List<ClientValidationErrorModel> errors = new List<ClientValidationErrorModel>();
            if (permission.Type == Enums.PermissionType.Menu)
            {
                if (permission.Icon == null)
                {
                    ModelState.AddModelError("Icon", Language.GetString("AlertAndMessage_lackOfIcon"));
                }

                if (permission.Priority < 1)
                {
                    ModelState.AddModelError("Priority", Language.GetString("AlertAndMessage_lackOfPriority"));
                }
                if (string.IsNullOrWhiteSpace(permission.Routes))
                {
                    ModelState.AddModelError("Routes", Language.GetString("AlertAndMessage_lackOfRoutes"));
                }
            }
            else if (permission.Type == Enums.PermissionType.Module)
            {
                if (permission.MenuIdOfModule == "-1")
                {
                    ModelState.AddModelError("MenuIdOfModule", Language.GetString("AlertAndMessage_lackOfMenuIdofModule"));
                }
                if (string.IsNullOrWhiteSpace(permission.Routes))
                {
                    ModelState.AddModelError("Routes", Language.GetString("AlertAndMessage_lackOfRoutes"));
                }
            }

            if (!ModelState.IsValid)
            {
                errors = ModelState.Generate();
                return new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
            }

            try
            {

                var result = _permissionRepository.Save(permission);
                if(result.IsCompleted)
                {
                    if (!result.Result.Succeeded)
                    {
                        finalResult = new JsonResult(new { Status = "error",
                            Message = Language.GetString("AlertAndMessage_TryLator"), ModelStateErrors = errors });
                    }
                    else
                    {
                        finalResult = new JsonResult(new { Status = "success", Message = Language.GetString("AlertAndMessage_InsertionDoneSuccessfully"), ModelStateErrors = errors });
                    }
                }else
                {
                    finalResult = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
                }
            }
            catch (Exception e)
            {

                finalResult = new  JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }
            return finalResult;
        }

        [HttpGet]
        public async Task<IActionResult> GetPermission(string id)
        {
            try
            {
                var permission = await _permissionRepository.FetchPermission(id);

                return Json(new { Status = "success", Data = permission });
            }
            catch (Exception e)
            {
                return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PermissionDTO permission)
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
                result =  new JsonResult(new { Status = "error",
                    Message = Language.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
            }

            if (permission.Type == Enums.PermissionType.Menu)
            {
                if (permission.Icon == null)
                {
                    var obj = new ClientValidationErrorModel
                    {
                        Key = "Icon",
                        ErrorMessage = Language.GetString("AlertAndMessage_lackOfIcon"),
                    };

                    errors.Add(obj);
                }

                if (permission.Priority < 1)
                {
                    var obj = new ClientValidationErrorModel
                    {
                        Key = "Priority",
                        ErrorMessage = Language.GetString("AlertAndMessage_lackOfPriority"),
                    };

                    errors.Add(obj);
                }
                if (string.IsNullOrWhiteSpace(permission.Routes))
                {
                    var obj = new ClientValidationErrorModel
                    {
                        Key = "Route",
                        ErrorMessage = Language.GetString("AlertAndMessage_lackOfRoutes")
                    };

                    errors.Add(obj);
                }

                if (permission.Icon == null || 
                    permission.Priority == 0 ||
                    string.IsNullOrWhiteSpace(permission.Routes))
                {
                    result =  new JsonResult(new { Status = "error", Message = "", ModelStateErrors = errors });
                }

            }
            else if (permission.Type == Enums.PermissionType.Module)
            {
                if (permission.MenuIdOfModule == "-1")
                {
                    var obj = new ClientValidationErrorModel
                    {
                        Key = "MenuIdOfModule",
                        ErrorMessage = Language.GetString("AlertAndMessage_lackOfMenuIdofModule"),
                    };

                    errors.Add(obj);
                }
                if (string.IsNullOrWhiteSpace(permission.Routes))
                {
                    var obj = new ClientValidationErrorModel
                    {
                        Key = "Route",
                        ErrorMessage = Language.GetString("AlertAndMessage_lackOfRoutes"),
                    };

                    errors.Add(obj);
                }

                if (string.IsNullOrWhiteSpace(permission.Routes) || permission.MenuIdOfModule == "-1")
                {
                    result =new  JsonResult(new { Status = "error", Message = "", ModelStateErrors = errors });
                }

            }

            try
            {
                var res = await _permissionRepository.Save(permission);

                if (!res.Succeeded)
                {
                    result = new JsonResult(new
                    {
                        Status = "error",
                        Message = Language.GetString("AlertAndMessage_TryLator"),
                        ModelStateErrors = errors
                    });
                }
                else
                {
                    result = new JsonResult(new { 
                                                  Status = "success", 
                                                  Message = Language.GetString("AlertAndMessage_EditionDoneSuccessfully"), 
                                                  ModelStateErrors = errors 
                                                });
                }
            }
            catch (Exception e)
            {

               result =  new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id, string modificationReason)
        {
            JsonResult result;
            try
            {
                var res = await _permissionRepository.Delete(id, modificationReason);
                if (res.Succeeded)
                {
                    result = Json(new {
                        Status = "success", 
                        Message = Language.GetString("AlertAndMessage_DeletionDoneSuccessfully") });
                }else
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


       

        [HttpGet]
        public async Task<IActionResult> MenusSelectList(Enums.PermissionType typeMenu)
        {
            
            JsonResult result;
            try
            {
                var list = await _permissionRepository.MenusPermission(typeMenu);

                result =  new JsonResult(new { Status = "success", Data = list });
            }
            catch (Exception e)
            {
                result = new JsonResult(new { Status = "error", Message = "" });
            }
            return result;
        }
    }
}
