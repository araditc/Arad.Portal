using Arad.Portal.Models.Shared.Permission;
using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.Areas.Admin.ViewComponents;

public class InnerMenuViewComponent : ViewComponent
{

    public IViewComponentResult Invoke(PermissionTreeViewDto model)
    {
        return View("~/Areas/Admin/Views/Shared/Components/InnerMenu/Default.cshtml",model);
    }
}