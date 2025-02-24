using System.Collections.Generic;

using Arad.Portal.DataLayer.Models.Shared.Counter;


using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.Areas.Shared.ViewComponents;

public class CounterViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(List<Counter> counters)
    {
        return View("~/Areas/Shared/Views/Shared/Components/Counter/Default.cshtml", counters);
    }
}