using Arad.Portal.Models.Shared;

namespace Arad.Portal.Models.UI;

public class ProductPageViewModel
{
    public int CurrentPage { get; init; }
    public long ItemsCount { get; init; }
    public int PageSize { get; init; }
    public string Navigation { get; init; }
    public string QueryParams { get; set; }

    public SelectedFilter Filter { get; init; }
}