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
    public class GroupSectionViewComponent : ViewComponent
    {
        private readonly IProductGroupRepository _groupRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILanguageRepository _lanRepository;
        private readonly IDomainRepository _domainRepository;
        public GroupSectionViewComponent(IProductGroupRepository groupRepository,IHttpContextAccessor accessor,
            IDomainRepository domainRepository, ILanguageRepository languageRepository)
        {
            _groupRepository = groupRepository;
            _lanRepository = languageRepository;
            _domainRepository = domainRepository;
            _accessor = accessor;
        }

        public async Task<IViewComponentResult> InvokeAsync(GroupSection groupSection)
        {
            var result = new CommonViewModel();
            var domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
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
            result.Groups = _groupRepository.GetsDirectChildren(groupSection.GroupsWithProducts, domainName, groupSection.ProductGroupId,
                groupSection.CountToTake, groupSection.CountToSkip);

            //groupSection.CountToSkip += 4;
            //groupSection.CountToTake = 4;
            //result.GroupSection = groupSection;
            ViewBag.CurLangId = groupSection.DefaultLanguageId;
            return View(result);
        }
    }
}
