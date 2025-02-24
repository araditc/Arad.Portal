using Arad.Portal.DataLayer.Entities.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Models.Shared;
using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.ProductSpecification;

public class ProductSpecificationDto
{
    public string? Id { get; init; }

    public string SpecificationGroupId { get; init; }
    /// <summary>
    /// languageId, specificationGroupName, specificationName and list of its values
    /// </summary>
    public List<MultiLingualProperty> SpecificationNameValues { get; init; } = [];

    public bool IsDeleted { get; init; }

    public ControlType ControlType { get; init; }

    public string? ModificationReason { get; init; }

    public string AssociatedDomainId { get; init; }
}