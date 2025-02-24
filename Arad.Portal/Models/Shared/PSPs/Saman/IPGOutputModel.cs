namespace Arad.Portal.Models.Shared.PSPs.Saman;

public class IpgOutputModel
{
    public IpgOutputModel()
    {
        TransactionDetail = new();
    }
    public VerifyInfo TransactionDetail { get; set; }

    public int ResultCode { get; set; }

    public string ResultDescription { get; set; }

    public bool Success { get; set; }
}

public class VerifyInfo
{
    /// <summary>
    /// شماره مرجع
    /// </summary>
    public string Rrn { get; set; }
    /// <summary>
    /// رسید دیجیتالی
    /// </summary>
    public string RefNum { get; set; }

    public string MaskedPan { get; set; }

    public string HashedPan { get; set; }

    public int TerminalNumber { get; set; }
    /// <summary>
    /// مبلغ ارسالی به درگاه
    /// </summary>
    public int OriginalAmount { get; set; }
    /// <summary>
    /// مبلغ کسر شده از کارت
    /// </summary>
    public int AffectiveAmount { get; set; }
    /// <summary>
    /// تاریخ تراکنش
    /// </summary>
    public string StraceDate { get; set; }
    /// <summary>
    /// کد رهگیری
    /// </summary>
    public string StraceNo { get; set; }
}