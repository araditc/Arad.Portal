using Arad.Portal.DataLayer.Entities.General.SliderModule;
using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.Areas.Shared.ViewComponents;

public class SliderLayer : ViewComponent
{
    public IViewComponentResult Invoke(Layer model)
    {
        return View("~/Areas/Shared/Views/Shared/Components/SliderLayer/Default.cshtml", model);
    }
}