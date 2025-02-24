using Arad.Portal.DataLayer.Models.Shared;

using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.ProductSpecificationGroup;

public class SpecificationGroupDto
{
    public SpecificationGroupDto()
    {
        GroupNames = new();
    }
    public string Id { get; init; }
    public List<MultiLingualProperty> GroupNames { get; init; }
    public string? ModificationReason { get; init; }
    public bool IsDeleted { get; init; }
    public string AssociatedDomainId { get; set; }
}