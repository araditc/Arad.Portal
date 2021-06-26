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
            BasicData = new();
            UserData = new();
            TransactionItems = new();
        }
        public string TransactionId { get; set; }

        public List<EventData> EventsData { get; set; }

        public TransactionBasicData BasicData { get; set; }

        public TransactionUserData UserData { get; set; }

        public List<TransactionItem> TransactionItems { get; set; }

        public TransactionApportionData ApportionData { get; set; }

        public List<Parameter<string, string>> AdditionalData { get; set; }


    }

    public class TransactionItem
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string UnitPrice { get; set; }
        public string DiscountAmountPerUnit { get; set; }
        public int Count { get; set; }
        public string TotalAmountToPay { get; set; }
    }

    public class TransactionApportionData
    {
        public List<AccountApportion> ProductApportions { get; set; }
        public string TotalAmountPaid { get; set; }
    }

    public class AccountApportion
    {
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string Iban { get; set; }
        public string Apportion { get; set; }
        public string PaymentIdentifier { get; set; }
        public bool IsMain { get; set; }
    }

    public class TransactionBasicData
    {
        public string InvoiceNumber { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreationDateTime { get; set; }

        /// <summary>
        /// needed in some Psps
        /// </summary>
        public string PaymentId { get; set; }
        public string OrderId { get; set; }
        public string TerminalId { get; set; }
        public string TerminalName { get; set; }

        //???
        public string InternalTokenIdentifier { get; set; }
        public string OverallConcatDescription { get; set; }
        public Enums.PaymentIdentifierMethod PaymentIdentifierMethod { get; set; }
        /// <summary>
        /// شماره پیگیری تراکنش
        /// </summary>
        public string ReferenceId { get; set; }
        public Enums.PaymentStage Stage { get; set; }
        public Enums.PspType PspType { get; set; }
        public string Description { get; set; }
    }


    public class TransactionUserData
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public bool PayVerifySmsIsSent { get; set; }
        public string PayVerifySmsText { get; set; }
    }

    public class EventData
    {

        public PspActions ActionType { get; set; }
        public DateTime ActionDateTime { get; set; }
        public string JsonContent { get; set; }
        public string ActionDescription { get; set; }
    }

    public class Parameter<T1, T2>
    {
        public T1 Key { get; set; }
        public T2 Value { get; set; }
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
