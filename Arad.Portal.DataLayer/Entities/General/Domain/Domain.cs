using Arad.Portal.DataLayer.Entities.General.Email;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.DesignStructure;
using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Entities.General.Domain
{
    public class Domain : BaseEntity
    {
        public Domain()
        {
            Prices = new();
            DomainPaymentProviders = new();
            HomePageDesign = new();
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string DomainId { get; set; }

        public string DomainName { get; set; }

        public string OwnerUserId { get; set; }

        public string OwnerUserName { get; set; }

        public string DefaultLanguageId { get; set; }

        public string DefaultLangSymbol { get; set; }

        public string DefaultLanguageName { get; set; }

        public string DefaultCurrencyId { get; set; }

        public string DefaultCurrencyName { get; set; }

        public bool IsDefault { get; set; }

        public SMTP SMTPAccount  { get; set; }

        public POP POPAccount { get; set; }

        public bool IsMultiLinguals { get; set; }

        public bool IsShop { get; set; }

        public List<Price> Prices { get; set; }

        public List<ProviderDetail> DomainPaymentProviders { get; set; } 

        public InvoiceNumberProcedure InvoiceNumberProcedure { get; set; }
        /// <summary>
        /// if InvoiceNumberProcedure = CustomFromMyInstance owner should fill this prop
        /// otherwise main domain will generate the invoice number for this domain
        /// </summary>
        public string InvoiceNumberInitializer { get; set; }
        public int? IncreasementValue { get; set; }

        /// <summary>
        /// روش حمل پیش فرض فروشنده
        /// </summary>
        public string DefaultShippingTypeId { get; set; }

        public List<PageDesignContent> HomePageDesign { get; set; }
        
    }
    public class ProviderDetail
    {
        public PspType PspType { get; set; }
        public string DomainValueProvider { get; set; }
    }
    public enum InvoiceNumberProcedure
    {
        FromMainDomain,
        CustomFromMyInstance
    }
    
}
