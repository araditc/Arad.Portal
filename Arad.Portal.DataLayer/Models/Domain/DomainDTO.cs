using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Models.Domain
{
    public class DomainDTO
    {
        public DomainDTO()
        {
            Prices = new();
            DomainPaymentProviders = new();
        }
        public string DomainId { get; set; }

        public string DomainName { get; set; }

        public string OwnerUserId { get; set; }

        public string OwnerUserName { get; set; }

        public string DefaultLanguageId { get; set; }

        public string DefaultLangSymbol { get; set; }

        public string DefaultLanguageName { get; set; }

        public string DefaultCurrencyName { get; set; }

        public string DefaultCurrencyId { get; set; }

        public bool IsDefault { get; set; }

        //public Price  DomainPrice { get; set; }

        public bool IsDeleted { get; set; }

        public List<PriceDTO> Prices { get; set; }

        public List<ProviderDetailDTO> DomainPaymentProviders { get; set; }

        public string InvoiceNumberProcedure { get; set; }

        /// <summary>
        /// if InvoiceNumberProcedure=CustomFromMyInstance owner should fill this prop
        /// otherwise main domain will generate the invoice number for this domain
        /// </summary>
        public string InvoiceNumberInitializer { get; set; }

        public int? IncreasementValue { get; set; }

        public string MainPageTemplateId { get; set; }

        /// <summary>
        /// for example [0] : "moduleId" or "moduleId1 <br/> moduleId2 <br/>" as one object of keyVal
        /// </summary>
        public List<KeyVal> MainPageTemplateParamsValue { get; set; }

        /// <summary>
        /// parameters in  Modules
        /// </summary>
        public List<ModuleParams> MainPageModuleParamsWithValues { get; set; }
        public string ContentTemplateId { get; set; }

        /// <summary>
        /// for example [0] : "moduleId" as one object of keyVal
        /// </summary>
        public List<KeyVal> ContentTemplateParamsValue { get; set; }
        /// <summary>
        /// parameters in  Modules
        /// </summary>
        public List<ModuleParams> ContentModuleParamsWithValues { get; set; }
        public string ProductTemplateId { get; set; }
        /// <summary>
        /// for example [0] : "moduleId" as one object of keyVal
        /// </summary>
        public List<KeyVal> ProductTemplateParamsValue { get; set; }
        /// <summary>
        /// parameters in  Modules
        /// </summary>
        public List<ModuleParams> ProductModuleParamsWithValues { get; set; }
    }

    public class ProviderDetailDTO
    {
        public string Type { get; set; }
        public PspType PspType { get; set; }
        public string DomainValueProvider { get; set; }
    }
}
