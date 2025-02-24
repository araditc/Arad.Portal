using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.Models.Shared.Product;

public class ProductUnitViewModel
{
    public ProductUnitViewModel()
    {
        UnitName = new();
    }
    public string ProductUnitId { get; init; }

    public bool IsDeleted { get; init; }

    public MultiLingualProperty UnitName { get; init; }
}