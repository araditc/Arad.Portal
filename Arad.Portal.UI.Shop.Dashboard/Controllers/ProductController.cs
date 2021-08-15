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

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductUnitRepository _unitRepository;
        private readonly IProductGroupRepository _productGroupRepository;
        private readonly IProductSpecGroupRepository _specGroupRepository;
        private readonly IPermissionView _permissionViewManager;
        private readonly ILanguageRepository _lanRepository;
        private readonly ICurrencyRepository _curRepository;
        public ProductController(
            IProductRepository productRepository, IPermissionView permissionView,
            ILanguageRepository languageRepository, IProductGroupRepository productGroupRepository,
            ICurrencyRepository currencyRepository, IProductUnitRepository unitRepository,
            IProductSpecGroupRepository specGroupRepository)
        {
            _productRepository = productRepository;
            _permissionViewManager = permissionView;
            _lanRepository = languageRepository;
            _curRepository = currencyRepository;
            _productGroupRepository = productGroupRepository;
            _unitRepository = unitRepository;
            _specGroupRepository = specGroupRepository;
        }
        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<ProductViewModel> result = new PagedItems<ProductViewModel>();
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            ViewBag.Permissions = dicKey;
            try
            {
                result = await _productRepository.List(Request.QueryString.ToString());
                var defLangId = _lanRepository.GetDefaultLanguage().LanguageId;
                ViewBag.DefLangId = defLangId;
                ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
               
                var groupList = _productGroupRepository.GetAlActiveProductGroup(defLangId);
                ViewBag.ProductGroupList = groupList;
               
                var unitList = _unitRepository.GetAllActiveProductUnit(defLangId);
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
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = await _productRepository.ProductFetch(id);
            }

            var lan = _lanRepository.GetDefaultLanguage();
            var specGroupList = _specGroupRepository.AllActiveSpecificationGroup(lan.LanguageId);
            specGroupList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.SpecificationGroupList = specGroupList;

            var groupList = _productGroupRepository.GetAlActiveProductGroup(lan.LanguageId);
            groupList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.ProductGroupList = groupList;

            var currencyList = _curRepository.GetAllActiveCurrency();
            currencyList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.CurrencyList = currencyList;

            ViewBag.LangId = lan.LanguageId;
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            return View(model);

        }

    }
}
