using Arad.Portal.DataLayer.Entities.General.SliderModule;
using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
    public class SliderLayer : ViewComponent
    {
        public IViewComponentResult Invoke(Layer model)
        {
            return View("Default", model);
        }
    }
}
