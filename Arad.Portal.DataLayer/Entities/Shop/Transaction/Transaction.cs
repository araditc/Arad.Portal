using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Models.Shared;
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

        /// <summary>
        /// this property would be worked as ResNumber In saman gateway
        /// </summary>
        public string MainInvoiceNumber { get; set; }

        public List<EventData> EventsData { get; set; }

        public PaymentGatewayData BasicData { get; set; }

        public bool FinalSettlement { get; set; }

        public long FinalPriceToPay { get; set; }

        public CustomerData CustomerData { get; set; }

        public List<InvoicePerSeller> SubInvoices { get; set; }
      
        public List<Parameter<string, string>> AdditionalData { get; set; }
    }

    
    public class InvoicePerSeller
    {
        /// <summary>
        /// شماره فاکتور فروشنده
        /// </summary>
        public string SellerInvoiceId { get; set; }
        /// <summary>
        /// اطلاعات تسویه با فروشنده
        /// </summary>
        public SettlementInfo SettlementInfo { get; set; }
        
        public PurchasePerSeller ParchasePerSeller { get; set; }
    }

    public class SettlementInfo
    {
        /// <summary>
        /// شماره پرداخت یا تراکنش واریز وجه به حساب فروشنده
        /// </summary>
        public string DepositeNumber { get; set; }

        /// <summary>
        /// شماره پیگیری تراکنش پرداخت به فروشنده
        /// </summary>
        public string ReferenceId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime SettlementDate { get; set; }

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
        
        public string ReservationNumber { get; set; }
        /// <summary>
       ///شماره پیگیری تراکنش که از درگاه بانکی دریافت میشود
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
     }

    public class EventData
    {
        public PspActions ActionType { get; set; }
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
        /// </summary>
        DepositToBankAccount,
        /// <summary>
        /// انتقال وجه کارت به کارت
        /// </summary>
        CardToCard,
        Others
    }
    public enum PspActions
    {
        [Description("درخواست توکن")]
        ClientTokenRequest = 1,

        [Description("پاسخ درخواست توکن از سمت درگاه")]
        PspTokenResponse = 2,

        [Description("ارسال اطلاعات به درگاه جهت لاگین")]
        ClientRequestToLogin = 3,

        [Description("دریافت پاسخ لاگین از درگاه")]
        PspLoginResponse = 4,

        [Description("ارسال اطلاعات تراکنش جهت ساین")]
        ClientRequestGenerateTransactionDataToSign = 5,

        [Description("پاسخ درگاه برای ساین اطلاعات تراکنش")]
        PspResponseGenerateTransactionDataToSign = 6,

        [Description("ارسال اطلاعات پرداخت از سمت درگاه ")]
        PspSendCallback = 7,

        [Description("درخواست تایید تراکنش از سمت کلاینت")]
        ClientVerifyRequest = 8,

        [Description("پاسخ تایید تراکنش از درگاه")]
        PspVerifyResponse = 9,

        [Description("درخواست برگشت تراکنش از سمت کلاینت")]
        ClientRequestReverseTransaction = 10,

        [Description("پاسخ درگاه برای برگشت تراکنش")]
        PspResponseReverseTransaction = 11
    }

}
