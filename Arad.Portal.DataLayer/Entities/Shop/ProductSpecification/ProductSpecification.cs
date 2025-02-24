using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Models.Shared;

using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.Shop.ProductSpecification;

public class ProductSpecification : BaseEntity
{
    public ProductSpecification()
    {
        SpecificationNameValues = new ();
    }
    public string SpecificationGroupId { get; set; }

    /// <summary>
    /// languageId, GroupName for SpecificationGroupName, Name for SpecificationName and NameValues (as list of avalable values for this specification) will be filled here
    /// </summary>
    public List<MultiLingualProperty> SpecificationNameValues { get; set; }

    public ControlType ControlType { get; set; }
}

public enum ControlType
{
    CheckBoxList = 1,
    /// <summary>
    /// something like price range
    /// </summary>
    //RangeSlider = 3
}