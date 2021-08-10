using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Domain
{
    public class DomainDTO
    {
        public DomainDTO()
        {
            Prices = new();
        }
        public string DomainId { get; set; }

        public string DomainName { get; set; }

        public string OwnerUserId { get; set; }

        public string OwnerUserName { get; set; }

        public Price  DomainPrice { get; set; }

        public List<Price> Prices { get; set; }
    }
}
