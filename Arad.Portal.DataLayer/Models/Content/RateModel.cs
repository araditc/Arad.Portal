using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Content
{
    public class RateModel
    {

        public string EntityId { get; set; }

        public int Score { get; set; }

        public bool IsContent { get; set; }

        public bool IsNew { get; set; }
    }
}
