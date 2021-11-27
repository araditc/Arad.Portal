using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Models.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILanguageRepository _lanRepository;
        private readonly IDomainRepository _domainRepository;
     
        public ProductController(IProductRepository productRepository, IHttpContextAccessor accessor,
            ILanguageRepository lanRepository, IDomainRepository domainRepository)
        {
            _productRepository = productRepository;
            _accessor = accessor;
            _lanRepository = lanRepository;
            _domainRepository = domainRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

       [Route("{language}/product/{**slug}")]
        public IActionResult Details(long slug)
        {
            var domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            var domainEntity = _domainRepository.FetchByName(domainName);
            var lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
            var entity = _productRepository.FetchByCode(slug, domainEntity.ReturnValue);
            var lanId = _lanRepository.FetchBySymbol(lanIcon);
            ViewBag.CurCurrencyId = domainEntity.ReturnValue.DefaultCurrencyId;
            ViewBag.CurLanguageId = lanId;
            return View(entity);
        }
    }
}
