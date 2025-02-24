using Arad.Portal.DataLayer.Models.Shared;

using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Product;

public class ProductUnitDto
{
    public string Id { get; init; }

    public List<MultiLingualProperty> UnitNames { get; init; } = [];

    public string? ModificationReason { get; init; }
    public bool IsDeleted { get; init; }
    public string AssociatedDomainId { get; set; }
}