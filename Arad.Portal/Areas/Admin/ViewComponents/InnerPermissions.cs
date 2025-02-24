using Arad.Portal.Models.Shared.Permission;
using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.Areas.Admin.ViewComponents;

public class InnerPermissions : ViewComponent
{
    public IViewComponentResult Invoke(ListPermissions model)
    {
        return View(model);
    }
}