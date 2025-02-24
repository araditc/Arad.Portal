using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.ContentCategory;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Content;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.ContentCategory;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.Helpers.Admin;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.ContentCategory;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;

using Serilog;

using SixLabors.ImageSharp;

using Image = Arad.Portal.DataLayer.Models.Shared.Image;

namespace Arad.Portal.Areas.Admin.Controllers.Content;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class ContentCategoryController(
    IContentCategoryRepository contentCategoryRepository,
    CodeGenerator codeGenerator,
    IConfiguration configuration,
    IContentRepository contentRepository,
    IModificationRepository modificationRepository,
    IMapper mapper,
    MinioHelper minioHelper,
    ControllerHelper controllerHelper,
    ILogger logger)
    : Controller
{
    [HttpGet]
    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        PagedItems<ContentCategoryViewModel> result = new();

        try
        {
            NameValueCollection filter = HttpUtility.ParseQueryString(Request.QueryString.ToString());
            Language lan = controllerHelper.GetDefaultLanguage();

            if (string.IsNullOrWhiteSpace(filter["page"]))
            {
                filter.Set("page", "1");
            }

            if (string.IsNullOrWhiteSpace(filter["PageSize"]))
            {
                filter.Set("PageSize", "20");
            }

            if (string.IsNullOrWhiteSpace(filter["LanguageId"]))
            {
                filter.Set("LanguageId", lan.Id);
            }

            int page = Convert.ToInt32(filter["page"]);
            int pageSize = Convert.ToInt32(filter["PageSize"]);
            string langId = filter["LanguageId"]!;
            string filterKey = "";

            if (!string.IsNullOrWhiteSpace(filter["filter"]))
            {
                filterKey = filter["filter"]!;
            }

            long totalCount;
            string? domainId = "";

            if (userDb.IsSystemAccount)
            {
                totalCount = await contentCategoryRepository.GetCountAsync(c => true, cancellationToken);
            }
            else
            {
                domainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;
                totalCount = await contentCategoryRepository.GetCountAsync(c => c.AssociatedDomainId == domainId, cancellationToken);
            }

            List<ContentCategoryViewModel> list =
                (await contentCategoryRepository.GetListAsync(c => (domainId == "" || c.AssociatedDomainId == domainId) && (filterKey == "" || c.CategoryNames.Any(a => a.Name.Contains(filterKey))), cancellationToken))
                .OrderByDescending(c => c.CreationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ContentCategoryViewModel
                             {
                                 ContentCategoryId = c.Id,
                                 ParentCategoryId = c.ParentCategoryId,
                                 CategoryType = c.CategoryType,
                                 IsDeleted = c.IsDeleted,
                                 CategoryName = c.CategoryNames.Any(a => a.LanguageId == langId) ? c.CategoryNames.First(a => a.LanguageId == langId) : c.CategoryNames.First()
                             })
                .ToList();

            result.CurrentPage = page;
            result.Items = list;
            result.ItemsCount = totalCount;
            result.PageSize = pageSize;
            result.QueryString = Request.QueryString.ToString();

            ViewBag.DefLangId = controllerHelper.GetDefaultLanguage().Id;
            ViewBag.LangList = controllerHelper.GetAllActiveLanguage();
            ViewBag.IsSystemAccount = userDb.IsSystemAccount;
            ViewBag.Domains = controllerHelper.GetAllActiveDomains();
        }
        catch (Exception ex)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {ex.Message} occured. stack trace: {nameof(ContentCategoryController)}/{nameof(List)}");
        }

        return View(result);
    }

    [HttpGet]
    public async ValueTask<IActionResult> CheckUrlFriendUniqueness(string id, string url, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return Json(new { Status = "Error", Message = "URL cannot be empty" });
        }

        string urlFriend = $"/category/{url}".ToLower();
        bool isUniqueUrlFriend;

        if (string.IsNullOrEmpty(id)) // insert
        {
            isUniqueUrlFriend = !await contentCategoryRepository.AnyAsync(c =>
                                                                              c.CategoryNames.Any(a => a.UrlFriend.ToLower() == urlFriend),
                                                                          cancellationToken);
        }
        else // update
        {
            isUniqueUrlFriend = !await contentCategoryRepository.AnyAsync(c =>
                                                                              c.Id != id && c.CategoryNames.Any(a => a.UrlFriend.ToLower() == urlFriend),
                                                                          cancellationToken);
        }

        return Json(new { Status = isUniqueUrlFriend ? "Success" : "Error", Message = isUniqueUrlFriend ? "URL is unique" : "URL isn't unique" });
    }

    public async ValueTask<IActionResult> AddEdit(string id, CancellationToken cancellationToken)
    {
        ContentCategory model = new();

        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        if (userDb.IsSystemAccount)
        {
            ViewBag.Domains = controllerHelper.GetAllActiveDomains();
        }

        if (!string.IsNullOrEmpty(id))
        {
            model = await controllerHelper.ContentCategoryFetch(id, true);

            model.ContentPhotosSlider ??= [];
        }
        else
        {
            model.ContentPhotosSlider = [];
            model.CategoryCode = codeGenerator.GetNewId(cancellationToken);
        }

        Domain domain = controllerHelper.GetCurrentUserDomain();
        model.AssociatedDomainId = domain.Id;

        foreach (Image item in model.ContentPhotosSlider)
        {
            CultureInfo current = new("en-US") { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
            Thread.CurrentThread.CurrentCulture = current;

            string objectName = $"{domain.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
            (bool success, byte[] imageData) = await minioHelper.GetObject("contentcategoryimage", objectName);

            if (success)
            {
                item.Content = Convert.ToBase64String(imageData);
            }
        }

        ViewBag.IsSysAcc = userDb.IsSystemAccount;

        Language lan = controllerHelper.GetDefaultLanguage();
        ViewBag.LangId = lan.Id;

        List<SelectListModel> categoryList = controllerHelper.AllActiveContentCategory(lan.Id, model.AssociatedDomainId);
        ViewBag.CategoryList = categoryList;
        List<SelectListModel> lst = [];
        lst.AddRange(from int i in Enum.GetValues(typeof(ContentCategoryType)) let name = Enum.GetName(typeof(ContentCategoryType), i) select new SelectListModel { Text = name, Value = i.ToString() });

        lst.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });

        ViewBag.CategoryTypes = lst;
        ViewBag.PicSize = configuration["ContentImageSize:Size"]!;

        ViewBag.ImageRatio = controllerHelper.GetAllImageRatio(); ;

        ViewBag.ImageTemplate = controllerHelper.GetAllImageTemplate();

        ViewBag.LangList = controllerHelper.GetAllActiveLanguage();
        ContentCategoryDto contentCategoryDto = mapper.Map<ContentCategoryDto>(model);

        return View(contentCategoryDto);
    }

    [HttpGet]
    public IActionResult GetContentCategoryList(string lid, string did)
    {
        JsonResult result;
        List<SelectListModel> lst = [];

        if (!string.IsNullOrEmpty(did))
        {
            lst = controllerHelper.AllActiveContentCategory(lid, did);
        }

        if (lst.Count > 0)
        {
            result = new(new { Status = "success", Data = lst.OrderBy(m => m.Text) });
        }
        else
        {
            result = new(new { Status = "error", Message = UtilityLanguage.GetString(ConstMessages.ObjectNotFound) });
        }

        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Add([FromBody] ContentCategoryDto dto, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        if (!ModelState.IsValid)
        {
            List<AjaxValidationErrorModel> errors = [];

            foreach (string modelStateKey in ModelState.Keys)
            {
                ModelStateEntry? modelStateVal = ModelState[modelStateKey];

                if (modelStateVal != null)
                {
                    errors.AddRange(modelStateVal.Errors
                                                 .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
            }

            result = Json(new { Status = "ModelError", ModelStateErrors = errors });
        }
        else
        {
            foreach (MultiLingualProperty item in dto.CategoryNames)
            {
                Language lan = controllerHelper.FetchLanguage(item.LanguageId);
                item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                item.LanguageName = lan.LanguageName;
                item.LanguageSymbol = lan.Symbol;
                item.UrlFriend = item.UrlFriend.ToLower();
            }

            foreach (Image item in dto.ContentPhotosSlider)
            {
                CultureInfo current = new("en-US") { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
                Thread.CurrentThread.CurrentCulture = current;
                MemoryStream ms = new();
                bool bucketResult = await minioHelper.MakeBucket("contentcategoryimage");

                if (bucketResult)
                {
                    byte[] bytes;
                    string imageFormat;

                    if (item.Content.Contains("data:image/png;base64,"))
                    {
                        imageFormat = "image/png";
                        bytes = Convert.FromBase64String(item.Content.Replace("data:image/png;base64,", ""));
                        item.FileName = "RandomContentImage.png";
                        SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(bytes);
                        await image.SaveAsPngAsync(ms, cancellationToken);
                    }
                    else
                    {
                        imageFormat = "image/jpeg";
                        bytes = Convert.FromBase64String(item.Content.Replace("data:image/jpeg;base64,", ""));
                        item.FileName = "RandomContentImage.jpg";
                        SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(bytes);
                        await image.SaveAsJpegAsync(ms, cancellationToken);
                    }

                    ms.Seek(0, SeekOrigin.Begin);

                    Domain domain = controllerHelper.GetCurrentUserDomain();
                    string objectName;

                    if (!string.IsNullOrEmpty(item.ImageId))
                    {
                        objectName = $"{domain.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
                    }

                    else
                    {
                        item.ImageId = Guid.NewGuid().ToString();
                        objectName = $"{domain.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
                    }

                    bool isSave = await minioHelper.Upload("contentcategoryimage", objectName, ms, imageFormat, ms.Length);

                    if (isSave)
                    {
                        logger.Information($"image with {item.ImageId} id in content category with {dto.Id} id saved correctly");
                    }
                    else
                    {
                        logger.Error($"image not save correctly. stack trace: {nameof(ContentCategoryController)}/{nameof(Add)}");
                    }
                }
                else
                {
                    logger.Error($"something is wrong in making bucket stack trace: {nameof(ContentCategoryController)}/{nameof(Add)}");
                }
            }

            Result<ContentCategory> opResult = new();

            try
            {
                ContentCategory equivalentEntity = mapper.Map<ContentCategory>(dto);
                equivalentEntity.Id = Guid.NewGuid().ToString();

                equivalentEntity.CreationDate = DateTime.Now;
                equivalentEntity.CreatorUserId = userDb.Id;
                equivalentEntity.CreatorUserName = userDb.UserName;

                equivalentEntity.IsActive = true;
                opResult = await contentCategoryRepository.InsertAsync(equivalentEntity, cancellationToken);

                if (opResult.Succeeded)
                {
                    opResult.Succeeded = true;
                    opResult.Message = ConstMessages.SuccessfullyDone;
                    await codeGenerator.SaveToDb(dto.CategoryCode, cancellationToken);
                    logger.Information($"user with {userDb.Id}, {userDb.UserName} data could create content category with {equivalentEntity.Id} id successfully.");

                    Modification modification = new()
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    ActionTypes = ActionTypes.Insert,
                                                    CollectionType = CollectionType.ContentCategory,
                                                    Ip = controllerHelper.GetUserIpAddress(),
                                                    ModifierId = userDb.Id,
                                                    ModifierUserName = userDb.UserName,
                                                    ModifyDateTime = DateTime.Now,
                                                    RecordId = equivalentEntity.Id
                                                };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
                }
                else
                {
                    opResult.Message = ConstMessages.ErrorInSaving;
                }
            }
            catch (Exception e)
            {
                logger.Error($"user with {userDb.Id}, {userDb.UserName} data got error {e.Message}. stack trace: {nameof(ContentCategoryController)}/{nameof(Add)}");
                opResult.Succeeded = false;
                opResult.Message = ConstMessages.ExceptionOccured;
            }

            result = Json(opResult.Succeeded
                              ? new { Status = "Success", opResult.Message }
                              : new { Status = "Error", opResult.Message });
        }

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            Task<ContentCategory> dto = controllerHelper.ContentCategoryFetch(id, true);
            {
                Result<ContentCategory> res = await contentCategoryRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);

                if (res.Succeeded)
                {
                    Modification modification = new()
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    ActionTypes = ActionTypes.Restore,
                                                    CollectionType = CollectionType.ContentCategory,
                                                    Ip = controllerHelper.GetUserIpAddress(),
                                                    ModifierId = userDb.Id,
                                                    ModifierUserName = userDb.UserName,
                                                    ModifyDateTime = DateTime.Now,
                                                    RecordId = dto.Result.Id
                                                };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    res.Message = ConstMessages.SuccessfullyDone;
                    result = new(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully") });
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, restoring content category with {id} id done successfully.");
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
            result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ContentCategoryController)}/{nameof(Restore)}");
        }

        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Edit([FromBody] ContentCategoryDto dto, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        Result<ContentCategory> saveResult = new();

        try
        {
            if (dto == null)
            {
                return BadRequest("Invalid data received.");
            }

            if (!ModelState.IsValid)
            {
                List<AjaxValidationErrorModel> errors = [];

                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry? modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal?.Errors.Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }) ?? Array.Empty<AjaxValidationErrorModel>());
                }

                result = Json(new { Status = "ModelError", ModelStateErrors = errors });

                return result;
            }

            ContentCategory contentCategory = await controllerHelper.ContentCategoryFetch(dto.Id, true);

            foreach (MultiLingualProperty category in dto.CategoryNames)
            {
                string[] arr = category.UrlFriend.Split(' ');
                string urlFriend = string.Join("-", arr);
                category.UrlFriend = urlFriend;
            }

            foreach (MultiLingualProperty item in dto.CategoryNames)
            {
                Language lan = controllerHelper.FetchLanguage(item.LanguageId);
                item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                item.LanguageName = lan.LanguageName;
                item.LanguageSymbol = lan.Symbol;
                item.UrlFriend = item.UrlFriend.ToLower();
            }

            foreach (Image item in dto.ContentPhotosSlider)
            {
                CultureInfo current = new("en-US") { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
                Thread.CurrentThread.CurrentCulture = current;
                MemoryStream ms = new();
                bool bucketResult = await minioHelper.MakeBucket("contentcategoryimage");

                if (bucketResult)
                {
                    byte[] bytes;
                    string imageFormat;

                    if (item.Content.Contains("data:image/png;base64,"))
                    {
                        imageFormat = "image/png";
                        bytes = Convert.FromBase64String(item.Content.Replace("data:image/png;base64,", ""));
                        item.FileName = "RandomContentImage.png";
                        SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(bytes);
                        await image.SaveAsPngAsync(ms, cancellationToken);
                    }
                    else
                    {
                        imageFormat = "image/jpeg";
                        bytes = Convert.FromBase64String(item.Content.Replace("data:image/jpeg;base64,", ""));
                        item.FileName = "RandomContentImage.jpg";
                        SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(bytes);
                        await image.SaveAsJpegAsync(ms, cancellationToken);
                    }

                    ms.Seek(0, SeekOrigin.Begin);

                    Domain domain = controllerHelper.GetCurrentUserDomain();
                    string objectName;

                    if (!string.IsNullOrEmpty(item.ImageId) && item.ImageId != "undefined")
                    {
                        objectName = $"{domain.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
                        bool isDelete = await minioHelper.RemoveObject("contentcategoryimage", objectName);

                        if (isDelete)
                        {
                            logger.Information($"image with {item.ImageId} id in content category with {dto.Id} id removed correctly");
                        }
                        else
                        {
                            logger.Error($"image not removed correctly. stack trace: {nameof(ContentCategoryController)}/{nameof(Edit)}");
                        }
                    }

                    else
                    {
                        item.ImageId = Guid.NewGuid().ToString();
                        objectName = $"{domain.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
                    }

                    bool isSave = await minioHelper.Upload("contentcategoryimage", objectName, ms, imageFormat, ms.Length);

                    if (isSave)
                    {
                        logger.Information($"image with {item.ImageId} id in content category with {dto.Id} id saved correctly");
                        item.Content = "";
                    }
                    else
                    {
                        logger.Error($"image not save correctly. stack trace: {nameof(ContentCategoryController)}/{nameof(Edit)}");
                    }
                }
                else
                {
                    logger.Error($"something is wrong in making bucket stack trace: {nameof(ContentCategoryController)}/{nameof(Edit)}");
                }
            }

            ContentCategory contentCategoryModel = mapper.Map(dto, contentCategory);

            //if (dto.IsDeleted)
            //{
            //    contentCategoryModel.IsDeleted = true;
            //}
            //else
            //{
            //    contentCategoryModel.CategoryNames = dto.CategoryNames;
            //}
            //contentCategoryModel.CreationDate = contentCategory.CreationDate;
            //contentCategoryModel.CreatorUserId = contentCategory.CreatorUserId;
            //contentCategoryModel.CreatorUserName = contentCategory.CreatorUserName;
            //contentCategoryModel.IsActive = contentCategory.IsActive;
            //contentCategoryModel.IsDeleted = contentCategory.IsDeleted;

            saveResult = await contentCategoryRepository.UpdateAsync(contentCategoryModel, cancellationToken);

            if (saveResult.Succeeded)
            {
                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, editing content category with {contentCategoryModel.Id} id done successfully.");
                saveResult.Message = ConstMessages.SuccessfullyDone;
                Modification modification = new()
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                ActionTypes = ActionTypes.Update,
                                                CollectionType = CollectionType.ContentCategory,
                                                Ip = controllerHelper.GetUserIpAddress(),
                                                ModifierId = userDb.Id,
                                                ModifierUserName = userDb.UserName,
                                                ModifyDateTime = DateTime.Now,
                                                RecordId = contentCategoryModel.Id
                                            };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
            }
            else
            {
                saveResult.Message = ConstMessages.ErrorInSaving;
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ContentCategoryController)}/{nameof(Edit)}");
        }

        result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message } : new { Status = "Error", saveResult.Message });

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        Result<ContentCategory> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            #region check object dependency
            bool allowDeletion = !await contentRepository.AnyAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
            #endregion

            if (allowDeletion)
            {
                ContentCategory entity = await contentCategoryRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                if (entity != null)
                {
                    opResult = await contentCategoryRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, true, cancellationToken);

                    if (opResult.Succeeded)
                    {
                        logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, deleting content category with {id} id done successfully.");
                        opResult.Message = ConstMessages.SuccessfullyDone;
                        Modification modification = new()
                                                    {
                                                        Id = Guid.NewGuid().ToString(),
                                                        ActionTypes = ActionTypes.Delete,
                                                        CollectionType = CollectionType.ContentCategory,
                                                        Ip = controllerHelper.GetUserIpAddress(),
                                                        ModifierId = userDb.Id,
                                                        ModifierUserName = userDb.UserName,
                                                        ModifyDateTime = DateTime.Now,
                                                        RecordId = entity.Id
                                                    };
                        Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
                    }
                    else
                    {
                        opResult.Message = ConstMessages.GeneralError;
                    }
                }
                else
                {
                    opResult.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
                }
            }
            else
            {
                opResult.Message = UtilityLanguage.GetString("AlertAndMessage_DeletedNotAllowedForDependencies");
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ContentCategoryController)}/{nameof(Delete)}");
        }

        return Json(opResult.Succeeded
                        ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }
}