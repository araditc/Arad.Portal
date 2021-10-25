using Arad.Portal.DataLayer.Contracts.Shop.Product;
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
        public ProductController(IProductRepository productRepository, IHttpContextAccessor accessor)
        {
            _productRepository = productRepository;
            _accessor = accessor;
        }
        public IActionResult Index()
        {
            return View();
        }

       [Route("/Product/{slug}")]
        public IActionResult Details(string slug)
        {
            var domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            var entity = _productRepository.FetchProductWithSlug(slug, domainName);
            return View(entity);
        }
    }
}
