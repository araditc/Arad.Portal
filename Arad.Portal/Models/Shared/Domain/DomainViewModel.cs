using Arad.Portal.DataLayer.Models.Shared;

using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Domain;

public class DomainViewModel
{
    public DomainViewModel()
    {
        Prices = new();
    }
    public string DomainId { get; init; }

    public string DomainName { get; init; }

    public string Title { get; set; }

    public string OwnerUserId { get; set; }

    public string OwnerUserName { get; init; }

    public string DefaultLanguageId { get; set; }

    public string DefaultLanguageName { get; init; }

    public string DefaultCurrencyName { get; init; }

    public string DefaultCurrencyId { get; init; }

    public bool IsDefault { get; init; }

    //public Price  DomainPrice { get; set; }

    public bool IsDeleted { get; init; }

    public List<Price> Prices { get; init; }
}