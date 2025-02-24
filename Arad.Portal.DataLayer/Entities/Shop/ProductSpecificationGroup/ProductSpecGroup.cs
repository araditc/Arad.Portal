using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Models.Shared;

using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.Shop.ProductSpecificationGroup;

/// <summary>
/// eac specification belongs to a ProductSpecGroup and each ProductSpecGroup can Have aone or more Specification
/// </summary>
public class ProductSpecGroup : BaseEntity
{
    public ProductSpecGroup()
    {
        GroupNames = new List<MultiLingualProperty>();
    }
        

    /// <summary>
    /// Language, Currency and Name as productSpecGroupName will be filled here
    /// </summary>
    public List<MultiLingualProperty> GroupNames { get; set; }
}