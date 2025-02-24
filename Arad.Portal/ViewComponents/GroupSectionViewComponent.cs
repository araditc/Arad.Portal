using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductGroup;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Arad.Portal.ViewComponents;

public class GroupSectionViewComponent : ViewComponent
{
    private readonly IProductGroupRepository _groupRepository;
    private readonly IHttpContextAccessor _accessor;
    private readonly ILanguageRepository _lanRepository;
    private readonly ControllerHelper _controllerHelper;
    private readonly IDomainRepository _domainRepository;
    public GroupSectionViewComponent(IProductGroupRepository groupRepository, IHttpContextAccessor accessor,
                                     IDomainRepository domainRepository, ILanguageRepository languageRepository, ControllerHelper controllerHelper)
    {
        _groupRepository = groupRepository;
        _lanRepository = languageRepository;
        _controllerHelper = controllerHelper;
        _domainRepository = domainRepository;
        _accessor = accessor;
    }

    public async Task<IViewComponentResult> InvokeAsync(GroupSection groupSection)
    {
        CommonViewModel result = new CommonViewModel();
        string domainName = _controllerHelper.GetCurrentDomainName();
        //if(groupSection.GroupsWithProducts == null)
        //{
        //    groupSection.GroupsWithProducts = new List<string>();
        //    groupSection.GroupsWithProducts = await _groupRepository.AllGroupIdsWhichEndInProducts(domainName);
        //}
        //if (groupSection.TotalCount == 0)
        //{
        //    groupSection.TotalCount = await _groupRepository
        //        .GetDircetChildrenCount(domainName, groupSection.ProductGroupId, groupSection.GroupsWithProducts);
        //}
        //  result.Groups = _groupRepository.GetsDirectChildren(groupSection.GroupsWithProducts, domainName, groupSection.ProductGroupId,
        //    groupSection.CountToTake, groupSection.CountToSkip);

        //groupSection.CountToSkip += 4;
        //groupSection.CountToTake = 4;
        //result.GroupSection = groupSection;
        ViewBag.CurLangId = groupSection.DefaultLanguageId;
        return View(result);
    }
}