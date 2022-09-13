using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class DynamicFilterViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(List<DynamicFilter> filters)
        {
            return View(filters);
        }
    }
}
