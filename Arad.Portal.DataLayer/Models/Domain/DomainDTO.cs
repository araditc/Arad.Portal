using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Domain
{
    public class DomainDTO
    {
        public DomainDTO()
        {
            Prices = new();
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

        public List<ProviderDetail> DomainPaymentProviders { get; set; }

        public InvoiceNumberProcedure InvoiceNumberProcedure { get; set; }

        /// <summary>
        /// if InvoiceNumberProcedure=CustomFromMyInstance owner should fill this prop
        /// otherwise main domain will generate the invoice number for this domain
        /// </summary>
        public string InvoiceNumberInitializer { get; set; }
    }
}
