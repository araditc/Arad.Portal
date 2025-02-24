using System.Collections.Generic;

using Arad.Portal.DataLayer.Models.Shared.DesignStructure;

using Microsoft.AspNetCore.Mvc;

using System.Linq;
using Serilog;

namespace Arad.Portal.Areas.Shared.ViewComponents;

public class SocialMediaViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ModuleParameters moduleParameters)
    {
        //// Check if moduleParameters is null or if SelectedIds is null or empty
        //if (moduleParameters == null || moduleParameters.SelectedIds == null || !moduleParameters.SelectedIds.Any())
        //{
        //    // Log a warning and return a placeholder view or an empty view
        //    Log.Warning("ModuleParameters or SelectedIds are null or empty in SocialMediaViewComponent.");
        //}

        //List<string> appUrls = new();
        //// Assuming SelectedIds contains a comma-separated string of URLs


        //if (moduleParameters..Count == 1)
        //{
        //    appUrls = moduleParameters.SelectedIds.First().Split(",").ToList();
        //}
        //else
        //{
        //    appUrls = moduleParameters.SelectedIds;
        //}
        //// Check if appUrls is empty
        //if (!appUrls.Any())
        //{
        //    Log.Warning("No URLs found in SelectedIds after splitting.");
        //}

        // Return the view with the list of URLs
        return View("~/Areas/Shared/Views/Shared/Components/SocialMedia/Default.cshtml", moduleParameters.SocialMedias);
    }
}