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

namespace Arad.Portal.UI.Shop.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDomainRepository _domainRepository;
      
        public HomeController(ILogger<HomeController> logger, IHttpContextAccessor accessor,
                              IDomainRepository domainRepository) :base(accessor)
        {
            _logger = logger;
            _domainRepository = domainRepository;
        }

        public IActionResult Index()
        {
            //define current culture of user based on default domain culture
            var domainDto = _domainRepository.FetchByName(domainName).ReturnValue;
            if (CultureInfo.CurrentCulture.Name != domainDto.DefaultLangSymbol)
            {
                Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(domainDto.DefaultLangSymbol))
                    , new CookieOptions()
                    {
                        Expires = DateTimeOffset.Now.AddYears(1)
                    });
            }
            return View();
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
