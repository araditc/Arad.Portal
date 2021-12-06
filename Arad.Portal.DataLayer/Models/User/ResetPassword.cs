namespace Arad.Portal.DataLayer.Models.User
{
    public class ResetPassword
    {
        public string FullCellPhoneNumber { get; set; }

        public string CellPhoneNumber { get; set; }

        public string UserName { get; set; }

        public string Captcha { get; set; }
    }
}
