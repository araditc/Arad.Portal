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
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Models.ProductGroup;
using Arad.Portal.DataLayer.Models.ContentCategory;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class SearchController : BaseController
    {
        private readonly IDomainRepository _domainRepository;
        private readonly ILanguageRepository _lanRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly LuceneService _luceneService;
        private readonly IConfiguration _configuration;
        private readonly IProductGroupRepository _groupRepository;
        private readonly IContentCategoryRepository _categoryRepository;
        public SearchController(
            ILanguageRepository lanRepository, LuceneService luceneService,
            IConfiguration configuration, IProductGroupRepository grpRepository, IContentCategoryRepository categoryRepository,
            IDomainRepository domainRepository, IHttpContextAccessor accessor) :base(accessor, domainRepository)
        {
            _domainRepository = domainRepository;
            _luceneService = luceneService;
            _lanRepository = lanRepository;
            _configuration = configuration;
            _groupRepository = grpRepository;
            _categoryRepository = categoryRepository;
            _accessor = accessor;
        }

        [HttpGet]
        [Route("{language}/search")]
        public async Task<IActionResult> Index([FromQuery] string key)
        {
            List<LuceneSearchIndexModel> lst = new List<LuceneSearchIndexModel>();
            try
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    var domainEntity = _domainRepository.FetchByName(this.DomainName, false).ReturnValue;
                    string lanIcon;

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
                    string lanId = _lanRepository.FetchBySymbol(lanIcon);
                    searchDirectories.Add(Path.Combine(mainPath, "Product", lanIcon));
                    ViewBag.LanIcon = lanIcon;
                    string msg = "";
                    lst = _luceneService.Search(key.Trim(), searchDirectories);
                    if (lst.Count == 0)
                    {
                        msg = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_NoInformationWasFound");
                    }
                    foreach (var item in lst)
                    {

                        for (var i = 0; i < item.GroupIds.Count; i++)
                        {
                            string id = "";
                            ProductGroupDTO grp = new ProductGroupDTO();
                            string name = string.Empty;
                            ContentCategoryDTO cat = new ContentCategoryDTO();
                            if (item.IsProduct)
                            {
                                grp = _groupRepository.ProductGroupFetch(item.GroupIds[i]);
                                id = grp.GroupCode.ToString();
                                name = grp.MultiLingualProperties.Any(_ => _.LanguageId == lanId) ? grp.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).Name : grp.MultiLingualProperties.FirstOrDefault().Name;

                            }
                            else
                            {
                                cat = await _categoryRepository.ContentCategoryFetch(item.GroupIds[i]);
                                id = cat.CategoryCode.ToString();
                                name = cat.CategoryNames.Any(_ => _.LanguageId == lanId) ? cat.CategoryNames.FirstOrDefault(_ => _.LanguageId == lanId).Name : cat.CategoryNames.FirstOrDefault().Name;
                            }
                            item.SuggestionObjs.Add(new SuggestionObject()
                            {

                                Phrase = $"{key} {GeneralLibrary.Utilities.Language.GetString("Search_IN")} {name}",
                                IsProduct = item.IsProduct,
                                UrlParameter = id
                            }) ;

                        };
                    }
                    var list = lst.SelectMany(_ => _.SuggestionObjs).ToList();
                    var lst2 = list.Distinct(new SuggestionObjectComparer());

                    return Json(new { status = "success", data = lst2, message = msg });
                }
            }
            catch (Exception ex)
            {
               
            }
            return Json(new { status = "error" });
        }
    }
}
