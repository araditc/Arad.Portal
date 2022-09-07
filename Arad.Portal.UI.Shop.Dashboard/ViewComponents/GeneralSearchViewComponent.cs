using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
    public class GeneralSearchViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string key)
        {
            return View(key);
        }
    }
}
