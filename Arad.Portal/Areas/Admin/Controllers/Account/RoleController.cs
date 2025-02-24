using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.ApplicationRole;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Entities.General.Permission;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Permission;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Role;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.Role;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Serilog;

namespace Arad.Portal.Areas.Admin.Controllers.Account;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class RoleController(
    IRoleRepository roleRepository,
    IUserRepository userRepository,
    IModificationRepository modificationRepository,
    IPermissionRepository permissionRepository,
    IMapper mapper,
    ControllerHelper controllerHelper,
    ILogger logger)
    : Controller
{

    [HttpGet]
    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        string queryString = Request.QueryString.ToString();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        PagedItems<RoleListViewModel> result = new();

        try
        {
            NameValueCollection filter = HttpUtility.ParseQueryString(queryString);

            if (string.IsNullOrWhiteSpace(filter["page"]))
            {
                filter.Set("page", "1");
            }

            if (string.IsNullOrWhiteSpace(filter["pageSize"]))
            {
                filter.Set("pageSize", "20");
            }

            int page = Convert.ToInt32(filter["page"]);
            int pageSize = Convert.ToInt32(filter["pageSize"]);
            Domain domain = controllerHelper.GetCurrentUserDomain();
            long totalCount = await roleRepository.GetCountAsync(c => !c.IsDeleted && c.AssociatedDomainId == domain.Id, cancellationToken);
            IEnumerable<ApplicationRole> list;
            if (userDb.IsSystemAccount)
            {
                list = (await roleRepository.GetAllAsync(cancellationToken)).Where(_ => true);
            }

            else if (domain.OwnerUserId == userDb.Id)
            {
                list = (await roleRepository.GetAllAsync(cancellationToken)).Where(c => c.AssociatedDomainId == domain.Id);
            }
            else
            {
                list = (await roleRepository.GetAllAsync(cancellationToken)).Where(c => c.AssociatedDomainId == domain.Id && c.CreatorUserId == userDb.Id).Skip((page - 1) * pageSize);

            }

            List<RoleListViewModel> lst = list.Take(pageSize)
                                              .Select(c => new RoleListViewModel
                                              {
                                                  Id = c.Id,
                                                  RoleName = c.Name,
                                                  CreatorId = c.CreatorUserId,
                                                  CreatorUserName = c.CreatorUserName,
                                                  CreationDateTime = c.CreationDate,
                                                  IsActive = c.IsActive,
                                                  IsDeleted = c.IsDeleted
                                              })
                                              .ToList();

            result.CurrentPage = page;
            result.Items = lst;
            result.ItemsCount = totalCount;
            result.PageSize = pageSize;
            result.QueryString = queryString;
        }
        catch (Exception ex)
        {
            result.CurrentPage = 1;
            result.Items = [];
            result.ItemsCount = 0;
            result.PageSize = 10;
            result.QueryString = queryString;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {ex.Message} occured. stack trace: {nameof(RoleController)}/{nameof(List)}");
        }

        return View(result);
    }

    [HttpGet]
    public IActionResult New()
    {
        RoleDto dto = new();

        return View("Upsert", dto);
    }

    [HttpGet]
    public Task<IActionResult> Edit(string id)
    {
        RoleDto roleDto = controllerHelper.FetchRole(id);

        return roleDto == null ? Task.FromResult<IActionResult>(RedirectToAction("PageOrItemNotFound", "Account")) : Task.FromResult<IActionResult>(View("Upsert", roleDto));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> Upsert([FromForm] RoleDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            List<AjaxValidationErrorModel> errors = [];

            foreach (string modelStateKey in ModelState.Keys)
            {
                ModelStateEntry modelStateVal = ModelState[modelStateKey];

                if (modelStateVal != null)
                {
                    errors.AddRange(modelStateVal.Errors
                                                 .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
            }

            return Ok(new { Status = "ModelError", ModelStateErrors = errors });
        }

        Domain domain = controllerHelper.GetCurrentUserDomain();
        Result<ApplicationRole> saveResult = new();
        ApplicationUser userDb = controllerHelper.GetCurrentUser(cancellationToken).Result;

        try
        {

            if (!string.IsNullOrWhiteSpace(dto.Id))
            {
                ApplicationRole model = await roleRepository.FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);

                if (model == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }

                bool isUserInRole = await userRepository.AnyAsync(c => c.UserRoleId == dto.Id, cancellationToken);
                if (isUserInRole)
                {
                    return Ok(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_UpdatedNotAllowedForDependencies") });
                }

                model.PermissionIds = dto.PermissionIds.Split(",").ToList();
                model.Name = dto.Name;
                model.AssociatedDomainId = domain.Id;
                saveResult = await roleRepository.UpdateAsync(model, cancellationToken);

                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName} upsert role done successfully to role with {model.Id} id.");
                saveResult.Succeeded = true;
                saveResult.Text = ConstMessages.SuccessfullyDone;

                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Update,
                    CollectionType = CollectionType.ApplicationRole,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = dto.Id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
            }
            else
            {
                ApplicationRole model = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    PermissionIds = dto.PermissionIds.Split(",").ToList(),
                    CreationDate = DateTime.Now,
                    CreatorUserId = controllerHelper.GetCurrentUserId(),
                    CreatorUserName = controllerHelper.GetCurrentUser(cancellationToken).Result.UserName,
                    IsActive = true,
                    AssociatedDomainId = domain.Id
                };

                saveResult = await roleRepository.InsertAsync(model, cancellationToken);
                saveResult.Succeeded = true;
                saveResult.Message = ConstMessages.SuccessfullyDone;

                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Insert,
                    CollectionType = CollectionType.ApplicationRole,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = dto.Id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
            }
        }
        catch (Exception e)
        {
            saveResult.Succeeded = false;
            saveResult.Message = ConstMessages.ErrorInSaving;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(RoleController)}/{nameof(Upsert)}");
        }

        return Ok(saveResult.Succeeded ? new { Status = "Success", saveResult.Message } : new { Status = "Error", saveResult.Message });
    }

    [HttpGet]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            bool isUserInRole = await userRepository.AnyAsync(c => c.UserRoleId == id, cancellationToken);
            if (isUserInRole)
            {
                result = Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_DeletedNotAllowedForDependencies") });
            }
            else
            {
                Result<ApplicationRole> res = await roleRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, true, cancellationToken);

                if (res.Succeeded)
                {
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName} deleting user with {id} id is done successfully");
                    result = Json(new { Status = "success", res.Message });
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Delete,
                        CollectionType = CollectionType.ApplicationRole,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
                }
                else
                {
                    logger.Warning($"userId: {userDb.Id}, userName: {userDb.UserName} deleting user with {id} id isn't done successfully");
                    result = Json(new { Status = "error", res.Message });
                }
            }
        }
        catch (Exception e)
        {
            result = Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(RoleController)}/{nameof(Delete)}");
        }

        return result;
    }

    [HttpGet]
    public async Task<IActionResult> ListPermissions(string id = "")
    {
        RoleDto? roleDto = controllerHelper.FetchRole(id);
        ApplicationUser userDb = await controllerHelper.GetCurrentUser();

        if (string.IsNullOrEmpty(roleDto?.Id))
        {
            roleDto = controllerHelper.FetchRole(userDb.UserRoleId);
        }

        List<Permission> allPermissions = await permissionRepository.GetAllAsync();
        List<Permission> permissions;
        if (userDb.IsSystemAccount)
        {
            permissions = allPermissions;
        }
        else
        {
            permissions = allPermissions
                          .Where(p => roleDto != null && roleDto.PermissionIds.Contains(p.Id))
                          .ToList();
        }

        List<JsTree> jsTrees = ConvertToJsTree(permissions);

        return Ok(jsTrees);

        List<JsTree> ConvertToJsTree(List<Permission> list)
        {
            List<JsTree> trees = [];
            foreach (Permission permission in list.OrderBy(p => p.Priority))
            {
                JsTree jsTree = new()
                {
                    Id = permission.Id,
                    Text = UtilityLanguage.GetString($"PermissionTitle_{permission.Title}"),
                    State = new(),
                    Children = ConvertToJsTree(permission.Children)
                };

                foreach (DataLayer.Entities.General.Permission.Action action in permission.Actions)
                {
                    JsTree actionJsTree = new() { Id = action.PermissionId, Text = UtilityLanguage.GetString($"PermissionTitle_{action.Title}"), State = new State(), Children = new() };
                    jsTree.Children.Add(actionJsTree);

                    if (roleDto != null && id != "" && !string.IsNullOrWhiteSpace(roleDto.PermissionIds) && roleDto.PermissionIds.Contains(action.PermissionId))
                    {
                        actionJsTree.State.Selected = true;
                    }
                }

                if (roleDto != null && id != "" && !string.IsNullOrWhiteSpace(roleDto.PermissionIds) && roleDto.PermissionIds.Contains(permission.Id))
                {
                    jsTree.State.Selected = true;
                }

                trees.Add(jsTree);
            }

            return trees;
        }
    }


    [HttpGet]
    public async ValueTask<IActionResult> ChangeActivation(string id, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        bool isUserInRole = await userRepository.AnyAsync(c => c.UserRoleId == id, cancellationToken);
        JsonResult result;

        try
        {
            RoleDto role = controllerHelper.FetchRole(id);

            if (isUserInRole)
            {
                result = Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_DisabledNotAllowedForDependencies") });
            }
            else
            {
                if (role.IsActive == true)
                {
                    Result<ApplicationRole> res = await roleRepository.UpdateAsync(c => c.Id == id, m => m.IsActive, false, cancellationToken);


                    if (res.Succeeded)
                    {
                        result = Json(new { Status = "success", res.Message, result = role.IsActive.ToString() });
                        logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName} changing activation of role with {id} id done successfully");
                        Modification modification = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ActionTypes = ActionTypes.Delete,
                            CollectionType = CollectionType.ApplicationRole,
                            Ip = controllerHelper.GetUserIpAddress(),
                            ModifierId = userDb.Id,
                            ModifierUserName = userDb.UserName,
                            ModifyDateTime = DateTime.Now,
                            RecordId = id
                        };
                        Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
                    }
                    else
                    {
                        result = Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
                    }
                }
                else
                {
                    Result<ApplicationRole> res = await roleRepository.UpdateAsync(c => c.Id == id, m => m.IsActive, true, cancellationToken);

                    if (res.Succeeded)
                    {
                        result = Json(new { Status = "success", res.Message, result = role.IsActive.ToString() });
                        logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName} changing activation of role with {id} id done successfully");
                    }
                    else
                    {
                        result = Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
                    }
                }
            }
        }
        catch (Exception e)
        {
            result = Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(RoleController)}/{nameof(ChangeActivation)}");
        }

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        ApplicationRole role = await roleRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        try
        {
            if (role == null)
            {
                result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
                return result;
            }
            Result<ApplicationRole> res = await roleRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);

            Modification modification = new()
            {
                Id = Guid.NewGuid().ToString(),
                ActionTypes = ActionTypes.Restore,
                CollectionType = CollectionType.ApplicationRole,
                Ip = controllerHelper.GetUserIpAddress(),
                ModifierId = userDb.Id,
                ModifierUserName = userDb.UserName,
                ModifyDateTime = DateTime.Now,
                RecordId = role.Id
            };
            Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

            result = new(new { Status = res.Succeeded ? "success" : "error", Message = res.Succeeded ? UtilityLanguage.GetString("AlertAndMessage_SuccessfullyDone") : UtilityLanguage.GetString("AlertAndMessage_TryLater") });
            logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName} restoring role with {id} id done successfully");
        }
        catch (Exception e)
        {
            result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(RoleController)}/{nameof(Restore)}");
        }

        return result;
    }
}