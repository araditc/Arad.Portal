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
                result = Json(new { Status = "error", Message = "لطفا مجددا امتحان نمایید." });
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
                    ModelState.AddModelError("Icon", "لطفا فایل آیکون منو را مشخص نمایید.");
                }

                if (permission.Priority < 1)
                {
                    ModelState.AddModelError("Priority", "لطفا الویت منو را مشخص نمایید.");
                }
                if (string.IsNullOrWhiteSpace(permission.Routes))
                {
                    ModelState.AddModelError("Routes", "لطفا مسیر منو را مشخص نمایید.");
                }
            }
            else if (permission.Type == Enums.PermissionType.Module)
            {
                if (permission.MenuIdOfModule == "-1")
                {
                    ModelState.AddModelError("MenuIdOfModule", "لطفا منو ماژول را مشخص نمایید.");
                }
                if (string.IsNullOrWhiteSpace(permission.Routes))
                {
                    ModelState.AddModelError("Routes", "لطفا مسیر ماژول را مشخص نمایید.");
                }
            }

            if (!ModelState.IsValid)
            {
                errors = ModelState.Generate();
                return new JsonResult(new { Status = "error", Message = "فیلدهای ضروری تکمیل گردد.", ModelStateErrors = errors });
            }

            try
            {

                var result = _permissionRepository.Save(permission);
                if(result.IsCompleted)
                {
                    if (!result.Result.Succeeded)
                    {
                        finalResult = new JsonResult(new { Status = "error",
                            Message = "لطفا مجددا تلاش کنید.", ModelStateErrors = errors });
                    }
                    else
                    {
                        finalResult = new JsonResult(new { Status = "success", Message = "دسترسی با موفقیت اضافه گردید.", ModelStateErrors = errors });
                    }
                }else
                {
                    finalResult = new JsonResult(new { Status = "error", Message = "لطفا مجددا تلاش کنید." });
                }
            }
            catch (Exception e)
            {

                finalResult = new  JsonResult(new { Status = "error", Message = "لطفا مجددا تلاش کنید." });
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
                return Json(new { Status = "error", Message = "لطفا مجددا امتحان نمایید." });
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
                    Message = "فیلدهای ضروری تکمیل گردد.", ModelStateErrors = errors });
            }

            if (permission.Type == Enums.PermissionType.Menu)
            {
                if (permission.Icon == null)
                {
                    var obj = new ClientValidationErrorModel
                    {
                        Key = "Icon",
                        ErrorMessage = "لطفا فایل آیکون منو را مشخص نمایید.",
                    };

                    errors.Add(obj);
                }

                if (permission.Priority < 1)
                {
                    var obj = new ClientValidationErrorModel
                    {
                        Key = "Priority",
                        ErrorMessage = "لطفا الویت منو را مشخص نمایید.",
                    };

                    errors.Add(obj);
                }
                if (string.IsNullOrWhiteSpace(permission.Routes))
                {
                    var obj = new ClientValidationErrorModel
                    {
                        Key = "Route",
                        ErrorMessage = "لطفا مسیر منو را مشخص نمایید.",
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
                        ErrorMessage = "لطفا منو ماژول را مشخص نمایید.",
                    };

                    errors.Add(obj);
                }
                if (string.IsNullOrWhiteSpace(permission.Routes))
                {
                    var obj = new ClientValidationErrorModel
                    {
                        Key = "Route",
                        ErrorMessage = "لطفا مسیر ماژول را مشخص نمایید.",
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
                        Message = "لطفا مجددا تلاش کنید.",
                        ModelStateErrors = errors
                    });
                }
                else
                {
                    result = new JsonResult(new { 
                                                  Status = "success", 
                                                  Message = "دسترسی با موفقیت ویرایش گردید.", 
                                                  ModelStateErrors = errors 
                                                });
                }
            }
            catch (Exception e)
            {

               result =  new JsonResult(new { Status = "error", Message = "لطفا مجددا تلاش کنید." });
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
                        Message = "حذف دسترسی با موفقیت انجام شد" });
                }else
                {
                    result = Json(new { Status = "error", Message = "لطفا مجددا امتحان نمایید." });
                }
            }
            catch (Exception e)
            {
               result = Json(new { Status = "error", Message = "لطفا مجددا امتحان نمایید." });
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
