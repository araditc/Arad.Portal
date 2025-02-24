using Microsoft.AspNetCore.Mvc;


namespace Arad.Portal.Areas.Admin.ViewComponents;

public class AddUserComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View("Default");

    }
}