using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductGroup;
using Arad.Portal.Helpers.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.General.Domain;

namespace Arad.Portal.ViewComponents;

public class DynamicFilterViewComponent(
    ILanguageRepository languageRepository,
    IHttpContextAccessor accessor,
    IProductGroupRepository groupRepository,
    IDomainRepository domainRepository,
    ControllerHelper controllerHelper)
    : ViewComponent
{

    public async Task<IViewComponentResult> InvokeAsync(ModelOutputFilter filter, string groupId = null)
    {
        string domainName = $"{accessor.HttpContext.Request.Host}";
        Domain domainEntity = controllerHelper.FetchDomainByName(domainName, false).ReturnValue;
        string lanId = controllerHelper.FetchLanguageBySymbol(CultureInfo.CurrentCulture.Name);

        List<SelectListModel> result = [];
        if (!string.IsNullOrWhiteSpace(groupId))
        {
            result = (await groupRepository.GetListAsync(g => g.IsActive && !g.IsDeleted)).Select(g => new SelectListModel()
                                                                                                        {
                                                                                                            Value = g.Id,
                                                                                                            Text = g.MultiLingualProperties.Any(p => p.LanguageId == lanId) ?
                                                                                                                       g.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId).Name : g.MultiLingualProperties.FirstOrDefault().Name

                                                                                                        }).ToList();
        }
        else
        {
            result = (await groupRepository.GetListAsync(g => g.IsActive && !g.IsDeleted && g.ParentId == groupId)).Select(g => new SelectListModel()
                                                                                                                                 {
                                                                                                                                     Value = g.Id,
                                                                                                                                     Text = g.MultiLingualProperties.Any(p => p.LanguageId == lanId) ?
                                                                                                                                                g.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId).Name : g.MultiLingualProperties.FirstOrDefault().Name

                                                                                                                                 }).ToList();
        }
        ViewBag.ProductGroupList = result;
        return View(filter);
    }
}