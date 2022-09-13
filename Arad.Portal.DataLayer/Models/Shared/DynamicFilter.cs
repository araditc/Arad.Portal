using Arad.Portal.DataLayer.Entities.Shop.ProductSpecification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class DynamicFilter
    {
        public DynamicFilter()
        {
            PossibleValues = new();
        }
        public string SpecificationId { get; set; }

        public ControlType ControlType { get; set; }

        public string SelectedValue { get; set; }

        public long? MinRange { get; set; }

        public long? MaxRange { get; set; }

        public long? Step { get; set; }

        public List<string> PossibleValues { get; set; }
    }
}
