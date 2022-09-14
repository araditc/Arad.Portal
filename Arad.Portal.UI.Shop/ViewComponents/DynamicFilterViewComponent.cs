using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class DynamicFilterViewComponent : ViewComponent
    {
        private readonly ILanguageRepository _lanRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly IProductGroupRepository _groupRepository;
        public DynamicFilterViewComponent(ILanguageRepository languageRepository,
            IHttpContextAccessor accessor, IProductGroupRepository groupRepository,
            IDomainRepository domainRepository)
        {
            _lanRepository = languageRepository;
            _domainRepository = domainRepository;
            _accessor = accessor;
            _groupRepository = groupRepository;
        }
        public async Task<IViewComponentResult> InvokeAsync(ModelOutputFilter filter, string groupId = null)
        {
            string domainName = $"{_accessor.HttpContext.Request.Host}";
            var domainEntity = _domainRepository.FetchByName(domainName, false).ReturnValue;
            var lanId = _lanRepository.FetchBySymbol(CultureInfo.CurrentCulture.Name);
            ViewBag.ProductGroupList = _groupRepository.GetSubGroups(lanId, domainEntity.DomainId, groupId);
            return View(filter);
        }
    }
}
