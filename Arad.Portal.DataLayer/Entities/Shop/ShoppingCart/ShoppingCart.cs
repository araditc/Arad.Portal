using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.Shop.ShoppingCart
{
    /// <summary>
    /// owner Of this shopping cart is creatorUserId of baseEntity
    /// </summary>
    public class ShoppingCart : BaseEntity
    {
        public ShoppingCart()
        {
            Details = new();
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string UserCartId { get; set; }

        public EntityCulture ShoppingCartCulture { get; set; }

        public List<InvoicePerSeller> Details { get; set; }

    }


    public class InvoicePerSeller
    {
        public string SellerId { get; set; }

        public List<ShoppingCartDetail> Details { get; set; }

        public TransferType TransferType { get; set; }

        public decimal TransferExpense { get; set; }

        public decimal  TotalOfDetailsAmountWithTransfer { get; set; }
    }
    public class ShoppingCartDetail : BaseEntity
    {
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public int OrderCount { get; set; }

        public decimal PricePerUnit { get; set; }

        public decimal DiscountPricePerUnit { get; set; }

        public decimal TotalAmountToPay { get; set; }
    }

    public enum TransferType
    {
        Post,
        Courier
    }
}
