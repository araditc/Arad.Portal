using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Domain
{
    public class DomainPrice
    {
        public string DomainId { get; set; }
        public Price.Price Price { get; set; }
        public string ModificationReason { get; set; }
    }
}
