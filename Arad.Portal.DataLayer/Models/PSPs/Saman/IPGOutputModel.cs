using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.PSPs.Saman
{
    public class IPGOutputModel
    {
        public IPGOutputModel()
        {
            VerifyInfo = new VerifyInfo();
        }
        public VerifyInfo VerifyInfo { get; set; }

        public int ResultCode { get; set; }

        public string ResultDescription { get; set; }

        public bool Success { get; set; }
    }

    public class VerifyInfo
    {
        /// <summary>
        /// شماره مرجع
        /// </summary>
        public string RRN { get; set; }
        /// <summary>
        /// رسید دیجیتالی
        /// </summary>
        public string RefNum { get; set; }
        public int TerminalNumber { get; set; }
        /// <summary>
        /// مبلغ ارسالی به درگاه
        /// </summary>
        public int OriginalAmount { get; set; }
        /// <summary>
        /// مبلغ کسر شده از کارت
        /// </summary>
        public int AffectiveAmount { get; set; }
        /// <summary>
        /// تاریخ تراکنش
        /// </summary>
        public string StraceDate { get; set; }
        /// <summary>
        /// کد رهگیری
        /// </summary>
        public string StraceNo { get; set; }
    }
}
