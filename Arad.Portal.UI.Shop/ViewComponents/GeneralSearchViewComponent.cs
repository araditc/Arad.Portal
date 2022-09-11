using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class GeneralSearchViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string filter="")
        {
            DataLayer.Models.Shared.Filter obj = new DataLayer.Models.Shared.Filter();
            obj.Keyword = filter;
            return View(obj);
        }
    }
}
