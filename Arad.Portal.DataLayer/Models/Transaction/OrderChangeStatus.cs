using Arad.Portal.DataLayer.Entities.Shop.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Transaction
{
    public class OrderChangeStatus
    {
        public string TransactionId { get; set; }

        public OrderStatus OrderStatus { get; set; }
    }
}
