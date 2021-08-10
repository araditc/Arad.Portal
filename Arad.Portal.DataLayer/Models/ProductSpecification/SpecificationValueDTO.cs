using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.ProductSpecification
{
    public class SpecificationValueDTO
    {

        public string ProductSpecificationId { get; set; }

        /// <summary>
        /// | seperated values
        /// </summary>
        public string specificationValue { get; set; }
    }
}
