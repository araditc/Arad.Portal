using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.Shop.ShoppingCart
{
    /// <summary>
    /// owner Of this shopping cart is creatorUserId of baseEntity and 
    /// IsActive is the current cart of this user each time for a user in a domain one cart isActive 
    /// the cart when going to paymentGateway became inactive or remove from user basket
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

        public List<PurchasePerSeller> Details { get; set; }
    }


    public class PurchasePerSeller
    {
        public string SellerId { get; set; }

        public List<ShoppingCartDetail> Products { get; set; }

        public TransferType TransferType { get; set; }

        public decimal TransferExpense { get; set; }

        public decimal  TotalDetailsAmountWithTransfer { get; set; }
    }
    public class ShoppingCartDetail : BaseEntity
    {
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public string SellerId { get; set; }

        public int OrderCount { get; set; }

        /// <summary>
        /// price without discount
        /// </summary>
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
