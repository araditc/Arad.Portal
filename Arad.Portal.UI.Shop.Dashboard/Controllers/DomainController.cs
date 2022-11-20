using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Models.Domain;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Contracts.General.Currency;
using Microsoft.AspNetCore.Authorization;
using Arad.Portal.DataLayer.Contracts.General.Services;
using static Arad.Portal.DataLayer.Models.Shared.Enums;
using System.Reflection;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.DesignStructure;
using Microsoft.AspNetCore.Hosting;
using System.Globalization;
using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Microsoft.Extensions.Configuration;
using Arad.Portal.DataLayer.Models.DesignStructure;
using Arad.Portal.DataLayer.Contracts.General.SliderModule;
using Arad.Portal.DataLayer.Contracts.General.Content;
using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Contracts.General.BasicData;
using Microsoft.AspNetCore.Http;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class DomainController : Controller
    {
        private readonly IDomainRepository _domainRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IContentCategoryRepository _categoryRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILanguageRepository _lanRepository;
        private readonly ICurrencyRepository _curRepository;
        private readonly ISliderRepository _sliderRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IBasicDataRepository _basicDataRepository;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _accessor;
        
      

        public DomainController(IDomainRepository domainRepository, UserManager<ApplicationUser> userManager,
            IProviderRepository providerRepository,
            IWebHostEnvironment hostEnvironment,
            IProductRepository productRepository,
            IModuleRepository moduleRepository,
            ILanguageRepository lanRepository,
            IBasicDataRepository basicDataRepository,
            ISliderRepository sliderRepository,
            IConfiguration configuration,
            IContentRepository contentRepository,
            IHttpContextAccessor accessor,
            IContentCategoryRepository categoryRepository,
            ICurrencyRepository currencyRepository)
        {
            _domainRepository = domainRepository;
            _lanRepository = lanRepository;
            _userManager = userManager;
            _curRepository = currencyRepository;
            _providerRepository = providerRepository;
            _moduleRepository = moduleRepository;
            _webHostEnvironment = hostEnvironment;
            _productRepository = productRepository;
            _configuration = configuration;
            _sliderRepository = sliderRepository;
            _contentRepository = contentRepository;
            _basicDataRepository = basicDataRepository;
            _categoryRepository = categoryRepository;
            _accessor = accessor;
        }

        
        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<DomainViewModel> result = new PagedItems<DomainViewModel>();
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                result = await _domainRepository.AllDomainList(Request.QueryString.ToString());
                ViewBag.DefLangId = _lanRepository.GetDefaultLanguage(currentUserId).LanguageId;
                ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            }
            catch (Exception)
            {
            }
            return View(result);
        }

        public async Task<IActionResult> AddEdit(string id = "")
        {
            var model = new DomainDTO();
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDB = await _userManager.FindByIdAsync(currentUserId);
            if (userDB.IsSystemAccount)
            {
                var vendorList = await _userManager.GetUsersForClaimAsync(new Claim("Vendor", "True"));
                var vendors = vendorList.Select(_ => new SelectListModel()
                {
                    Text = _.Profile.FullName,
                    Value = _.Id
                }).ToList();
                vendors.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "-1" });
                ViewBag.Vendors = vendors;
            }
            //var paymentProviders = _providerRepository.GetProvidersPerType(DataLayer.Entities.General.Service.ProviderType.Payment);
            var paymentProviders = _domainRepository.GetPspTypesEnum();
            ViewBag.Providers = paymentProviders;

            if (!string.IsNullOrWhiteSpace(id))
            {
                model = _domainRepository.FetchDomain(id).ReturnValue;
                model.SupportedCultures = _basicDataRepository.GetList("SupportedCultures", false).Select(_ => _.Text).ToList();
            }
            
            var lan = _lanRepository.GetDefaultLanguage(currentUserId);
            ViewBag.LangId = lan.LanguageId;
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            
            var shippingTypelist = _basicDataRepository.GetList("ShippingType", true);
            ViewBag.ShippingTypeList = shippingTypelist;

            var currencyList = _curRepository.GetAllActiveCurrency();
            ViewBag.CurrencyList = currencyList;
            ViewBag.DefCurrency = _curRepository.GetDefaultCurrency(currentUserId).ReturnValue.CurrencyId;
            ViewBag.Activelanguages = _lanRepository.GetAllActiveLanguage();
            var supportedCultures = _basicDataRepository.GetList("SupportedCultures", false);
            foreach (var item in supportedCultures)
            {
                var lanId = _lanRepository.FetchBySymbol(item.Text);
                model.SupportedCultures.Add(lanId);
            }

            var invoiceNumberEnum = _domainRepository.GetInvoiceNumberProcedureEnum();
            ViewBag.InvoiceNumberEnum = invoiceNumberEnum;

            return View(model);
        }


       [HttpPost]
       public async Task<IActionResult> SavePageContent([FromBody]DomainPageModel model)
       {
            var domainEntity = _domainRepository.FetchDomain(model.DomainId);

            //var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            //var languageId = _lanRepository.FetchBySymbol(lanIcon);

            var obj = new PageDesignContent()
            {
                LanguageId = model.LanguageId,
                HeaderPart = model.HeaderPart,
                FooterPart = model.FooterPart,
                MainPageContainerPart = model.MainPageContainerPart
            };
           
            switch (model.PageType)
            {
                case PageType.HomePage:
                    if (domainEntity.ReturnValue.HomePageDesign.Any(_ => _.LanguageId == model.LanguageId))
                    {
                        var home = domainEntity.ReturnValue.HomePageDesign.FirstOrDefault(_ => _.LanguageId == model.LanguageId);
                        home.HeaderPart = obj.HeaderPart;
                        home.FooterPart = obj.FooterPart;
                        home.MainPageContainerPart = obj.MainPageContainerPart;
                    }
                    else
                    {
                        domainEntity.ReturnValue.HomePageDesign.Add(obj);
                    }
                    break;
                case PageType.BlogPage:
                    if (domainEntity.ReturnValue.BlogPageDesign.Any(_ => _.LanguageId == model.LanguageId))
                    {
                        var blog = domainEntity.ReturnValue.BlogPageDesign.FirstOrDefault(_ => _.LanguageId == model.LanguageId);
                        blog.HeaderPart = obj.HeaderPart;
                        blog.FooterPart = obj.FooterPart;
                        blog.MainPageContainerPart = obj.MainPageContainerPart;
                    }
                    else
                    {
                        domainEntity.ReturnValue.BlogPageDesign.Add(obj);
                    }
                    break;
                case PageType.ProductPage:
                    if (domainEntity.ReturnValue.ProductPageDesign.Any(_ => _.LanguageId == model.LanguageId))
                    {
                        var pro = domainEntity.ReturnValue.ProductPageDesign.FirstOrDefault(_ => _.LanguageId == model.LanguageId);
                        pro.HeaderPart = obj.HeaderPart;
                        pro.FooterPart = obj.FooterPart;
                        pro.MainPageContainerPart = obj.MainPageContainerPart;
                    }
                    else
                    {
                        domainEntity.ReturnValue.ProductPageDesign.Add(obj);
                    }
                    break;
            }
            var result = await _domainRepository.EditDomain(domainEntity.ReturnValue);

            return Json(result.Succeeded ? new { Status = "Success", result.Message }
                 : new { Status = "Error", result.Message });
       }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] DomainDTO dto)
        {

            JsonResult result;
            if (!ModelState.IsValid)
            {
                var errors = new List<AjaxValidationErrorModel>();

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
                dto.DomainId = Guid.NewGuid().ToString();
                #region DomainOwner user update
                var ownerUser = await _userManager.FindByNameAsync(dto.OwnerUserName);
                ownerUser.Domains.Add(new UserDomain() { DomainId = dto.DomainId, DomainName = dto.DomainName, IsOwner = true });
                await _userManager.UpdateAsync(ownerUser);
                #endregion


                foreach (var item in dto.DomainPaymentProviders)
                {
                    item.PspType = (PspType)Enum.Parse(typeof(PspType), item.Type);
                } 
                foreach (var item in dto.Prices)
                {
                    var cur = _curRepository.FetchCurrency(item.CurrencyId);

                    item.PriceId = Guid.NewGuid().ToString();
                    item.Symbol = cur.ReturnValue.Symbol;
                    item.Prefix = cur.ReturnValue.Symbol;
                    item.SDate = DateHelper.ToEnglishDate(item.StartDate.Split(" ")[0]);
                }

                if(dto.Prices.Where(_=>_.IsActive = false && _.EDate  == null).Any())
                {
                    var incorrectBoundary = dto.Prices.Where(_ => _.IsActive = false && _.EDate == null).FirstOrDefault();
                    var activePrice = dto.Prices.Where(_ => _.IsActive && _.EDate == null).FirstOrDefault();
                    incorrectBoundary.EDate = activePrice.SDate.Value.AddDays(-1);
                }

                var supportedIds = dto.SupportedCultures;
                
                var i = 1;
                foreach (var lanId in supportedIds)
                {
                    var lanEntity = _lanRepository.FetchLanguage(lanId);
                    var basicObject = new DataLayer.Entities.General.BasicData.BasicData()
                    {
                        AssociatedDomainId = dto.DomainId,
                        BasicDataId = Guid.NewGuid().ToString(),
                        GroupKey = "SupportedCultures",
                        Value = i.ToString(),
                        Text = lanEntity.Symbol,
                        Order = i,
                        CreationDate = DateTime.UtcNow,
                        IsActive = true,
                        CreatorUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    };
                    await _basicDataRepository.InsertNewRecord(basicObject);
                    i++;
                }

                Result saveResult = await _domainRepository.AddDomain(dto);
                
                result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                : new { Status = "Error", saveResult.Message });
            }
            return result;
        }

        [HttpGet]
        public IActionResult PageDesign(string domainId, PageType pageType)
        {
            //var moduleList = _moduleRepository.GetAllModules();
            //ViewBag.ModuleList = moduleList;
            var domainEntity = _domainRepository.FetchDomain(domainId).ReturnValue;

            ViewBag.PageType = pageType;
            List<PageDesignContent> finalModel = new List<PageDesignContent>();
            ViewBag.DomainId = domainId;
            //ViewBag.IsShop = domainEntity.IsShop;
            //ViewBag.IsMultilingual = domainEntity.IsMultiLinguals;
            ViewBag.Url =  _configuration["LocalStaticFileShown"];
            var lanList = _lanRepository.GetAllActiveLanguage();
            lanList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "-1" });
            ViewBag.LangList = lanList;
            var imageRatioList = _productRepository.GetAllImageRatio();
            ViewBag.ImageRatio = imageRatioList;
            var imageSize = _configuration["ProductImageSize:Size"];
            ViewBag.PicSize = imageSize;
            switch (pageType)
            {
                case PageType.HomePage:
                    foreach (var design in domainEntity.HomePageDesign)
                    {
                        if (string.IsNullOrWhiteSpace(design.LanguageName))
                        {
                            design.LanguageName = _lanRepository.FetchLanguage(design.LanguageId).LanguageName;
                        }
                    }
                    finalModel = domainEntity.HomePageDesign;
                    break;
                case PageType.ProductPage:
                    foreach (var design in domainEntity.ProductPageDesign)
                    {
                        if (string.IsNullOrWhiteSpace(design.LanguageName))
                        {
                            design.LanguageName = _lanRepository.FetchLanguage(design.LanguageId).LanguageName;
                        }
                    }
                    finalModel = domainEntity.ProductPageDesign;
                    break;
                  
                case PageType.BlogPage:
                    foreach (var design in domainEntity.BlogPageDesign)
                    {
                        if (string.IsNullOrWhiteSpace(design.LanguageName))
                        {
                            design.LanguageName = _lanRepository.FetchLanguage(design.LanguageId).LanguageName;
                        }
                    }
                    finalModel = domainEntity.BlogPageDesign;
                    break;
                 
            }
            return View("~/Views/Domain/PrimaryTemplateDesignPage.cshtml", finalModel);
        }


        [HttpPost]
        public IActionResult StoreDesignPreview([FromBody] TemplateDesign model)
        {
            var key = Guid.NewGuid().ToString();
            HttpContext.Session.SetComplexData(key, model);

            return Json(new { key = key });

        }

        [HttpGet]
        public IActionResult DesignPreview(string key)
        {

            TemplateDesign data = HttpContext.Session.GetComplexData<TemplateDesign>(key);

            var lanEntity = _lanRepository.FetchLanguage(data.LanguageId);
            data.LangSymbol = lanEntity.Symbol.Substring(0, 2);
           
            return View("PrimaryTemplatePreview", data);
        }


        #region GetModulesViewComponents
        [HttpGet]
        public IActionResult GetProductModuleViewComponent(ProductOrContentType productType, ProductTemplateDesign selectionTemplate,
            int count, DataLayer.Entities.General.SliderModule.TransActionType loadAnimation, LoadAnimationType loadAnimationType)
        {
            return ViewComponent("SpecialProduct", new { productType, selectionTemplate, count, loadAnimation, loadAnimationType });
        }


        [HttpGet]
        public IActionResult GetLoginProfileModuleViewComponent(string domainId, bool isShop)
        {
            return ViewComponent("LoginProfile", new { domainId = domainId, isShop = isShop });
        }

        [HttpGet]
        public IActionResult GetMultiLingualModuleViewComponent()
        {
            return ViewComponent("MultiLingual");
        }

        [HttpGet]
        public IActionResult GetGeneralSearchViewComponent()
        {
            return ViewComponent("GeneralSearch");
        }

       
        [HttpPost]
        public IActionResult GetContentModuleViewComponent(ProductOrContentType contentType, ContentTemplateDesign selectionTemplate, int? count,
            DataLayer.Entities.General.SliderModule.TransActionType loadAnimation, LoadAnimationType loadAnimationType, SelectionType selectedType, string catId,
            [FromBody]List<string> selectedIds)
        {

            var moduleParameters = new ModuleParameters()
            {
                ProductOrContentType = contentType,
                ContentTemplateDesign = selectionTemplate,
                Count = count,
                LoadAnimation = loadAnimation,
                LoadAnimationType = loadAnimationType,
                SelectionType = selectedType,
                CatId = catId,
                SelectedIds = selectedIds
            };
            return ViewComponent("ContentTemplates", moduleParameters);
        }

        [HttpGet]
        public IActionResult GetSliderViewComponent(string sliderId)
        {
            return ViewComponent("Slider", new { sliderId });
        }

        public IActionResult GetStoreMenuViewComponent()
        {
            return ViewComponent("StoreMenuModule");
        }

        #endregion GetModulesViewComponents

        public async Task<IActionResult> GetSpecificModule(string moduleName, string id, int colCount, string rn, string cn, string sec, string langId)
        {
            var module = _moduleRepository.FetchModuleByName(moduleName);
            //??? testing
            //id = "28d0433f-2bb6-4ef9-bad7-0a18a28d9004";

            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var viewName = $"_{moduleName}.cshtml";
            var imageTemplatePath = _webHostEnvironment.WebRootPath;
            var productOrContentTypes = _moduleRepository.GetAllProductOrContentTypes();
            ViewBag.ProductOrContentTypeList = productOrContentTypes;
            ViewBag.DomainId = id;
           
            switch (moduleName.ToLower())
            {
                case "productlist":
                    var productTemplateList = _moduleRepository.GetAllProductTemplateDesign();
                    foreach (var item in productTemplateList)
                    { 
                       item.ImageUrl = Path.Combine(imageTemplatePath, $"Template/Product/{item.Text}.jpg");
                    }
                    ViewBag.ProductTemplateList = productTemplateList;
                    ViewBag.TransactionType = _moduleRepository.GetAllTransactionType();
                    ViewBag.LoadAnimationType = _moduleRepository.GetAllLoadAnimationType();
                    break;
                case "contentlist":
                    var contentTemplateDesigns = _moduleRepository.GetAllContentTemplateDesign();
                    foreach (var item in contentTemplateDesigns)
                    {
                        if (item.Text.ToLower() != "forth")
                        {
                            item.ImageUrl = Path.Combine(imageTemplatePath, $"Template/Content/{item.Text}.jpg");
                        }
                        else
                        {
                            if (CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                            {
                                item.ImageUrl = Path.Combine(imageTemplatePath, "Template/Content/Forth-rtl.jpg");
                            }
                            else
                            {
                                item.ImageUrl = Path.Combine(imageTemplatePath, "Template/Content/Forth-ltr.jpg");
                            }
                        }
                    }
                    ViewBag.ContentTemplateList = contentTemplateDesigns;
                    ViewBag.TransactionType = _moduleRepository.GetAllTransactionType();
                    ViewBag.LoadAnimationType = _moduleRepository.GetAllLoadAnimationType();
                    ViewBag.SelectionType = _moduleRepository.GetAllSelectionType();
                    ViewBag.CategoryList = await _categoryRepository.AllActiveContentCategory(langId, currentUserId);
                    ViewBag.ContentList = _contentRepository.GetContentsList(id, currentUserId, "");
                    break;
                case "imagetextslider":
                    var sliderList = _sliderRepository.ActiveSliderList(id);
                    ViewBag.SliderList = sliderList;
                    break;
                case "horizantalstoremenu":
                break;
                case "generalsearch":
                    break;
                case "advertisement":
                    break;
                default:
                    break;
            }

            ViewBag.ColCnt =  colCount;

            ViewBag.RowNumber = rn;
            ViewBag.ColNumber = cn;
            ViewBag.Section = sec;
            return PartialView($"~/Views/Domain/{viewName}", null);
        }


        [HttpPost]
        public IActionResult SanitizeCkEditorContent([FromBody]string html)
        {
            var res = HtmlSanitizer.SanitizeHtml(html);
            return Json(new { isvalid = res });
        }

        [HttpPost]
        public IActionResult GetProviderParams([FromQuery] int pspVal)
        {
            var htmlResult = "";
            var psp = (PspType)pspVal;
            switch (psp)
            {
                //case PspType.IranKish:
                //    htmlResult = GenerateForm(typeof(IrankishModel).GetProperties());
                //    break;
                case PspType.Saman:
                    htmlResult = GenerateForm(typeof(SamanModel).GetProperties());
                    break;
                //case PspType.Parsian:
                //    htmlResult = GenerateForm(typeof(ParsianModel).GetProperties());
                //    break;
            }
            return Content(htmlResult);
        }

        private string GenerateForm(PropertyInfo[] properties)
        {
            var finalHTML = "";
            foreach (var prop in properties)
            {
                finalHTML += $"<div class='form-group col-md-3'><label class='form-label' for='{prop.Name}'>{prop.Name}</label><br/><input type='text' id='{prop.Name}' class='form-control gatewayPar ltr' value='' /></div>";
            }
            
            return finalHTML;
        }

        [HttpGet]
        public async Task<IActionResult> Restore(string id)
        {
            JsonResult result;
            try
            {
                var dto = _domainRepository.FetchDomain(id);
                if (dto == null)
                {
                    result = new JsonResult(new
                    {
                        Status = "error",
                        Message = Language.GetString("AlertAndMessage_EntityNotFound")
                    });
                }
                else
                {
                    var res = await _domainRepository.Restore(id);
                    if (res.Succeeded)
                    {
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

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] DomainDTO dto)
        {
            JsonResult result;
            //Result saveResult;
            DomainDTO model;
            var domainId = User.GetClaimValue("RelatedDomain");
            if (!ModelState.IsValid)
            {
                var errors = new List<AjaxValidationErrorModel>();
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                foreach (var item in dto.DomainPaymentProviders)
                {
                    item.PspType = (PspType)Enum.Parse(typeof(PspType), item.Type);
                }
                foreach (var item in dto.Prices)
                {
                    var cur = _curRepository.FetchCurrency(item.CurrencyId);

                    item.PriceId = Guid.NewGuid().ToString();
                    item.Symbol = cur.ReturnValue.Symbol;
                    item.Prefix = cur.ReturnValue.Symbol;
                    item.SDate = DateHelper.ToEnglishDate(item.StartDate.Split(" ")[0]);
                }
                model = _domainRepository.FetchDomain(dto.DomainId).ReturnValue;
                if (model == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }

                if (dto.Prices.Where(_ => _.IsActive = false && _.EDate == null).Any())
                {
                    var incorrectActivation = dto.Prices.Where(_ => _.IsActive = false && _.EDate == null).FirstOrDefault();
                    var activePrice = dto.Prices.Where(_ => _.IsActive && _.EDate == null).FirstOrDefault();
                    incorrectActivation.EDate = activePrice.SDate.Value.AddDays(-1);
                }
                List<string> supportedCulsInput = new List<string>();
                var currentSupportedCultures = _basicDataRepository.GetList("SupportedCultures", false).Select(_=>_.Text);
                foreach (var item in dto.SupportedCultures)
                {
                    var lanEntity = _lanRepository.FetchLanguage(item);
                    supportedCulsInput.Add(lanEntity.Symbol);
                }
                var intersection = supportedCulsInput.Intersect(currentSupportedCultures);
                if(intersection.Count() != currentSupportedCultures.Count() || intersection.Count() != supportedCulsInput.Count())
                {
                    var res = await _basicDataRepository.DeleteGroupKeyListInDomain(domainId, "SupportedCultures");
                    int i = 1;
                    foreach (var item in supportedCulsInput)
                    {
                        await _basicDataRepository.InsertNewRecord(new DataLayer.Entities.General.BasicData.BasicData()
                        {
                            AssociatedDomainId = domainId,
                            BasicDataId = Guid.NewGuid().ToString(),
                            CreationDate = DateTime.UtcNow,
                            GroupKey = "SupportedCultures",
                            Value = i.ToString(),
                            Text = item,
                            Order = i
                        });
                        i++;
                    }
                }

                Result saveResult = await _domainRepository.EditDomain(dto);

                result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                 : new { Status = "Error", saveResult.Message });
            }
           
            return result;
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            Result opResult = await _domainRepository.DeleteDomain(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }


        public IActionResult GetRelatedColsTemplateWidths(int count)
        {
            JsonResult result;
            var res = new List<SelectListModel>();
            switch (count)
            {
                case 1:
                    res = _domainRepository.GetOneColsTemplateWidthEnum();
                    break;
                case 2:
                    res = _domainRepository.GetTwoColsTemplateWidthEnum();
                    break;
                case 3:
                    res = _domainRepository.GetThreeColsTemplateWidthEnum();
                    break;
                case 4:
                    res = _domainRepository.GetFourColsTemplateWidthEnum();
                    break;
                case 5:
                    res = _domainRepository.GetFiveColsTemplateWidthEnum();
                    break;
                case 6:
                    res = _domainRepository.GetSixColsTemplateWidthEnum();
                    break;
            }

            if (res.Count() > 0)
            {
                result = new JsonResult(new { Status = "success", Data = res });
            }
            else
            {
                result = new JsonResult(new { Status = "notFound", Message = "" });
            }

            return result;
        }
        [HttpPost]
        public IActionResult SaveBackgroundImageToServer([FromBody] BgImage model)
        {
            JsonResult result;
            var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
            var path = "images/DomainDesign";
            
            var url = ImageFunctions.SaveBackgroundImage(model.Base64ImageContent, path, localStaticFileStorageURL);
            if (!string.IsNullOrWhiteSpace(url))
            {
                 result = Json(new { status = "success", url = $"/{url}" , selectedRowGuid = model.SelectedRowGuid , section = model.SelectedSection  });
            }else
            {
                 result = Json(new { status = "failed", selectedRowGuid = model.SelectedRowGuid, section = model.SelectedSection });
            }
            return result;
        }

        [HttpGet]
        [Route("images/DomainDesign/{**slug}")]
        public IActionResult GetDomainDesignImages(string slug)
        {
            var path = $"/images/DomainDesign/{slug}";
            (byte[] fileContents, string mimeType) = ImageFunctions.GetImageWithActualSize(path, _configuration["LocalStaticFileStorage"]);
            return File(fileContents, mimeType);
        }


        [HttpPost]
        public async Task<IActionResult> GetRowWithSelectedColumns([FromBody]RowSelectedColumnsModel obj)
        {

            var moduleList = _moduleRepository.GetAllModules();
            ViewBag.ModuleList = moduleList;
            var imageTemplatePath = _webHostEnvironment.WebRootPath;
            var domainEntity = _domainRepository.FetchDomain(obj.DomainId).ReturnValue;
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var viewName = "";
            switch(obj.Count)
            {
                case 1:
                    viewName = "_OneColumn.cshtml";
                    break;
                case 2:
                    viewName = "_TwoColumns.cshtml";
                    break;
                case 3:
                    viewName = "_ThreeColumns.cshtml";
                    break;
                case 4:
                    viewName = "_FourColumns.cshtml";
                    break;
                case 5:
                    viewName = "_FiveColumns.cshtml";
                    break;
                case 6:
                    viewName = "_SixColumns.cshtml";
                    break;
              
            }
            ViewBag.ProductOrContentTypeList = _moduleRepository.GetAllProductOrContentTypes();
            var productTemplateList = _moduleRepository.GetAllProductTemplateDesign();
            ViewBag.ProductTemplateList = productTemplateList;
            ViewBag.TransactionType = _moduleRepository.GetAllTransactionType();
            ViewBag.LoadAnimationType = _moduleRepository.GetAllLoadAnimationType();
            var contentTemplateDesigns = _moduleRepository.GetAllContentTemplateDesign();
            foreach (var item in contentTemplateDesigns)
            {
                if (item.Text.ToLower() != "forth")
                {
                    item.ImageUrl = Path.Combine(imageTemplatePath, $"Template/Content/{item.Text}.jpg");
                }
                else
                {
                    if (CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                    {
                        item.ImageUrl = Path.Combine(imageTemplatePath, "Template/Content/Forth-rtl.jpg");
                    }
                    else
                    {
                        item.ImageUrl = Path.Combine(imageTemplatePath, "Template/Content/Forth-ltr.jpg");
                    }
                }
            }
            ViewBag.ContentTemplateList = contentTemplateDesigns;
            ViewBag.SelectionType = _moduleRepository.GetAllSelectionType();
            ViewBag.CategoryList = await _categoryRepository.AllActiveContentCategory(domainEntity.DefaultLanguageId, currentUserId);
            ViewBag.ContentList = _contentRepository.GetContentsList(obj.DomainId, currentUserId, "");
            //??? testing
            //obj.DomainId = "28d0433f-2bb6-4ef9-bad7-0a18a28d9004";
            ViewBag.SliderList = _sliderRepository.ActiveSliderList(obj.DomainId);
            ViewBag.ColWidth = obj.ColumnWidth;
            ViewBag.RowNumber = obj.RowNumber;
            ViewBag.DomainId = obj.DomainId;
            ViewBag.Guid = obj.RowGuid;
            return PartialView($"~/Views/Domain/{viewName}", obj.RowData);
        }
    }
}
