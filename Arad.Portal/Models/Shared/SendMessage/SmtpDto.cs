using System.ComponentModel.DataAnnotations;

using Arad.Portal.DataLayer.Entities.General.SendMessage;

namespace Arad.Portal.Models.Shared.SendMessage;

public class SmtpDto
{
    public string Id { get; set; }
    public string AssociatedDomainId { get; set; }
    [Required]
    public string Server { get; init; }
    [Required]
    public string Port { get; init; }
    [Required]
    public string UserName { get; init; }
    [Required]
    public string Password { get; init; }
}