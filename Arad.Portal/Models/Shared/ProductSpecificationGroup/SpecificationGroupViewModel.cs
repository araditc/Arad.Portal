using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.Models.Shared.ProductSpecificationGroup;

public class SpecificationGroupViewModel
{
    public SpecificationGroupViewModel()
    {
        GroupName = new();
    }
    public string SpecificationGroupId { get; init; }
    public MultiLingualProperty GroupName { get; init; }
    //public string ModificationReason { get; set; }
    public bool IsDeleted { get; init; }
}