using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.CustomAttributes;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.Transaction
{
    public class Transaction : BaseEntity
    {
        public Transaction()
        {
            EventsData = new();
            SubInvoices = new();
            BasicData = new();
            CustomerData = new();
            AdditionalData = new();
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string TransactionId { get; set; }

        public string ShoppingCartId { get; set; }

        /// <summary>
        /// this property would be worked as ResNumber In saman gateway
        /// </summary>
        public string MainInvoiceNumber { get; set; }

        /// <summary>
        /// all detail information for logging
        /// </summary>
        public List<EventData> EventsData { get; set; }

        public PaymentGatewayData BasicData { get; set; }

        /// <summary>
        /// the sum of all product prices after apply discount or couponcode for this payment
        /// </summary>
        public decimal FinalPriceToPay { get; set; }

        /// <summary>
        /// information of user who has done this transaction
        /// </summary>
        public CustomerData CustomerData { get; set; }

        /// <summary>
        /// order status surely initialized after successfull payment
        /// </summary>
        public  OrderStatus? OrderStatus { get; set; }
        
        public List<InvoicePerSeller> SubInvoices { get; set; }
      
        public List<Parameter<string, string>> AdditionalData { get; set; }
    }

   

    public enum OrderStatus
    {
        [CustomDescription("Enum_OrderRegistered")]
        OrderRegitered = 1,
        [CustomDescription("Enum_OrderApproved")]
        Approved,
        [CustomDescription("Enum_CanceledByUser")]
        CanceledByUser,
        [CustomDescription("Enum_ReadyToSend")]
        ReadyToSend,
        [CustomDescription("Enum_Sent")]
        Sent
    }
    public class InvoicePerSeller
    {
        /// <summary>
        /// seller invoice Number
        /// maybe shoppingCart contains products from differennt seller so each seller has individual invoice and invoicenumber
        /// </summary>
        public string SellerInvoiceId { get; set; }

        /// <summary>
        /// if a siteOwner sells his product in mainDomain and this transaction happen on main Domain then this is information about how main domain reach a settlement with the site owner 
        /// </summary>
        public SettlementInfo SettlementInfo { get; set; }

        /// <summary>
        /// list of product of  seller in this transaction (if transaction happens in any domain rather than main Domain then it has only one seller
        /// who is site owner but if transaction happens on mainDomain other site that have purpose to sale their product one main Domain too have to set IsPublishedOnMainDomain field of product entity to true then they can sale that product on Main Domain too  
        /// </summary>
        public PurchasePerSeller PurchasePerSeller { get; set; }
    }

    public class SettlementInfo
    {
        /// <summary>
        ///  payment number or deposite number to seller bank account
        /// </summary>
        public string DepositeNumber { get; set; }

        /// <summary>
        /// refrenceNumber of payment transaction to seller bank account
        /// </summary>
        public string ReferenceId { get; set; }

        /// <summary>
        /// the date which this payment for settlement happens
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime SettlementDate { get; set; }

        /// <summary>
        /// the way of this settlement
        /// </summary>
        public PaymentType PayementType { get; set; }

        public string AdditionalData { get; set; }

    }
    public class PaymentGatewayData
    {
        /// <summary>
        /// needed in some Psps
        /// </summary>
        public string PaymentId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreationDateTime { get; set; }
        public string ShoppinCartId { get; set; }
        public string OverallConcatDescription { get; set; }
        
        /// <summary>
        /// a customize string to access our Data in differenct places
        /// </summary>
        public string ReservationNumber { get; set; }
        /// <summary>
        ///reference number which is given by payment Gateway
        /// </summary>
        public string ReferenceId { get; set; }
        public Enums.PaymentStage Stage { get; set; }
        public Enums.PspType PspType { get; set; }
        public string Description { get; set; }
    }


    public class CustomerData
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        /// <summary>
        /// the Id of address which user has purpose to recieve gis payment stuffs
        /// </summary>
        public string ShippingAddressId { get; set; }
    }

    public class EventData
    {
        public PspActions ActionType { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ActionDateTime { get; set; }
        public string JsonContent { get; set; }
        public string additionalData { get; set; }
    }

    public class Parameter<T1, T2>
    {
        public T1 Key { get; set; }
        public T2 Value { get; set; }
    }

    public enum PaymentType
    {
        /// <summary>
        /// واریز به شماره حساب فروشنده
        /// deposite to seller bank account
        /// </summary>
        DepositToBankAccount,
        /// <summary>
        /// انتقال وجه کارت به کارت
        /// transfer money via card to card
        /// </summary>
        CardToCard,
        /// <summary>
        /// any other way which not mentionded here
        /// </summary>
        Others
    }
    /// <summary>
    /// this enum is for interacting with payment gateway during online payment in application
    /// </summary>
    public enum PspActions
    {
       
        [CustomDescription("EnumDesc_ClientTokenRequest")]
        ClientTokenRequest = 1,
     
        [CustomDescription("EnumDesc_PspTokenResponse")]
        PspTokenResponse = 2,
      
        [CustomDescription("EnumDesc_ClientRequestToLogin")]
        ClientRequestToLogin = 3,
     
        [CustomDescription("EnumDesc_PspLoginResponse")]
        PspLoginResponse = 4,
      
        [CustomDescription("EnumDesc_ClientRequestDataToSignIn")]
        ClientRequestGenerateTransactionDataToSign = 5,
      
        [CustomDescription("EnumDesc_PspResponseToDataToSignIn")]
        PspResponseGenerateTransactionDataToSign = 6,
      
        [CustomDescription("EnumDesc_PspSendCallback")]
        PspSendCallback = 7,
       
        [CustomDescription("EnumDesc_ClientVerifyRequest")]
        ClientVerifyRequest = 8,
      
        [CustomDescription("EnumDesc_PspVerifyResponse")]
        PspVerifyResponse = 9,
     
        [CustomDescription("EnumDesc_ClientRequestReverseTransaction")]
        ClientRequestReverseTransaction = 10,
      
        [CustomDescription("EnumDesc_PspResponseReverseTransaction")]
        PspResponseReverseTransaction = 11
    }

}
