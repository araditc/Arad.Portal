using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.Order
{
    public class Order : BaseEntity
    {
        public Order()
        {
            Details = new();
        }
        public string OrderId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime OrderDate { get; set; }

        public string OrderUserId { get; set; }

        public List<OrderDetail> Details { get; set; }

        public OrderStatus OrderStatus { get; set; }

    }

    public class OrderDetail
    {
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal PricePerUnit { get; set; }

        public decimal DiscountPerUnit { get; set; }

        public int OrderCount { get; set; }

        public decimal TotalAmountToPay { get; set; }
    }

    public enum OrderStatus
    {
        SuccessfullPaid = 0,
        UnSuccessfulPaid = 1
    }
}


