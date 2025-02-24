namespace Arad.Portal.Models.Shared.User;

public class ChangePassDto
{
    public string ReturnUrl { get; init; }

    public string CellPhoneNumber { get; init; }

    public string FullCellPhoneNumber { get; set; }

    public string Email { get; init; }

    public string Username { get; init; }
    public string Captcha { get; init; }

    public string SecurityCode { get; init; }

}