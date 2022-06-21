using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.SliderModule;
using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
    public class LoginProfile : ViewComponent
    {
        private readonly IDomainRepository _domainRepository;
        public LoginProfile(IDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public IViewComponentResult Invoke(string domainId, bool isShop)
        {
            var domainObj = _domainRepository.FetchDomain(domainId).ReturnValue;
            domainObj.IsShop = isShop;
            _domainRepository.EditDomain(domainObj);

            return View("Default", domainObj.IsShop);
        }
    }
}
