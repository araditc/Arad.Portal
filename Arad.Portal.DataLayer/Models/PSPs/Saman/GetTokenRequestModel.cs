using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.PSPs.Saman
{
    public class GetTokenRequestModel
    {
        public string Action { get; set; }

        public long Amount { get; set; }

        public string TerminalId { get; set; }

        public string RedirectURL { get; set; }

        public string ResNum { get; set; }

        public long CellNumber { get; set; }

        /// <summary>
        /// برای حساب های تسهسمی پر میشود
        /// </summary>
        public IBANInfo[] SettleMentIBANInfo { get; set; }

    }

    public class IBANInfo
    {
        public string IBAN { get; set; }

        public long Amount { get; set; }

        public string PurchaseID { get; set; }
    }
}
