using Arad.Portal.DataLayer.Entities.General.SendMessage;

namespace Arad.Portal.Models.Shared.SendMessage;

public class SendMessageDto
{
    public string Id { get; init; }
    public string Server { get; init; }
    public string Port { get; init; }
    public string UserName { get; init; }
    public string Password { get; init; }
    public SendType SendType { get; init; }
    public int SendTypeId { get; init; }
    public int ProviderId { get; init; }
    public Provider Provider { get; init; }
    public string BaseAddress { get; init; }
    public string SenderNumber { get; init; }
    public string ApiKey { get; init; }
    public string AssociatedDomainId { get; init; }

}