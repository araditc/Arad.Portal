using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Models.Shared.DesignStructure;

using System.Collections.Generic;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.Models.Shared.Domain;

public class DomainDto
{
    public DomainDto()
    {
        Prices = new();
        DomainPaymentProviders = new();
        HomePageDesign = new();
        SupportedLangId = new();
    }
    public string Id { get; set; }

    public string DomainName { get; set; }

    public string Title { get; set; }

    public string OwnerUserId { get; set; }

    public string OwnerUserName { get; set; }

    public string DefaultLanguageId { get; set; }

    public string DefaultLangSymbol { get; set; }

    public string DefaultLanguageName { get; set; }

    public string DefaultCurrencyName { get; set; }

    public string DefaultCurrencyId { get; set; }

    public int DefaultShippingTypeId { get; set; }

    public bool IsDefault { get; set; }

    //public Price  DomainPrice { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsMultiLinguals { get; set; }

    public bool IsShop { get; set; }

    public List<PriceDto> Prices { get; set; }

    public List<string> SupportedLangId { get; set; }

    public List<ProviderDetailDto> DomainPaymentProviders { get; set; }

    public InvoiceNumberProcedure InvoiceNumberProcedure { get; set; }

    /// <summary>
    /// if InvoiceNumberProcedure=CustomFromMyInstance owner should fill this prop
    /// otherwise main domain will generate the invoice number for this domain
    /// </summary>
    public string InvoiceNumberInitializer { get; set; }

    public int? IncreasementValue { get; set; }

    public string LastInvoiceNumber { get; set; }

    public string MainPageTemplateId { get; set; }

    public List<PageDesignContent> HomePageDesign { get; set; }

    public List<PageDesignContent> ProductPageDesign { get; set; }

    public List<PageDesignContent> BlogPageDesign { get; set; }

}

public class ProviderDetailDto
{
    public string Type { get; init; }
    public PspType PspType { get; set; }
    public string DomainValueProvider { get; init; }
}