using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Models.Shared;
using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.Shop.ProductUnit;

/// <summary>
/// product have different unit for counting this as avalable units for product that can be extended
/// </summary>
public class ProductUnit : BaseEntity
{
    public ProductUnit()
    {
        UnitNames = new();
    }
    /// <summary>
    /// Language and Name as unitName will be store here
    /// </summary>
    public List<MultiLingualProperty> UnitNames { get; set; }

}