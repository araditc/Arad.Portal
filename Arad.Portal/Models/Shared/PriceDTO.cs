using System;

namespace Arad.Portal.Models.Shared;

public class PriceDto
{
    public string PriceId { get; set; }

    public string CurrencyId { get; init; }

    public string CurrencyName { get; init; }

    public string Symbol { get; set; }

    public string Prefix { get; set; }

    public long PriceValue { get; init; }

    public bool IsActive { get; set; }

    public string StartDate { get; init; }

    public DateTime? SDate { get; set; }

    public string EndDate { get; init; }

    public DateTime? EDate { get; set; }
}