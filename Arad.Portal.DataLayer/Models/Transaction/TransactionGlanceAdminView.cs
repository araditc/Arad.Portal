using Arad.Portal.DataLayer.Entities.Shop.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Models.Transaction
{
    public class TransactionGlanceAdminView
    {
        public string UserId { get; set; }

        public string TransactionId { get; set; }

        public string MainInvoiceNumber { get; set; }

        public string UserName { get; set; }

        public string UserFullName { get; set; }

        public DateTime RegisteredDate { get; set; }

        public DateTime PaymentDate { get; set; }

        public long TotalAmount { get; set; }

        public int OrderItemCount { get; set; }

        public PaymentStage PaymentStage { get; set; }

        public OrderStatus? OrderStatus { get; set; }
    }
}
