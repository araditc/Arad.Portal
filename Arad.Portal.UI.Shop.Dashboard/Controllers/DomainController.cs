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

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class DomainController : Controller
    {
        private readonly IDomainRepository _domainRepository;
        private readonly IPermissionView _permissionViewManager;
        private readonly IModuleRepository _moduleRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly ILanguageRepository _lanRepository;
        private readonly ICurrencyRepository _curRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        

        public DomainController(IDomainRepository domainRepository, UserManager<ApplicationUser> userManager,
            IProviderRepository providerRepository,
            IWebHostEnvironment hostEnvironment,
            IModuleRepository moduleRepository,
            IPermissionView permissionView, ILanguageRepository lanRepository,
            ICurrencyRepository currencyRepository)
        {
            _domainRepository = domainRepository;
            _permissionViewManager = permissionView;
            _lanRepository = lanRepository;
            _userManager = userManager;
            _curRepository = currencyRepository;
            _providerRepository = providerRepository;
            _moduleRepository = moduleRepository;
            _webHostEnvironment = hostEnvironment;
        }

        public async Task<string> RenderViewComponent(string viewComponent, object args)
        {
            var sp = HttpContext.RequestServices;

            var helper = new DefaultViewComponentHelper(
                sp.GetRequiredService<IViewComponentDescriptorCollectionProvider>(),
                HtmlEncoder.Default,
                sp.GetRequiredService<IViewComponentSelector>(),
                sp.GetRequiredService<IViewComponentInvokerFactory>(),
                sp.GetRequiredService<IViewBufferScope>());

            using (var writer = new StringWriter())
            {
                var context = new ViewContext(ControllerContext, NullView.Instance, ViewData, TempData, writer, new HtmlHelperOptions());
                helper.Contextualize(context);
                var result = await helper.InvokeAsync(viewComponent, args);
                result.WriteTo(writer, HtmlEncoder.Default);
                await writer.FlushAsync();
                return writer.ToString();
            }
        }


        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<DomainViewModel> result = new PagedItems<DomainViewModel>();
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Permissions = dicKey;
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
                var vendorList = await _userManager.GetUsersForClaimAsync(new Claim("AppRole", "True"));
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
            }
            
            var lan = _lanRepository.GetDefaultLanguage(currentUserId);
            ViewBag.LangId = lan.LanguageId;
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();

            var currencyList = _curRepository.GetAllActiveCurrency();
            ViewBag.CurrencyList = currencyList;
            ViewBag.DefCurrency = _curRepository.GetDefaultCurrency(currentUserId).ReturnValue.CurrencyId;

            var invoiceNumberEnum = _domainRepository.GetInvoiceNumberProcedureEnum();
            ViewBag.InvoiceNumberEnum = invoiceNumberEnum;

            return View(model);
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
                Result saveResult = await _domainRepository.AddDomain(dto);
                result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                : new { Status = "Error", saveResult.Message });
            }
            return result;

        }

        [HttpGet]
        public IActionResult HomePageDesign(string domainId)
        {
            var temList = _moduleRepository.GetAllTemplate();
            ViewBag.TemplateList = temList;
            var moduleList = _moduleRepository.GetAllModules();
            ViewBag.ModuleList = moduleList;

            ViewBag.DomainId = domainId;
            return View();
        }

        /// <summary>
        /// id is domainId
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult GetSpecificTemplateView(string templateName, string id)
        {
            var template = _moduleRepository.FetchTemplateByName(templateName);
            var moduleList = _moduleRepository.GetAllModules();
            ViewBag.DomainId = id;
            ViewBag.ModuleList = moduleList;
            ViewBag.TemplateId = template.TemplateId;
            var viewName = $"_{templateName}.cshtml";
            return PartialView($"~/Views/Domain/{viewName}");
        }

       

        [HttpPost]
        public IActionResult StoreDesignPreview([FromBody] List<KeyVal> parameters)
        {
            var key = Guid.NewGuid().ToString();
            HttpContext.Session.SetComplexData(key, parameters);

            return Json(new { key = key });

        }

        [HttpGet]
        public IActionResult DesignPreview(string key, string id)
        {
            var template = _moduleRepository.FetchTemplateById(id);
            List<KeyVal> datas = HttpContext.Session.GetComplexData<List<KeyVal>>(key);

            switch (template.TemplateName)
            {
                case "TemplateNumber1":
                    var first = datas.FirstOrDefault(_ => _.Key == "[0]").Val;
                    var second = datas.FirstOrDefault(_ => _.Key =="[1]").Val;
                    var third = datas.FirstOrDefault(_ => _.Key == "[2]").Val;
                    template.HtmlContent = template.HtmlContent.Replace("[0]", first);
                    template.HtmlContent = template.HtmlContent.Replace("[1]", second);
                    template.HtmlContent = template.HtmlContent.Replace("[2]", third);
                    break;
                case "TemplateNumber2":
                    break;
                default:
                    break;
            }
            return View(template);
        }

        [HttpGet]
        public IActionResult GetProductModuleViewComponent(ProductOrContentType productType, ProductTemplateDesign selectionTemplate, int count)
        {
            return ViewComponent("SpecialProduct", new { productType, selectionTemplate, count });
        }


        [HttpPost]
        public async  Task<IActionResult> SendModuleParams([FromBody]DomainPageModel model)
        {
            var domainDto = _domainRepository.FetchDomain(model.DomainId).ReturnValue;
            switch (model.PageType)
            {
                case PageType.MainPage:
                    domainDto.MainPageTemplateId = model.TemplateId;
                    domainDto.MainPageTemplateParamsValue = model.ParamsValue;
                    domainDto.MainPageModuleParamsWithValues = model.ModuleParams;
                    break;
                case PageType.contentPage:
                    domainDto.ContentTemplateId = model.TemplateId;
                    domainDto.ContentTemplateParamsValue = model.ParamsValue;
                    domainDto.ContentModuleParamsWithValues = model.ModuleParams;
                    break;
                case PageType.ProductPage:
                    domainDto.ProductTemplateId = model.TemplateId;
                    domainDto.ProductTemplateParamsValue = model.ParamsValue;
                    domainDto.ProductModuleParamsWithValues = model.ModuleParams;
                    break;
                default:
                    break;
            }
            var res = await _domainRepository.EditDomain(domainDto);
            return Json(res.Succeeded ? new { Status = "Success", res.Message }
                 : new { Status = "Error", res.Message });
        }


        [HttpGet]
        public async Task<IActionResult> GetPageFromDomain(string domainId, PageType pageType)
        {
            var domainEntity = _domainRepository.FetchDomain(domainId).ReturnValue;
            Template templateEntity = new Template();
            var allModules = _moduleRepository.GetAllModuleList();
            switch (pageType)
            {
                case PageType.MainPage:
                    templateEntity = _moduleRepository.FetchTemplateById(domainEntity.MainPageTemplateId);
                    foreach (var par in domainEntity.MainPageTemplateParamsValue)
                    {
                        string htmlPart = "";
                        var moduleList = par.Val.Split("<br/>");
                        foreach (var moduleId in moduleList)
                        {
                            var moduleEntity = allModules.FirstOrDefault(_ => _.ModuleId == moduleId);
                            var moduleParameters = domainEntity.MainPageModuleParamsWithValues
                                .FirstOrDefault(_ => _.Place == par.Key && _.ModuleId == moduleId);
                            switch (moduleEntity.ModuleCategoryType)
                            {
                                //case ModuleCategoryType.Advertisement:
                                //    break;
                                //case ModuleCategoryType.ImageSlider:
                                //    break;
                                case ModuleCategoryType.Product:
                                    htmlPart += await RenderViewComponent("SpecialProduct", moduleParameters.ParamsValue);
                                    break;
                                case ModuleCategoryType.Content:
                                    htmlPart += await RenderViewComponent("ContentTemplates", moduleParameters.ParamsValue);
                                    break;
                                default:
                                    break;
                            }
                            htmlPart += "<br/>";
                        }
                        templateEntity.HtmlContent = templateEntity.HtmlContent.Replace(par.Key, htmlPart);
                    }
                    break;
                //case PageType.contentPage:
                //    break;
                //case PageType.ProductPage:
                //    break;
                default:
                    break;
            }
            return View(templateEntity);
        }

        [HttpGet]
        public IActionResult GetContentModuleViewComponent(ProductOrContentType contentType, ContentTemplateDesign selectionTemplate, int? count)
        {
            return ViewComponent("ContentTemplate", new { contentType, selectionTemplate, count });
        }

        public IActionResult GetSpecificModule(string moduleName, string id)
        {
            var module = _moduleRepository.FetchModuleByName(moduleName);
            var viewName = $"_{moduleName}.cshtml";
            var imageTemplatePath = _webHostEnvironment.WebRootPath;
            var productOrContentTypes = _moduleRepository.GetAllProductOrContentTypes();
            ViewBag.ProductOrContentTypeList = productOrContentTypes;
            ViewBag.DomainId=id;
            switch (moduleName.ToLower())
            {
                case "productlist":
                    var productTemplateList = _moduleRepository.GetAllProductTemplateDesign();
                   
                    ViewBag.ProductTemplateList = productTemplateList;
                    break;
                case "contentlist":
                    var contentTemplateDesigns = _moduleRepository.GetAllContentTemplateDesign();
                    foreach (var item in contentTemplateDesigns)
                    {
                        if (item.Text.ToLower() != "forth")
                        {
                            item.ImageUrl = System.IO.Path.Combine(imageTemplatePath, item.ImageUrl);
                        }
                        else
                        {
                            if (CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                            {
                                item.ImageUrl = Path.Combine(imageTemplatePath, "Template/forth-rtl.jpg");
                            }
                            else
                            {
                                item.ImageUrl = Path.Combine(imageTemplatePath, "Template/forth-ltr.jpg");
                            }
                        }
                    }
                    ViewBag.ContentTemplateList = contentTemplateDesigns;
                    break;
                default:
                    break;
            }
            return PartialView($"~/Views/Domain/{viewName}");
        }


        [HttpPost]
        public IActionResult SanitizeCkEditorContent([FromBody]string html)
        {
            var res = Helpers.HtmlSanitizer.SanitizeHtml(html);
            return Json(new { isvalid = res });
        }

        [HttpGet]
        public IActionResult ContentPageDesign(string domainId)
        {
            return View();
        }

        [HttpGet]
        public IActionResult ProductPageDesign(string domainId)
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetProviderParams([FromQuery] int pspVal)
        {
            var htmlResult = "";
            var psp = (PspType)pspVal;
            switch (psp)
            {
                case PspType.IranKish:
                    htmlResult = GenerateForm(typeof(IrankishModel).GetProperties());
                    break;
                case PspType.Saman:
                    htmlResult = GenerateForm(typeof(SamanModel).GetProperties());
                    break;
                case PspType.Parsian:
                    htmlResult = GenerateForm(typeof(ParsianModel).GetProperties());
                    break;
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


                Result saveResult = await _domainRepository.EditDomain(dto);

                result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                 : new { Status = "Error", saveResult.Message });
            }
           
            return result;
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            Result opResult = await _domainRepository.DeleteDomain(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }


        [HttpGet]
        public IActionResult GetRowWithSelectedColumns(string count, string rn)
        {

            var moduleList = _moduleRepository.GetAllModules();
            ViewBag.ModuleList = moduleList;

            var viewName = "";
            switch(count)
            {
                case "1":
                    viewName = "_OneColumn.cshtml";
                    break;
                case "2":
                    viewName = "_TwoColumns.cshtml";
                    break;
                case "3":
                    viewName = "_ThreeColumns.cshtml";
                    break;
                case "4":
                    viewName = "_FourColumns.cshtml";
                    break;
                case "6":
                    viewName = "_SixColumns.cshtml";
                    break;
              
            }
            ViewBag.RowNumber = rn;
            return PartialView(viewName);
        }
    }
}
