using Arad.Portal.DataLayer.Entities.Abstractions;


namespace Arad.Portal.DataLayer.Entities.General.Currency;

/// <summary>
/// only SysAccount user can define currency
/// </summary>
public class Currency : BaseEntity
{
    public string CurrencyName { get; set; }

    /// <summary>
    /// each currency has a prefix that define that currency it is that prefix
    /// </summary>
    public string Prefix { get; set; }

    public string Symbol { get; set; }

    public bool IsDefault { get; set; }

}