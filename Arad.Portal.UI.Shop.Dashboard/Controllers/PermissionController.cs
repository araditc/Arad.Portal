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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using AutoMapper;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class PermissionController : Controller
    {
        //private readonly IPermissionRepository _permissionRepository;
        //private readonly IMapper _mapper;


        //public PermissionController(IPermissionRepository repository,
        //    IMapper mapper)
        //{
        //    _permissionRepository = repository;
        //    _mapper = mapper;
        //}

        //[HttpGet]
        //public async Task<IActionResult>  AddEdit(string id)
        //{
        //    var model = new UserPermissionViewModel()
        //    {
        //        IsEditView = false,
        //    };
        //    if (!string.IsNullOrWhiteSpace(id))
        //    {
        //        model.Id = id;
        //        var permission = await _permissionRepository.FetchPermission(id);
        //        model.Icon = permission.Icon;
        //        model.IsEditView = true;
        //        model.MenuIdOfModule = permission.MenuIdOfModule;
        //        model.Method = permission.Method;
        //        model.ParentMenuId = permission.ParentMenuId;
        //        model.Priority = permission.Priority;
        //        model.Routes = string.Join(',',permission.Routes);
        //        model.Title = permission.Title;
        //        model.Type = permission.Type;
        //        model.ClientAddress = permission.ClientAddress;

        //    }
        //    return View(model);
        //}

        //[HttpGet]
        //public async Task<IActionResult> List()
        //{
        //    var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
        //    ViewBag.Permissions = dicKey;
        //    PagedItems<ListPermissionViewModel> list = await _permissionRepository.List(Request.QueryString.ToString());
        //    return View(list);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Save([FromForm] UserPermissionViewModel model)
        //{
        //    if (model.IsEditView)
        //    {
        //        if (string.IsNullOrWhiteSpace(model.ModificationReason))
        //        {
        //            ModelState.AddModelError("ModificationReason", Language.GetString("AlertAndMessage_ModificationReason"));
        //        }
        //    }else
        //    {
        //        if (model.Type == Enums.PermissionType.Menu)
        //        {
        //            if (model.Icon == null)
        //            {
        //                ModelState.AddModelError("Icon", Language.GetString("AlertAndMessage_lackOfIcon"));
        //            }

        //            if (model.Priority < 1)
        //            {
        //                ModelState.AddModelError("Priority", Language.GetString("AlertAndMessage_lackOfPriority"));
        //            }
        //            if (string.IsNullOrWhiteSpace(model.Routes))
        //            {
        //                ModelState.AddModelError("Routes", Language.GetString("AlertAndMessage_lackOfRoutes"));
        //            }
        //        }
        //        else if (model.Type == Enums.PermissionType.Module)
        //        {
        //            if (string.IsNullOrWhiteSpace(model.MenuIdOfModule))
        //            {
        //                ModelState.AddModelError("MenuIdOfModule", Language.GetString("AlertAndMessage_lackOfMenuIdofModule"));
        //            }
        //            if (string.IsNullOrWhiteSpace(model.Routes))
        //            {
        //                ModelState.AddModelError("Routes", Language.GetString("AlertAndMessage_lackOfRoutes"));
        //            }
        //        }
        //    }
            
        //    if (!ModelState.IsValid)
        //    {
        //        var errors = new List<AjaxValidationErrorModel>();

        //        foreach (var modelStateKey in ModelState.Keys)
        //        {
        //            var modelStateVal = ModelState[modelStateKey];
        //            errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
        //        }

        //        return Ok(new { Status = "ModelError", ModelStateErrors = errors });
        //    }

        //    var dto = new PermissionDTO()
        //    {
        //        ClientAddress = model.ClientAddress,
        //        Icon = model.Icon,
        //        MenuIdOfModule = model.MenuIdOfModule,
        //        Method = model.Method,
        //        ModificationReason = model.ModificationReason,
        //        ParentMenuId = model.ParentMenuId,
        //        PermissionId = model.IsEditView ? model.Id : "",
        //        Priority = model.Priority,
        //        Routes = model.Routes,
        //        Title = model.Title,
        //        Type = model.Type
        //    };
        //    var saveResult = await _permissionRepository.Save(dto);
        //    return Ok(saveResult.Succeeded ? new { Status = "Success", saveResult.Message } : new { Status = "Error", saveResult.Message });
        //}

        //[HttpDelete]
        //public async Task<IActionResult> Delete(string id)
        //{
        //    Result opResult = await _permissionRepository.Delete(id);
        //    return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message } : new { Status = "Error", opResult.Message });
        //}

        ////[HttpGet]
        ////public IActionResult Edit(string id)
        ////{
        ////    if (string.IsNullOrWhiteSpace(id))
        ////    {
        ////        return BadRequest();
        ////    }

        ////    var per = _repository.GetForEdit(id);
        ////    if (per.Succeeded)
        ////    {
               

                
        ////        return View("New", per.ReturnValue);
        ////    }

        ////    return StatusCode(500);
        ////}

        //[HttpGet]
        //public IActionResult GetLatestModifications(string id)
        //{
        //    var modifResult = _permissionRepository.GetModifications(id);

        //    if (modifResult.Succeeded)
        //    {
        //        return View(modifResult.ReturnValue);
        //    }

        //    return Content(modifResult.Message);
        //}

        //[HttpGet]
        //public async Task<IActionResult> Restore(string id)
        //{
        //    JsonResult result;
            
        //    try
        //    {
        //        var permissionEntity = await _permissionRepository.FetchPermission(id);
        //        if (permissionEntity == null)
        //        {
        //            result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_EntityNotFound") });
        //        }
        //        else
        //        {
        //            permissionEntity.IsDeleted = false;
        //            PermissionDTO dto = _mapper.Map<PermissionDTO>(permissionEntity);
        //            dto.Routes = string.Join(',', permissionEntity.Routes);
        //            dto.ModificationReason = "restore permission";

        //            var res = await _permissionRepository.Save(dto);

        //            if (res.Succeeded)
        //            {
        //                result = new JsonResult(new { Status = "success", Message = Language.GetString("AlertAndMessage_EditionDoneSuccessfully") });
        //            }
        //            else
        //            {
        //                result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
        //            }
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
        //    }
           
        //    return result;

        //}
       
        //[HttpGet]
        //public async Task<IActionResult> ChangeActivation(string id, bool IsActive, string modificationReason)
        //{
        //    JsonResult result;
        //    try
        //    {
        //        var res = await _permissionRepository.ChangeActivation(id, IsActive, modificationReason);
        //        if (res.Succeeded)
        //        {
        //            result =  Json(new { Status = "success", Message = res.Message });
        //        }else
        //        {
        //            result =  Json(new { Status = "error", Message = res.Message });
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
        //    }
        //    return result;
        //}

        //[HttpGet]
        //public async Task<IActionResult> MenusSelectList(Enums.PermissionType typeMenu)
        //{
            
        //    JsonResult result;
        //    try
        //    {
        //        var list = await _permissionRepository.MenusPermission(typeMenu);

        //        result =  new JsonResult(new { Status = "success", Data = list });
        //    }
        //    catch (Exception e)
        //    {
        //        result = new JsonResult(new { Status = "error", Message = "" });
        //    }
        //    return result;
        //}
    }
}
