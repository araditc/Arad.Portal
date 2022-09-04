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

namespace Arad.Portal.UI.Shop.Controllers
{
    public class SearchController : BaseController
    {
        private readonly IProductRepository _productRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly ILanguageRepository _lanRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ICurrencyRepository _curRepository;
        public SearchController(IProductRepository productRepository, IContentRepository contentRepository,
            ILanguageRepository lanRepository, ICurrencyRepository curRepository,
            IDomainRepository domainRepository, IHttpContextAccessor accessor) :base(accessor, domainRepository)
        {
            _productRepository = productRepository;
            _contentRepository = contentRepository;
            _domainRepository = domainRepository;
            _lanRepository = lanRepository;
            _curRepository = curRepository;
            _accessor = accessor;
        }
        /// <summary>
        /// this method were called first time that user enter any keyword an press search button
        /// </summary>
        /// <param name="keyword">this is the exact keyword that user enter in search</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{language}/search/initial")]
        public async Task<IActionResult> Initial(string keyword)
        {

        }

        /// <summary>
        /// this method were called after suggestion keywords for search
        /// then it can be just a keyword or a keyword in specigic category or group or a similar keyword to initial Keyword
        /// </summary>
        /// <param name="f">it is the keyword for search it can be exact initial keyword
        /// (first keyword that user enter without any changes) or can be a keyword from or suggestion list</param>
        /// <param name="categoryId">if this category specified then we look in contents with this specific categoryId</param>
        /// <param name="groupId">if this groupId specified then we look in products with specific groups</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{language}/search")]
        public async Task<IActionResult> Index(string f, 
            string contentCategoryName = "", string productGroupName = "")
        {
            List<GeneralSearchResult> lst = new List<GeneralSearchResult>();
            var domainEntity = _domainRepository.FetchByName(this.DomainName, false).ReturnValue;
            string lanIcon;
            string currencyId = string.Empty;
            if (CultureInfo.CurrentCulture.Name != null)
            {
                lanIcon = CultureInfo.CurrentCulture.Name;
                var ri = new RegionInfo(System.Threading.Thread.CurrentThread.CurrentUICulture.LCID);
                var curSymbol = ri.ISOCurrencySymbol;
                var currencyDto = _curRepository.GetCurrencyByItsPrefix(curSymbol);
                currencyId = currencyDto.CurrencyId;
            }
            else
            {
                lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
                currencyId = domainEntity.DefaultCurrencyId;
            }
            var lanId = _lanRepository.FetchBySymbol(lanIcon);
            lst = (await _productRepository.GeneralSearch(f, lanId, currencyId, domainEntity.DomainId)).ReturnValue;
            lst.AddRange((await _contentRepository.GeneralSearch(f, lanId, currencyId, domainEntity.DomainId)).ReturnValue);

            return new JsonResult(new { data = lst });
        }
    }
}
