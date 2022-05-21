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
            MainPage,
            contentPage,
            ProductPage
        }

        public enum PspType
        {
            //[Description(" ایران کیش")]
            IranKish,
            //[Description("سامان")]
            Saman
            //[Description(" پارسیان")]
            //Parsian,
            //[Description(" به پرداخت")]
            //BehPardakht,
           
        }

        public enum EmailEncryptionType
        {
            None = 1,
            TLS,
            SSL
        }

        /// <summary>
        /// شناسه واریز
        /// </summary>
        public enum PaymentIdentifierMethod
        {
            None = 0,
            FromUser = 1,
            FromProduct = 3,
            FixedForAll = 4
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
            [Description("ثبت اولیه تراکنش")]
            Initialized,
            [Description("ایجاد توکن.")]
            GenerateToken,
            [Description("هدایت به درگاه پرداخت.")]
            RedirectToIPG,
            [Description("در حال انتظار برای تایید تراکنش ")]
            DoneButNotConfirmed,
            [Description("موفق و تایید شده")]
            DoneAndConfirmed,
            [Description("ناموفق و تایید شده")]
            Failed,
            [Description("درحین پرداخت اپلیکیشن استاپ شده و پرداخت  رول بک میشود")]
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
