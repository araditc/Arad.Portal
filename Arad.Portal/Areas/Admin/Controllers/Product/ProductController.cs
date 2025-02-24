using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.MessageTemplate;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Entities.General.Notify;
using Arad.Portal.DataLayer.Entities.General.SMS;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Entities.Shop.ProductGroup;
using Arad.Portal.DataLayer.Entities.Shop.ProductUnit;
using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Shared.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.MessageTemplate;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Notification;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductUnit;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Promotion;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Transaction;
using Arad.Portal.DataLayer.Services;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.Helpers.Admin;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.Product;

using AutoMapper;

using Lucene.Net.Index;
using Lucene.Net.Store;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;

using SixLabors.ImageSharp;

using static Arad.Portal.DataLayer.Models.Shared.Enums;

using Directory = System.IO.Directory;
using ILogger = Serilog.ILogger;
using Image = Arad.Portal.DataLayer.Models.Shared.Image;
using Language = Arad.Portal.DataLayer.Entities.General.Language.Language;

namespace Arad.Portal.Areas.Admin.Controllers.Product;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class ProductController : Controller
{
    private readonly CodeGenerator _codeGenerator;
    private readonly IConfiguration _configuration;
    private readonly ControllerHelper _controllerHelper;
    private readonly IDomainRepository _domainRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _imageSize;
    private readonly ILanguageRepository _lanRepository;
    private readonly IModificationRepository _modificationRepository;
    private readonly ILogger _logger;
    private readonly LuceneService _luceneService;
    private readonly IMapper _mapper;
    private readonly IMessageTemplateRepository _messageTemplateRepository;
    private readonly MinioHelper _minioHelper;
    private readonly INotificationRepository _notificationRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProductUnitRepository _productUnitRepository;
    private readonly IPromotionRepository _promotionRepository;
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRepository _userRepository;

    public ProductController(UserManager<ApplicationUser> userManager,
                             CodeGenerator codeGenerator,
                             IProductRepository productRepository,
                             IUserRepository userRepository,
                             ILanguageRepository languageRepository,
                             IModificationRepository modificationRepository,
                             IPromotionRepository promotionRepository,
                             LuceneService luceneService,
                             IDomainRepository domainRepository,
                             IHttpContextAccessor accessor,
                             IConfiguration configuration,
                             IMapper mapper,
                             IShoppingCartRepository shoppingCartRepository,
                             IProductUnitRepository productUnitRepository,
                             ITransactionRepository transactionRepository,
                             IMessageTemplateRepository messageTemplateRepository,
                             INotificationRepository notificationRepository,
                             ILogger logger,
                             MinioHelper minioHelper,
                             ControllerHelper controllerHelper)
    {
        _productRepository = productRepository;
        _configuration = configuration;
        _mapper = mapper;
        _shoppingCartRepository = shoppingCartRepository;
        _productUnitRepository = productUnitRepository;
        _transactionRepository = transactionRepository;
        _messageTemplateRepository = messageTemplateRepository;
        _notificationRepository = notificationRepository;
        _logger = logger;
        _minioHelper = minioHelper;
        _controllerHelper = controllerHelper;
        _lanRepository = languageRepository;
        _modificationRepository = modificationRepository;
        _httpContextAccessor = accessor;
        _userManager = userManager;
        _luceneService = luceneService;
        _promotionRepository = promotionRepository;
        _codeGenerator = codeGenerator;
        _domainRepository = domainRepository;
        _userRepository = userRepository;
        _imageSize = _configuration["ProductImageSize:Size"];
    }

    [HttpGet]
    public async ValueTask<IActionResult> List()
    {
        PagedItems<ProductViewModel> result = new();
        string queryString = Request.QueryString.ToString();
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser();

        try
        {
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

                int page = Convert.ToInt32(filter["page"]);
                int pageSize = Convert.ToInt32(filter["PageSize"]);

                // long totalCount = await _context.ProductCollection.Find(c => true).CountDocumentsAsync();
                IQueryable<DataLayer.Entities.Shop.Product.Product> totalList;
                string? domainId;

                if (userDb.IsSystemAccount)
                {
                    totalList = (await _productRepository.GetAllAsync()).AsQueryable();
                }
                else
                {
                    domainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;
                    totalList = (await _productRepository.GetAllAsync()).AsQueryable().Where(p => p.AssociatedDomainId == domainId);
                }

                if (!string.IsNullOrWhiteSpace(filter["groupIds"]))
                {
                    totalList = totalList.Where(p => p.GroupIds.ToString()!.Contains(filter["groupIds"]!));
                }

                if (!string.IsNullOrWhiteSpace(filter["name"]))
                {
                    totalList = totalList
                        .Where(p => p.MultiLingualProperties.Any(a => a.Name.Contains(filter["name"]!)));
                }

                if (!string.IsNullOrWhiteSpace(filter["code"]))
                {
                    totalList = totalList
                        .Where(p => p.UniqueCode.Equals(filter["code"]));
                }

                if (!string.IsNullOrWhiteSpace(filter["desc"]))
                {
                    totalList = totalList
                        .Where(p => p.MultiLingualProperties.Any(a => a.Description.Contains(filter["desc"]!)));
                }

                if (!string.IsNullOrWhiteSpace(filter["from"]))
                {
                    totalList = totalList
                        .Where(p => p.CreationDate >= filter["from"].ToEnglishDate().ToUniversalTime());
                }

                if (!string.IsNullOrWhiteSpace(filter["to"]))
                {
                    totalList = totalList
                        .Where(p => p.CreationDate <= filter["to"].ToEnglishDate().ToUniversalTime());
                }

                if (!string.IsNullOrWhiteSpace(filter["inventory"]))
                {
                    totalList = totalList
                        .Where(p => p.Inventory.Sum(d => d.Count) <= int.Parse(filter["inventory"]!));
                }

                if (!string.IsNullOrWhiteSpace(filter["promotion"]) && Convert.ToBoolean(filter["promotion"]))
                {
                    totalList = totalList
                        .Where(p => p.Promotion != null &&
                                    p.Promotion.SDate <= DateTime.UtcNow &&
                                    (p.Promotion.EDate == null || p.Promotion.EDate <= DateTime.UtcNow));
                }

                if (!string.IsNullOrWhiteSpace(filter["exist"]) && Convert.ToBoolean(filter["exist"]))
                {
                    totalList = totalList
                        .Where(p => p.Inventory.Sum(d => d.Count) > 0);
                }

                if (string.IsNullOrWhiteSpace(filter["LanguageId"]))
                {
                    Language lan = await _lanRepository.FirstOrDefaultAsync(l => l.IsDefault);
                    filter.Set("LanguageId", lan.Id);
                }

                List<DataLayer.Entities.Shop.Product.Product> list = totalList.OrderByDescending(p => p.CreationDate)
                                                                              .Skip((page - 1) * pageSize)
                                                                              .Take(pageSize)
                                                                              .ToList();

                List<ProductViewModel> productList = [];

                foreach (DataLayer.Entities.Shop.Product.Product product in list)
                {
                    foreach (Image image in product.Images)
                    {
                        CultureInfo current = new("en-US") { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
                        Thread.CurrentThread.CurrentCulture = current;

                        Domain domain = _controllerHelper.GetCurrentUserDomain();

                        if (image.FileName != null)
                        {
                            string objectName = $"{domain.Id}/{image.ImageId}/{image.FileName.Replace(':', '-')}";
                            (bool success, byte[] imageData) = await _minioHelper.GetObject("productimage", objectName);

                            if (success)
                            {
                                image.Content = Convert.ToBase64String(imageData);
                            }
                        }
                    }

                    ProductViewModel productViewModel = new()
                    {
                        ProductId = product.Id,
                        GroupNames = product.GroupNames,
                        GroupIds = product.GroupIds,
                        Inventory = product.Inventory,
                        UniqueCode = product.UniqueCode,
                        ProductCode = product.ProductCode,
                        IsDeleted = product.IsDeleted,
                        MultiLingualProperties = product.MultiLingualProperties,
                        Images = product.Images,
                        Prices = product.Prices,
                        Unit = product.Unit,
                        CreationDate = product.CreationDate
                    };

                    productList.Add(productViewModel);
                }

                Language defLang = _controllerHelper.GetDefaultLanguage();
                CultureInfo current2 = new(defLang.Symbol) { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
                Thread.CurrentThread.CurrentCulture = current2;
                result.Items = productList;
                result.CurrentPage = page;
                result.ItemsCount = totalList.Count();
                result.PageSize = pageSize;
                result.QueryString = queryString;
            }
            catch (Exception e)
            {
                _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductController)}/{nameof(List)}");
                result.CurrentPage = 1;
                result.Items = [];
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }

            Language languageEntity = _controllerHelper.GetDefaultLanguage();
            ViewBag.DefLangId = languageEntity.Id;
            string langSymbol = CultureInfo.CurrentCulture.Name != null ? CultureInfo.CurrentCulture.Name.ToLower() : languageEntity.Symbol.ToLower();

            List<SelectListModel> languages = _controllerHelper.GetAllActiveLanguage();
            languages.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });
            ViewBag.LangList = languages;

            string staticFileStorageURL = _configuration["LocalStaticFileStorage"]!;
            ViewBag.Path = staticFileStorageURL;

            ViewBag.ProductGroupList = _controllerHelper.GetAllActiveProductGroup(languageEntity.Id);
            Domain defaultDomain = _controllerHelper.GetCurrentUserDomain();
            ViewBag.ShopUrl = defaultDomain.DomainName + "/" + langSymbol;

            ViewBag.ProductUnitList = _controllerHelper.GetAllActiveProductUnit(languageEntity.Id, "");

            ViewBag.IsSystemAccount = userDb.IsSystemAccount;
            ViewBag.Domains = _controllerHelper.GetAllActiveDomains();
        }
        catch (Exception e)
        {
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductController)}/{nameof(List)}");
        }

        return View(result);
    }

    public async ValueTask<IActionResult> AddEdit(string id, CancellationToken cancellationToken)
    {
        ProductOutputDto model = new();
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);

        if (userDb.IsSystemAccount)
        {
            List<ApplicationUser> vendorList = await _userRepository.GetListAsync(c => c.IsVendor == true, cancellationToken);
            ViewBag.Vendors = vendorList.ToList().Select(u => new SelectListModel { Text = u.Profile.FullName, Value = u.Id.ToString() });

            ViewBag.Domains = _controllerHelper.GetAllActiveDomains();
        }
        else
        {
            ViewBag.Vendors = "-1";
        }

        Domain domain = _controllerHelper.GetCurrentUserDomain();
        model.AssociatedDomainId = domain.Id;
        ViewBag.IsSysAcc = userDb.IsSystemAccount;
        ViewBag.ActivePromotionId = "-1";

        string fileShown = _configuration["LocalStaticFileShown"];
        ViewBag.Url = fileShown;

        ViewBag.ProductType = _controllerHelper.GetAllProductType();

        ViewBag.DownloadOptions = _controllerHelper.GetAllDownloadLimitationType();

        ViewBag.ImageRatio = _controllerHelper.GetAllImageRatio();

        if (!string.IsNullOrWhiteSpace(id))
        {
            model = await _controllerHelper.ProductFetch(id);

            foreach (Image item in model.Images)
            {
                CultureInfo current = new("en-US") { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
                Thread.CurrentThread.CurrentCulture = current;

                string objectName = $"{domain.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
                (bool success, byte[] imageData) = await _minioHelper.GetObject("productimage", objectName);

                if (success)
                {
                    item.Content = Convert.ToBase64String(imageData);
                }
            }

            Language defLang = _controllerHelper.GetDefaultLanguage();
            CultureInfo current2 = new(defLang.Symbol) { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
            Thread.CurrentThread.CurrentCulture = current2;
            int cnt = 1;

            foreach (MultiLingualProperty item in model.MultiLingualProperties)
            {
                item.Tag = cnt.ToString();
                cnt += 1;
            }

            bool result = false;
            DataLayer.Entities.Shop.Product.Product productEntity = await _productRepository.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (productEntity is { Promotion: not null })
            {
                if (productEntity.Promotion.PromotionType == PromotionType.Product &&
                    productEntity.Promotion.IsActive &&
                    productEntity.Promotion.SDate <= DateTime.Now &&
                    (productEntity.Promotion.EDate >= DateTime.Now || productEntity.Promotion == null))
                {
                    result = true;
                }
            }

            if (result)
            {
                ViewBag.ActivePromotionId = model.Promotion.Id;
            }
        }
        else
        {
            model.ProductCode = _codeGenerator.GetNewId(cancellationToken);
        }

        Language lan = _controllerHelper.GetDefaultLanguage();
        List<SelectListModel> specGroupList = await _controllerHelper.AllActiveSpecificationGroup(lan.Id, "", cancellationToken);
        specGroupList.Insert(0, new() { Text = UtilityLanguage.GetString("AlertAndMessage_Choose"), Value = "" });
        ViewBag.SpecificationGroupList = specGroupList;

        List<SelectListModel> groupList = _controllerHelper.GetAllActiveProductGroup(lan.Id);
        ViewBag.ProductGroupList = groupList;

        List<SelectListModel> currencyList = _controllerHelper.GetAllActiveCurrency();
        ViewBag.CurrencyList = currencyList;
        ViewBag.DefCurrency = _controllerHelper.GetDefaultCurrency().ReturnValue.Id;

        ViewBag.LangId = lan.Id;
        ViewBag.LangList = _controllerHelper.GetAllActiveLanguage();

        List<SelectListModel> unitList = await _controllerHelper.GetAllActiveProductUnit(lan.Id, "");
        unitList.Insert(0, new() { Text = UtilityLanguage.GetString("AlertAndMessage_Choose"), Value = "" });
        ViewBag.ProductUnitList = unitList;

        List<Promotion> alltypes = await _promotionRepository.GetListAsync(p => p.CreatorUserId == userDb.Id &&
                                                                                p.SDate <= DateTime.UtcNow &&
                                                                                (p.EDate >= DateTime.UtcNow || p.EDate == null) &&
                                                                                p.IsActive &&
                                                                                !p.AsUserCoupon,
                                                                           cancellationToken);
        List<SelectListModel> res = alltypes.ToList().Where(p => p.PromotionType == PromotionType.Product).Select(p => new SelectListModel { Text = p.Title, Value = p.Id.ToString() }).ToList();



        ViewBag.PromotionList = res;
        ViewBag.ImageTemplate = _controllerHelper.GetAllImageTemplate();
        ViewBag.PicSize = _imageSize;

        return View(model);
    }

    [HttpGet]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        Result<DataLayer.Entities.Shop.Product.Product> updateResult = new();
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            DataLayer.Entities.Shop.Product.Product entity = await _productRepository.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (entity == null)
            {
                entity.IsPublishedOnMainDomain = false;
            }

            await _productRepository.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            {
                bool check = await _shoppingCartRepository.AnyAsync(c => c.Details.Any(a => a.Products.Any(b => b.Id == id)) && !c.IsDeleted && c.IsActive, cancellationToken);
                check &= await _transactionRepository.AnyAsync(t => t.SubInvoices.Any(a => a.PurchasePerSeller.Products.Any(b => b.Id == id)), cancellationToken);

                bool allowDeletion = !check;

                if (allowDeletion)
                {
                    #region Add modification
                    //var mod = GetCurrentModification(modificationReason);
                    //entity.Modifications.Add(mod);
                    #endregion

                    updateResult = await _productRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, true, cancellationToken);

                    if (updateResult.Succeeded)
                    {
                        _logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},deleting content with {id} id done successfully");
                        updateResult.Succeeded = true;
                        updateResult.Message = ConstMessages.SuccessfullyDone;

                        Modification modification = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ActionTypes = ActionTypes.Delete,
                            CollectionType = CollectionType.Product,
                            Ip = _controllerHelper.GetUserIpAddress(),
                            ModifierId = userDb.Id,
                            ModifierUserName = userDb.UserName,
                            ModifyDateTime = DateTime.Now,
                            RecordId = entity.Id
                        };
                        Result<Modification> modifyInsert = await _modificationRepository.InsertAsync(modification, cancellationToken);
                    }
                    else
                    {
                        updateResult.Message = ConstMessages.GeneralError;
                    }
                }
                else
                {
                    updateResult.Message = ConstMessages.DeletedNotAllowedForDependencies;
                }
            }

            string domainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;

            #region delete related luceneIndexes
            List<string> lst = _configuration.GetSection("SupportedCultures").Get<string[]>().ToList();

            foreach (string cul in lst)
            {
                if (domainId != null)
                {
                    string mainPath = Path.Combine(_configuration["LocalStaticFileStorage"] ?? string.Empty, "LuceneIndexes", domainId, "Product", cul.Trim());

                    _luceneService.DeleteItemFromExistingIndex(mainPath, id);
                }

                if (!entity.IsPublishedOnMainDomain)
                {
                    continue;
                }

                string mainDomainId = _controllerHelper.GetCurrentUserDomain().Id;
                string mainDomainPath = Path.Combine(_configuration["LocalStaticFileStorage"] ?? string.Empty, "LuceneIndexes", mainDomainId, "Product", cul.Trim());
                _luceneService.DeleteItemFromExistingIndex(mainDomainPath, id);
            }
            #endregion
        }
        catch (Exception e)
        {
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductController)}/{nameof(Delete)}");
        }

        return Json(updateResult.Succeeded
                        ? new { Status = "Success", updateResult.Message }
                        : new { Status = "Error", updateResult.Message });
    }

    [HttpGet]
    public async ValueTask<IActionResult> CheckUrlFriendUniqueness(string id, string url, CancellationToken cancellationToken)
    {
        bool res;
        string urlFriend = $"/product/{url}";

        if (string.IsNullOrWhiteSpace(id)) //insert
        {
            res = !await _productRepository.AnyAsync(p => p.MultiLingualProperties.Any(a => a.UrlFriend == urlFriend), cancellationToken);
        }
        else
        {
            //update
            res = !await _productRepository.AnyAsync(c => c.Id != id && c.MultiLingualProperties.Any(multiLingualProperty => multiLingualProperty.UrlFriend == urlFriend), cancellationToken);
        }

        return Json(res
                        ? new { Status = "Success", Message = "url is unique" }
                        : new { Status = "Error", Message = "url isn't unique" });
    }

    [HttpPost]
    public async ValueTask<IActionResult> Add([FromQuery] bool isFromContent, [FromBody] ProductInputDto dto, CancellationToken cancellationToken)
    {
        JsonResult result = null;
        List<AjaxValidationErrorModel> errors = [];
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            if (!ModelState.IsValid)
            {
                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors
                                                 .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }

                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                bool res;

                if (string.IsNullOrEmpty(dto.Id))
                {
                    res = !await _productRepository.AnyAsync(p => p.UniqueCode == dto.UniqueCode, cancellationToken);
                }
                else
                {
                    res = !await _productRepository.AnyAsync(c => c.Id == dto.Id && c.UniqueCode == dto.UniqueCode, cancellationToken);
                }

                if (res)
                {
                    foreach (MultiLingualProperty item in dto.MultiLingualProperties)
                    {
                        Language lan = _controllerHelper.FetchLanguage(item.LanguageId);
                        item.LanguageSymbol = lan.Symbol;
                        item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                        item.UrlFriend = $"{item.UrlFriend}";
                        item.ProductGroupNames = [];

                        foreach (string name in from grp in dto.GroupIds select _controllerHelper.ProductGroupFetch(grp) into groupDto where groupDto.MultiLingualProperties.Any(p => p.LanguageId == item.LanguageId) select groupDto.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == item.LanguageId)?.Name)
                        {
                            item.ProductGroupNames.Add(name);
                        }
                    }

                    foreach (InventoryDetail item in dto.Inventory)
                    {
                        item.SpecValuesId = Guid.NewGuid().ToString();
                    }

                    foreach (PriceDto item in dto.Prices)
                    {
                        Result<Currency> cur = _controllerHelper.FetchCurrency(item.CurrencyId);

                        item.PriceId = Guid.NewGuid().ToString();
                        item.Symbol = cur.ReturnValue.Symbol;

                        if (isFromContent)
                        {
                            item.SDate = DateTime.UtcNow;
                        }
                        else
                        {
                            item.SDate = CultureInfo.CurrentCulture.Name.ToLower() == "fa-ir" ? item.StartDate.Split(" ")[0].ToEnglishDate() : DateTime.Parse(item.StartDate);
                            item.EDate = !string.IsNullOrWhiteSpace(item.EndDate) ? CultureInfo.CurrentCulture.Name.ToLower() == "fa-ir" ? item.EndDate.Split(" ")[0].ToEnglishDate() : DateTime.Parse(item.EndDate) : null;
                        }

                        item.Prefix = cur.ReturnValue.Symbol;
                        item.IsActive = true;
                    }

                    dto.SellerUserId = userDb.Id;

                    if (dto.SellerUserId != null)
                    {
                           dto.SellerUserName = userDb?.UserName;
                    }

                    if (dto.ProductType == ProductType.File)
                    {
                        CultureInfo currentCulture = new("en-US") { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
                        Thread.CurrentThread.CurrentCulture = currentCulture;

                        bool bucketResult = await _minioHelper.MakeBucket("product");

                        if (bucketResult)
                        {
                            if (dto.ProductFileContent != null)
                            {
                                int index = dto.ProductFileContent.IndexOf(",", StringComparison.Ordinal);

                                if (index >= 0)
                                {
                                    string base64Data = dto.ProductFileContent[(index + 1)..];
                                    byte[] fileBytes = Convert.FromBase64String(base64Data);
                                    using MemoryStream stream = new(fileBytes);

                                    dto.Id = Guid.NewGuid().ToString();
                                    Domain domain = _controllerHelper.GetCurrentUserDomain();
                                    string objectName = $"{domain.Id}/{dto.Id}/{dto.ProductFileName?.Replace(':', '-')}";

                                    bool isSave = await _minioHelper.Upload("product", objectName, stream, "application/zip", stream.Length);

                                    if (isSave)
                                    {
                                        _logger.Information($"Product with ID {dto.Id} saved correctly to the bucket.");
                                    }
                                    else
                                    {
                                        _logger.Error($"Failed to save product to the bucket. Stack trace: {nameof(ProductController)}/{nameof(Add)}");
                                    }
                                }
                                else
                                {
                                    _logger.Error("Invalid ProductFileContent format. Missing comma separator.");
                                }
                            }
                        }
                        else
                        {
                            _logger.Error($"Failed to create bucket. Stack trace: {nameof(ProductController)}/{nameof(Add)}");
                        }
                    }

                    DataLayer.Entities.Shop.Product.Product equivalentModel = _mapper.Map<DataLayer.Entities.Shop.Product.Product>(dto);

                    if (string.IsNullOrWhiteSpace(dto.Id)) //insert Case
                    {
                        equivalentModel.Id = Guid.NewGuid().ToString();
                    }

                    #region MultiLingualProperties
                    if (dto.MultiLingualProperties.Count > 0)
                    {
                        foreach (MultiLingualProperty item in equivalentModel.MultiLingualProperties.Where(item => string.IsNullOrWhiteSpace(item.MultiLingualPropertyId)))
                        {
                            item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                        }
                    }
                    #endregion

                    #region ProductUnit
                    if (!string.IsNullOrWhiteSpace(dto.UnitId))
                    {
                        ProductUnit unitEntity = await _productUnitRepository.FirstOrDefaultAsync(u => u.Id == dto.UnitId, cancellationToken);

                        if (unitEntity != null)
                        {
                            equivalentModel.Unit = unitEntity;
                        }
                    }
                    #endregion

                    #region Prices
                    equivalentModel.Prices = [];

                    foreach (PriceDto price in dto.Prices.OrderBy(p => p.StartDate))
                    {
                        if (price.IsActive && string.IsNullOrWhiteSpace(price.EndDate)) //price is valid from client
                        {
                            if (equivalentModel.Prices.Any(x => x.CurrencyId == price.CurrencyId && x.EndDate != null && x.IsActive))
                            {
                                Price? exist = equivalentModel.Prices.FirstOrDefault(x => x.CurrencyId == price.CurrencyId && x.EndDate != null && x.IsActive);

                                //equivalentModel.Prices.Remove(exist);
                                if (exist != null)
                                {
                                    exist.IsActive = false;
                                    exist.EndDate = DateTime.Now;
                                }

                                //equivalentModel.Prices.Add(exist);
                            }
                        }

                        if (price.SDate != null)
                        {
                            Price p = new()
                                      {
                                          PriceId = !string.IsNullOrWhiteSpace(price.PriceId) ? price.PriceId : Guid.NewGuid().ToString(),
                                          CurrencyId = price.CurrencyId,
                                          CurrencyName = price.CurrencyName,
                                          IsActive = string.IsNullOrWhiteSpace(price.PriceId) || price.IsActive,
                                          Prefix = price.Prefix,
                                          PriceValue = price.PriceValue,
                                          StartDate = price.SDate.Value,
                                          EndDate = price.EDate
                                      };
                            equivalentModel.Prices.Add(p);
                        }
                    }
                    #endregion

                    #region Promotion
                    if (!string.IsNullOrWhiteSpace(dto.PromotionId))
                    {
                        Promotion promotionEntity =
                            await _promotionRepository.FirstOrDefaultAsync(p => p.Id == dto.PromotionId, cancellationToken);

                        if (promotionEntity != null)
                        {
                            equivalentModel.Promotion = promotionEntity;
                        }
                    }
                    #endregion Promotion

                    #region images

                    foreach (Image item in dto.Pictures)
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
                        bool bucketResult = await _minioHelper.MakeBucket("productimage");
                        if (bucketResult)
                        {
                            byte[] bytes;
                            string imageFormat = "";
                            if (item.Content.Contains("data:image/png;base64,"))
                            {
                                imageFormat = "image/png";
                                bytes = Convert.FromBase64String(item.Content.Replace("data:image/png;base64,", ""));
                                item.FileName = "ProductImage.png";
                                SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(bytes);
                                await image.SaveAsPngAsync(ms, cancellationToken);
                            }
                            else
                            {
                                imageFormat = "image/jpeg";
                                bytes = Convert.FromBase64String(item.Content.Replace("data:image/jpeg;base64,", ""));
                                item.FileName = "ProductImage.jpg";
                                SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(bytes);
                                await image.SaveAsJpegAsync(ms, cancellationToken);
                            }

                            ms.Seek(0, SeekOrigin.Begin);

                            Domain domain = _controllerHelper.GetCurrentUserDomain();
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
                            bool isSave = await _minioHelper.Upload("productimage", objectName, ms, imageFormat, ms.Length);
                            if (isSave)
                            {
                                _logger.Information($"image with {item.ImageId} id in product with {dto.Id} id saved correctly");
                            }
                            else
                            {
                                _logger.Error($"image not save correctly. stack trace: {nameof(ProductController)}/{nameof(Add)}");
                            }
                        }
                        else
                        {
                            _logger.Error($"something is wrong in making bucket stack trace: {nameof(ProductController)}/{nameof(Add)}");
                        }
                    }

                    Language defLang = _controllerHelper.GetDefaultLanguage();
                    CultureInfo current2 = new(defLang.Symbol) { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
                    Thread.CurrentThread.CurrentCulture = current2;
                    equivalentModel.Images = dto.Pictures;
                    #endregion

                    equivalentModel.CreationDate = DateTime.Now;
                    equivalentModel.CreatorUserId = _controllerHelper.GetCurrentUserId();
                    equivalentModel.CreatorUserName = _controllerHelper.GetCurrentUser(cancellationToken).Result.UserName;
                    Result<DataLayer.Entities.Shop.Product.Product> saveResult = await _productRepository.InsertAsync(equivalentModel, cancellationToken);

                    if (saveResult.Succeeded == false)
                    {
                        foreach (Image item in dto.Pictures.Where(item => System.IO.File.Exists(item.Url)))
                        {
                            System.IO.File.Delete(item.Url);
                        }

                        saveResult.Message = ConstMessages.InternalServerErrorMessage;
                    }

                    Result luceneResult = new();

                    if (!saveResult.Succeeded)
                    {
                        return result = Json(saveResult.Succeeded && luceneResult.Succeeded
                                                 ? new { Status = "Success", saveResult.Message }
                                                 : new { Status = "Error", saveResult.Message });
                    }

                    {
                        saveResult.Message = ConstMessages.SuccessfullyDone;

                        Modification modification = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ActionTypes = ActionTypes.Insert,
                            CollectionType = CollectionType.Product,
                            Ip = _controllerHelper.GetUserIpAddress(),
                            ModifierId = userDb?.Id,
                            ModifierUserName = userDb?.UserName,
                            ModifyDateTime = DateTime.Now,
                            RecordId = equivalentModel.Id
                        };
                        Result<Modification> modifyInsert = await _modificationRepository.InsertAsync(modification, cancellationToken);

                        #region add to LuceneIndex
                        string mainDomainPath = "";
                        HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                        string? domainId = userDb?.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;

                        if (dto.IsPublishedOnMainDomain)
                        {
                            string mainDomainId = _controllerHelper.GetCurrentUserDomain().Id;
                            mainDomainPath = Path.Combine(_configuration["LocalStaticFileStorage"]!, "LuceneIndexes", mainDomainId, "Product");

                            if (!Directory.Exists(mainDomainPath))
                            {
                                Directory.CreateDirectory(mainDomainPath);
                                List<string> cultureList = (_configuration.GetSection("SupportedCultures").Get<string[]>() ?? []).ToList();

                                foreach (string culPath in cultureList.Select(cul => Path.Combine(mainDomainPath, cul.Trim())).Where(culPath => !Directory.Exists(culPath)))
                                {
                                    Directory.CreateDirectory(culPath);
                                }
                            }
                        }

                        if (domainId != null)
                        {
                            string mainPath = Path.Combine(_configuration["LocalStaticFileStorage"]!, "LuceneIndexes", domainId, "Product");

                            if (!Directory.Exists(mainPath))
                            {
                                Directory.CreateDirectory(mainPath);
                                List<string> cultureList = _configuration.GetSection("SupportedCultures").Get<string[]>()!.ToList();

                                foreach (string culPath in cultureList.Select(cul => Path.Combine(mainPath, cul.Trim())).Where(culPath => !Directory.Exists(culPath)))
                                {
                                    Directory.CreateDirectory(culPath);
                                }
                            }

                            List<string> lst = _configuration.GetSection("SupportedCultures").Get<string[]>()!.ToList();

                            foreach (string cul in lst)
                            {
                                string lanId = _controllerHelper.FetchLanguageBySymbol(cul);
                                List<string> groupNamesLanguage = [];
                                groupNamesLanguage.AddRange(dto.GroupIds.Select(grp => _controllerHelper.ProductGroupFetch(grp))
                                                               .Select(
                                                                   groupDto => groupDto.MultiLingualProperties.Any(p => p.LanguageId == lanId) ? groupDto.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId)?.Name : "")
                                                               .Where(groupName => !string.IsNullOrWhiteSpace(groupName))!);

                                if (!DirectoryReader.IndexExists(FSDirectory.Open(Path.Combine(mainPath, cul.Trim()))))
                                {
                                    Domain domainEntity = await _domainRepository.FirstOrDefaultAsync(d => d.Id == domainId, cancellationToken);
                                    List<DataLayer.Entities.Shop.Product.Product> productList;

                                    if (!domainEntity.IsDefault)
                                    {
                                        productList = (await _productRepository.GetListAsync(p => p.AssociatedDomainId == domainId, cancellationToken)).ToList();
                                    }
                                    else
                                    {
                                        productList = await _productRepository.GetListAsync(p => p.AssociatedDomainId == domainId || p.IsPublishedOnMainDomain, cancellationToken);
                                    }

                                    _luceneService.BuildProductIndexesPerLanguage(productList, Path.Combine(mainPath, cul.Trim()));
                                }

                                LuceneSearchIndexModel obj = new()
                                {
                                    Id = dto.Id,
                                    EntityName = dto.MultiLingualProperties.Any(p => p.LanguageId == lanId) ? dto.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId)?.Name : "",
                                    GroupIds = dto.GroupIds,
                                    Code = dto.ProductCode.ToString(),
                                    UniqueCode = dto.UniqueCode,
                                    GroupNames = groupNamesLanguage,
                                    TagKeywordList = dto.MultiLingualProperties.Any(p => p.LanguageId == lanId) ? dto.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId)?.TagKeywords : []
                                };
                                luceneResult = _luceneService.AddItemToExistingIndex(Path.Combine(mainPath, cul.Trim()), obj, true);

                                if (!dto.IsPublishedOnMainDomain)
                                {
                                    continue;
                                }

                                {
                                    if (!DirectoryReader.IndexExists(FSDirectory.Open(Path.Combine(mainDomainPath, cul.Trim()))))
                                    {
                                        Domain domainEntity = await _domainRepository.FirstOrDefaultAsync(d => d.Id == domainId, cancellationToken);
                                        List<DataLayer.Entities.Shop.Product.Product> productList;

                                        if (!domainEntity.IsDefault)
                                        {
                                            productList = await _productRepository.GetListAsync(p => p.AssociatedDomainId == domainId, cancellationToken);
                                        }
                                        else
                                        {
                                            productList = await _productRepository.GetListAsync(p => p.AssociatedDomainId == domainId || p.IsPublishedOnMainDomain, cancellationToken);
                                        }

                                        _luceneService.BuildProductIndexesPerLanguage(productList, Path.Combine(mainDomainPath, cul.Trim()));
                                    }
                                    else
                                    {
                                        _luceneService.AddItemToExistingIndex(Path.Combine(mainDomainPath, cul.Trim()), obj, true);
                                    }
                                }
                            }
                        }
                        #endregion

                        if (isFromContent)
                        {
                            return result = Json(saveResult.Succeeded && luceneResult.Succeeded
                                                     ? new { Status = "Success", saveResult.Message }
                                                     : new { Status = "Error", saveResult.Message });
                        }

                        await _codeGenerator.SaveToDb(dto.ProductCode, cancellationToken);
                        _logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},creating product with {dto.Id} id done successfully");
                    }

                    return result = Json(saveResult.Succeeded && luceneResult.Succeeded
                                             ? new { Status = "Success", saveResult.Message }
                                             : new { Status = "Error", saveResult.Message });
                }

                _logger.Warning($"userId: {userDb.Id}, userName: {userDb.UserName} error occured. stack trace: {nameof(ProductController)}/{nameof(Add)}");
                errors.Add(new() { Key = "UniqueCode", ErrorMessage = UtilityLanguage.GetString("AlertAndMessage_DuplicateUniqueCode") });

                return result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
        }
        catch (Exception e)
        {
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductController)}/{nameof(Add)}");
        }

        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Edit([FromBody] ProductInputDto dto, CancellationToken cancellationToken)
    {
        JsonResult result;
        bool previousPublishState = _controllerHelper.IsPublishOnMainDomain(dto.Id);
        List<AjaxValidationErrorModel> errors = [];
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);

        if (!ModelState.IsValid)
        {
            foreach (string modelStateKey in ModelState.Keys)
            {
                ModelStateEntry modelStateVal = ModelState[modelStateKey];
                errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
            }

            result = Json(new { Status = "ModelError", ModelStateErrors = errors });
        }
        else
        {
            if (await _productRepository.AnyAsync(c => c.Id == dto.Id && c.UniqueCode == dto.UniqueCode, cancellationToken))
            {
                foreach (MultiLingualProperty item in dto.MultiLingualProperties)
                {
                    Language lan = _controllerHelper.FetchLanguage(item.LanguageId);
                    item.LanguageSymbol = lan.Symbol;
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                    item.ProductGroupNames = [];

                    foreach (string name in from grp in dto.GroupIds select _controllerHelper.ProductGroupFetch(grp) into groupDto where groupDto.MultiLingualProperties.Any(p => p.LanguageId == item.LanguageId) select groupDto.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == item.LanguageId).Name)
                    {
                        item.ProductGroupNames.Add(name);
                    }
                }

                //var changeCulture = false;
                foreach (PriceDto item in dto.Prices)
                {
                    Result<Currency> cur = _controllerHelper.FetchCurrency(item.CurrencyId);

                    item.PriceId = Guid.NewGuid().ToString();
                    item.Symbol = cur.ReturnValue.Symbol;
                    item.Prefix = cur.ReturnValue.Symbol;
                    item.SDate = CultureInfo.CurrentCulture.Name.ToLower() == "fa-ir" ? item.StartDate.Split(" ")[0].ToEnglishDate() : DateTime.Parse(item.StartDate);
                    item.EDate = !string.IsNullOrWhiteSpace(item.EndDate) ? CultureInfo.CurrentCulture.Name.ToLower() == "fa-ir" ? item.EndDate.Split(" ")[0].ToEnglishDate() : DateTime.Parse(item.EndDate) : null;
                    item.IsActive = true;
                }

                foreach (InventoryDetail item in dto.Inventory)
                {
                    item.SpecValuesId = Guid.NewGuid().ToString();
                }

                if (dto.ProductType == ProductType.File)
                {
                    CultureInfo currentCulture = new("en-US") { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
                    Thread.CurrentThread.CurrentCulture = currentCulture;

                    bool bucketResult = await _minioHelper.MakeBucket("product");

                    if (bucketResult)
                    {
                        int index = dto.ProductFileContent.IndexOf(",", StringComparison.Ordinal);

                        if (index >= 0)
                        {
                            string base64Data = dto.ProductFileContent[(index + 1)..];
                            byte[] fileBytes = Convert.FromBase64String(base64Data);
                            using MemoryStream stream = new(fileBytes);

                            dto.Id = Guid.NewGuid().ToString();
                            Domain domain = _controllerHelper.GetCurrentUserDomain();
                            string objectName = $"{domain.Id}/{dto.Id}/{dto.ProductFileName.Replace(':', '-')}";

                            bool isSave = await _minioHelper.Upload("product", objectName, stream, "application/zip", stream.Length);

                            if (isSave)
                            {
                                _logger.Information($"Product with ID {dto.Id} saved correctly to the bucket.");
                            }
                            else
                            {
                                _logger.Error($"Failed to save product to the bucket. Stack trace: {nameof(ProductController)}/{nameof(Add)}");
                            }
                        }
                        else
                        {
                            _logger.Error("Invalid ProductFileContent format. Missing comma separator.");
                        }
                    }
                    else
                    {
                        _logger.Error($"Failed to create bucket. Stack trace: {nameof(ProductController)}/{nameof(Add)}");
                    }
                }

                Result<DataLayer.Entities.Shop.Product.Product> updateResult = new();
                bool changeUnavailableToAvailable = false;
                DataLayer.Entities.Shop.Product.Product product = await _productRepository.FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);

                if (product != null)
                {
                    int preInventory = product.Inventory?.Sum(c => c.Count) ?? 0;

                    DataLayer.Entities.Shop.Product.Product equivalentModel = _mapper.Map<DataLayer.Entities.Shop.Product.Product>(dto);

                    if (string.IsNullOrWhiteSpace(dto.Id)) //insert Case
                    {
                        equivalentModel.Id = Guid.NewGuid().ToString();
                    }

                    #region MultiLingualProperties
                    if (dto.MultiLingualProperties.Count > 0)
                    {
                        foreach (MultiLingualProperty item in equivalentModel.MultiLingualProperties)
                        {
                            if (string.IsNullOrWhiteSpace(item.MultiLingualPropertyId))
                            {
                                item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                            }
                        }
                    }
                    #endregion

                    #region ProductUnit
                    if (!string.IsNullOrWhiteSpace(dto.UnitId))
                    {
                        ProductUnit unitEntity = await _productUnitRepository.FirstOrDefaultAsync(u => u.Id == dto.UnitId, cancellationToken);

                        if (unitEntity != null)
                        {
                            equivalentModel.Unit = unitEntity;
                        }
                    }
                    #endregion

                    #region Prices
                    equivalentModel.Prices = [];

                    foreach (PriceDto price in dto.Prices.OrderBy(p => p.StartDate))
                    {
                        if (price.IsActive && string.IsNullOrWhiteSpace(price.EndDate)) //price is valid from client
                        {
                            if (equivalentModel.Prices.Any(x => x.CurrencyId == price.CurrencyId && x.EndDate != null && x.IsActive))
                            {
                                Price exist = equivalentModel.Prices.FirstOrDefault(x => x.CurrencyId == price.CurrencyId && x.EndDate != null && x.IsActive);

                                //equivalentModel.Prices.Remove(exist);
                                if (exist != null)
                                {
                                    exist.IsActive = false;
                                    exist.EndDate = DateTime.Now;
                                }

                                //equivalentModel.Prices.Add(exist);
                            }
                        }

                        Price p = new()
                        {
                            PriceId = !string.IsNullOrWhiteSpace(price.PriceId) ? price.PriceId : Guid.NewGuid().ToString(),
                            CurrencyId = price.CurrencyId,
                            CurrencyName = price.CurrencyName,
                            IsActive = string.IsNullOrWhiteSpace(price.PriceId) || price.IsActive,
                            Prefix = price.Prefix,
                            PriceValue = price.PriceValue,
                            StartDate = price.SDate.Value,
                            EndDate = price.EDate
                        };
                        equivalentModel.Prices.Add(p);
                    }
                    #endregion

                    #region Promotion
                    if (!string.IsNullOrWhiteSpace(dto.PromotionId))
                    {
                        Promotion promotionEntity =
                            await _promotionRepository.FirstOrDefaultAsync(p => p.Id == dto.PromotionId, cancellationToken);

                        if (promotionEntity != null)
                        {
                            equivalentModel.Promotion = promotionEntity;
                        }
                    }
                    #endregion Promotion

                    #region images
                    await _controllerHelper.ProductFetch(equivalentModel.Id);

                    equivalentModel.Images = dto.Pictures;

                    foreach (Image item in equivalentModel.Images)
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
                        bool bucketResult = await _minioHelper.MakeBucket("contentcategoryimage");
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

                            Domain domain = _controllerHelper.GetCurrentUserDomain();
                            string objectName = "";

                            if (!string.IsNullOrEmpty(item.ImageId))
                            {
                                objectName = $"{domain.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
                                bool isDelete = await _minioHelper.RemoveObject("productimage", objectName);

                                if (isDelete)
                                {
                                    _logger.Information($"image with {item.ImageId} id in content with {dto.Id} id removed correctly");
                                }
                                else
                                {
                                    _logger.Error($"image not removed correctly. stack trace: {nameof(ProductController)}/{nameof(Edit)}");
                                }
                            }

                            else
                            {
                                item.ImageId = Guid.NewGuid().ToString();
                                objectName = $"{domain.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
                            }
                            bool isSave = await _minioHelper.Upload("productimage", objectName, ms, imageFormat, ms.Length);
                            if (isSave)
                            {
                                _logger.Information($"image with {item.ImageId} id in product with {dto.Id} id saved correctly");
                            }
                            else
                            {
                                _logger.Error($"image not save correctly. stack trace: {nameof(ProductController)}/{nameof(Edit)}");
                            }
                        }
                        else
                        {
                            _logger.Error($"something is wrong in making bucket stack trace: {nameof(ProductController)}/{nameof(Edit)}");
                        }
                    }
                    #endregion

                    Language defLang = _controllerHelper.GetDefaultLanguage();
                    CultureInfo current2 = new(defLang.Symbol) { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
                    Thread.CurrentThread.CurrentCulture = current2;

                    if (preInventory == 0 && equivalentModel.Inventory != null && equivalentModel.Inventory.Sum(d => d.Count) > 0)
                    {
                        changeUnavailableToAvailable = true;
                    }

                    equivalentModel.CreationDate = product.CreationDate;
                    equivalentModel.CreatorUserName = product.CreatorUserName;
                    equivalentModel.CreatorUserId = product.CreatorUserId;
                    equivalentModel.IsActive = product.IsActive;
                    equivalentModel.IsDeleted = product.IsDeleted;
                    equivalentModel.AssociatedDomainId = product.AssociatedDomainId;
                    updateResult = await _productRepository.UpdateAsync(equivalentModel, cancellationToken);

                    if (updateResult.Succeeded)
                    {
                        Modification modification = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ActionTypes = ActionTypes.Update,
                            CollectionType = CollectionType.Product,
                            Ip = _controllerHelper.GetUserIpAddress(),
                            ModifierId = userDb.Id,
                            ModifierUserName = userDb.UserName,
                            ModifyDateTime = DateTime.Now,
                            RecordId = equivalentModel.Id
                        };
                        Result<Modification> modifyInsert = await _modificationRepository.InsertAsync(modification, cancellationToken);

                        _logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},editing product with {dto.Id} id done successfully");

                        if (changeUnavailableToAvailable)
                        {
                            string userId = _httpContextAccessor.HttpContext.User.Claims
                                                                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                                                                .Value;
                            await AddReminderProductNotify("ProductAvailibilityNotify", equivalentModel, userId, NotificationType.Sms, cancellationToken);
                        }

                        updateResult.Succeeded = true;
                        updateResult.Message = ConstMessages.SuccessfullyDone;
                    }
                    else
                    {
                        updateResult.Message = ConstMessages.GeneralError;
                    }
                }

                Result luceneResult = new();

                if (updateResult.Succeeded)
                {
                    #region Update lucene Index
                    string? domainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;
                    string mainDomainId = _controllerHelper.GetCurrentUserDomain().Id;
                    string mainPath = Path.Combine(_configuration["LocalStaticFileStorage"]!, "LuceneIndexes", domainId, "Product");
                    string mainDomainPath = Path.Combine(_configuration["LocalStaticFileStorage"]!, "LuceneIndexes", mainDomainId, "Product");
                    List<string> lst = _configuration.GetSection("SupportedCultures").Get<string[]>()!.ToList();

                    foreach (string cul in lst)
                    {
                        string indexPath = Path.Combine(mainPath, cul.Trim());
                        string mainDomainIndexPath = Path.Combine(mainDomainPath, cul.Trim());
                        string lanId = _controllerHelper.FetchLanguageBySymbol(cul.Trim());
                        List<string> groupNamesLanguage = [];
                        groupNamesLanguage.AddRange(dto.GroupIds.Select(grp => _controllerHelper.ProductGroupFetch(grp)).Select(groupDto => groupDto.MultiLingualProperties.Any(p => p.LanguageId == lanId) ? groupDto.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId).Name : "").Where(groupName => !string.IsNullOrWhiteSpace(groupName)));

                        LuceneSearchIndexModel obj = new()
                        {
                            Id = dto.Id,
                            EntityName = dto.MultiLingualProperties.Any(p => p.LanguageId == lanId) ? dto.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId).Name : "",
                            GroupIds = dto.GroupIds,
                            Code = dto.ProductCode.ToString(),
                            UniqueCode = dto.UniqueCode,
                            GroupNames = groupNamesLanguage,
                            TagKeywordList = dto.MultiLingualProperties.Any(p => p.LanguageId == lanId) ? dto.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId).TagKeywords : []
                        };

                        luceneResult = _luceneService.UpdateItemInIndex(indexPath, dto.Id, obj, true);

                        switch (previousPublishState)
                        {
                            case false when dto.IsPublishedOnMainDomain:
                                _luceneService.AddItemToExistingIndex(mainDomainIndexPath, obj, true);

                                break;

                            case true when dto.IsPublishedOnMainDomain:
                                _luceneService.UpdateItemInIndex(mainDomainIndexPath, dto.Id, obj, true);

                                break;

                            case true when !dto.IsPublishedOnMainDomain:
                                _luceneService.DeleteItemFromExistingIndex(mainDomainIndexPath, dto.Id);

                                break;
                        }
                    }
                    #endregion
                }

                result = Json(updateResult.Succeeded && luceneResult.Succeeded
                                  ? new { Status = "Success", updateResult.Message }
                                  : new { Status = "Error", updateResult.Message });
            }
            else
            {
                errors.Add(new() { Key = "UniqueCode", ErrorMessage = UtilityLanguage.GetString("AlertAndMessage_DuplicateUniqueCode") });
                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
        }

        return result;
    }

    private async ValueTask<Result> AddReminderProductNotify(string templateName, DataLayer.Entities.Shop.Product.Product product, string adminId, NotificationType notificationType, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            List<MessageTemplate> messageTemplates = await _messageTemplateRepository.GetListAsync(t => t.TemplateName.ToLower() == templateName, cancellationToken);

            if (!messageTemplates.Any())
            {
                return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundMessageTemplate") };
            }

            MessageTemplate messageTemplate = messageTemplates
                .FirstOrDefault(m => m.NotificationType == notificationType &&
                                     m.MessageTemplateMultiLingual.Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));

            //var adminUser = await _userManager.FindByIdAsync(adminId);

            string lanId = (await _lanRepository.FirstOrDefaultAsync(l => l.Symbol.ToLower() == CultureInfo.CurrentCulture.Name.ToLower(), cancellationToken)).Id;

            if (messageTemplate != null)
            {
                string productName = product.MultiLingualProperties.Any(p => p.LanguageId == lanId)
                                         ? product.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId)!.Name
                                         : product.MultiLingualProperties.FirstOrDefault()?.Name;

                if (_httpContextAccessor.HttpContext != null)
                {
                    string domainName = _httpContextAccessor.HttpContext.Request.Host.ToString();
                    string domainId = (await _domainRepository.FirstOrDefaultAsync(d => d.DomainName.ToLower() == domainName.ToLower(), cancellationToken)).Id;
                    Notification notification = new()
                    {
                        Type = notificationType,
                        ActionType = ActionType.ProductAvailibilityReminder,
                        Id = Guid.NewGuid().ToString(),
                        IsActive = true,
                        ScheduleDate = DateTime.Now.ToUniversalTime(),
                        SendStatus = NotificationSendStatus.Store,
                        CreationDate = DateTime.Now.ToUniversalTime(),
                        CreatorUserId = adminId,
                        CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                                                                                          .FirstOrDefault(c => c.Type == ClaimTypes.Name)
                                                                                          ?.Value,
                        TemplateName = "ProductNotifyRecord",
                        UserFullName = "",
                        SendMessageMetaData = new Sms(),
                        AssociatedDomainId = domainId,
                        ExtraData = new List<(string, string)> { ("productId", product.Id), ("productName", productName) }
                    };

                    await _notificationRepository.InsertAsync(notification, cancellationToken);
                }

                _logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},adding reminder product notify with {product.Id} id done successfully");

                return new() { Succeeded = true, Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") };
            }

            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundMessageTemplate") };
        }
        catch (Exception e)
        {
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductController)}/{nameof(AddReminderProductNotify)}");

            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_InsertError") };
        }
    }

    private static string SaveFileBase64String(string fileContent, string filePath, string fileName)
    {
        string res = $"ProductFiles/{fileName}";

        try
        {
            int index = fileContent.IndexOf(",", StringComparison.Ordinal);
            fileContent = fileContent[(index + 1)..];
            System.IO.File.WriteAllBytes(filePath, Convert.FromBase64String(fileContent));
        }
        catch (Exception)
        {
            res = "";
        }

        return res;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            DataLayer.Entities.Shop.Product.Product entity = await _productRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            bool mainDomainPublishState = false;

            if (entity != null)
            {
                mainDomainPublishState = entity.IsPublishedOnMainDomain;
            }

            Result<DataLayer.Entities.Shop.Product.Product> res = await _productRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);

            if (res.Succeeded)
            {
                if (entity != null)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Restore,
                        CollectionType = CollectionType.ContentCategory,
                        Ip = _controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = entity.Id
                    };
                    Result<Modification> modifyInsert = await _modificationRepository.InsertAsync(modification, cancellationToken);
                }

                #region add to LuceneIndex
                DataLayer.Entities.Shop.Product.Product product = await _productRepository.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

                string domainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;
                Domain mainDomainId = _controllerHelper.GetCurrentUserDomain();
                List<string> lst = _configuration.GetSection("SupportedCultures").Get<string[]>().ToList();

                foreach (string cul in lst)
                {
                    if (domainId != null)
                    {
                        string mainPath = Path.Combine(_configuration["LocalStaticFileStorage"] ?? string.Empty, "LuceneIndexes", domainId, "Product", cul.Trim());
                        string mainDomainPath = Path.Combine(_configuration["LocalStaticFileStorage"] ?? string.Empty, "LuceneIndexes", mainDomainId.ToString() ?? string.Empty, "Product", cul.Trim());
                        string lanId = _controllerHelper.FetchLanguageBySymbol(cul);
                        List<string> groupNamesLanguage = [];

                        foreach (string grp in product.GroupIds)
                        {
                            ProductGroup groupDto = _controllerHelper.ProductGroupFetch(grp);
                            string groupName = groupDto.MultiLingualProperties.Any(p => p.LanguageId == lanId) ? groupDto.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId)?.Name : "";

                            if (!string.IsNullOrWhiteSpace(groupName))
                            {
                                groupNamesLanguage.Add(groupName);
                            }
                        }

                        LuceneSearchIndexModel obj = new()
                        {
                            Id = id,
                            EntityName = product.MultiLingualProperties.Any(p => p.LanguageId == lanId) ? product.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId)?.Name : "",
                            GroupIds = product.GroupIds,
                            Code = product.ProductCode.ToString(),
                            UniqueCode = product.UniqueCode,
                            GroupNames = groupNamesLanguage,
                            TagKeywordList = product.MultiLingualProperties.Any(p => p.LanguageId == lanId) ? product.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId)?.TagKeywords : []
                        };
                        _luceneService.AddItemToExistingIndex(Path.Combine(mainPath, cul.Trim()), obj, true);

                        if (mainDomainPublishState)
                        {
                            _luceneService.AddItemToExistingIndex(Path.Combine(mainDomainPath, cul.Trim()), obj, true);
                        }
                    }
                }
                #endregion

                result = new(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully") });
            }
            else
            {
                result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
            }
        }
        catch (Exception e)
        {
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductController)}/{nameof(Restore)}");
            result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
        }

        return result;
    }
}