using Arad.Portal.DataLayer.Contracts.General.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;


namespace Arad.Portal.UI.Shop.Controllers
{
    public class SearchController : BaseController
    {

        private readonly IDomainRepository _domainRepository;
        public SearchController(IHttpContextAccessor accessor,IDomainRepository domainRepository):base(accessor, domainRepository)
        {
            _domainRepository = domainRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
