using System.Collections.Generic;

using Arad.Portal.DataLayer.Models.Shared.ProgressBar;

using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.Areas.Shared.ViewComponents;

public class ProgressBarsViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(List<ProgressBar> progressBars)
    {
        // Initialize the ViewModel directly in the return statement
        return View("~/Areas/Shared/Views/Shared/Components/ProgressBars/Default.cshtml", new ProgressBarViewModel { ProgressBars = progressBars });
    }
}