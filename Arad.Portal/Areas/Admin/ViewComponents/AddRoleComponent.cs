using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.Areas.Admin.ViewComponents;

public class AddRoleComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();

    }

}