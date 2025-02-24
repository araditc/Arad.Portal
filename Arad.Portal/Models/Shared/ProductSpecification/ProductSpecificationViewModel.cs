using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.Models.Shared.ProductSpecification;

public class ProductSpecificationViewModel
{
    public string ProductSpecificationId { get; init; }
    public string SpecificationGroupId { get; set; }
    /// <summary>
    /// languageId, specificationGroupName, specificationName and list of its values
    /// </summary>
    public MultiLingualProperty SpecificationNameValues { get; init; }
    public bool IsDeleted { get; init; }
    public string ModificationReason { get; set; }
}