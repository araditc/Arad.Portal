using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Content;
using Arad.Portal.DataLayer.Services;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.Helpers.Admin;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.Content;
using AutoMapper;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
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
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Menu;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Menu;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using Serilog;

using Image = Arad.Portal.DataLayer.Models.Shared.Image;

namespace Arad.Portal.Areas.Admin.Controllers.Content;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class ContentController
(IContentRepository contentRepository,
 IWebHostEnvironment webHostEnvironment,
 LuceneService luceneService,
 IModificationRepository modificationRepository,
 CodeGenerator codeGenerator,
 IConfiguration configuration,
 IMenuRepository menuRepository,
 IMapper mapper,
 ControllerHelper controllerHelper,
 MinioHelper minioHelper,
 Serilog.ILogger logger)
    : Controller
{


    [HttpGet]
    public async ValueTask<IActionResult> CheckUrlFriendUniqueness(string id, string url, CancellationToken cancellationToken)
    {
        string urlFriend = $"/blog/{url}";
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        bool result;
        string domainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;
        if (string.IsNullOrWhiteSpace(id)) //insert
        {
            result = !await contentRepository.AnyAsync(c => c.UrlFriend == urlFriend && c.AssociatedDomainId == domainId, cancellationToken);
        }
        else
        { //update
            result = !await contentRepository.AnyAsync(c => c.Id != id && c.UrlFriend == urlFriend && c.AssociatedDomainId == domainId, cancellationToken);
        }
        return await Task.FromResult<IActionResult>(Json(result ? new { Status = "Success", Message = "url is unique" }
                                                             : new { Status = "Error", Message = "url isn't unique" }));
    }

    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        PagedItems<ContentViewModel> result = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        string queryString = Request.QueryString.ToString();

        try
        {
            NameValueCollection filter = HttpUtility.ParseQueryString(queryString);
            string lanId = controllerHelper.GetCurrentUserDomain().DefaultLanguageId;

            int page = int.TryParse(filter["page"], out int parsedPage) ? parsedPage : 1;
            int pageSize = int.TryParse(filter["PageSize"], out int parsedPageSize) ? parsedPageSize : 20;
            string langId = string.IsNullOrWhiteSpace(filter["LanguageId"]) ? lanId : filter["LanguageId"]!;

            IQueryable<DataLayer.Entities.General.Content.Content> totalList;

            if (userDb.IsSystemAccount)
            {
                totalList = (await contentRepository.GetAllAsync(cancellationToken)).AsQueryable();
            }
            else
            {
                string? domainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;
                totalList = (await contentRepository.GetAllAsync(cancellationToken))
                                .Where(c => c.AssociatedDomainId == domainId).AsQueryable();
            }

            // Apply filtering conditions
            if (!string.IsNullOrWhiteSpace(filter["catId"]))
            {
                totalList = totalList.Where(c => c.ContentCategoryId == filter["catId"]);
            }

            if (!string.IsNullOrWhiteSpace(filter["filter"]))
            {
                string searchFilter = filter["filter"]!;
                totalList = totalList.Where(c => c.TagKeywords.Contains(searchFilter) || c.Contents.Contains(searchFilter));
            }

            totalList = totalList.Where(c => c.LanguageId == langId);
            int totalCount = totalList.Any() ? totalList.Count() : 0;

            List<ContentViewModel> list = totalList.OrderByDescending(c => c.CreationDate)
                                                   .Skip((page - 1) * pageSize)
                                                   .Take(pageSize)
                                                   .Select(c => new ContentViewModel()
                                                   {
                                                       Id = c.Id,
                                                       SeoDescription = c.SeoDescription,
                                                       LanguageName = c.LanguageName,
                                                       LanguageId = c.LanguageId,
                                                       Images = c.Images,
                                                       EndShowDate = c.EndShowDate,
                                                       ContentCategoryId = c.ContentCategoryId,
                                                       ContentCategoryName = c.ContentCategoryName,
                                                       ContentProviderName = c.ContentProviderName,
                                                       Description = c.Description,
                                                       SeoTitle = c.SeoTitle,
                                                       StartShowDate = c.StartShowDate,
                                                       SourceType = c.SourceType,
                                                       SubTitle = c.SubTitle,
                                                       TagKeywords = c.TagKeywords,
                                                       Title = c.Title,
                                                       UrlFriend = c.UrlFriend,
                                                       VisitCount = c.VisitCount,
                                                       IsDeleted = c.IsDeleted
                                                   }).ToList();

            // Assign to result object
            result.Items = list;
            result.CurrentPage = page;
            result.ItemsCount = totalCount;
            result.PageSize = pageSize;
            result.QueryString = queryString;

            // Set ViewBag Data
            ViewBag.DefLangId = lanId;
            ViewBag.LangList = controllerHelper.GetAllActiveLanguage();
            ViewBag.IsSystemAccount = userDb.IsSystemAccount;
            if (userDb.IsSystemAccount)
            {
                ViewBag.Domains = controllerHelper.GetAllActiveDomains();
            }

            ViewBag.CatList = controllerHelper.AllActiveContentCategory(lanId, "").ToList();
        }
        catch (Exception e)
        {
            result = new PagedItems<ContentViewModel>
            {
                CurrentPage = 1,
                Items = [],
                ItemsCount = 0,
                PageSize = 10,
                QueryString = queryString
            };
            Log.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. StackTrace: {e.StackTrace}");
        }

        return View(result);
    }


    public async ValueTask<IActionResult> AddEdit(CancellationToken cancellationToken, string id = "")
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        string staticFileStorageUrl = configuration["LocalStaticFileStorage"] ?? string.Empty;
        DataLayer.Entities.General.Language.Language lan = controllerHelper.GetDefaultLanguage();
        string fileShown = configuration["LocalStaticFileShown"] ?? string.Empty;
        ViewBag.Url = fileShown;
        ContentDto model = new();

        if (userDb.IsSystemAccount)
        {
            ViewBag.Domains = controllerHelper.GetAllActiveDomains();
        }
        Domain domain = controllerHelper.GetCurrentUserDomain();

        model.AssociatedDomainId = domain.Id;

        ViewBag.IsSysAcc = userDb.IsSystemAccount;

        if (!string.IsNullOrWhiteSpace(id))
        {
            DataLayer.Entities.General.Content.Content content = await controllerHelper.ContentFetch(id, false, cancellationToken);
            model = mapper.Map<ContentDto>(content);
            model.PersianStartShowDate = content.StartShowDate.ToPersianDdate();
            model.PersianEndShowDate = content.EndShowDate.ToPersianDdate();

            foreach (Image item in model.Images)
            {
                CultureInfo current = new("en-US")
                {
                    DateTimeFormat = new()
                    {
                        Calendar = new GregorianCalendar()
                    }
                };
                Thread.CurrentThread.CurrentCulture = current;

                string objectName = $"{domain.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
                (bool success, byte[] imageData) = await minioHelper.GetObject("contentimage", objectName);
                if (success)
                {
                    item.Content = Convert.ToBase64String(imageData);
                }
            }
            DataLayer.Entities.General.Language.Language defLang = controllerHelper.GetDefaultLanguage();
            CultureInfo current2 = new(defLang.Symbol)
            {
                DateTimeFormat = new()
                {
                    Calendar = new GregorianCalendar()
                }
            };
            Thread.CurrentThread.CurrentCulture = current2;

            if (string.IsNullOrWhiteSpace(staticFileStorageUrl))
            {
                staticFileStorageUrl = webHostEnvironment.WebRootPath;
            }

        }
        else
        {

            model.ContentCode = codeGenerator.GetNewId(cancellationToken);
        }

        ViewBag.ProductCode = codeGenerator.GetNewId(cancellationToken);
        ViewBag.ProductType = controllerHelper.GetAllProductType();
        ViewBag.DownloadOptions = controllerHelper.GetAllDownloadLimitationType();
        List<SelectListModel> groupList = controllerHelper.GetAllActiveProductGroup(lan.Id);
        ViewBag.ProductGroupList = groupList;
        ViewBag.baseHref = fileShown;
        List<SelectListModel> unitList = await controllerHelper.GetAllActiveProductUnit(lan.Id, "");
        unitList.Insert(0, new() { Text = UtilityLanguage.GetString("AlertAndMessage_Choose"), Value = "" });
        ViewBag.ProductUnitList = unitList;
        List<SelectListModel> currencyList = controllerHelper.GetAllActiveCurrency();
        ViewBag.CurrencyList = currencyList;

        ViewBag.StaticFileStorage = staticFileStorageUrl;

        List<SelectListModel> categoryList = controllerHelper.AllActiveContentCategory(lan.Id, "");
        categoryList = categoryList.OrderBy(l => l.Text).ToList();

        ViewBag.CatList = categoryList;

        List<SelectListModel> imageRatioList = controllerHelper.GetAllImageRatio();
        ViewBag.ImageRatio = imageRatioList;

        ViewBag.LangId = lan.Id;
        ViewBag.LangList = controllerHelper.GetAllActiveLanguage();
        ViewBag.ImageTemplate = controllerHelper.GetAllImageTemplate();

        ViewBag.AllSourceType = controllerHelper.GetAllSourceType();

        ViewBag.PicSize = configuration["ContentImageSize:Size"]!;

        return View(model);
    }

    [HttpGet]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        Result<DataLayer.Entities.General.Content.Content> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            DataLayer.Entities.General.Content.Content entity = await contentRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            opResult = await contentRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, true, cancellationToken);
            Modification modification = new()
            {
                Id = Guid.NewGuid().ToString(),
                ActionTypes = ActionTypes.Delete,
                CollectionType = CollectionType.Content,
                Ip = controllerHelper.GetUserIpAddress(),
                ModifierId = userDb.Id,
                ModifierUserName = userDb.UserName,
                ModifyDateTime = DateTime.Now,
                RecordId = entity.Id
            };
            Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

            opResult.Succeeded = true;
            Menu menu = await menuRepository.FirstOrDefaultAsync(c => c.SubName == entity.Title, cancellationToken);
            if (menu != null)
            {
                Result<Menu> menuOpResult = await menuRepository.UpdateAsync(c => c.Id == menu.Id, m => m.IsDeleted, true, cancellationToken);
                if (menuOpResult.Succeeded)
                {
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},deleting menu with {menu.Id} id done successfully");
                }
            }
            logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},deleting content with {id} id done successfully");
            string domainId = userDb.Domains.FirstOrDefault(d => d.IsOwner).DomainId;
            #region delete lucene index
            string mainPath = Path.Combine(configuration["LocalStaticFileStorage"], "LuceneIndexes", domainId, "Content");
            luceneService.DeleteItemFromExistingIndex(mainPath, id);
            #endregion
        }
        catch (Exception e)
        {
            opResult.Succeeded = false;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ContentController)}/{nameof(Delete)}");
        }
        return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }

    [HttpPost]
    public async ValueTask<IActionResult> Add([FromBody] ContentDto dto, CancellationToken cancellationToken)
    {
        Result<DataLayer.Entities.General.Content.Content> saveResult = new();
        Result luceneResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            List<AjaxValidationErrorModel> errors = [];

            if (!ModelState.IsValid)
            {
                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry modelStateVal = ModelState[modelStateKey];

                    if (modelStateVal != null)
                    {
                        errors.AddRange(modelStateVal.Errors
                                                     .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                    }
                }
                return Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {

                DataLayer.Entities.General.Content.Content model = mapper.Map<DataLayer.Entities.General.Content.Content>(dto);
                bool isCkEditorContentValid = HtmlSanitizer.SanitizeHtml(model.Contents);
                if (!isCkEditorContentValid)
                {
                    errors.Add(new() { Key = "Contents", ErrorMessage = UtilityLanguage.GetString("AlertAndMessage_InvalidEditorContent") });
                    return Json(new { Status = "ModelError", ModelStateErrors = errors });
                }
                else
                {
                    string replace = model.ContentCategoryName.Replace(" ", "-").ToLower();
                    model.UrlFriend = $"/{replace}/{model.UrlFriend}";

                    //var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                    //var path = "images/Contents";
                    foreach (Image item in model.Images)
                    {
                        CultureInfo current = new("en-US")
                        {
                            DateTimeFormat = new()
                            {
                                Calendar = new GregorianCalendar()
                            }
                        };
                        Thread.CurrentThread.CurrentCulture = current;
                        MemoryStream ms = new();
                        bool bucketResult = await minioHelper.MakeBucket("contentimage");
                        if (bucketResult)
                        {
                            byte[] bytes;
                            string imageFormat = "";
                            if (item.Content.Contains("data:image/png;base64,"))
                            {
                                imageFormat = "image/png";
                                bytes = Convert.FromBase64String(item.Content.Replace("data:image/png;base64,", ""));
                                item.FileName = "ContentImage.png";
                                SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(bytes);
                                await image.SaveAsPngAsync(ms, cancellationToken);
                            }
                            else
                            {
                                imageFormat = "image/jpeg";
                                bytes = Convert.FromBase64String(item.Content.Replace("data:image/jpeg;base64,", ""));
                                item.FileName = "ContentImage.jpg";
                                SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(bytes);
                                await image.SaveAsJpegAsync(ms, cancellationToken);
                            }

                            ms.Seek(0, SeekOrigin.Begin);

                            Domain domain = controllerHelper.GetCurrentUserDomain();
                            string objectName = "";

                            if (!string.IsNullOrEmpty(item.ImageId))
                            {
                                objectName = $"{domain.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
                            }

                            else
                            {
                                item.ImageId = Guid.NewGuid().ToString();
                                objectName = $"{domain.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
                            }
                            bool isSave = await minioHelper.Upload("contentimage", objectName, ms, imageFormat, ms.Length);
                            if (isSave)
                            {
                                logger.Information($"image with {item.ImageId} id in content with {dto.Id} id saved correctly");
                            }
                            else
                            {
                                logger.Error($"image not save correctly. stack trace: {nameof(ContentController)}/{nameof(Add)}");
                            }
                        }
                        else
                        {
                            logger.Error($"something is wrong in making bucket stack trace: {nameof(ContentController)}/{nameof(Add)}");
                        }
                    }
                    DataLayer.Entities.General.Language.Language defLang = controllerHelper.GetDefaultLanguage();
                    CultureInfo current2 = new(defLang.Symbol)
                    {
                        DateTimeFormat = new()
                        {
                            Calendar = new GregorianCalendar()
                        }
                    };
                    Thread.CurrentThread.CurrentCulture = current2;
                    model.Id = Guid.NewGuid().ToString();
                    model.CreatorUserId = userDb.Id;
                    model.CreatorUserName = userDb.UserName;
                    model.IsActive = true;
                    model.IsDeleted = false;
                    model.CreationDate = DateTime.Now;
                    saveResult = await contentRepository.InsertAsync(model, cancellationToken);
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},adding content with {model.Id} id done successfully");
                    saveResult.Succeeded = true;
                    saveResult.Message = ConstMessages.SuccessfullyDone;
                    string domainId = "";
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Insert,
                        CollectionType = CollectionType.Content,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = model.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    domainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;
                    if (saveResult.Succeeded)
                    {
                        #region add to LuceneIndex 

                        string mainPath = Path.Combine(configuration["LocalStaticFileStorage"], "LuceneIndexes", domainId, "Content");
                        if (!System.IO.Directory.Exists(mainPath))
                        {
                            System.IO.Directory.CreateDirectory(mainPath);
                        }
                        bool isExist = DirectoryReader.IndexExists(FSDirectory.Open(mainPath));
                        if (!isExist)
                        {

                            List<DataLayer.Entities.General.Content.Content> contentList = await contentRepository.GetListAsync(c => c.AssociatedDomainId == domainId, cancellationToken);
                            luceneService.BuildContentIndexesPerLanguage(contentList, mainPath);
                            luceneResult.Succeeded = true;
                        }
                        else
                        {
                            LuceneSearchIndexModel obj = new()
                            {
                                Id = "",
                                EntityName = dto.Title,
                                GroupIds = [dto.ContentCategoryId],
                                Code = dto.ContentCode.ToString(),
                                GroupNames = [dto.ContentCategoryName],
                                TagKeywordList = dto.TagKeywords
                            };
                            luceneResult = luceneService.AddItemToExistingIndex(mainPath, obj, false);
                        }


                        #endregion
                        await codeGenerator.SaveToDb(dto.ContentCode, cancellationToken);
                    }

                }
            }

        }
        catch (Exception e)
        {
            saveResult.Succeeded = false;
            saveResult.Message = ConstMessages.ErrorInSaving;
            luceneResult.Succeeded = false;
            luceneResult.Message = ConstMessages.ErrorInSaving;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ContentController)}/{nameof(Add)}");
        }
        JsonResult result = Json(saveResult.Succeeded && luceneResult.Succeeded ? new { Status = "Success", saveResult.Message }
                                     : new { Status = "Error", saveResult.Message });
        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Edit([FromBody] ContentDto dto, CancellationToken cancellationToken)
    {
        JsonResult result;
        Result luceneRes = new();
        Result saveResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            List<AjaxValidationErrorModel> errors = [];
            if (!ModelState.IsValid)
            {
                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry modelStateVal = ModelState[modelStateKey];

                    if (modelStateVal != null)
                    {
                        errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                    }
                }
                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                DataLayer.Entities.General.Content.Content model = mapper.Map<DataLayer.Entities.General.Content.Content>(dto);
                string domainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;

                if (domainId == null)
                {
                    return Json(new { Status = "Error", Message = "Domain not found" });
                }

                bool isCkEditorContentValid = HtmlSanitizer.SanitizeHtml(model.Contents);
                if (!isCkEditorContentValid)
                {
                    errors.Add(new() { Key = "Contents", ErrorMessage = UtilityLanguage.GetString("AlertAndMessage_InvalidEditorContent") });
                    result = Json(new { Status = "ModelError", ModelStateErrors = errors });
                }
                else
                {
                    string[] arr = dto.UrlFriend.Split(' ');
                    string replace = dto.ContentCategoryName.Replace(" ", "-").ToLower();
                    if (!dto.UrlFriend.Contains(replace))
                    {
                        dto.UrlFriend = $"/{replace}/" + string.Join("-", arr);
                    }

                    foreach (Image item in model.Images)
                    {
                        CultureInfo current = new("en-US")
                        {
                            DateTimeFormat = new()
                            {
                                Calendar = new GregorianCalendar()
                            }
                        };
                        Thread.CurrentThread.CurrentCulture = current;
                        MemoryStream ms = new();
                        bool bucketResult = await minioHelper.MakeBucket("contentimage");
                        if (bucketResult)
                        {
                            byte[] bytes;
                            string imageFormat = "";
                            if (item.Content.Contains("data:image/png;base64,"))
                            {
                                imageFormat = "image/png";
                                bytes = Convert.FromBase64String(item.Content.Replace("data:image/png;base64,", ""));
                                item.FileName = "ContentImage.png";
                                SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(bytes);
                                await image.SaveAsPngAsync(ms, cancellationToken);
                            }
                            else
                            {
                                imageFormat = "image/jpeg";
                                bytes = Convert.FromBase64String(item.Content.Replace("data:image/jpeg;base64,", ""));
                                item.FileName = "ContentImage.jpg";
                                SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(bytes);
                                await image.SaveAsJpegAsync(ms, cancellationToken);
                            }

                            ms.Seek(0, SeekOrigin.Begin);

                            Domain domain = controllerHelper.GetCurrentUserDomain();
                            string objectName = "";

                            if (!string.IsNullOrEmpty(item.ImageId) && item.ImageId != "undefined")
                            {
                                objectName = $"{domain.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
                                bool isDelete = await minioHelper.RemoveObject("contentimage", objectName);

                                if (isDelete)
                                {
                                    logger.Information($"image with {item.ImageId} id in content with {dto.Id} id removed correctly");
                                }
                                else
                                {
                                    logger.Error($"image not removed correctly. stack trace: {nameof(ContentController)}/{nameof(Edit)}");
                                }
                            }

                            else
                            {
                                item.ImageId = Guid.NewGuid().ToString();
                                objectName = $"{domain.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
                            }
                            bool isSave = await minioHelper.Upload("contentimage", objectName, ms, imageFormat, ms.Length);
                            if (isSave)
                            {
                                logger.Information($"image with {item.ImageId} id in content with {dto.Id} id saved correctly");
                            }
                            else
                            {
                                logger.Error($"image not save correctly. stack trace: {nameof(ContentController)}/{nameof(Edit)}");
                            }
                        }
                        else
                        {
                            logger.Error($"something is wrong in making bucket stack trace: {nameof(ContentController)}/{nameof(Edit)}");
                        }
                    }

                    DataLayer.Entities.General.Language.Language defLang = controllerHelper.GetDefaultLanguage();
                    CultureInfo current2 = new(defLang.Symbol)
                    {
                        DateTimeFormat = new()
                        {
                            Calendar = new GregorianCalendar()
                        }
                    };
                    Thread.CurrentThread.CurrentCulture = current2;

                    saveResult = await controllerHelper.UpdateContent(dto, cancellationToken);
                    if (!saveResult.Succeeded && saveResult.Message == ConstMessages.NotRecordChange)
                    {
                        saveResult.Succeeded = true;
                    }
                    if (saveResult.Succeeded)
                    {
                        Modification modification = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ActionTypes = ActionTypes.Update,
                            CollectionType = CollectionType.Content,
                            Ip = controllerHelper.GetUserIpAddress(),
                            ModifierId = userDb.Id,
                            ModifierUserName = userDb.UserName,
                            ModifyDateTime = DateTime.Now,
                            RecordId = dto.Id
                        };
                        Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                        Domain domain = controllerHelper.FetchDomain(domainId).ReturnValue;

                        logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, editing content with {model.Id} id done successfully");

                        string mainPath = Path.Combine(configuration["LocalStaticFileStorage"], "LuceneIndexes", domainId, "Content");
                        LuceneSearchIndexModel obj = new()
                        {
                            Id = dto.Id,
                            EntityName = dto.Title,
                            GroupIds = [dto.ContentCategoryId],
                            Code = dto.ContentCode.ToString(),
                            GroupNames = [dto.ContentCategoryName],
                            TagKeywordList = dto.TagKeywords
                        };
                        luceneRes = luceneService.UpdateItemInIndex(mainPath, dto.Id, obj, false);
                    }
                }
            }
        }
        catch (Exception e)
        {
            saveResult.Succeeded = false;
            saveResult.Message = ConstMessages.ErrorInSaving;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName}, error {e.Message} occurred. Stack trace: {nameof(ContentController)}/{nameof(Edit)}", e);
        }

        result = Json(saveResult.Succeeded && luceneRes.Succeeded ? new { Status = "Success", saveResult.Message }
                          : new { Status = "Error", saveResult.Message });
        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            string domainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;
            Result<DataLayer.Entities.General.Content.Content> updateResult = await contentRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);
            if (updateResult.Succeeded)
            {
                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Restore,
                    CollectionType = CollectionType.Content,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},restoring content with {id} id done successfully");
                #region add to LuceneIndex
                DataLayer.Entities.General.Content.Content content = await contentRepository.GetByIdAsync(id, cancellationToken);

                string mainPath = Path.Combine(configuration["LocalStaticFileStorage"], "LuceneIndexes", domainId, "Content");
                LuceneSearchIndexModel obj = new()
                {
                    Id = id,
                    EntityName = content.Title,
                    GroupIds = [content.ContentCategoryId],
                    Code = content.ContentCode.ToString(),
                    GroupNames = [content.ContentCategoryName],
                    TagKeywordList = content.TagKeywords
                };
                luceneService.AddItemToExistingIndex(mainPath, obj, false);
                #endregion

                updateResult.Succeeded = true;
                updateResult.Message = ConstMessages.SuccessfullyDone;
                result = new(new
                {
                    Status = "success",
                    Message = UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully")
                });
            }
            else
            {
                updateResult.Succeeded = false;
                updateResult.Message = ConstMessages.ErrorInSaving;
                result = new(new
                {
                    Status = "error",
                    Message = UtilityLanguage.GetString("AlertAndMessage_TryLater")
                });
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ContentController)}/{nameof(Restore)}");
            result = new(new
            {
                Status = "error",
                Message = UtilityLanguage.GetString("AlertAndMessage_TryLater")
            });
        }
        return result;
    }
}