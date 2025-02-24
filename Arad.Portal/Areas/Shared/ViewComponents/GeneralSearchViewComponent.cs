using System.Globalization;

using Arad.Portal.Models.Shared;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.Areas.Shared.ViewComponents;

public class GeneralSearchViewComponent(IHttpContextAccessor accessor) : ViewComponent
{
    public IViewComponentResult Invoke(string filter = "")
    {
        Filter obj = new() { Keyword = filter };

        string lanIcon = CultureInfo.CurrentCulture.Name != null ? CultureInfo.CurrentCulture.Name.ToLower() : accessor.HttpContext.Request.Path.Value.Split("/")[1].ToLower();
        ViewBag.LanIcon = lanIcon;
        return View("~/Areas/Shared/Views/Shared/Components/GeneralSearch/Default.cshtml", obj);
    }
}