using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.Models.Shared.Domain;

public class DomainPrice
{
    public string DomainId { get; set; }
    public Price Price { get; set; }
    public string ModificationReason { get; set; }
}