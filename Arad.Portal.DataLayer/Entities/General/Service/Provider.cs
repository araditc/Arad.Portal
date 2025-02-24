using Arad.Portal.DataLayer.Entities.Abstractions;

namespace Arad.Portal.DataLayer.Entities.General.Service;

public class Provider : BaseEntity
{
    public ProviderType ProviderType { get; set; }

    public string ProviderName { get; set; }

    public string Template { get; set; }
}

public enum ProviderType
{
    Shipping,
    Payment,
    SMS
    //,
    //etc
}