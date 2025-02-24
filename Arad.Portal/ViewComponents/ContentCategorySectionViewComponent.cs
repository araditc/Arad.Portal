using Arad.Portal.DataLayer.Repositories.Interfaces.General.ContentCategory;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared.ContentCategory;
using Arad.Portal.Models.UI;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.General.Domain;

namespace Arad.Portal.ViewComponents;

public class ContentCategorySectionViewComponent(
    IContentCategoryRepository categoryRepository,
    IHttpContextAccessor accessor,
    IDomainRepository domainRepository,
    ILanguageRepository languageRepository,
    ControllerHelper controllerHelper,
    IMapper mapper)
    : ViewComponent
{

    public IViewComponentResult Invoke(CategorySection categorySection)
    {
        CommonViewModel result = new CommonViewModel();
        string domainName = controllerHelper.GetCurrentDomainName();
        Domain domain = domainRepository.FirstOrDefault(c => c.DomainName == "https://" + domainName);
        List<DataLayer.Entities.General.ContentCategory.ContentCategory> lst = categoryRepository
                                                                               .GetList(c => c.ParentCategoryId == categorySection.ContentCategoryId && !c.IsDeleted && c.AssociatedDomainId == domain.Id)
                                                                               .ToList();
        result.Categories = mapper.Map<List<ContentCategoryDto>>(lst);

        ViewBag.CurLangId = categorySection.DefaultLanguageId;
        return View(result);
    }
}