using System;

using Arad.Portal.DataLayer.Entities.General.SendMessage;

namespace Arad.Portal.Models.Shared.SendMessage;

public class SendMessageViewModel
{
    public string Id { get; set; }
    public string Server { get; set; }
    public string Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public SendType SendType { get; set; }
    public Provider Provider { get; set; }
    public string BaseAddress { get; set; }
    public string SenderNumber { get; set; }
    public string ApiKey { get; set; }
    public bool IsDeleted { get; set; }

    public DateTime CreatingDate { get; set; }
}