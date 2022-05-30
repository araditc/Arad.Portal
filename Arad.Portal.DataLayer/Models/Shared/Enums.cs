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
            //[Description(" ایران کیش")enum
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

        public enum OneColsTemplateWidth
        {
            One
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
