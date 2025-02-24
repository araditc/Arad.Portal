namespace Arad.Portal.Models.Shared.User;

public class LoginDto
{
    public string ReturnUrl { get; set; }

    public string Username { get; set; }

    public string FullUserName { get; set; }

    public string Password { get; set; }

    public bool RememberMe { get; set; }

    public string Captcha { get; set; }
    public string DesiredImageUrl { get; set; }
}