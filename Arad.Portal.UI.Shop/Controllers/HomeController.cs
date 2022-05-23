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

namespace Arad.Portal.UI.Shop.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDomainRepository _domainRepository;

        public HomeController(ILogger<HomeController> logger,
            IHttpContextAccessor accessor,
            IWebHostEnvironment env,
                              IDomainRepository domainRepository) : base(accessor, env)
        {
            _logger = logger;
            _domainRepository = domainRepository;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            //define current culture of user based on default domain culture
            var result = _domainRepository.FetchByName(DomainName, false);

            if (result.Succeeded)
            {
                return View(result.ReturnValue.MainPageContainerPart != null ?
               result.ReturnValue.MainPageContainerPart : new DataLayer.Models.DesignStructure.MainPageContentPart());
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
