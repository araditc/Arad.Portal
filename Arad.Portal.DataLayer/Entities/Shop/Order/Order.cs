using Arad.Portal.DataLayer.Models.Order;
using Arad.Portal.DataLayer.Models.Shared;
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

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string OrderId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime OrderDate { get; set; }

        public string OrderUserId { get; set; }

        public EntityCulture OrderCulture { get; set; }

        public List<OrderDetail> Details { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public decimal TotalAmountOfOrder { get; set; }

    }
    public enum OrderStatus
    {
        Paid = 0,
        UnPaid = 1
    }

   
}


