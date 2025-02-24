using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.Areas.Admin.ViewComponents;

public class LoadingGridData : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View("Default");

    }
}