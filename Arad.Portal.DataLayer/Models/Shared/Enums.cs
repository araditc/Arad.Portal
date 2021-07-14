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

            [Description(" به پرداخت")]
            BehPardakht,

            [Description(" پارسیان")]
            Parsian
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

            [Description("موفق")]
            DoneAndConfirmed,

            [Description("ناموفق")]
            Failed
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
            Send,
            Error
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
            [Description("List")]
            List = 0,
            [Description("Add")]
            Add = 1,
            [Description("Remove")]
            Remove = 2,
            [Description("Edit")]
            Edit = 3,
            [Description("Password")]
            Password = 4,
            [Description("Active")]
            Active = 5,
            [Description("Dependency")]
            Dependency = 6,
            [Description("Details")]
            Details = 7,
        }
    }
}
