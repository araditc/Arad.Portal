using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Models.Shared;
using System.Globalization;
using Arad.Portal.DataLayer.Services;
using Microsoft.Extensions.Configuration;
using System.IO;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductGroup;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.ContentCategory;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using System.Threading;
using Arad.Portal.Controllers.Base;
using Arad.Portal.DataLayer.Entities.General.ContentCategory;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.Shop.ProductGroup;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;

namespace Arad.Portal.Controllers.Setting;

public class SearchController(
    ILanguageRepository lanRepository,
    LuceneService luceneService,
    IConfiguration configuration,
    IProductGroupRepository grpRepository,
    IContentCategoryRepository categoryRepository,
    IDomainRepository domainRepository,
    IHttpContextAccessor accessor,
    ControllerHelper controllerHelper)
    : BaseController(accessor, domainRepository, lanRepository)
{
    private readonly IHttpContextAccessor _accessor = accessor;

    [HttpGet]
    [Route("{language}/search/index2")]
    public async ValueTask<IActionResult> Index2(string key, CancellationToken cancellationToken)
    {
        List<LuceneSearchIndexModel> lst = [];
        try
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                Domain domainEntity = controllerHelper.FetchDomainByName(DomainName, false).ReturnValue;
                string lanIcon;

                if (CultureInfo.CurrentCulture.Name != null)
                {
                    lanIcon = CultureInfo.CurrentCulture.Name;
                }
                else
                {
                    lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
                }
                string mainPath = Path.Combine(configuration["LocalStaticFileStorage"], "LuceneIndexes", domainEntity.Id);
                List<string> searchDirectories =
                [
                    Path.Combine(mainPath, "Content")
                ];
                string lanId = controllerHelper.FetchLanguageBySymbol(lanIcon);
                //searchDirectories.Add(Path.Combine(mainPath, "Product", lanIcon));
                ViewBag.LanIcon = lanIcon;
                string msg = "";
                lst = luceneService.Search(key.Trim(), searchDirectories);
                if (lst.Count == 0)
                {
                    msg = UtilityLanguage.GetString("AlertAndMessage_NoInformationWasFound");
                }
                foreach (LuceneSearchIndexModel item in lst)
                {
                    foreach (string t in item.GroupIds)
                    {
                        string id = "";
                        //ProductGroupDTO grp = new ProductGroupDTO();
                        string name = "";
                        //ContentCategoryDTO cat = new ContentCategoryDTO();
                        if (item.IsProduct)
                        {
                            string groupId = t;
                            ProductGroup grp = controllerHelper.ProductGroupFetch(t);
                            id = grp.GroupCode.ToString();
                            name = grp.MultiLingualProperties.Any(p => p.LanguageId == lanId) ? grp.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId).Name : grp.MultiLingualProperties.FirstOrDefault().Name;

                        }
                        else
                        {
                            ContentCategory cat = await controllerHelper.ContentCategoryFetch(t, false);
                            if (!string.IsNullOrEmpty(cat.Id))
                            {
                                id = cat.CategoryCode.ToString();
                                name = cat.CategoryNames.Any(p => p.LanguageId == lanId) ? cat.CategoryNames.FirstOrDefault(p => p.LanguageId == lanId).Name : cat.CategoryNames.FirstOrDefault().Name;
                            }

                        }

                        if (id == "" || string.IsNullOrEmpty(name))
                        {
                            continue;
                        }

                        SuggestionObject suggest = new()
                                                   {
                                                       Phrase = $"{key} {UtilityLanguage.GetString("Search_IN")} {name}",
                                                       IsProduct = item.IsProduct,
                                                       UrlParameter = id
                                                   };
                        item.SuggestionObjs.Add(suggest);
                    }

                    ;
                }
                List<SuggestionObject> list = lst.SelectMany(m => m.SuggestionObjs).ToList();
                IEnumerable<SuggestionObject> lst2 = list.Distinct(new SuggestionObjectComparer());

                return Json(new { status = "success", data = lst2, message = msg });
            }
        }
        catch (Exception ex)
        {
            return Json(new { status = "error", message = UtilityLanguage.GetString(ConstMessages.GeneralError) });
        }
        return Json(new { status = "error", message = UtilityLanguage.GetString(ConstMessages.ObjectNotFound) });
    }
}