using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Contracts.Shop.ProductUnit;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Contracts.Shop.ProductSpecificationGroup;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Contracts.Shop.Promotion;
using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using SixLabors.ImageSharp.Formats;
using Arad.Portal.DataLayer.Contracts.General.User;
using Arad.Portal.DataLayer.Services;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IProductUnitRepository _unitRepository;
        private readonly IProductGroupRepository _productGroupRepository;
        private readonly IProductSpecGroupRepository _specGroupRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILanguageRepository _lanRepository;
        private readonly ICurrencyRepository _curRepository;
        private readonly IConfiguration _configuration;
        private readonly IDomainRepository _domainRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly LuceneService _luceneService;
       
        private readonly string imageSize = "";
        private readonly CodeGenerator _codeGenerator;
        
        public ProductController(UserManager<ApplicationUser> userManager,CodeGenerator codeGenerator,
            IProductRepository productRepository,IUserRepository userRepository,
            ILanguageRepository languageRepository, IProductGroupRepository productGroupRepository,
            ICurrencyRepository currencyRepository, IProductUnitRepository unitRepository,
            IProductSpecGroupRepository specGroupRepository,IPromotionRepository promotionRepository,
            IWebHostEnvironment webHostEnvironment, IHttpClientFactory clientFactory,
            LuceneService luceneService,IDomainRepository domainRepository,
            IHttpContextAccessor accessor, IConfiguration configuration)
        {
            _productRepository = productRepository;
            _configuration = configuration;
            _lanRepository = languageRepository;
            _curRepository = currencyRepository;
            _productGroupRepository = productGroupRepository;
            _unitRepository = unitRepository;
            _clientFactory = clientFactory;
            _specGroupRepository = specGroupRepository;
            _httpContextAccessor = accessor;
            _userManager = userManager;
            _luceneService = luceneService;
            _promotionRepository = promotionRepository;
            _webHostEnvironment = webHostEnvironment;
            _codeGenerator = codeGenerator;
            _domainRepository = domainRepository;
            _userRepository = userRepository;
            imageSize = _configuration["ProductImageSize:Size"];
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<ProductViewModel> result = new PagedItems<ProductViewModel>();
            
            try
            {
                var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userDb = await _userManager.FindByIdAsync(currentUserId);
                var domainId = userDb.Domains.FirstOrDefault(_ => _.IsOwner).DomainId;
                var domainEntity = _domainRepository.FetchDomain(domainId);
                
                result = await _productRepository.List(Request.QueryString.ToString(), userDb);
                
                var languageEntity = _lanRepository.GetDefaultLanguage(currentUserId);
                var defLangId = languageEntity.LanguageId;
                ViewBag.DefLangId = defLangId;
                var langSymbol = CultureInfo.CurrentCulture.Name != null ? CultureInfo.CurrentCulture.Name.ToLower() : languageEntity.Symbol.ToLower();
                ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
                var contentRoot = _webHostEnvironment.ContentRootPath;
                //ViewBag.Path = contentRoot;
                var staticFileStorageURL = _configuration["LocalStaticFileStorage"];
                ViewBag.Path = staticFileStorageURL;
                var groupList = await _productGroupRepository.GetAlActiveProductGroup(defLangId, currentUserId);
                ViewBag.ProductGroupList = groupList;
                var scheme = HttpContext.Request.Scheme;
                //ViewBag.ShopUrl = scheme + "://" + domainEntity.ReturnValue.DomainName+"/"+ langSymbol;
                
                ViewBag.ShopUrl = scheme + "://" + HttpContext.Request.Host + "/" + langSymbol;
                var unitList = await _unitRepository.GetAllActiveProductUnit(defLangId, currentUserId, "");
                ViewBag.ProductUnitList = unitList;
            }
            catch (Exception)
            {
            }
            return View(result);
        }

        public async Task<IActionResult> AddEdit(string id = "")
        {
            var model = new ProductOutputDTO();
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDB = await _userManager.FindByIdAsync(currentUserId);
            if (userDB.IsSystemAccount)
            {
                var vendorList = await _userManager.GetUsersForClaimAsync(new Claim("Vendor", "True"));
                ViewBag.Vendors = vendorList.ToList().Select(_=>new SelectListModel()
                {
                    Text = _.Profile.FullName,
                    Value = _.Id
                });

                ViewBag.Domains = _domainRepository.GetAllActiveDomains();
            }
            else
            {
                ViewBag.Vendors = "-1";
            }

            model.AssociatedDomainId = userDB.Domains.FirstOrDefault(_ => _.IsOwner).DomainId;
            ViewBag.IsSysAcc = userDB.IsSystemAccount;
            ViewBag.ActivePromotionId = "-1";


            var fileShown = _configuration["LocalStaticFileShown"];
            ViewBag.Url = fileShown;

            ViewBag.ProductType = _userRepository.GetAllProductType();
            ViewBag.DownloadOptions = _userRepository.GetAllDownloadLimitationType();

            var imageRatioList = _productRepository.GetAllImageRatio();
            ViewBag.ImageRatio = imageRatioList;


           
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = await _productRepository.ProductFetch(id);
                var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                var cnt = 1;
                foreach (var item in model.MultiLingualProperties)
                {
                    item.Tag = cnt.ToString();
                    cnt += 1;
                }
                if (_productRepository.HasActiveProductPromotion(id))
                {
                    ViewBag.ActivePromotionId = model.Promotion.PromotionId;
                }
            }else
            {
                model.ProductCode = _codeGenerator.GetNewId();
            }

            var lan = _lanRepository.GetDefaultLanguage(currentUserId);
            var specGroupList = await _specGroupRepository.AllActiveSpecificationGroup(lan.LanguageId, currentUserId, "");
            specGroupList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.SpecificationGroupList = specGroupList;

            var groupList = await _productGroupRepository.GetAlActiveProductGroup(lan.LanguageId, currentUserId);
            //groupList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.ProductGroupList = groupList;

            var currencyList = _curRepository.GetAllActiveCurrency();
            ViewBag.CurrencyList = currencyList;
            ViewBag.DefCurrency = _curRepository.GetDefaultCurrency(currentUserId).ReturnValue.CurrencyId;

            ViewBag.LangId = lan.LanguageId;
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();

            var unitList = await _unitRepository.GetAllActiveProductUnit(lan.LanguageId, currentUserId, "");
            unitList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.ProductUnitList = unitList;

            ViewBag.PromotionList = _promotionRepository.GetActivePromotionsOfCurrentUser(currentUserId, PromotionType.Product);
            
            ViewBag.PicSize = imageSize;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var mainDomainPublishState = _productRepository.IsPublishOnMainDomain(id);
            Result opResult = await _productRepository.DeleteProduct(id, "delete");
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDb = await _userManager.FindByIdAsync(currentUserId);
            var domainId = userDb.Domains.FirstOrDefault(_ => _.IsOwner).DomainId;

            #region delete related luceneIndexes
            var lst = _configuration.GetSection("SupportedCultures").Get<string[]>().ToList();
            foreach (var cul in lst)
            {
                var mainPath = Path.Combine(_configuration["LocalStaticFileStorage"], "LuceneIndexes", domainId, "Product", cul.Trim());
                
                _luceneService.DeleteItemFromExistingIndex(mainPath, id);
                if(mainDomainPublishState)
                {
                    var mainDomainId = _domainRepository.FetchDefaultDomain().ReturnValue.DomainId;
                    var mainDomainPath = Path.Combine(_configuration["LocalStaticFileStorage"], "LuceneIndexes", mainDomainId, "Product", cul.Trim());
                    _luceneService.DeleteItemFromExistingIndex(mainDomainPath, id);
                }
            }
            #endregion
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }

        [HttpGet]
        public IActionResult CheckUrlFriendUniqueness(string id, string url)
        {
            var urlFriend = $"/product/{url}";
            var res = _productRepository.IsUniqueUrlFriend(urlFriend, id);

            return Json(res ? new { Status = "Success", Message = "url is unique" }
            : new { Status = "Error", Message = "url isnt unique" });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromQuery]bool isFromContent, [FromBody] ProductInputDTO dto)
        {
            JsonResult result ;
            var errors = new List<AjaxValidationErrorModel>();
            if (!ModelState.IsValid)
            {
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors
                        .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                if (_productRepository.IsCodeUnique(dto.UniqueCode))
                {
                    
                    foreach (var item in dto.MultiLingualProperties)
                    {
                        var lan = _lanRepository.FetchLanguage(item.LanguageId);
                        item.LanguageSymbol = lan.Symbol;
                        item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                        item.UrlFriend = $"{item.UrlFriend}";
                        item.ProductGroupNames = new();
                        foreach (var grp in dto.GroupIds)
                        {
                            var groupDto = _productGroupRepository.ProductGroupFetch(grp);
                            if (groupDto.MultiLingualProperties.Any(_ => _.LanguageId == item.LanguageId))
                            {
                                var name = groupDto.MultiLingualProperties.Where(_ => _.LanguageId == item.LanguageId).FirstOrDefault().Name;
                                item.ProductGroupNames.Add(name);
                            }
                        }
                    }
                    foreach (var item in dto.Inventory)
                    {
                        item.SpecValuesId = Guid.NewGuid().ToString();
                    }
                    foreach (var item in dto.Prices)
                    {
                        var cur = _curRepository.FetchCurrency(item.CurrencyId);

                        item.PriceId = Guid.NewGuid().ToString();
                        item.Symbol = cur.ReturnValue.Symbol;
                        if(isFromContent)
                        {
                            item.SDate = DateTime.UtcNow;
                        }else
                        {
                            item.SDate = CultureInfo.CurrentCulture.Name.ToLower() == "fa-ir" ? DateHelper.ToEnglishDate(item.StartDate.Split(" ")[0]) : DateTime.Parse(item.StartDate);
                            item.EDate = !string.IsNullOrWhiteSpace(item.EndDate) ?
                                (CultureInfo.CurrentCulture.Name.ToLower() == "fa-ir" ? DateHelper.ToEnglishDate(item.EndDate.Split(" ")[0]) : DateTime.Parse(item.EndDate)) : null;
                        }
                        
                        item.Prefix = cur.ReturnValue.Symbol;
                        item.IsActive = true;
                    }

                    if(isFromContent)
                    {
                        dto.SellerUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                        var userEntity = await _userManager.FindByIdAsync(dto.SellerUserId);
                        dto.SellerUserName = userEntity.UserName;
                    }

                    var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                    var path = "images/Products";
                    var productFilePath = "ProductFiles";
                    if (dto.ProductType == Enums.ProductType.File)
                    {
                        var finalPath = Path.Combine(localStaticFileStorageURL, productFilePath, dto.ProductFileName);
                        var res = SaveFileBase64String(dto.ProductFileContent, finalPath, dto.ProductFileName);
                        if (res != "")
                            dto.ProductFileUrl = res;
                    }
                    foreach (var pic in dto.Pictures)
                    {
                        var res = ImageFunctions.SaveImageModel(pic, path, localStaticFileStorageURL);
                        if (res.Key != Guid.Empty.ToString())
                        {
                            pic.ImageId = res.Key;
                            pic.Url = res.Value;
                            pic.Content = "";
                        }
                    }
                    Result<string> saveResult = await _productRepository.Add(dto);
                    Result luceneResult = new Result();
                    if (saveResult.Succeeded)
                    {
                        #region add to LuceneIndex 
                        var mainDomainId = "";
                        var mainDomainPath = "";
                        var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                        var userDb = await _userManager.FindByIdAsync(currentUserId);
                        var domainId = userDb.Domains.FirstOrDefault(_ => _.IsOwner).DomainId;
                        if (dto.IsPublishedOnMainDomain)
                        {
                            mainDomainId = _domainRepository.FetchDefaultDomain().ReturnValue.DomainId;
                            mainDomainPath = Path.Combine(_configuration["LocalStaticFileStorage"], "LuceneIndexes", mainDomainId, "Product");
                            if(!System.IO.Directory.Exists(mainDomainPath))
                            {
                                System.IO.Directory.CreateDirectory(mainDomainPath);
                                var cultureList = _configuration.GetSection("SupportedCultures").Get<string[]>().ToList();
                                foreach (var cul in cultureList)
                                {
                                    var culPath = Path.Combine(mainDomainPath, cul.Trim());
                                    if (!System.IO.Directory.Exists(culPath))
                                    {
                                        System.IO.Directory.CreateDirectory(culPath);
                                    }
                                }
                            }
                        }
                        
                        var mainPath = Path.Combine(_configuration["LocalStaticFileStorage"], "LuceneIndexes", domainId, "Product");
                        if(!System.IO.Directory.Exists(mainPath))
                        {
                            System.IO.Directory.CreateDirectory(mainPath);
                            var cultureList = _configuration.GetSection("SupportedCultures").Get<string[]>().ToList();
                            foreach (var cul in cultureList)
                            {
                                var culPath = Path.Combine(mainPath, cul.Trim());
                                if(!System.IO.Directory.Exists(culPath))
                                {
                                    System.IO.Directory.CreateDirectory(culPath);
                                }
                            }
                        }
                        var lst = _configuration.GetSection("SupportedCultures").Get<string[]>().ToList();
                        foreach (var cul in lst)
                        {
                            var lanId = _lanRepository.FetchBySymbol(cul);
                            List<string> GroupNamesInlanguage = new List<string>();
                            foreach (var grp in dto.GroupIds)
                            {
                                var groupDto = _productGroupRepository.ProductGroupFetch(grp);
                                var groupName = groupDto.MultiLingualProperties.Any(_ => _.LanguageId == lanId) ?
                                    groupDto.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).Name : "";
                                if(!string.IsNullOrWhiteSpace(groupName))
                                GroupNamesInlanguage.Add(groupName);
                            }
                            if(!DirectoryReader.IndexExists(FSDirectory.Open(Path.Combine(mainPath, cul.Trim()))))
                            {
                                var productList = _productRepository.AllProducts(domainId);
                                _luceneService.BuildProductIndexesPerLanguage(productList, Path.Combine(mainPath, cul.Trim()));
                            }
                            var obj = new LuceneSearchIndexModel()
                            {
                                ID = saveResult.ReturnValue,
                                EntityName = dto.MultiLingualProperties.Any(_ => _.LanguageId == lanId) ?
                                dto.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).Name : "",
                                GroupIds = dto.GroupIds,
                                Code = dto.ProductCode.ToString(),
                                UniqueCode = dto.UniqueCode,
                                GroupNames = GroupNamesInlanguage,
                                TagKeywordList = dto.MultiLingualProperties.Any(_ => _.LanguageId == lanId) ?
                                dto.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).TagKeywords : new List<string>()
                            };
                            luceneResult = _luceneService.AddItemToExistingIndex(Path.Combine(mainPath, cul.Trim()), obj, true);
                            if(dto.IsPublishedOnMainDomain)
                            {
                                if(!DirectoryReader.IndexExists(FSDirectory.Open(Path.Combine(mainDomainPath, cul.Trim()))))
                                {
                                    var productList = _productRepository.AllProducts(mainDomainId);
                                    _luceneService.BuildProductIndexesPerLanguage(productList, Path.Combine(mainDomainPath, cul.Trim()));
                                }
                                else
                                {
                                    _luceneService.AddItemToExistingIndex(Path.Combine(mainDomainPath, cul.Trim()), obj, true);
                                }
                                
                            }
                        }
                        #endregion 
                        if(!isFromContent)
                        {
                            _codeGenerator.SaveToDB(dto.ProductCode);
                        }
                    }
                    result = Json(saveResult.Succeeded && luceneResult.Succeeded ? new { Status = "Success", saveResult.Message }
                    : new { Status = "Error", saveResult.Message });
                }else
                {
                    errors.Add(new AjaxValidationErrorModel() { Key = "UniqueCode", ErrorMessage = Language.GetString("AlertAndMessage_DuplicateUniqueCode") });
                    result = Json(new { Status = "ModelError", ModelStateErrors = errors });
                }
            }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ProductInputDTO dto)
        {
            JsonResult result;
            var previousPublishState = _productRepository.IsPublishOnMainDomain(dto.ProductId);
            var errors = new List<AjaxValidationErrorModel>();
            if (!ModelState.IsValid)
            {
              
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel
                    { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                if(_productRepository.IsCodeUnique(dto.UniqueCode.ToLower(), dto.ProductId))
                {
                    foreach (var item in dto.MultiLingualProperties)
                    {
                        var lan = _lanRepository.FetchLanguage(item.LanguageId);
                        item.LanguageSymbol = lan.Symbol;
                        item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                        item.ProductGroupNames = new();
                        foreach (var grp in dto.GroupIds)
                        {
                            var groupDto = _productGroupRepository.ProductGroupFetch(grp);
                            if (groupDto.MultiLingualProperties.Any(_ => _.LanguageId == item.LanguageId))
                            {
                                var name = groupDto.MultiLingualProperties.Where(_ => _.LanguageId == item.LanguageId).FirstOrDefault().Name;
                                item.ProductGroupNames.Add(name);
                            }
                        }
                    }

                    //var changeCulture = false;
                    foreach (var item in dto.Prices)
                    {
                        var cur = _curRepository.FetchCurrency(item.CurrencyId);

                        item.PriceId = Guid.NewGuid().ToString();
                        item.Symbol = cur.ReturnValue.Symbol;
                        item.Prefix = cur.ReturnValue.Symbol;
                        item.SDate = CultureInfo.CurrentCulture.Name.ToLower() == "fa-ir" ? DateHelper.ToEnglishDate(item.StartDate.Split(" ")[0]) : DateTime.Parse(item.StartDate);
                        item.EDate = !string.IsNullOrWhiteSpace(item.EndDate) ?
                            (CultureInfo.CurrentCulture.Name.ToLower() == "fa-ir" ? DateHelper.ToEnglishDate(item.EndDate.Split(" ")[0]) : DateTime.Parse(item.EndDate)  ) : null;
                        item.IsActive = true;
                    }

                    foreach (var item in dto.Inventory)
                    {
                        item.SpecValuesId = Guid.NewGuid().ToString();
                    }
                    
                    var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                    var path = "images/Products";
                    var productFilePath = "ProductFiles";
                    if(dto.ProductType == Enums.ProductType.File)
                    {
                        var finalPath = Path.Combine(localStaticFileStorageURL, productFilePath, dto.ProductFileName);
                        var res = SaveFileBase64String(dto.ProductFileContent, finalPath, dto.ProductFileName);
                        if (res != "")
                            dto.ProductFileUrl = res;
                    }
                    foreach (var pic in dto.Pictures)
                    {
                        Guid isGuidKey;
                        if (!Guid.TryParse(pic.ImageId, out isGuidKey))
                        {
                            //its insert and imageId which was int from client replace with guid
                            var res = ImageFunctions.SaveImageModel(pic, path, localStaticFileStorageURL);
                            if (res.Key != Guid.Empty.ToString())
                            {
                                pic.ImageId = res.Key;
                                pic.Url = res.Value;
                                pic.Content = "";
                            }
                            //otherwise its  is update then it has no url ;
                        }
                    }
                    Result saveResult = await _productRepository.UpdateProduct(dto);
                    Result luceneResult = new Result();
                    if (saveResult.Succeeded)
                    {
                        #region Update lucene Index
                        var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                        var userDb = await _userManager.FindByIdAsync(currentUserId);
                        var domainId = userDb.Domains.FirstOrDefault(_ => _.IsOwner).DomainId;
                        var mainDomainId = _domainRepository.FetchDefaultDomain().ReturnValue.DomainId;
                        var mainPath = Path.Combine(_configuration["LocalStaticFileStorage"], "LuceneIndexes", domainId ,"Product");
                        var mainDomainPath = Path.Combine(_configuration["LocalStaticFileStorage"], "LuceneIndexes", mainDomainId, "Product");
                        var lst = _configuration.GetSection("SupportedCultures").Get<string[]>().ToList();
                        foreach (var cul in lst)
                        {
                            var indexPath = Path.Combine(mainPath, cul.Trim());
                            var mainDomainIndexPath = Path.Combine(mainDomainPath, cul.Trim());
                            var lanId = _lanRepository.FetchBySymbol(cul.Trim());
                            List<string> GroupNamesInlanguage = new List<string>();
                            foreach (var grp in dto.GroupIds)
                            {
                                var groupDto = _productGroupRepository.ProductGroupFetch(grp);
                                var groupName = groupDto.MultiLingualProperties.Any(_ => _.LanguageId == lanId) ?
                                    groupDto.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).Name : "";
                                if (!string.IsNullOrWhiteSpace(groupName))
                                    GroupNamesInlanguage.Add(groupName);
                            }
                            var obj = new LuceneSearchIndexModel()
                            {
                                ID = dto.ProductId,
                                EntityName = dto.MultiLingualProperties.Any(_ => _.LanguageId == lanId) ?
                                dto.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).Name : "",
                                GroupIds = dto.GroupIds,
                                Code = dto.ProductCode.ToString(),
                                UniqueCode = dto.UniqueCode,
                                GroupNames = GroupNamesInlanguage,
                                TagKeywordList = dto.MultiLingualProperties.Any(_ => _.LanguageId == lanId) ?
                                dto.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).TagKeywords : new List<string>()
                            };
                           
                            luceneResult = _luceneService.UpdateItemInIndex(indexPath, dto.ProductId, obj, true);
                            if(!previousPublishState  && dto.IsPublishedOnMainDomain)
                            {
                                _luceneService.AddItemToExistingIndex(mainDomainIndexPath, obj, true);

                            }else if(previousPublishState && dto.IsPublishedOnMainDomain)
                            {
                                _luceneService.UpdateItemInIndex(mainDomainIndexPath, dto.ProductId, obj, true);

                            }else if(previousPublishState && !dto.IsPublishedOnMainDomain)
                            {
                                _luceneService.DeleteItemFromExistingIndex(mainDomainIndexPath, dto.ProductId);
                            }
                        }
                        #endregion
                    }
                    result = Json(saveResult.Succeeded && luceneResult.Succeeded ? new { Status = "Success", saveResult.Message }
                    : new { Status = "Error", saveResult.Message });
                }else
                {
                    errors.Add(new AjaxValidationErrorModel() { Key = "UniqueCode", ErrorMessage = Language.GetString("AlertAndMessage_DuplicateUniqueCode") });
                    result = Json(new { Status = "ModelError", ModelStateErrors = errors });
                }
                
            }
            return result;
        }

        private static string SaveFileBase64String(string FileContent, string filePath, string fileName)
        {
            var res = $"ProductFiles/{fileName}";
            try
            {
                var index = FileContent.IndexOf(",");
                FileContent = FileContent.Substring(index+1);
                System.IO.File.WriteAllBytes(filePath, Convert.FromBase64String(FileContent));
            }
            catch (Exception)
            {
                res = "";
            }
            return res;
        }

        //private async Task<string> GetToken()
        //{
        //    try
        //    {
        //        var tokenUrl = _configuration["StaticFilesPlace:TokenUrl"];
        //        var userName = _configuration["StaticFilesPlace:User"];
        //        var password = _configuration["StaticFilesPlace:Password"];
        //        var scope = _configuration["StaticFilesPlace:Scope"];

        //        var client = _clientFactory.CreateClient();
        //        var keyValues = new List<KeyValuePair<string, string>> {
        //            new KeyValuePair<string, string>("username", userName),
        //            new KeyValuePair<string, string>("password", password),
        //            new KeyValuePair<string, string>("scope", /*"ApiAccess"*/scope),
        //        };

        //        client.BaseAddress = new Uri(tokenUrl);
        //        var content = new FormUrlEncodedContent(keyValues);
        //        var response = await client.PostAsync(new Uri(tokenUrl), content).Result.Content.ReadAsStringAsync();
        //        //var response = await client.PostAsync(/*"/connect/token",*/ content).Content.ReadAsStringAsync();
        //        return response;
        //        //T
        //        //var data = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponseModel>(await response.Content.ReadAsStringAsync());
        //        //access_token = data.access_token;
        //        //Logger.WriteLogFile($"access token = {access_token}");

        //    }
        //    catch (Exception e)
        //    {
        //        return string.Empty;
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> Restore(string id)
        {
            JsonResult result;
            try
            {
                var mainDomainPublishState = _productRepository.IsPublishOnMainDomain(id);
                var res = await _productRepository.Restore(id);
                if (res.Succeeded)
                {
                    #region add to LuceneIndex
                    var product = await _productRepository.ProductSelect(id);
                    var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var userDb = await _userManager.FindByIdAsync(currentUserId);
                    var domainId = userDb.Domains.FirstOrDefault(_ => _.IsOwner).DomainId;
                    var mainDomainId = _domainRepository.FetchDefaultDomain().ReturnValue.DomainId;
                    var lst = _configuration.GetSection("SupportedCultures").Get<string[]>().ToList();
                    foreach (var cul in lst)
                    {
                        var mainPath = Path.Combine(_configuration["LocalStaticFileStorage"], "LuceneIndexes", domainId, "Product", cul.Trim());
                        var mainDomainPath = Path.Combine(_configuration["LocalStaticFileStorage"], "LuceneIndexes", mainDomainId, "Product", cul.Trim());
                        var lanId = _lanRepository.FetchBySymbol(cul);
                        List<string> GroupNamesInlanguage = new List<string>();
                        foreach (var grp in product.GroupIds)
                        {
                            var groupDto = _productGroupRepository.ProductGroupFetch(grp);
                            var groupName = groupDto.MultiLingualProperties.Any(_ => _.LanguageId == lanId) ?
                                groupDto.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).Name : "";
                            if (!string.IsNullOrWhiteSpace(groupName))
                                GroupNamesInlanguage.Add(groupName);
                        }

                        var obj = new LuceneSearchIndexModel()
                        {
                            ID = id,
                            EntityName = product.MultiLingualProperties.Any(_ => _.LanguageId == lanId) ?
                            product.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).Name : "",
                            GroupIds = product.GroupIds,
                            Code = product.ProductCode.ToString(),
                            UniqueCode = product.UniqueCode,
                            GroupNames = GroupNamesInlanguage,
                            TagKeywordList = product.MultiLingualProperties.Any(_ => _.LanguageId == lanId) ?
                              product.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).TagKeywords : new List<string>()
                        };
                        _luceneService.AddItemToExistingIndex(Path.Combine(mainPath, cul.Trim()), obj, true);

                        if(mainDomainPublishState)
                        {
                            _luceneService.AddItemToExistingIndex(Path.Combine(mainDomainPath, cul.Trim()), obj, true);
                        }
                    }
                    #endregion
                    result = new JsonResult(new
                    {
                        Status = "success",
                        Message = Language.GetString("AlertAndMessage_EditionDoneSuccessfully")
                    });
                }
                else
                {
                    result = new JsonResult(new
                    {
                        Status = "error",
                        Message = Language.GetString("AlertAndMessage_TryLator")
                    });
                }
            }
            catch (Exception ex)
            {
                result = new JsonResult(new
                {
                    Status = "error",
                    Message = Language.GetString("AlertAndMessage_TryLator")
                });
            }
            return result;
        }

    }
}
