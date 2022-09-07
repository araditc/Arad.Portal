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
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class SearchController : BaseController
    {
        private readonly IDomainRepository _domainRepository;
        private readonly ILanguageRepository _lanRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILuceneService _luceneService;
        private readonly IConfiguration _configuration;
        public SearchController(
            ILanguageRepository lanRepository, ILuceneService luceneService,
            IConfiguration configuration,
            IDomainRepository domainRepository, IHttpContextAccessor accessor) :base(accessor, domainRepository)
        {
            _domainRepository = domainRepository;
            _luceneService = luceneService;
            _lanRepository = lanRepository;
            _configuration = configuration;
            _accessor = accessor;
        }

        [HttpGet]
        [Route("{language}/search")]
        public IActionResult Index([FromQuery] string key)
        {
            List<LuceneSearchIndexModel> lst = new List<LuceneSearchIndexModel>();
            var domainEntity = _domainRepository.FetchByName(this.DomainName, false).ReturnValue;
            string lanIcon;
           // string currencyId = string.Empty;
            if (CultureInfo.CurrentCulture.Name != null)
            {
                lanIcon = CultureInfo.CurrentCulture.Name;
            }
            else
            {
                lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
            }
            var mainPath = Path.Combine(_configuration["LocalStaticFileStorage"], "LuceneIndexes", domainEntity.DomainId);
            List<string> searchDirectories = new List<string>();
            searchDirectories.Add(Path.Combine(mainPath, "Content"));
            searchDirectories.Add(Path.Combine(mainPath, "Product", lanIcon));

            lst = _luceneService.Search(f.Trim(), searchDirectories);

            return new JsonResult(new { data = lst });
        }
    }
}
