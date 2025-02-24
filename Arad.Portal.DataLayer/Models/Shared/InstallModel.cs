using Arad.Portal.DataLayer.Entities.General.Email;

namespace Arad.Portal.DataLayer.Models.Shared;

public class InstallModel
{
    public bool HasDefaultHomeTemplate { get; init; }

    #region ApplicationUserSection

    public string UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public string Password { get; init; }
    public string RePassword { get; init; }
    public string PhoneNumber { get; set; }
    public string FullMobile { get; init; }
    public string DefaultLanguageId { get; set; }
    #endregion

    #region DomainSection
    public bool IsShop { get; set; }
    public bool IsMultiLinguals { get; set; }
    public string DomainId { get; set; }
    public string DomainName { get; set; }
    public string Title { get; set; }
    public string CurrencyId { get; set; }
    public Smtp SmtpAccount { get; init; } = new();

    #endregion

    #region appsetting
    public string ConnectionString { get; set; }
    public string LogFileDirectory { get; init; }
    public string LocalStaticFileStorage { get; init; }

    public string SmsEndpoint { get; init; }
    public string SmsLineNumber { get; init; }
    public string SmsUserName { get; init; }
    public string SmsPassword { get; init; }
    public string SmsCompany { get; init; }
    public string TokenEndpoint { get; init; }
    public string TokenUserName { get; init; }
    public string TokenPassword { get; init; }


    #endregion
}