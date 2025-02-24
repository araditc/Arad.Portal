using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.Comment;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Comment;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Content;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.Comment;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Serilog;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Arad.Portal.Areas.Admin.Controllers.Setting;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class CommentController(
    ICommentRepository commentRepository,
    IDomainRepository domainRepository,
    IMapper mapper,
    IProductRepository productRepository,
    IContentRepository contentRepository,
    IModificationRepository modificationRepository,
    ControllerHelper controllerHelper,
    ILogger logger)
    : Controller
{

    [Route("{language?}/Admin/ProductComments/List")]
    [Route("{language?}/Admin/ContentComments/List")]

    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        PagedItems<CommentViewModel> result = new();
        string referenceSource = Request.Path.ToString().Split("/")[1];
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        switch (referenceSource)
        {
            case "ProductComments":
                ViewBag.Title = UtilityLanguage.GetString("Menu_ProductComments");
                ViewBag.Name = UtilityLanguage.GetString("Menu_Product");
                ViewBag.lbl = UtilityLanguage.GetString("tbl_ProductName");

                break;

            case "ContentComments":
                ViewBag.Title = UtilityLanguage.GetString("Menu_ContentComments");
                ViewBag.Name = UtilityLanguage.GetString("Menu_Content");
                ViewBag.lbl = UtilityLanguage.GetString("ContentTitle");

                break;
        }

        try
        {
            string queryString;

            if (!string.IsNullOrEmpty(Request.QueryString.ToString()))
            {
                queryString = Request.QueryString.ToString();
                queryString += $"&refType={(referenceSource == "ProductComments" ? ReferenceType.Product : ReferenceType.Content)}";
            }
            else
            {
                queryString = $"?refType={(referenceSource == "ProductComments" ? ReferenceType.Product : ReferenceType.Content)}";
            }

            string langId = userDb.Profile.DefaultLanguageId;

            try
            {
                NameValueCollection filter = HttpUtility.ParseQueryString(queryString);

                if (string.IsNullOrWhiteSpace(filter["page"]))
                {
                    filter.Set("page", "1");
                }

                if (string.IsNullOrWhiteSpace(filter["PageSize"]))
                {
                    filter.Set("PageSize", "20");
                }

                Enum.TryParse(filter["refType"], true, out ReferenceType referenceType);
                int page = Convert.ToInt32(filter["page"]);
                int pageSize = Convert.ToInt32(filter["PageSize"]);
                long totalCount;
                IQueryable<Comment> totalList;

                if (userDb.IsSystemAccount)
                {
                    totalCount = await commentRepository.GetCountAsync(c => c.ReferenceType == referenceType, cancellationToken);
                    totalList = (await commentRepository.GetListAsync(c => c.ReferenceType == referenceType, cancellationToken)).AsQueryable();
                }
                else
                {
                    totalCount = await commentRepository.GetCountAsync(c => c.ReferenceType == referenceType && c.AssociatedDomainId == userDb.Domains.FirstOrDefault(a => a.IsOwner).DomainId, cancellationToken);
                    totalList = (await commentRepository.GetListAsync(c => c.ReferenceType == referenceType && c.AssociatedDomainId == userDb.Domains.FirstOrDefault(a => a.IsOwner).DomainId, cancellationToken)).AsQueryable();
                }

                if (!string.IsNullOrWhiteSpace(filter["fDate"]))
                {
                    totalList = totalList
                        .Where(c => c.CreationDate >= filter["fDate"].ToEnglishDate().ToUniversalTime());
                }

                if (!string.IsNullOrWhiteSpace(filter["tDate"]))
                {
                    totalList = totalList
                        .Where(c => c.CreationDate <= filter["tDate"].ToEnglishDate().ToUniversalTime());
                }

                if (!string.IsNullOrWhiteSpace(filter["domainId"]))
                {
                    totalList = totalList
                        .Where(c => c.AssociatedDomainId == filter["domainId"]);
                }

                List<CommentViewModel> list = totalList.OrderByDescending(c => c.CreationDate)
                                                       .Skip((page - 1) * pageSize)
                                                       .Take(pageSize)
                                                       .Select(c => new CommentViewModel
                                                       {
                                                           CommentId = c.Id,
                                                           Content = c.Content,
                                                           CreatorUserId = c.CreatorUserId,
                                                           CreationDate = c.CreationDate,
                                                           CreatorUserName = c.CreatorUserName,
                                                           IsApproved = c.IsApproved,
                                                           LikeCount = c.LikeCount,
                                                           DislikeCount = c.DislikeCount,
                                                           ParentCommentId = c.ParentId,
                                                           ReferenceId = c.ReferenceId,
                                                           ReferenceType = c.ReferenceType,
                                                           IsDeleted = c.IsDeleted,
                                                           AssociatedDomainId = c.AssociatedDomainId
                                                       })
                                                       .ToList();

                foreach (CommentViewModel item in list)
                {
                    item.ParentCommentContent = await commentRepository.AnyAsync(c => c.Id == item.ParentCommentId) ? (await commentRepository.FirstOrDefaultAsync(c => c.Id == item.ParentCommentId)).Content : "";

                    switch (item.ReferenceType)
                    {
                        case ReferenceType.Product:
                            {
                                if (await commentRepository.AnyAsync(c => c.Id == item.ReferenceId))
                                {
                                    DataLayer.Entities.Shop.Product.Product productEntity = await productRepository.FirstOrDefaultAsync(p => p.Id == item.ReferenceId);

                                    if (productEntity != null)
                                    {
                                        item.ReferenceTitle = productEntity.MultiLingualProperties.Any(p => p.LanguageId == langId) ? productEntity.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == langId)?.Name : "";
                                    }
                                }

                                break;
                            }

                        case ReferenceType.Content:
                            item.ReferenceTitle = await commentRepository.AnyAsync(c => c.Id == item.ReferenceId) ? (await contentRepository.FirstOrDefaultAsync(c => c.Id == item.ReferenceId)).Title : "";

                            break;
                    }

                    if (item.CreationDate != null)
                    {
                        item.PersianCreationDate = item.CreationDate.Value.ToPersianDdate();
                    }

                    item.DomainName = (await domainRepository.FirstOrDefaultAsync(d => d.Id == item.AssociatedDomainId)).DomainName;
                }

                result.Items = list;
                result.CurrentPage = page;
                result.ItemsCount = totalCount;
                result.PageSize = pageSize;
                result.QueryString = queryString;
            }
            catch (Exception e)
            {
                result.CurrentPage = 1;
                result.Items = [];
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
                logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(CommentController)}/{nameof(List)}");
            }

            ViewBag.IsSysAcc = userDb.IsSystemAccount;

            if (userDb.IsSystemAccount)
            {
                ViewBag.Domains = controllerHelper.GetAllActiveDomains();
            }

            List<SelectListModel> slm = [];
            slm.AddRange(from int i in Enum.GetValues(typeof(ReferenceType)) let name = Enum.GetName(typeof(ReferenceType), i) select new SelectListModel { Text = name, Value = i.ToString() });

            ViewBag.ReferenceTypes = slm;
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(CommentController)}/{nameof(List)}");
        }

        return View(result);
    }


    [Route("/Admin/ProductComments/AddEdit/{id?}")]
    [Route("/Admin/ContentComments/AddEdit/{id?}")]
    public async ValueTask<IActionResult> AddEdit(string id, CancellationToken cancellationToken = default)
    {
        string referenceSource = Request.Path.ToString().Split("/")[1];

        switch (referenceSource)
        {
            case "ProductComments":
                ViewBag.Title = UtilityLanguage.GetString("Menu_ProductComments");
                ViewBag.Name = UtilityLanguage.GetString("Menu_Product");
                ViewBag.lbl = UtilityLanguage.GetString("tbl_ProductName");

                break;

            case "ContentComments":
                ViewBag.Title = UtilityLanguage.GetString("Menu_ContentComments");
                ViewBag.Name = UtilityLanguage.GetString("Menu_Content");
                ViewBag.lbl = UtilityLanguage.GetString("ContentTitle");

                break;
        }

        CommentDto model = new();

        if (!string.IsNullOrEmpty(id))
        {
            Comment result = await commentRepository.GetByIdAsync(id);
            model = mapper.Map<CommentDto>(result);
        }

        List<SelectListModel> selectLists = [];
        selectLists.AddRange(from int i in Enum.GetValues(typeof(ReferenceType)) let name = Enum.GetName(typeof(ReferenceType), i) select new SelectListModel { Text = name, Value = i.ToString() });

        ViewBag.ReferenceTypes = selectLists;

        return View(model);
    }

    [HttpGet]
    [Route("/Admin/ProductComments/Delete/{id?}")]
    [Route("/Admin/ContentComments/Delete/{id?}")]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        Result<Comment> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            Comment comment = await commentRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (comment != null)
            {
                opResult = await commentRepository.DeleteAsync(comment, cancellationToken);

                if (opResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Delete,
                        CollectionType = CollectionType.Comment,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = comment.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},deleting comment with {id} id done successfully");
                    opResult.Message = $"delete the comment by userId={userDb.Id} and userName={userDb.UserName} in date={DateTime.UtcNow.ToPersianDdate()}";
                }
                else
                {
                    opResult.Message = ConstMessages.ErrorInSaving;
                }
            }
            else
            {
                opResult.Message = ConstMessages.ObjectNotFound;
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(CommentController)}/{nameof(Delete)}");
        }

        return Json(opResult.Succeeded
                        ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }

    [Route("/Admin/ProductComments/ApproveComment")]
    [Route("/Admin/ContentComments/ApproveComment")]
    public async ValueTask<IActionResult> ApproveComment(string commentId, bool isApproved = true, CancellationToken cancellationToken = default)
    {
        Comment entity = await commentRepository.FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        Result<Comment> updateResult = new();

        try
        {
            if (entity != null)
            {
                updateResult = await commentRepository.UpdateAsync(c => c.Id == commentId, m => m.IsApproved, isApproved, cancellationToken);

                if (updateResult.Succeeded)
                {
                    updateResult.Succeeded = true;
                    updateResult.Message = ConstMessages.SuccessfullyDone;
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},deleting comment with {commentId} id done successfully");
                }
                else
                {
                    updateResult.Message = ConstMessages.GeneralError;
                }
            }
            else
            {
                updateResult.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(CommentController)}/{nameof(ApproveComment)}");
        }

        return Json(updateResult.Succeeded
                        ? new { Status = "Success", updateResult.Message }
                        : new { Status = "Error", updateResult.Message });
    }

    [HttpPost]
    public async ValueTask<IActionResult> Edit([FromBody] CommentDto dto, CancellationToken cancellationToken)
    {
        Result<Comment> saveResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            if (!ModelState.IsValid)
            {
                List<AjaxValidationErrorModel> errors = [];

                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry modelStateVal = ModelState[modelStateKey];

                    if (modelStateVal != null)
                    {
                        errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                    }
                }

                return Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                Comment comment = await commentRepository.FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);

                if (comment == null)
                {
                    return Json(new { Status = "ModelError" });
                }
                Comment model = mapper.Map(dto, comment);

                saveResult = await commentRepository.UpdateAsync(c => c.Id == model.Id, m => m, model, cancellationToken);

                if (saveResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Update,
                        CollectionType = CollectionType.Comment,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = model.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},editing comment with {dto.Id} id done successfully");
                    saveResult.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    saveResult.Message = ConstMessages.ErrorInSaving;
                }
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(CommentController)}/{nameof(Edit)}");
            saveResult.Message = ConstMessages.ExceptionOccured;
        }

        JsonResult result = Json(saveResult.Succeeded
                                     ? new { Status = "Success", saveResult.Message }
                                     : new { Status = "Error", saveResult.Message });

        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Add([FromBody] CommentDto dto, CancellationToken cancellationToken)
    {
        Result<Comment> saveResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
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

                Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                Comment model = mapper.Map<Comment>(dto);
                model.IsActive = true;
                saveResult = await commentRepository.InsertAsync(model, cancellationToken);

                if (saveResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Insert,
                        CollectionType = CollectionType.Comment,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = model.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},adding comment with {dto.Id} id done successfully");
                    saveResult.ReturnValue = model;
                    saveResult.Succeeded = true;
                    saveResult.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    saveResult.Message = ConstMessages.ErrorInSaving;
                }
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(CommentController)}/{nameof(Add)}");
            saveResult.Message = ConstMessages.ExceptionOccured;
        }

        JsonResult result = Json(saveResult.Succeeded
                                     ? new { Status = "Success", saveResult.Message }
                                     : new { Status = "Error", saveResult.Message });

        return result;
    }

    [HttpGet]
    [Route("/Admin/ProductComments/Restore/{id?}")]
    [Route("/Admin/ContentComments/Restore/{id?}")]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            Comment comment = await commentRepository.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (comment == null)
            {
                result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_EntityNotFound") });
            }
            else
            {
                Result<Comment> res = await commentRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);

                if (res.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Restore,
                        CollectionType = CollectionType.Comment,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = comment.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},restoring comment with {id} id done successfully");
                    res.Message = ConstMessages.SuccessfullyDone;
                    result = new(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully") });
                }
                else
                {
                    res.Message = ConstMessages.ErrorInSaving;
                    result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
                }
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(CommentController)}/{nameof(Restore)}");
            result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
        }

        return result;
    }
}