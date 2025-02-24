using System.ComponentModel.DataAnnotations;

using Arad.Portal.DataLayer.Entities.General.SendMessage;

namespace Arad.Portal.Models.Shared.SendMessage;

public class SmsDto
{
    public string? Id { get; set; }
    [Required]
    public string BaseAddress { get; init; }
    [Required]
    public string SenderNumber { get; init; }
    [Required]
    public string ApiKey { get; init; }
    [Required]
    public Provider Provider { get; set; }
    public string AssociatedDomainId { get; set; }
}