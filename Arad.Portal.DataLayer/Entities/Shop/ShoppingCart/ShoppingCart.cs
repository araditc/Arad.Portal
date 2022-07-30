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
        public string ShoppingCartId { get; set; }
      
        public EntityCulture ShoppingCartCulture { get; set; }

        public long FinalPriceToPay { get; set; }

        public List<PurchasePerSeller> Details { get; set; }
    }


    public class PurchasePerSeller
    {
        public PurchasePerSeller()
        {
            Products = new();
        }
        public string SellerId { get; set; }

        /// <summary>
        /// //??? should be changed to seller full name
        /// </summary>
        public string SellerUserName { get; set; }

        public List<ShoppingCartDetail> Products { get; set; }

        /// <summary>
        /// the value property of basicData class with groupkey equal to  'ShippingType'
        /// </summary>
        public int ShippingTypeId { get; set; }

        public long ShippingExpense { get; set; }

        public long TotalDetailsAmountWithShipping { get; set; }
    }
    public class ShoppingCartDetail : BaseEntity
    {

        public string ShoppingCartDetailId { get; set; }

        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public string SellerId { get; set; }

        public int OrderCount { get; set; }

        /// <summary>
        /// price without discount
        /// </summary>
        public long PricePerUnit { get; set; }

        public long DiscountPricePerUnit { get; set; }

        /// <summary>
        /// (PricePerUnit - DiscountPerUnit ) * OrderCount
        /// </summary>
        public long TotalAmountToPay { get; set; }
    }

    //public enum ShippingType
    //{
    //    Post,
    //    Courier
    //}
}
