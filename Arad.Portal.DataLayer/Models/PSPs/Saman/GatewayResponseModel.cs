using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.PSPs.Saman
{
    public class GatewayResponseModel
    {
        /// <summary>
        /// شماره ترمینال
        /// </summary>
        public string MID { get; set; }
        /// <summary>
        /// وضعیت تراکنش حروف انگلیسی
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// وضعیت تراکنش مقدار عددی
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// شماره مرجع
        /// </summary>
        public string RRN { get; set; }
        /// <summary>
        /// رسید دیجیتالی خرید
        /// </summary>
        public string RefNum { get; set; }
        /// <summary>
        /// شماره خرید
        /// </summary>
        public string ResNum { get; set; }
        /// <summary>
        /// شماره ترمینال
        /// </summary>
        public string TerminalId { get; set; }
        /// <summary>
        /// شماره رهگیری
        /// </summary>
        public string TraceNo { get; set; }
        /// <summary>
        /// مبلغ
        /// </summary>
        public long Amount { get; set; }
        /// <summary>
        /// دستمزد
        /// </summary>
        public long Wage { get; set; }
        /// <summary>
        /// شماره کارتی که تراکنش با آن انجام شده است
        /// </summary>
        public string SecurePan { get; set; }
        /// <summary>
        /// شماره کارت هش شده sha256
        /// </summary>
        public string HashedCardNumber { get; set; }
    }
}
