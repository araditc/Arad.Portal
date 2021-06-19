using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Price;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.Domain
{
    public class Domain : BaseEntity
    {
        public Domain()
        {
            Prices = new ();
        }

        public string DomainId { get; set; }

        public string DomainName { get; set; }
       
        public ApplicationUser Owner { get; set; }

        public List<Price> Prices { get; set; }
    }
}
