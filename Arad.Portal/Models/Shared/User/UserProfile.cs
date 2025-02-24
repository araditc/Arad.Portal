using Arad.Portal.GeneralLibrary.CustomAttributes;

namespace Arad.Portal.Models.Shared.User;

public class UserProfile
{
    [ErrorMessage("AlertAndMessage_NameRequired")]
    public string Name { get; set; }

    [ErrorMessage("AlertAndMessage_LastNameRequired")]
    public string LastName { get; set; }
}