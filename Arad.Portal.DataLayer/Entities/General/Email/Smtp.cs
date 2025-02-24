
using Arad.Portal.DataLayer.Entities.Abstractions;

namespace Arad.Portal.DataLayer.Entities.General.Email;

public class Smtp
{
    public string Server { get; init; }
    public string Port { get; init; }
    public string UserName { get; init; }
    public string Password { get; init; }
}