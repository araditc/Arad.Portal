using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class ProductUnitDTO 
    {
        public string ProductUnitId { get; set; }
        public string UnitName { get; set; }
        public string LanguageId { get; set; }
        public string  LanguageName { get; set; }
        public string ModificationReason { get; set; }
        public bool IsDeleted { get; set; }
    }
}
