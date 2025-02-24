namespace Arad.Portal.Models.Shared.User;

public class RegisterVm
{
    public string? ReturnUrl { get; set; }

    public string? CellPhoneNumber { get; set; }

    public string FullCellPhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Username { get; set; }
    public string Captcha { get; set; }

    public string SecurityCode { get; set; }

    public string? CurrentPass { get; set; }
    public string? NewPass { get; set; }
    public string? ReNewPass { get; set; }
}