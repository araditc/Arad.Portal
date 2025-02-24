using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.Models.Shared.ProductGroup;

public class ProductGroupViewModel
{
    public ProductGroupViewModel()
    {
        MultiLingualProperty = new();
    }
    public string ProductGroupId { get; init; }

    public MultiLingualProperty MultiLingualProperty { get; init; }

    public string ParentId { get; set; }

    public bool IsDeleted { get; init; }

    public string ModificationReason { get; set; }

    public DataLayer.Entities.Shop.Promotion.Promotion Promotion { get; set; }
}