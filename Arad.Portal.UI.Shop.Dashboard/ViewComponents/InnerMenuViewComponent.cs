using Arad.Portal.DataLayer.Models.Permission;
using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
    public class InnerMenuViewComponent : ViewComponent
    {

        public IViewComponentResult Invoke(PermissionTreeViewDto model)
        {
            return View(model);
        }
    }
}
