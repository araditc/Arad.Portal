using Arad.Portal.DataLayer.Contracts.General.Content;
using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.User;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Contracts.Shop.ProductUnit;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Content;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Services;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class ContentController : Controller
    {
        
        private readonly IContentRepository _contentRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IContentCategoryRepository _contentCategoryRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly ILanguageRepository _lanRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly ICurrencyRepository _curRepository;
        private readonly IProductGroupRepository _productGroupRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductUnitRepository _unitRepository;
        private readonly ILuceneService _luceneService;
        private readonly string imageSize = "";
        private readonly CodeGenerator _codeGenerator;
        public ContentController(IContentRepository contentRepository, IWebHostEnvironment webHostEnvironment,
                                    IContentCategoryRepository contentCategoryRepository,IProductUnitRepository unitRepository,
                                    ILanguageRepository languageRepository, UserManager<ApplicationUser> userManager,
                                    ILuceneService luceneService,
                                    CodeGenerator codeGenerator,IUserRepository userRepository,ICurrencyRepository curRepository,
                                    IDomainRepository domainRepository,IProductGroupRepository groupRepository,
                                    IHttpContextAccessor accessor, IConfiguration configuration)
        {
            _contentRepository = contentRepository;
            _webHostEnvironment = webHostEnvironment;
            _contentCategoryRepository = contentCategoryRepository;
            _lanRepository = languageRepository;
            _userManager = userManager;
            _luceneService = luceneService;
            _configuration = configuration;
            _domainRepository = domainRepository;
            _httpContextAccessor = accessor;
            imageSize = _configuration["ContentImageSize:Size"];
            _codeGenerator = codeGenerator;
            _userRepository = userRepository;
            _productGroupRepository = groupRepository;
            _unitRepository = unitRepository;
            _curRepository = curRepository;
        }

        [HttpGet]
        public IActionResult CheckUrlFriendUniqueness(string id, string url)
        {
            var urlFriend = $"/blog/{url}";
            var domainId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("RelatedDomain"))?.Value;
            if (domainId == null)
            {
                domainId = _domainRepository.FetchDefaultDomain().ReturnValue.DomainId;
            }
            var res = _contentRepository.IsUniqueUrlFriend(urlFriend, domainId, id);

            return Json(res ? new { Status = "Success", Message = "url is unique" }
            : new { Status = "Error", Message = "url isnt unique" });
        }

        public async Task<IActionResult> List()
        {
            PagedItems<ContentViewModel> result = new PagedItems<ContentViewModel>();
           
            try
            {
                result = await _contentRepository.List(Request.QueryString.ToString());

                var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var defLangId = _lanRepository.GetDefaultLanguage(currentUserId).LanguageId;
                ViewBag.DefLangId = defLangId;
                ViewBag.LangList = _lanRepository.GetAllActiveLanguage();

                var categoryList =await _contentCategoryRepository.AllActiveContentCategory(defLangId, currentUserId);
                categoryList = categoryList.OrderBy(_ => _.Text).ToList();
                categoryList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
                ViewBag.CatList = categoryList;

            }
            catch (Exception ex)
            {
            }
            return View(result);
        }

       
        public async Task<IActionResult> AddEdit(string id = "")
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var staticFileStorageURL = _configuration["LocalStaticFileStorage"];
            var lan = _lanRepository.GetDefaultLanguage(currentUserId);
            var fileShown = _configuration["LocalStaticFileShown"];
            ViewBag.Url = fileShown;
            var model = new ContentDTO();
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = await _contentRepository.ContentFetch(id);
                
                if (string.IsNullOrWhiteSpace(staticFileStorageURL))
                {
                    staticFileStorageURL = _webHostEnvironment.WebRootPath;
                }
                
            }else
            {
                model.ContentCode = _codeGenerator.GetNewId();
            }

            ViewBag.ProductCode = _codeGenerator.GetNewId();
            ViewBag.ProductType = _userRepository.GetAllProductType();
            ViewBag.DownloadOptions = _userRepository.GetAllDownloadLimitationType();
            var groupList = await _productGroupRepository.GetAlActiveProductGroup(lan.LanguageId, currentUserId);
            ViewBag.ProductGroupList = groupList;
            ViewBag.baseHref = fileShown;
            var unitList = _unitRepository.GetAllActiveProductUnit(lan.LanguageId);
            unitList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.ProductUnitList = unitList;
            var currencyList = _curRepository.GetAllActiveCurrency();
            ViewBag.CurrencyList = currencyList;

            ViewBag.StaticFileStorage = staticFileStorageURL;
           
            var categoryList = await _contentCategoryRepository.AllActiveContentCategory(lan.LanguageId, currentUserId);
            categoryList = categoryList.OrderBy(_ => _.Text).ToList();
            //categoryList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.CatList = categoryList;

            var imageRatioList = _contentRepository.GetAllImageRatio();
            ViewBag.ImageRatio = imageRatioList;

            ViewBag.LangId = lan.LanguageId;
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();


            ViewBag.AllSourceType = _contentRepository.GetAllSourceType();

            ViewBag.PicSize = imageSize;
            //if(string.IsNullOrWhiteSpace(model.FileLogo))
            //{
            //    model.FileLogo = "/imgs/NoImage.png";
            //}
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            Result opResult = await _contentRepository.Delete(id, "delete");
            var domainId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("RelatedDomain"))?.Value;
            #region delete lucene index
            var mainPath = Path.Combine(_configuration["LocalStaticFileStorage"], "LuceneIndexes", domainId, "Content");
            _luceneService.DeleteItemFromExistingIndex(mainPath, id);
            #endregion

            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ContentDTO dto)
        {
            JsonResult result;
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
                var isCkEditorContentValid = HtmlSanitizer.SanitizeHtml(dto.Contents);
                if(!isCkEditorContentValid)
                {
                    errors.Add(new AjaxValidationErrorModel() {Key = "Contents", ErrorMessage = Language.GetString("AlertAndMessage_InvalidEditorContent") });
                    result = Json(new { Status = "ModelError", ModelStateErrors = errors });
                }
                else
                {
                    dto.UrlFriend = $"/blog/{dto.UrlFriend}";

                    var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                    var path = "images/Contents";
                    foreach (var pic in dto.Images)
                    {
                        var res = ImageFunctions.SaveImageModel(pic, path, localStaticFileStorageURL);
                        if (res.Key != Guid.Empty.ToString())
                        {
                            pic.ImageId = res.Key;
                            pic.Url = res.Value;
                            pic.Content = "";
                        }
                    }
                   
                    Result<string> saveResult = await _contentRepository.Add(dto);
                    var domainId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("RelatedDomain"))?.Value;
                    if (saveResult.Succeeded)
                    {
                        #region add to LuceneIndex 
                        var mainPath = Path.Combine(_configuration["LocalStaticFileStorage"], "LuceneIndexes", domainId, "Content");
                        if (!Directory.Exists(mainPath))
                        {
                            Directory.CreateDirectory(mainPath);
                        }
                        var obj = new LuceneSearchIndexModel()
                        {
                            ID = saveResult.ReturnValue,
                            EntityName = dto.Title,
                            GroupIds = new List<string> { dto.ContentCategoryId},
                            Code = dto.ContentCode.ToString(),
                            GroupNames = new List<string> { dto.ContentCategoryName },
                            TagKeywordList = dto.TagKeywords
                        };
                        _luceneService.AddItemToExistingIndex(mainPath, obj, false);
                      
                        #endregion 


                        _codeGenerator.SaveToDB(dto.ContentCode);
                    }
                    result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                    : new { Status = "Error", saveResult.Message });
                }
            }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ContentDTO dto)
        {
            JsonResult result;
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
                //if (dto.LogoContent != "")
                //dto.FileLogo = dto.LogoContent;
                var domainId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("RelatedDomain"))?.Value;
                var isCkEditorContentValid = HtmlSanitizer.SanitizeHtml(dto.Contents);
                if (!isCkEditorContentValid)
                {
                    errors.Add(new AjaxValidationErrorModel() { Key = "Contents", ErrorMessage = Language.GetString("AlertAndMessage_InvalidEditorContent") });
                    result = Json(new { Status = "ModelError", ModelStateErrors = errors });
                }
                else
                {
                    dto.UrlFriend = $"/blog/{dto.UrlFriend}";
                    var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                    var path = "images/Contents";
                    foreach (var pic in dto.Images)
                    {
                        if (!string.IsNullOrEmpty(pic.Content))
                        {
                            var res = ImageFunctions.SaveImageModel(pic, path, localStaticFileStorageURL);
                            if (res.Key != Guid.Empty.ToString())
                            {
                                pic.ImageId = res.Key;
                                pic.Url = res.Value;
                                pic.Content = "";
                            }
                        }
                    }

                    Result saveResult = await _contentRepository.Update(dto);
                    if(saveResult.Succeeded)
                    {
                        #region update luceneIndex
                        var mainPath = Path.Combine(_configuration["LocalStaticFileStorage"], "LuceneIndexes", domainId, "Content");
                        var obj = new LuceneSearchIndexModel()
                        {
                            ID = dto.ContentId,
                            EntityName = dto.Title,
                            GroupIds = new List<string> { dto.ContentCategoryId },
                            Code = dto.ContentCode.ToString(),
                            GroupNames = new List<string> { dto.ContentCategoryName},
                            TagKeywordList = dto.TagKeywords
                        };
                        _luceneService.UpdateItemInIndex(mainPath, dto.ContentId, obj, false);
                        #endregion
                    }
                    result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                    : new { Status = "Error", saveResult.Message });
                }
            }
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> Restore(string id)
        {
            JsonResult result;
            try
            {
                var domainId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("RelatedDomain"))?.Value;
                var res = await _contentRepository.Restore(id);
                if (res.Succeeded)
                {
                    #region add to LuceneIndex
                    var content = await _contentRepository.ContentSelect(id);
                   
                    var mainPath = Path.Combine(_configuration["LocalStaticFileStorage"], "LuceneIndexes", domainId, "Content");
                    var obj = new LuceneSearchIndexModel()
                    {
                        ID = id,
                        EntityName = content.Title,
                        GroupIds = new List<string> { content.ContentCategoryId },
                        Code = content.ContentCode.ToString(),
                        GroupNames = new List<string> { content.ContentCategoryName },
                        TagKeywordList = content.TagKeywords
                    };
                    _luceneService.AddItemToExistingIndex(mainPath, obj, false);
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


