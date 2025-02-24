using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Models.Shared;

using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Domain;

public class DomainModel
{
    public string Id { get; set; }

    public string DomainName { get; init; }

    public List<MultiLingualProperty> Titles { get; init; } = [];

    public string OwnerUserId { get; init; }

    public string OwnerUserName { get; init; }

    public string DefaultLanguageId { get; init; }

    public string DefaultLanguageName { get; init; }

    public string DefaultCurrencyName { get; init; }

    public string? DefaultCurrencyId { get; init; }

    public int DefaultShippingTypeId { get; init; }

    public bool IsDefault { get; init; }

    //public Price  DomainPrice { get; set; }

    public bool IsDeleted { get; init; }

    public bool IsMultiLinguals { get; init; }

    public bool IsShop { get; init; }

    public List<PriceDto> Prices { get; init; } = [];

    public List<string> SupportedLangId { get; init; } = [];

    public List<ProviderDetailDto> DomainPaymentProviders { get; init; } = [];

    public InvoiceNumberProcedure InvoiceNumberProcedure { get; init; }

    /// <summary>
    /// if InvoiceNumberProcedure=CustomFromMyInstance owner should fill this prop
    /// otherwise main domain will generate the invoice number for this domain
    /// </summary>
    public string InvoiceNumberInitializer { get; init; }

    public int? IncreasementValue { get; init; }

    public string LastInvoiceNumber { get; init; }

    public Image LogoImage { get; set; } = new();

    public Image FavicoImage { get; set; } = new();

}