using Arad.Portal.GeneralLibrary.CustomAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public static class Enums
    {
        public enum PageType
        {
            HomePage,
            BlogPage,
            ProductPage
        }

        public enum PspType
        {
            //[Description(" ایران کیش")]
             //IranKish,
            [Description("سامان")]
            /// <summary>
            /// the saman payment gateway implemented here
            /// </summary>
            Saman
            //[Description(" پارسیان")]
            //Parsian,
            //[Description(" به پرداخت")]
            //BehPardakht,
           
        }

        public enum ProductType
        {
            [CustomDescription("EnumDesc_PhisicalStuff")]
            PhisicalStuff = 1,
            [CustomDescription("EnumDesc_File")]
            File = 2
        }

        public enum DownloadLimitationType
        {
            [CustomDescription("EnumDesc_NoLimitation")]
            NoLimitation = 1,
            [CustomDescription("EnumDesc_TimeDuration")]
            TimeDuration = 2,
            [CustomDescription("EnumDesc_TimeDurationWithCnt")]
            TimeDurationWithCnt = 3,
            [CustomDescription("EnumDesc_DownloadCount")]
            DownloadCount = 4
        }

        public enum EmailEncryptionType
        {
            None = 1,
            TLS,
            SSL
        }

        public enum OneColsTemplateWidth
        {
            One = 1
        }

        public enum TwoColsTemplateWidth
        {
            One_One =1,
            One_Two =2,
            One_Three =3,
            One_Five =4,
            One_Eleven = 5,
            Five_Seven = 6,
            Seven_Five = 7,
            Eleven_One = 8,
            Five_One = 9,
            Three_One = 10,
            Two_One = 11
        }


        public enum ProductSortingType
        {
            [CustomDescription("EnumDesc_Newest")]
            Newest = 1,
            [CustomDescription("EnumDesc_MostVisited")]
            MostVisited = 2,
            [CustomDescription("EnumDesc_MostPopular")]
            MostPopular = 3,
            [CustomDescription("EnumDesc_BestSelling")]
            BestSelling = 4,
            //[CustomDescription("EnumDesc_MostExpensive")]
            //MostExpensive = 5,
            //[CustomDescription("EnumDesc_Cheapest")]
            //Cheapest = 6

        }
        public enum ThreeColsTemplateWidth
        {
           One_One_One = 1,
           One_Four_One = 2,
           One_Two_One = 3,
           Five_Two_Five = 4,
           One_Ten_One = 5
        }

        public enum FourColsTemplateWidth
        {
            One_One_One_One = 1,
            One_Five_Five_One = 2,
            One_Two_Two_One = 3,
            Five_One_One_Five = 4,
            Two_One_One_Two = 5,
            One_Two_One_Two = 6,
            Two_One_Two_One = 7
        }

        public enum FiveColsTemplateWidth
        {
            One_Two_Six_Two_One,
            Two_One_Six_One_Two
        }

        public enum SixColsTemplateWidth
        {
            One_One_One_One_One_One,
            One_Two_Three_Three_Two_One,
            Three_Two_One_One_Two_Three,
            Two_One_Three_Three_One_Two,
            One_Three_Two_Two_Three_One
            
        }

       
        public enum DefaultEncoding
        { 
            ASCII,
            Default,
            Latin1,
            BigEndianUnicode,
            UTF32,
            UTF8,
            Unicode
        }
        public enum PaymentStage
        {
            [CustomDescription("EnumDesc_InitialTransaction")]
            Initialized,
            [CustomDescription("EnumDesc_TokenGeneration")]
            GenerateToken,
            [CustomDescription("EnumDesc_LeadToPaymentGateway")]
            RedirectToIPG,
            [CustomDescription("EnumDesc_WaitingforTransactionApproval")]
            DoneButNotConfirmed,
            [CustomDescription("EnumDesc_SuccessfullAndApproved")]
            DoneAndConfirmed,
            [CustomDescription("EnumDesc_UnsuccessfullAndApproved")]
            Failed,
            [CustomDescription("EnumDesc_RollBack")]
            ForcedToCancelledBySystem
        }


        public enum NotificationType
        {
            [CustomDescription("EnumDesc_Email")]
            Email = 1,
            [CustomDescription("EnumDesc_Sms")]
            Sms,
            [CustomDescription("EnumDesc_Notification")]
            Notification
        }

        public enum NotificationSendStatus
        {
            Store = 1,
            Posted,
            Error,
            Sending
        }
        
    }
}
