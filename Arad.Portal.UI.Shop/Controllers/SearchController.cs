using Arad.Portal.DataLayer.Contracts.General.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Contracts.General.Content;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Models.Shared;
using System.Globalization;
using Arad.Portal.DataLayer.Services;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class SearchController : BaseController
    {
        private readonly IDomainRepository _domainRepository;
        private readonly ILanguageRepository _lanRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ICurrencyRepository _curRepository;
        private readonly ILuceneService _luceneService;
        public SearchController(
            ILanguageRepository lanRepository, ICurrencyRepository curRepository,ILuceneService luceneService,
            IDomainRepository domainRepository, IHttpContextAccessor accessor) :base(accessor, domainRepository)
        {
            _domainRepository = domainRepository;
            _luceneService = luceneService;
            _lanRepository = lanRepository;
            _curRepository = curRepository;
            _accessor = accessor;
        }
       
        
        //[HttpGet]
        //[Route("{language}/search")]
        //public async Task<IActionResult> Index([FromQuery] string f) 
        //{
        //    List<GeneralSearchResult> lst = new List<GeneralSearchResult>();
        //    var domainEntity = _domainRepository.FetchByName(this.DomainName, false).ReturnValue;
        //    string lanIcon;
        //    string currencyId = string.Empty;
        //    if (CultureInfo.CurrentCulture.Name != null)
        //    {
        //        lanIcon = CultureInfo.CurrentCulture.Name;
        //        var ri = new RegionInfo(System.Threading.Thread.CurrentThread.CurrentUICulture.LCID);
        //        var curSymbol = ri.ISOCurrencySymbol;
        //        var currencyDto = _curRepository.GetCurrencyByItsPrefix(curSymbol);
        //        currencyId = currencyDto.CurrencyId;
        //    }
        //    else
        //    {
        //        lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
        //        currencyId = domainEntity.DefaultCurrencyId;
        //    }
        //    var lanId = _lanRepository.FetchBySymbol(lanIcon);
        //    lst = (await _productRepository.GeneralSearch(f, lanId, currencyId, domainEntity.DomainId)).ReturnValue;
        //    lst.AddRange((await _contentRepository.GeneralSearch(f, lanId, currencyId, domainEntity.DomainId)).ReturnValue);

        //    return new JsonResult(new { data = lst });
        //}
    }
}
