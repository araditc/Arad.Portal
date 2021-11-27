using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.Announcement
{
    public class Announcement
    {
        public string AnnouncementId { get; set; }

        public string ProductId { get; set; }

        public string UserId { get; set; }

        public bool IsNew { get; set; }
    }
}
