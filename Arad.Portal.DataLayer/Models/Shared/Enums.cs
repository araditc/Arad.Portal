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

        public enum PspType
        {
            [Description(" ایران کیش")]
            IranKish,
            [Description("  سامان کیش")]
            Saman,
            [Description(" پارسیان")]
            Parsian,
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
  

        public enum PermissionType
        {
            Module = 0,
            Menu = 1,
            BaseMenu = 2
        }

        /// <summary>
        /// PermissionViewKey
        /// </summary>
        public enum PermissionMethod
        {
            [CustomDescription("EnumDesc_List")]
            List = 0,
            [CustomDescription("EnumDesc_Add")]
            Add = 1,
            [CustomDescription("EnumDesc_Remove")]
            Remove = 2,
            [CustomDescription("EnumDesc_Edit")]
            Edit = 3,
            [CustomDescription("EnumDesc_Password")]
            Password = 4,
            [CustomDescription("EnumDesc_Active")]
            Active = 5,
            [CustomDescription("EnumDesc_Dependency")]
            Dependency = 6,
            [CustomDescription("EnumDesc_Details")]
            Details = 7,
        }
    }
}
