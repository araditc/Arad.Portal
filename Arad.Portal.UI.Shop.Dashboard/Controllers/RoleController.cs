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
using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class RoleController : Controller
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly UserExtensions _userExtensions;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;


        public RoleController(IRoleRepository repository, IPermissionRepository permissionRepository,
            UserExtensions userExtensions, IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _roleRepository = repository;
            _permissionRepository = permissionRepository;
            _userExtensions = userExtensions;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            //var claims = User.Claims;
            PagedItems<RoleListViewModel> list = await _roleRepository.RoleList(Request.QueryString.ToString());
            return View(list);
        }
        
        [HttpGet]
        public IActionResult New()
        {
            RoleDTO dto = new();
            return View("Upsert", dto);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var roleDto = _roleRepository.FetchRole(id);
            if (roleDto == null)
            {
                return RedirectToAction("PageOrItemNotFound", "Account");
            }

            return View("Upsert", roleDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert([FromForm] RoleDTO dto)
        {
            if (!ModelState.IsValid)
            {
                List<AjaxValidationErrorModel> errors = new();
                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors
                        .Select(error => new AjaxValidationErrorModel
                        { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }

                return Ok(new { Status = "ModelError", ModelStateErrors = errors });
            }
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var dbUser = await _userManager.FindByIdAsync(currentUserId);

            //if (dbUser.IsSystemAccount)
            //{
            //    List<string> currentUserPermission = await _permissionRepository.GetAllNestedPermissionIds();
            //    //if (dto.PermissionIds != null && dto.PermissionIds.Split(',').Any(item => !currentUserPermission.Contains(item)))
            //    //{
            //    //    return RedirectToAction("AccessDenied", "Account");
            //    //}
            //}

            Result saveResult;
            if (!string.IsNullOrWhiteSpace(dto.RoleId))
            {
                var roleDto = _roleRepository.FetchRole(dto.RoleId);

                if (roleDto == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }
                //dto.ModificationReason = $"PermissionIdsCount : {dto.PermissionIds.Count}";
                //dto.HasModifications = true;
                saveResult = await _roleRepository.Update(dto);
            }
            else
            {
                saveResult = await _roleRepository.Add(dto);
            }

            return Ok(saveResult.Succeeded ? new { Status = "Success", saveResult.Message } : new { Status = "Error", saveResult.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            //bool result = await _repository.Delete(id);

            //return Ok(new { Result = result });

            JsonResult Result;
            try
            {
                var res = await _roleRepository.Delete(id, "");
                if (res.Succeeded)
                {
                    Result = Json(new { Status = "success", Message = res.Message });
                }
                else
                {
                    Result = Json(new { Status = "error", Message = res.Message });
                }
            }
            catch (Exception e)
            {
                Result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }

            return Result;
        }

        [HttpGet]
        public async Task<IActionResult> ListPermissions(string id = "")
        {
            var roleDto = _roleRepository.FetchRole(id);
            List<DataLayer.Entities.General.Permission.Permission> permissions = _permissionRepository.GetAllPermissions();
            List<JsTree> jsTrees = ConvertToJsTree(permissions);

            List<JsTree> ConvertToJsTree(List<DataLayer.Entities.General.Permission.Permission> list)
            {
                List<JsTree> trees = new();
                foreach (DataLayer.Entities.General.Permission.Permission permission in list.OrderBy(p => p.Priority))
                {
                    JsTree jsTree = new()
                    {
                        Id = permission.PermissionId,
                        Text = Language.GetString($"PermissionTitle_{permission.Title}"),
                        State = new State(),
                        Children = ConvertToJsTree(permission.Children)
                    };

                    foreach (DataLayer.Entities.General.Permission.Action action in permission.Actions)
                    {
                        JsTree actionJsTree = new() { Id = action.PermissionId, Text = Language.GetString($"PermissionTitle_{action.Title}"), State = new State(), Children = new() };
                        jsTree.Children.Add(actionJsTree);

                        if (roleDto != null && !string.IsNullOrWhiteSpace(roleDto.PermissionIds) && roleDto.PermissionIds.Contains(action.PermissionId))
                        {
                            actionJsTree.State.Selected = true;
                        }
                    }

                    if (roleDto != null && !string.IsNullOrWhiteSpace(roleDto.PermissionIds) && roleDto.PermissionIds.Contains(permission.PermissionId))
                    {
                        jsTree.State.Selected = true;
                    }

                    trees.Add(jsTree);
                }

                return trees;
            }

            return Ok(jsTrees);
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
                    var role = _roleRepository.FetchRole(id);
                    result = Json(new { Status = "success", Message = res.Message, result = role.IsActive.ToString() });
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


        [HttpGet]
        public async Task<IActionResult> Restore(string id)
        {
            JsonResult result;
            try
            {
                var res = await _roleRepository.Restore(id);
                result = new JsonResult(new { Status = res.Succeeded ? "success" : "error", 
                     Message = res.Succeeded ? Language.GetString("AlertAndMessage_SuccessfullyDone") : Language.GetString("AlertAndMessage_TryLator") });
               
            }
            catch (Exception e)
            {
                result = new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }
            return result;
           
        }


    }
}
