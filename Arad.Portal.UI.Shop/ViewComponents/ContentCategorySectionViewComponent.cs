using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class ContentCategorySectionViewComponent : ViewComponent
    {
        private readonly IContentCategoryRepository _categoryRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILanguageRepository _lanRepository;
        private readonly IDomainRepository _domainRepository;
        public ContentCategorySectionViewComponent(IContentCategoryRepository categoryRepository,IHttpContextAccessor accessor,
            IDomainRepository domainRepository, ILanguageRepository languageRepository)
        {
            _categoryRepository = categoryRepository;
            _lanRepository = languageRepository;
            _domainRepository = domainRepository;
            _accessor = accessor;
        }

        public async Task<IViewComponentResult> InvokeAsync(CategorySection categorySection)
        {
            var result = new CommonViewModel();
            var domainName = _domainRepository.GetDomainName();

            result.Categories = _categoryRepository.GetDirectChildrens(categorySection.ContentCategoryId,
                categorySection.CountToTake, categorySection.CountToSkip);

            
            ViewBag.CurLangId = categorySection.DefaultLanguageId;
            return View(result);
        }
    }
}
