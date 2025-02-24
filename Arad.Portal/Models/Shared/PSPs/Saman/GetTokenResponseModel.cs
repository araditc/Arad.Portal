namespace Arad.Portal.Models.Shared.PSPs.Saman;

public class GetTokenResponseModel
{
    public int Status { get; set; }
    public int ErrorCode { get; set; }
    public string ErrorDesc { get; set; }
    public string Token { get; set; }
}