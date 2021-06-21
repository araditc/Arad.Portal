using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.County
{
    public class County : BaseEntity
    {
        public County()
        {
            Districts = new ();
        }
        public string Id { get; set; }

        public string Name { get; set; }

        public string StateName { get; set; }

        public string StateId { get; set; }

        public List<District.District> Districts { get; set; }
    }
}
