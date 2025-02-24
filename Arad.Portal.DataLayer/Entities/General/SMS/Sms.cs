using Arad.Portal.DataLayer.Entities.Abstractions;

namespace Arad.Portal.DataLayer.Entities.General.SMS;

public class Sms
{
    public string BaseAddress { get; init; }
    public string SenderNumber { get; init; }
    public string ApiKey { get; init; }
}