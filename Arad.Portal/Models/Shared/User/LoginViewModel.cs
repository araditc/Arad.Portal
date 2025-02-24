namespace Arad.Portal.Models.Shared.User;

public class LoginViewModel
{
    public string ReturnUrl { get; init; }
    public string Username { get; init; }
    public string Password { get; init; }
    public bool RememberMe { get; init; }
    public string Captcha { get; init; }
}