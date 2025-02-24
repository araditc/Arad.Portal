namespace Arad.Portal.Models.Shared.Currency;

public class CurrencyDto
{
    public string Id { get; init; }

    public string CurrencyName { get; init; }

    public string Prefix { get; init; }

    public string Symbol { get; init; }

    public bool IsDeleted { get; init; }

    public bool IsDefault { get; init; }

}