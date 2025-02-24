using Arad.Portal.DataLayer.Repositories.Interfaces.General.Currency;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductGroup;
using Arad.Portal.Models.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.Domain;

namespace Arad.Portal.ViewComponents;

public class ProductsInGroupSectionViewComponent : ViewComponent
{
    private readonly IProductGroupRepository _groupRepository;
    private readonly IHttpContextAccessor _accessor;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly ILanguageRepository _lanRepository;
    private readonly IDomainRepository _domainRepository;
    public ProductsInGroupSectionViewComponent(IProductGroupRepository groupRepository,
                                               ILanguageRepository lanRepository, IDomainRepository domainRepository,
                                               IHttpContextAccessor accessor, ICurrencyRepository currencyRepository)
    {
        _accessor = accessor;
        _groupRepository = groupRepository;
        _currencyRepository = currencyRepository;
        _lanRepository = lanRepository;
        _domainRepository = domainRepository;
    }

    public IViewComponentResult Invoke(ProductsInGroupSection productSection)
    {
        CommonViewModel result = new CommonViewModel();
        string domainName = $"{_accessor.HttpContext.Request.Host}";
        Domain domainEntity = _domainRepository.First(c => c.DomainName == domainName && c.IsDefault == true);
        //    result.ProductList = _groupRepository
        //       .GetLatestProductInThisGroup(domainName, productSection.ProductGroupId, productSection.CountToTake, productSection.CountToSkip);
        string defaultCulture = _accessor.HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
        string defLangSymbol;
        if (defaultCulture != null)
        {
            defLangSymbol = defaultCulture.Split("|")[0][2..];
        }
        else
        {
            string defLangId = domainEntity.DefaultLanguageId;
            defLangSymbol = _lanRepository.First(c => c.Id == defLangId).Symbol;
        }
        CultureInfo currentCultureInfo = new(defLangSymbol, false);
        RegionInfo ri = new RegionInfo(currentCultureInfo.LCID);
        string currencyPrefix = ri.ISOCurrencySymbol;
        Currency currencyDto = _currencyRepository.First(c => c.Prefix == currencyPrefix);
        ViewBag.CurrencySymbol = currencyDto.Symbol;

        ViewBag.CurLangId = productSection.DefaultLanguageId;
        return View(result);
    }
}