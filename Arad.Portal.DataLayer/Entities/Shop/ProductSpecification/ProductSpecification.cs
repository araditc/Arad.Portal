using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.ProductSpecification;
using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.ProductSpecification
{
    public class ProductSpecification : BaseEntity
    {
        public ProductSpecification()
        {
            SpecificationNameValues = new ();
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string ProductSpecificationId { get; set; }

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
        TwoStateFlags = 2,
        /// <summary>
        /// something like price range
        /// </summary>
        RangeSlider = 3
    }
}
