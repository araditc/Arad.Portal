using Arad.Portal.DataLayer.Models.Shared;
using System.Collections.Generic;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.Models.Shared;

public class SelectedFilter
{
    public SelectedFilter()
    {
        SelectedDynamicFilters = new();
        GroupIds = new();
        ProductSortingType = ProductSortingType.Newest;
    }
    public decimal? FirstPrice { get; init; }

    public decimal? LastPrice { get; init; }

    public List<string> GroupIds { get; init; }

    public bool? IsAvailable { get; init; }

    public ProductSortingType ProductSortingType { get; init; }

    public List<SelectedDynamicFilter> SelectedDynamicFilters { get; init; }
}