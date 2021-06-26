using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.Shop.ShoppingCart
{
    public class ShoppingCart : BaseEntity
    {
        public ShoppingCart()
        {
            Details = new();
        }
        public string UserCartId { get; set; }

        public List<ShoppingCartDetail> Details { get; set; }

    }

    public class ShoppingCartDetail : BaseEntity
    {
        public string ShoppingCartDetailId { get; set; }

        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public int OrderCount { get; set; }

        public decimal PricePerUnit { get; set; }

        public decimal DiscountPricePerUnit { get; set; }

        public decimal TotalAmountToPay { get; set; }
    }
}
