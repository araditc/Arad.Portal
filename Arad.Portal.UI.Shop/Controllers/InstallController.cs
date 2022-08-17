using Arad.Portal.DataLayer.Contracts.General.Content;
using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Menu;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Repositories.General.User.Mongo;
using MongoDB.Driver;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.Currency;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class InstallController : BaseController
    {
        private readonly IDomainRepository _domainRepository;
        private readonly IMapper _mapper;
        private readonly IMenuRepository _menuRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IContentCategoryRepository _contentCategoryRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IHttpContextAccessor _accessor;
        private AppSetting _appSetting = new AppSetting();
        private bool isReadyToStart = false;
       
        public InstallController(IDomainRepository domainRepository, IMenuRepository menuRepository, 
                                 UserManager<ApplicationUser> userManager, IHttpContextAccessor accessor,
                                 ILanguageRepository lanRepository, ICurrencyRepository curRepository,
                                 IMapper mapper, 
                                 IContentCategoryRepository categoryRepository, IContentRepository contentRepository ):base(accessor, domainRepository)
        {
            _domainRepository = domainRepository;
            _userManager = userManager;
            _contentCategoryRepository = categoryRepository;
            _contentRepository = contentRepository;
            _menuRepository = menuRepository;
            _mapper = mapper;
            _languageRepository = lanRepository;
            _currencyRepository = curRepository;
            _accessor = accessor;
        }
        public IActionResult Index()
        {
            var model = new InstallModel();
            _appSetting = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build().Get<AppSetting>();
            model.AppSetting = _appSetting;
            var defaultDomainResult = _domainRepository.FetchDefaultDomain();
            if(defaultDomainResult.Succeeded)
            {
                model.DefaultDomain = _mapper.Map<Domain>(defaultDomainResult.ReturnValue);
            }
            var sysAccountUser = _userManager.Users.FirstOrDefault(_ => _.IsSystemAccount);
            if(sysAccountUser != null)
            {
                model.SystemAccountUser = sysAccountUser;
            }
            var currencyList = _currencyRepository.GetAllActiveCurrency();
            ViewBag.CurrencyList = currencyList;
            ViewBag.LangList = _languageRepository.GetAllActiveLanguage();
            return View(model);
        }

        //public IActionResult SaveData()
        //{

        //}
    }
}
