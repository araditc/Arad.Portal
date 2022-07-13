using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Contracts.General.Language;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class ContentCategoryController : BaseController
    {
        private readonly IContentCategoryRepository _categoryRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly IHttpContextAccessor _accessor;
      
        public ContentCategoryController(IContentCategoryRepository categoryRepository,
            IDomainRepository domainRepository,
            ILanguageRepository lanRepository,
            IHttpContextAccessor accessor):base(accessor, domainRepository)
        {
            _categoryRepository = categoryRepository;
            _domainRepository = domainRepository;
            _accessor = accessor;
            _languageRepository = lanRepository;
        }

        [Route("{language}/category")]
        public IActionResult Index()
        {
            ViewData["DomainTitle"] = this.DomainTitle;
            ViewData["PageTitle"] = GeneralLibrary.Utilities.Language.GetString("design_ArticleCategories");
            return View();
        }
        [Route("{language}/category/{**slug}")]
        public async  Task<IActionResult> Details(string slug)
        {
            //var domainEntity = _domainRepository.FetchByName(this.DomainName, true).ReturnValue;
            //var entity = _categoryRepository.FetchByCode(slug, domainEntity.DomainId);
            ////direct children in categories and contents in category in bloglist property
            //return View(entity);
            var domainEntity = _domainRepository.FetchByName(this.DomainName, true).ReturnValue;
            CommonViewModel model = new CommonViewModel();
            var path = _accessor.HttpContext.Request.Path.ToString();
            var lanIcon = path.Split("/")[1].ToLower();
            ViewData["DomainTitle"] = this.DomainTitle;
            ViewData["PageTitle"] = slug.Replace("-", " ");
            var langId = _languageRepository.FetchBySymbol(lanIcon);
            ViewData["CurLangId"] = langId;
            var id = _categoryRepository.FetchByCode(slug);
            if(!string.IsNullOrWhiteSpace(id))
            {
                var catSection = new CategorySection()
                {
                    ContentCategoryId = id,
                    CountToSkip = 0,
                    CountToTake = 4,
                    DefaultLanguageId = langId
                };
                catSection.CategoriesWithContent = await _categoryRepository.AllCategoryIdsWhichEndInContents(this.DomainName);
                model.CategorySection = catSection;

                var contentSection = new ContentsInCategorySection()
                {
                    ContentCategoryId = id,
                    CountToSkip = 0,
                    CountToTake = 4,
                    DefaultLanguageId = langId
                };
                model.ContentsInCategorySection = contentSection;
                //var model = _groupRepository.FetchByCode(slug);
                return View(model);
            }
            else
            {
                return Redirect($"~/{lanIcon}/ExceptionHandler/PageNotFound");
            }
           
        }

        [HttpPost]
        public IActionResult GetMyCategoryVC(CategorySection categorySection)
        {
            return ViewComponent("ContentCategorySection", categorySection);
        }

        public IActionResult GetContentsInCategoryVC(ContentsInCategorySection contentsInCategory)
        {
            return ViewComponent("ContentsInCategorySection", contentsInCategory);
        }
    }
}
