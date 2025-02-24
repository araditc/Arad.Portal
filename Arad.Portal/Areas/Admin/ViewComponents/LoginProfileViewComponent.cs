using Arad.Portal.DataLayer.Repositories.Interfaces.General.BasicData;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared.Domain;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.General.Domain;

namespace Arad.Portal.Areas.Admin.ViewComponents;

public class LoginProfileViewComponent(IDomainRepository domainRepository, ControllerHelper controllerHelper, IMapper mapper, IBasicDataRepository basicDataRepository)
    : ViewComponent
{
    private readonly ControllerHelper _controllerHelper = controllerHelper;

    public async Task<IViewComponentResult> InvokeAsync(string domainId, bool isShop, CancellationToken cancellationToken)
    {
        Domain domainObj = await domainRepository.FirstOrDefaultAsync(c => c.Id == domainId, cancellationToken);
        if (domainObj == null)
        {
            // Handle the case where the domain object is not found
            // return View("Default", null); // You might want to return an error view here
        }

        DomainDto dto = mapper.Map<DomainDto>(domainObj);
        dto.SupportedLangId = (await basicDataRepository.GetListAsync(d => d.AssociatedDomainId == domainObj.Id && d.GroupKey == "SupportedCultures", cancellationToken))
                                                  .Select(d => d.Value)
                                                  .ToList();

        domainObj.IsShop = isShop;
        await domainRepository.UpdateAsync(domainObj, cancellationToken);

        return View("Default", domainObj.IsShop); // Adjust the model you pass to the view as needed
    }
}