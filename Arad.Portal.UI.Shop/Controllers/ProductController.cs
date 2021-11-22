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
        public ProductController(IProductRepository productRepository, IHttpContextAccessor accessor)
        {
            _productRepository = productRepository;
            _accessor = accessor;
        }
        public IActionResult Index()
        {
            return View();
        }

       [Route("{language}/product/{**slug}")]
        public IActionResult Details(long slug)
        {
            var domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            var lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
            var entity = _productRepository.FetchByCode(slug);
            foreach (var item in entity.Images)
            {
                var obj = new PhotoCraddleModel()
                {
                   original = $"../FileManager/GetScaledImage?path={item.Url}&height={2000}",
                   preview = $"../FileManager/GetScaledImage?path={item.Url}&height={500}",
                   thumbnail=$"../FileManager/GetScaledImage?path={item.Url}&height={100}",
                   title = item.Title
                };
                entity.Sliders.Add(obj);
            }
            return View(entity);
        }
    }
}
