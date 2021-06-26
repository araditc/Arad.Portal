using Arad.Portal.DataLayer.Entities.Shop.Order;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Order
{
    public class OrderDTO
    {
        public OrderDTO()
        {
            Details = new();
        }
        public string OrderId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime OrderDate { get; set; }

        public string OrderUserId { get; set; }

        public List<OrderDetail> Details { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public decimal TotalAmountOfOrder { get; set; }
    }
}
