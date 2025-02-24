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
using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Shared.ProductGroup;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductGroup;
using Arad.Portal.Helpers.Admin;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.ProductGroup;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;

using Serilog;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

using Image = SixLabors.ImageSharp.Image;

namespace Arad.Portal.Areas.Admin.Controllers.Product;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class ProductGroupController : Controller
{
    private readonly CodeGenerator _codeGenerator;
    private readonly IConfiguration _configuration;
    private readonly ControllerHelper _controllerHelper;
    private readonly string _imageSize;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly MinioHelper _minioHelper;
    private readonly IProductGroupRepository _productGroupRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IModificationRepository _modificationRepository;

    public ProductGroupController(IProductGroupRepository productGroupRepository,
                                  CodeGenerator codeGenerator,
                                  IConfiguration configuration,
                                  IWebHostEnvironment webHostEnvironment,
                                  IModificationRepository modificationRepository,
                                  IProductRepository productRepository,
                                  IMapper mapper,
                                  ControllerHelper controllerHelper,
                                  ILogger logger,
                                  MinioHelper minioHelper)
    {
        _productGroupRepository = productGroupRepository;
        _codeGenerator = codeGenerator;
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
        _modificationRepository = modificationRepository;
        _productRepository = productRepository;
        _mapper = mapper;
        _controllerHelper = controllerHelper;
        _logger = logger;
        _minioHelper = minioHelper;
        _imageSize = _configuration["ProductGroupImageSize:Size"]!;
    }

    [HttpGet]
    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        PagedItems<ProductGroupViewModel> result = new();
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);
        string queryString = Request.QueryString.ToString();
        Language lan = _controllerHelper.GetDefaultLanguage();
        ViewBag.LangId = lan.Id;
        ViewBag.IsSystemAccount = userDb.IsSystemAccount;
        ViewBag.Domains = _controllerHelper.GetAllActiveDomains();
        ViewBag.LangList = _controllerHelper.GetAllActiveLanguage();
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

            if (string.IsNullOrWhiteSpace(filter["LanguageId"]))
            {
                filter.Set("LanguageId", lan.Id);
            }

            if (string.IsNullOrWhiteSpace(filter["Name"]))
            {
                filter.Set("Name", "");
            }

            string langId = filter["LanguageId"]!;
            int page = Convert.ToInt32(filter["page"]);
            int pageSize = Convert.ToInt32(filter["PageSize"]);
            string filterName = filter["Name"]!;
            long totalCount;
            string? domainId;

            if (userDb.IsSystemAccount)
            {
                totalCount = await _productGroupRepository.GetCountAsync(c => true, cancellationToken);
            }
            else
            {
                domainId = userDb.Domains.FirstOrDefault(c => c.IsOwner)?.DomainId;
                totalCount = await _productGroupRepository.GetCountAsync(c => c.AssociatedDomainId == domainId, cancellationToken);
            }

            List<ProductGroupViewModel> lst = (await _productGroupRepository.GetAllAsync(cancellationToken)).AsQueryable()
                                                                                                            .OrderByDescending(g => g.CreationDate)
                                                                                                            .Where(g => g.MultiLingualProperties.Any(a => a.Name.Contains(filterName)) &&
                                                                                                                        g.MultiLingualProperties.Any(p => p.LanguageId.ToString() == langId))
                                                                                                            .Skip((page - 1) * pageSize)
                                                                                                            .Take(pageSize)
                                                                                                            .Select(g => new ProductGroupViewModel
                                                                                                                         {
                                                                                                                             ProductGroupId = g.Id,
                                                                                                                             ParentId = g.ParentId,
                                                                                                                             IsDeleted = g.IsDeleted,
                                                                                                                             MultiLingualProperty = g.MultiLingualProperties.Any(a => a.LanguageId.ToString() == langId)
                                                                                                                                                        ? g.MultiLingualProperties.First(a => a.LanguageId.ToString() == langId)
                                                                                                                                                        : g.MultiLingualProperties.First()
                                                                                                                         })
                                                                                                            .ToList();
            result.CurrentPage = page;
            result.Items = lst;
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
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductGroupController)}/{nameof(List)}");
        }

        

        return View(result);
    }

    public async ValueTask<IActionResult> AddEdit(string id, CancellationToken cancellationToken)
    {
        ProductGroup model = new();
        await _controllerHelper.GetCurrentUser(cancellationToken);

        if (!string.IsNullOrWhiteSpace(id))
        {
            model = _controllerHelper.ProductGroupFetch(id);
            CultureInfo current = new("en-US") { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
            Thread.CurrentThread.CurrentCulture = current;

            Domain domain = _controllerHelper.GetCurrentUserDomain();
            
            if (model.GroupImage != null)
            {
                string objectName = $"{domain.Id}/{model.GroupImage.ImageId}/{model.GroupImage.FileName.Replace(':', '-')}";
                (bool success, byte[] imageData) = await _minioHelper.GetObject("productgroupimage", objectName);

                if (success)
                {
                    model.GroupImage.Content = Convert.ToBase64String(imageData);
                }
            }
            
            string staticFileStorageURL = _configuration["LocalStaticFileStorage"] ?? _webHostEnvironment.WebRootPath;

            if (model.GroupImage == null || string.IsNullOrWhiteSpace(model.GroupImage.Content))
            {
                model.GroupImage = new();
            }
        }
        else
        {
            model.GroupCode = _codeGenerator.GetNewId();
        }

        Language defLang = _controllerHelper.GetDefaultLanguage();
        CultureInfo current2 = new(defLang.Symbol) { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
        Thread.CurrentThread.CurrentCulture = current2;
        ViewBag.ProductGroupList = _controllerHelper.GetAllActiveProductGroup(defLang.Id);
        ViewBag.LangId = defLang.Id;
        ViewBag.PicSize = _imageSize;
        ViewBag.LangList = _controllerHelper.GetAllActiveLanguage();
        ViewBag.CurrencyList = _controllerHelper.GetAllActiveCurrency();
        ProductGroupDto dto = _mapper.Map<ProductGroupDto>(model);

        return View(dto);
    }

    [HttpGet]
    public async ValueTask<IActionResult> GetProductGroupList(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);
        List<SelectListModel> lst = _controllerHelper.GetAllActiveProductGroup(id);

        if (lst.Any())
        {
            result = new(new { Status = "success", Data = lst });
            _logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},getting product group list with {id} id done successfully");
        }
        else
        {
            result = new(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString(ConstMessages.ObjectNotFound) });
        }

        return result;
    }

    [HttpGet]
    public async Task<IActionResult> CheckUrlFriendUniqueness(string id, string url, CancellationToken cancellationToken)
    {
        bool res;
        string urlFriend = $"/group/{url}";

        if (string.IsNullOrEmpty(id)) //insert
        {
            res = !await _productGroupRepository.AnyAsync(g => g.MultiLingualProperties.Any(a => a.UrlFriend == urlFriend), cancellationToken);
        }
        else
        {
            //update
            res = !await _productGroupRepository.AnyAsync(c => c.Id == id && c.MultiLingualProperties.Any(multiLingualProperty => multiLingualProperty.UrlFriend == urlFriend), cancellationToken);
        }

        return Json(res
                        ? new { Status = "Success", Message = "url Is unique" }
                        : new { Status = "Error", Message = "url isn't unique" });
    }

    [HttpPost]
    public async ValueTask<IActionResult> Add([FromBody] ProductGroupDto dto, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);

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

            result = Json(new { Status = "ModelError", ModelStateErrors = errors });
        }
        else
        {
            foreach (MultiLingualProperty item in dto.MultiLingualProperties)
            {
                Language lan = _controllerHelper.FetchLanguage(item.LanguageId);
                item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                item.LanguageName = lan.LanguageName;
                item.LanguageSymbol = lan.Symbol;
                Result<Currency> cur = _controllerHelper.FetchCurrency(item.CurrencyId);
                item.CurrencyName = cur.ReturnValue.CurrencyName;
                item.CurrencyPrefix = cur.ReturnValue.Prefix;
                item.CurrencySymbol = cur.ReturnValue.Symbol;
            }

            Language lang = _controllerHelper.GetDefaultLanguage();

            if (!string.IsNullOrWhiteSpace(dto.GroupImage.Content))
            {
                CultureInfo current = new("en-US") { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
                Thread.CurrentThread.CurrentCulture = current;
                dto.GroupImage.Title = dto.MultiLingualProperties.Any(p => p.LanguageId == lang.Id)
                                           ? dto.MultiLingualProperties.FirstOrDefault(c => c.LanguageId == lang.Id)?.Name
                                           : dto.MultiLingualProperties.FirstOrDefault()?.Name;

                bool bucketResult = await _minioHelper.MakeBucket("productgroupimage");

                if (bucketResult)
                {
                    dto.GroupImage.ImageId = Guid.NewGuid().ToString();
                    byte[] bytes = Convert.FromBase64String(dto.GroupImage.Content.Replace("data:image/jpeg;base64,", ""));
                    Image image = Image.Load(bytes);
                    MemoryStream ms = new();
                    await image.SaveAsJpegAsync(ms, cancellationToken);
                    ms.Seek(0, SeekOrigin.Begin);
                    Domain domain = _controllerHelper.GetCurrentUserDomain();
                    string objectName = $"{domain.Id}/{dto.GroupImage.ImageId}/{dto.GroupImage.FileName.Replace(':', '-')}";
                    bool isSave = await _minioHelper.Upload("productgroupimage", objectName, ms, "image/jpeg", ms.Length);

                    if (isSave)
                    {
                        _logger.Information($"image with {dto.GroupImage.ImageId} id in product group with {dto.Id} id saved correctly");
                    }
                    else
                    {
                        _logger.Error($"image not save correctly. stack trace: {nameof(ProductGroupController)}/{nameof(Add)}");
                    }
                }
                else
                {
                    _logger.Error($"something went wrong for making bucket stack trace: {nameof(ProductGroupController)}/{nameof(Add)}");
                }
            }
            else
            {
                dto.GroupImage = null;
            }

            Language defLang = _controllerHelper.GetDefaultLanguage();
            CultureInfo current2 = new(defLang.Symbol) { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
            Thread.CurrentThread.CurrentCulture = current2;
            ProductGroup productGroupModel = _mapper.Map<ProductGroup>(dto);
            productGroupModel.CreationDate = DateTime.Now;
            productGroupModel.AssociatedDomainId = _controllerHelper.GetCurrentUserDomain().Id;
            productGroupModel.CreatorUserId = _controllerHelper.GetCurrentUserId();
            productGroupModel.CreatorUserName = _controllerHelper.GetCurrentUser(cancellationToken).Result.UserName;
            Result<ProductGroup> saveResult = new();

            try
            {
                productGroupModel.Id = Guid.NewGuid().ToString();
                productGroupModel.IsActive = true;
                productGroupModel.GroupImage = dto.GroupImage;
                saveResult = await _productGroupRepository.InsertAsync(productGroupModel, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductGroupController)}/{nameof(Add)}");
                saveResult.Message = ConstMessages.ErrorInSaving;
            }

            if (saveResult.Succeeded)
            {
                Modification modification = new()
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                ActionTypes = ActionTypes.Insert,
                                                CollectionType = CollectionType.ProductGroup,
                                                Ip = _controllerHelper.GetUserIpAddress(),
                                                ModifierId = userDb.Id,
                                                ModifierUserName = userDb.UserName,
                                                ModifyDateTime = DateTime.Now,
                                                RecordId = productGroupModel.Id
                                            };
                Result<Modification> modifyInsert = await _modificationRepository.InsertAsync(modification, cancellationToken);

                _logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},adding product group with {dto.Id} id done successfully");
                saveResult.Message = ConstMessages.SuccessfullyDone;
                await _codeGenerator.SaveToDb(dto.GroupCode, cancellationToken);
            }
            else
            {
                saveResult.Message = ConstMessages.ErrorInSaving;
            }

            result = Json(saveResult.Succeeded
                              ? new { Status = "Success", saveResult.Message }
                              : new { Status = "Error", saveResult.Message });
        }

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            ProductGroup productGroupModel = _controllerHelper.ProductGroupFetch(id);

            if (productGroupModel == null)
            {
                result = new(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_EntityNotFound") });
            }
            else
            {
                Result<ProductGroup> res = await _productGroupRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);

                if (res.Succeeded)
                {
                    Modification modification = new()
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    ActionTypes = ActionTypes.Restore,
                                                    CollectionType = CollectionType.ProductGroup,
                                                    Ip = _controllerHelper.GetUserIpAddress(),
                                                    ModifierId = userDb.Id,
                                                    ModifierUserName = userDb.UserName,
                                                    ModifyDateTime = DateTime.Now,
                                                    RecordId = productGroupModel.Id
                                                };
                    Result<Modification> modifyInsert = await _modificationRepository.InsertAsync(modification, cancellationToken);

                    _logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},restoring product group with {id} id done successfully");
                    res.Message = ConstMessages.SuccessfullyDone;
                    result = new(new { Status = "success", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully") });
                }
                else
                {
                    res.Message = ConstMessages.ErrorInSaving;
                    result = new(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_TryLater") });
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductGroupController)}/{nameof(Restore)}");
            result = new(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_TryLater") });
        }

        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Edit([FromBody] ProductGroupDto dto, CancellationToken cancellationToken)
    {
        Result<ProductGroup> saveResult = new();
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);

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

            CultureInfo current = new("en-US") { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
            Thread.CurrentThread.CurrentCulture = current;

            if (dto.GroupImage != null)
            {
                if (Guid.TryParse(dto.GroupImage.ImageId, out Guid _) && !string.IsNullOrWhiteSpace(dto.GroupImage.Content))
                {
                    byte[] bytes = Convert.FromBase64String(dto.GroupImage.Content.Replace("data:image/jpeg;base64,", ""));
                    Image image = Image.Load(bytes);
                    MemoryStream ms = new();
                    await image.SaveAsJpegAsync(ms, cancellationToken);
                    ms.Seek(0, SeekOrigin.Begin);
                    bool bucketResult = await _minioHelper.MakeBucket("productgroupimage");

                    if (bucketResult)
                    {
                        Domain domain = _controllerHelper.GetCurrentUserDomain();
                        string objectName = $"{domain.Id}/{dto.GroupImage.ImageId}/{dto.GroupImage.FileName.Replace(':', '-')}";
                        await _minioHelper.RemoveObject("productgroupimage", objectName);
                        bool isSave = await _minioHelper.Upload("productgroupimage", objectName, ms, "image/jpeg", ms.Length);

                        if (isSave)
                        {
                            _logger.Information($"image with {dto.GroupImage.ImageId} id in product group with {dto.Id} id saved correctly");
                        }
                        else
                        {
                            _logger.Error($"image not save correctly. stack trace: {nameof(ProductGroupController)}/{nameof(Edit)}");
                        }
                    }
                    else
                    {
                        _logger.Error($"something went wrong in making bucket stack trace: {nameof(ProductGroupController)}/{nameof(Edit)}");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(dto.GroupImage.Url))
                {
                    dto.GroupImage.Url = dto.GroupImage.Url.Replace("/", "\\");
                }
            }

            ProductGroup model = _controllerHelper.ProductGroupFetch(dto.Id);

            if (model == null)
            {
                return RedirectToAction("PageOrItemNotFound", "Account");
            }

            foreach (MultiLingualProperty item in dto.MultiLingualProperties)
            {
                Language lan = _controllerHelper.FetchLanguage(item.LanguageId);
                item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                item.LanguageName = lan.LanguageName;
                item.LanguageSymbol = lan.Symbol;
                Result<Currency> res = _controllerHelper.FetchCurrency(item.CurrencyId);
                item.CurrencyName = res.ReturnValue.CurrencyName;
                item.CurrencyPrefix = res.ReturnValue.Prefix;
                item.CurrencySymbol = res.ReturnValue.Symbol;
            }

            Language defLang = _controllerHelper.GetDefaultLanguage();
            CultureInfo current2 = new(defLang.Symbol) { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
            Thread.CurrentThread.CurrentCulture = current2;
            ProductGroup productGroupModel = _mapper.Map(dto , model);

            saveResult = await _productGroupRepository.UpdateAsync(productGroupModel, cancellationToken);

            if (saveResult.Succeeded)
            {
                Modification modification = new()
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                ActionTypes = ActionTypes.Update,
                                                CollectionType = CollectionType.ProductGroup,
                                                Ip = _controllerHelper.GetUserIpAddress(),
                                                ModifierId = userDb.Id,
                                                ModifierUserName = userDb.UserName,
                                                ModifyDateTime = DateTime.Now,
                                                RecordId = productGroupModel.Id
                                            };
                Result<Modification> modifyInsert = await _modificationRepository.InsertAsync(modification, cancellationToken);
                saveResult.Message = ConstMessages.SuccessfullyDone;
                _logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},editing product group with {dto.Id} id done successfully");
            }
            else
            {
                saveResult.Succeeded = false;
                saveResult.Message = ConstMessages.ErrorInSaving;
                saveResult.Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
            }
        }
        catch (Exception e)
        {
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductGroupController)}/{nameof(Edit)}");
        }

        return Json(saveResult.Succeeded
                        ? new { Status = "Success", saveResult.Message }
                        : new { Status = "Error", saveResult.Message });
    }

    [HttpGet]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        Result<ProductGroup> opResult = new();
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            #region check object dependency
            bool allowDeletion = true;

            //??? this part should be checked in mongo
            bool check = await _productRepository.AnyAsync(c => c.GroupIds.Contains(id) && !c.IsDeleted, cancellationToken);
            bool check2 = await _productGroupRepository.AnyAsync(c => c.ParentId == id && !c.IsDeleted, cancellationToken);

            if (check || check2)
            {
                allowDeletion = false;
            }
            #endregion

            if (allowDeletion)
            {
                ProductGroup productGroupModel = await _productGroupRepository.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

                if (productGroupModel != null)
                {
                    opResult = await _productGroupRepository.UpdateAsync(g => g.Id == id, m => m.IsDeleted, true, cancellationToken);

                    if (opResult.Succeeded)
                    {
                        Modification modification = new()
                                                    {
                                                        Id = Guid.NewGuid().ToString(),
                                                        ActionTypes = ActionTypes.Update,
                                                        CollectionType = CollectionType.ProductGroup,
                                                        Ip = _controllerHelper.GetUserIpAddress(),
                                                        ModifierId = userDb.Id,
                                                        ModifierUserName = userDb.UserName,
                                                        ModifyDateTime = DateTime.Now,
                                                        RecordId = productGroupModel.Id
                                                    };
                        Result<Modification> modifyInsert = await _modificationRepository.InsertAsync(modification, cancellationToken);

                        _logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, deleting product group with {id} id done successfully");
                        opResult.Message = ConstMessages.SuccessfullyDone;
                        opResult.Succeeded = true;
                    }
                    else
                    {
                        opResult.Message = ConstMessages.GeneralError;
                    }
                }
                else
                {
                    opResult.Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
                }
            }
            else
            {
                opResult.Message = ConstMessages.DeletedNotAllowedForDependencies;
            }
        }
        catch (Exception e)
        {
            opResult.Message = ConstMessages.ExceptionOccured;
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductGroupController)} / {nameof(Delete)}");
        }

        return Json(opResult.Succeeded
                        ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }
}