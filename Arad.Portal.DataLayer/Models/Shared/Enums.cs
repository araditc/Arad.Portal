using Arad.Portal.GeneralLibrary.CustomAttributes;
using System.ComponentModel;

namespace Arad.Portal.DataLayer.Models.Shared;

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

    public enum OneColsTemplateWidth
    {
        One = 1
    }

    public enum TwoColsTemplateWidth
    {
        OneOne =1,
        OneTwo =2,
        OneThree =3,
        OneFive =4,
        OneEleven = 5,
        FiveSeven = 6,
        SevenFive = 7,
        ElevenOne = 8,
        FiveOne = 9,
        ThreeOne = 10,
        TwoOne = 11
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
        OneOneOne = 1,
        OneFourOne = 2,
        OneTwoOne = 3,
        FiveTwoFive = 4,
        OneTenOne = 5
    }

    public enum FourColsTemplateWidth
    {
        OneOneOneOne = 1,
        OneFiveFiveOne = 2,
        OneTwoTwoOne = 3,
        FiveOneOneFive = 4,
        TwoOneOneTwo = 5,
        OneTwoOneTwo = 6,
        TwoOneTwoOne = 7
    }

    public enum FiveColsTemplateWidth
    {
        OneTwoSixTwoOne,
        TwoOneSixOneTwo
    }

    public enum SixColsTemplateWidth
    {
        OneOneOneOneOneOne,
        OneTwoThreeThreeTwoOne,
        ThreeTwoOneOneTwoThree,
        TwoOneThreeThreeOneTwo,
        OneThreeTwoTwoThreeOne
            
    }

    public enum EightColsTemplateWidth
    {
        OneOneOneOneOneOneOneOne,
        OneTwoThreeTwoThreeOneOneOne,
        TwoOneThreeThreeTwoOneOneTwo,
        ThreeOneTwoTwoOneThreeTwoOne,
        OneOneTwoThreeTwoThreeTwoOne

    }

    public enum DefaultEncoding
    { 
        Ascii,
        Default,
        Latin1,
        BigEndianUnicode,
        Utf32,
        Utf8,
        Unicode
    }
    public enum PaymentStage
    {
        [CustomDescription("EnumDesc_InitialTransaction")]
        Initialized,
        [CustomDescription("EnumDesc_TokenGeneration")]
        GenerateToken,
        [CustomDescription("EnumDesc_LeadToPaymentGateway")]
        RedirectToIpg,
        [CustomDescription("EnumDesc_WaitingforTransactionApproval")]
        DoneButNotConfirmed,
        [CustomDescription("EnumDesc_SuccessfullAndApproved")]
        DoneAndConfirmed,
        [CustomDescription("EnumDesc_UnsuccessfullAndApproved")]
        Failed,
        [CustomDescription("EnumDesc_RollBack")]
        ForcedToCancelledBySystem
    }


    public enum ActionType
    {
        [CustomDescription("EnumDesc_NoExtraAction")]
        NoExtraAction = 1,

        [CustomDescription("EnumDesc_ProductAvailibilityReminder")]
        ProductAvailibilityReminder = 2,
    }
       
    public enum StateType
    {
        Add,
        Delete,
        None
    }
    public enum ImageTemplateType
    {
        [CustomDescription("EnumDesc_HexagonalLogo")]
        HexagonalLogo,
        [CustomDescription("EnumDesc_SliderWithSubtitle")]
        SliderWithSubtitle,
        [CustomDescription("EnumDesc_Circular")]
        Circular,
        [CustomDescription("EnumDesc_Square")]
        Square,
        [CustomDescription("EnumDesc_x21andx41")]
        X21Andx41,
        [CustomDescription("EnumDesc_x21")]
        X21,
        [CustomDescription("EnumDesc_x51")]
        X51,
        [CustomDescription("EnumDesc_x61")]
        X61,
        [CustomDescription("EnumDesc_x71")]
        X71,
        [CustomDescription("ContentSlider")]
        ContentSlider,
        [CustomDescription("ContentSideBar")]
        ContentSideBar,
    }
    public enum EmailEncryptionType
    {
        None = 1,

        Tls,

        SSL
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
        Store = 0,
        Posted = 1,
        Error = 2,
        Sending = 3
    }

    public enum SliderType
    {
        BootstrapCarousel = 1,
        Swiper = 2
    }
}