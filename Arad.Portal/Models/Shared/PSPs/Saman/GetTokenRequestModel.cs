namespace Arad.Portal.Models.Shared.PSPs.Saman;

public class GetTokenRequestModel
{
    public string Action { get; set; }

    public long Amount { get; set; }

    public string TerminalId { get; set; }

    public string RedirectURL { get; set; }

    public string ResNum { get; set; }

    public long CellNumber { get; set; }

    /// <summary>
    /// برای حساب های تسهسمی پر میشود
    /// </summary>
    public IbanInfo[] SettleMentIbanInfo { get; set; }

}

public class IbanInfo
{
    public string Iban { get; set; }

    public long Amount { get; set; }

    public string PurchaseId { get; set; }
}