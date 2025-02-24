using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.General.ContentCategory;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.BasicData;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Services;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.ContentCategory;
using Arad.Portal.Models.Shared.Domain;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Serilog;

namespace Arad.Portal.Areas.Admin.Controllers.Setting;

[Area("Admin")]
public class SearchController(
    ILanguageRepository lanRepository,
    LuceneService luceneService,
    IConfiguration configuration,
    ControllerHelper controllerHelper,
    ILogger logger,
    IMapper mapper,
    IBasicDataRepository basicDataRepository) : Controller
{
    [HttpGet]
    [Route("{language}/search")]
    public async ValueTask<IActionResult> Index([FromQuery] string key, CancellationToken cancellationToken)
    {
        Result<DomainDto> resultDomainDto = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            string domainId = userDb.Domains.FirstOrDefault(a => a.IsOwner)?.DomainId;

            if (!string.IsNullOrWhiteSpace(key))
            {
                Result<Domain> dbEntity = controllerHelper.FetchDomain(domainId);

                if (dbEntity == null)
                {
                    resultDomainDto.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
                }

                DomainDto dto = mapper.Map<DomainDto>(dbEntity);
                dto.SupportedLangId = (await basicDataRepository.GetAllAsync(cancellationToken))
                                      .Where(d => dbEntity != null && d.AssociatedDomainId == dbEntity.ReturnValue.Id && d.GroupKey == "SupportedCultures")
                                      .Select(d => new string(d.Value))
                                      .ToList();
                resultDomainDto.Succeeded = true;
                resultDomainDto.Message = ConstMessages.SuccessfullyDone;
                resultDomainDto.ReturnValue = dto;

                string lanIcon = CultureInfo.CurrentCulture.Name;
                string mainPath = Path.Combine(configuration["LocalStaticFileStorage"] ?? string.Empty, "LuceneIndexes", resultDomainDto.ReturnValue.Id);
                List<string> searchDirectories =
                [
                    Path.Combine(mainPath, "Content")
                ];
                string lanId = (await lanRepository.FirstOrDefaultAsync(c => c.Symbol == lanIcon, cancellationToken)).Id;
                searchDirectories.Add(Path.Combine(mainPath, "Product", lanIcon));
                ViewBag.LanIcon = lanIcon;
                string msg = "";
                List<LuceneSearchIndexModel> lst = luceneService.Search(key.Trim(), searchDirectories);

                if (lst.Count == 0)
                {
                    msg = UtilityLanguage.GetString("AlertAndMessage_NoInformationWasFound");
                }

                foreach (LuceneSearchIndexModel item in lst)
                {
                    foreach (string t in item.GroupIds)
                    {
                        string id = "";
                        string name = "";

                        if (item.IsProduct)
                        {
                            ProductGroup productGroupModel = controllerHelper.ProductGroupFetch(t);
                            id = productGroupModel.GroupCode.ToString();
                            name = productGroupModel.MultiLingualProperties.Any(p => p.LanguageId == lanId)
                                       ? productGroupModel.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId)?.Name
                                       : productGroupModel.MultiLingualProperties.FirstOrDefault()?.Name;
                        }
                        else
                        {
                            Task<ContentCategory> category = controllerHelper.ContentCategoryFetch(t);
                            ContentCategoryDto cat = mapper.Map<ContentCategoryDto>(category);

                            if (!string.IsNullOrEmpty(cat.Id))
                            {
                                id = cat.CategoryCode.ToString();
                                name = cat.CategoryNames.Any(p => p.LanguageId == lanId) ? cat.CategoryNames.FirstOrDefault(p => p.LanguageId == lanId)?.Name : cat.CategoryNames.FirstOrDefault()?.Name;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                        {
                            continue;
                        }

                        SuggestionObject suggest = new() { Phrase = $"{key} {UtilityLanguage.GetString("Search_IN")} {name}", IsProduct = item.IsProduct, UrlParameter = id };
                        item.SuggestionObjs.Add(suggest);
                    }

                }

                List<SuggestionObject> list = lst.SelectMany(m => m.SuggestionObjs).ToList();
                IEnumerable<SuggestionObject> lst2 = list.Distinct(new SuggestionObjectComparer());

                return Json(new { status = "success", data = lst2, message = msg });
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SearchController)}/{nameof(Index)}");
            resultDomainDto.Message = ConstMessages.ExceptionOccured;

            return Json(new { status = "error", message = UtilityLanguage.GetString(ConstMessages.GeneralError) });
        }

        return Json(new { status = "error", message = UtilityLanguage.GetString(ConstMessages.ObjectNotFound) });
    }
}