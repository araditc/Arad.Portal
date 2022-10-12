namespace Arad.Portal.DataLayer.Models.User
{
    public class LoginDTO
    {
        public string ReturnUrl { get; set; }
        
        public string Username { get; set; }

        public string FullUserName { get; set; }

        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public string Captcha { get; set; }
    }
}
