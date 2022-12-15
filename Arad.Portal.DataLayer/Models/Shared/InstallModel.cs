using Arad.Portal.DataLayer.Entities.General.Email;
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
            SMTPAccount = new();
        }

        public bool HasDefaultHomeTemplate { get; set; }

        #region ApplicationUserSection

        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string RePassword { get; set; }
        public string PhoneNumber { get; set; }
        public string FullMobile { get; set; }
        public string DefaultLanguageId { get; set; }
        #endregion

        #region DomainSection
        public bool IsShop { get; set; }
        public bool IsMultiLinguals { get; set; }
        public string DomainId { get; set; }
        public string DomainName { get; set; }
        public string Title { get; set; }
        public string CurrencyId { get; set; }
        public SMTP SMTPAccount { get; set; }

        #endregion

        #region appsetting
        public string ConnectionString { get; set; }
        public string LogFileDirectory { get; set; }
        public string LocalStaticFileStorage { get; set; }

        public string SmsEndpoint { get; set; }
        public string SmsLineNumber { get; set; }
        public string SmsUserName { get; set; }
        public string SmsPassword { get; set; }
        public string SmsCompany { get; set; }
        public string TokenEndpoint { get; set; }
        public string TokenUserName { get; set; }
        public string TokenPassword { get; set; }


        #endregion
    }
}
