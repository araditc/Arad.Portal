using Arad.Portal.GeneralLibrary.Utilities;

using Microsoft.AspNetCore.Mvc;

using Serilog;

namespace Arad.Portal.Areas.Shared.ViewComponents;

public class PageViewModel
{
    public int CurrentPage { get; init; }
    public long ItemsCount { get; init; }
    public int PageSize { get; init; }
    public string Navigation { get; init; }
    public string QueryParams { get; set; }
}

public class PaginationViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(PageViewModel model)
    {
        return View("~/Areas/Shared/Views/Shared/Components/Pagination/Default.cshtml", model);
    }
}