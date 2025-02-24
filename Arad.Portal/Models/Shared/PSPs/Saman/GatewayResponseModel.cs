namespace Arad.Portal.Models.Shared.PSPs.Saman;

public class GatewayResponseModel
{
    /// <summary>
    /// شماره ترمینال
    /// </summary>
    public string Mid { get; set; }
    /// <summary>
    /// وضعیت تراکنش حروف انگلیسی
    /// </summary>
    public string State { get; init; }
    /// <summary>
    /// وضعیت تراکنش مقدار عددی
    /// </summary>
    public int Status { get; init; }
    /// <summary>
    /// شماره مرجع
    /// </summary>
    public string Rrn { get; set; }
    /// <summary>
    /// رسید دیجیتالی خرید
    /// </summary>
    public string RefNum { get; set; }
    /// <summary>
    /// شماره خرید
    /// </summary>
    public string ResNum { get; init; }
    /// <summary>
    /// شماره ترمینال
    /// </summary>
    public string TerminalId { get; init; }
    /// <summary>
    /// شماره رهگیری
    /// </summary>
    public string TraceNo { get; set; }
    /// <summary>
    /// مبلغ
    /// </summary>
    public long Amount { get; set; }
    /// <summary>
    /// دستمزد
    /// </summary>
    public long Wage { get; set; }
    /// <summary>
    /// شماره کارتی که تراکنش با آن انجام شده است
    /// </summary>
    public string SecurePan { get; set; }
    /// <summary>
    /// شماره کارت هش شده sha256
    /// </summary>
    public string HashedCardNumber { get; set; }
}