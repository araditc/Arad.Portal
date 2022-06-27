using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.UI.Shop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Contracts.General.Content;
using System.Security.Claims;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.Extensions.Configuration;
using System.IO;
using Arad.Portal.UI.Shop.Helpers;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDomainRepository _domainRepository;
        private readonly ILanguageRepository _lanRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly IConfiguration _configuration;
        public HomeController(ILogger<HomeController> logger,
            IHttpContextAccessor accessor,
            ILanguageRepository lanRepo,
            IWebHostEnvironment env,
            IConfiguration config,
            IDomainRepository domainRepository) : base(accessor, env)
        {
            _logger = logger;
            _domainRepository = domainRepository;
            _lanRepository = lanRepo;
            _accessor = accessor;
            _configuration = config;
        }

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("{language}/ckEditorContentImages/{**slug}")]
        //public IActionResult GetCkEditorContentImages(string slug)
        //{
        //    var path = $"/ckEditorContentImages/{slug}";
        //    (byte[] fileContents, string mimeType) = GetImageWithActualSize(path);
        //    return File(fileContents, mimeType);
        //}

        private (byte[], string) GetImageWithActualSize(string path)
        {
            var localStaticFileStorage = _configuration["LocalStaticFileStorage"];
            string finalPath;
            if (!string.IsNullOrWhiteSpace(path))
            {
                if (path.StartsWith("/"))
                    path = path[1..];
                finalPath = Path.Combine(localStaticFileStorage, path).Replace("\\", "/");

                if (!System.IO.File.Exists(finalPath))
                {
                    finalPath = "/imgs/NoImage.png";
                }
                var fileName = Path.GetFileName(finalPath);
                var mimeType = ImageFunctions.GetMIMEType(fileName);
                byte[] fileContent = System.IO.File.ReadAllBytes(finalPath);
                return (fileContent, mimeType);
            }
            else
            {
                finalPath = "/imgs/NoImage.png";
                var fileName = Path.GetFileName(finalPath);
                var mimeType = ImageFunctions.GetMIMEType(fileName);
                byte[] fileContent = System.IO.File.ReadAllBytes(finalPath);
                return (fileContent, mimeType);
            }
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            
            var result = _domainRepository.FetchByName(DomainName, false);
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            var lanId = _lanRepository.FetchBySymbol(lanIcon);

            if (result.Succeeded )
            {
                if(!result.ReturnValue.IsMultiLinguals) //single language
                {
                    var lan = result.ReturnValue.DefaultLanguageId;
                    var lanEntity = _lanRepository.FetchLanguage(lan);
                    Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(lanEntity.Symbol))
                    , new CookieOptions()
                        {
                            Expires = DateTimeOffset.Now.AddYears(1),
                            Domain = result.ReturnValue.DomainName
                        });
                }


                if(result.ReturnValue.HomePageDesign.Any(_=>_.LanguageId == lanId))
                {
                    var m = result.ReturnValue.HomePageDesign.FirstOrDefault(_ => _.LanguageId == lanId);
                    return View(m.MainPageContainerPart);
                }
                else
                {
                    return View((new DataLayer.Models.DesignStructure.MainPageContentPart()));
                }
            }
            else
            {
                return View(new DataLayer.Models.DesignStructure.MainPageContentPart());
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }



        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
