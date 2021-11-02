using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class StoreMenu : ViewComponent
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDomainRepository _domainRepository;
        private readonly ILanguageRepository _languageRepository;
        public StoreMenu(IHttpContextAccessor accessor, UserManager<ApplicationUser> userManager, 
            IDomainRepository domainRepository, ILanguageRepository languageRepository)
        {
            _accessor = accessor;
            _userManager = userManager;
            _domainRepository = domainRepository;
            _languageRepository = languageRepository;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menues = new List
        }
    }
}
