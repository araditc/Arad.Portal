using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Contracts.General.Domain;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class ContentCategoryController : BaseController
    {
        private readonly IContentCategoryRepository _categoryRepository;
        private readonly IDomainRepository _domainRepository;
      
        public ContentCategoryController(IContentCategoryRepository categoryRepository,
            IWebHostEnvironment env,
            IDomainRepository domainRepository,
            IHttpContextAccessor accessor):base(accessor, env)
        {
            _categoryRepository = categoryRepository;
            _domainRepository = domainRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Route("{language}/category/{**slug}")]
        public IActionResult Details(long slug)
        {
            var domainEntity = _domainRepository.FetchByName(this.DomainName, true).ReturnValue;
            var entity = _categoryRepository.FetchByCode(slug, domainEntity.DomainId);
            return View(entity);
        }
    }
}
