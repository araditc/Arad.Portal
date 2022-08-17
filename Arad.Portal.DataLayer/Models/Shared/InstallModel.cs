using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class InstallModel
    {
        public InstallModel()
        {
            DefaultDomain = new();
            SystemAccountUser = new();
            AppSetting = new();
        }

        public Entities.General.Domain.Domain DefaultDomain { get; set; } // if there is an smtp account it should set here for domain

        public Entities.General.User.ApplicationUser SystemAccountUser { get; set; }

        public string Password { get; set; }

        public string RePassword { get; set; }

        public string PhoneNumber { get; set; }

        public string CurrencyId { get; set; }

        public long PriceValue { get; set; }

        public string FullMobile { get; set; }

        public AppSetting AppSetting { get; set; }

        public bool HaveDefaultHomeTemplate { get; set; }
    }
}
