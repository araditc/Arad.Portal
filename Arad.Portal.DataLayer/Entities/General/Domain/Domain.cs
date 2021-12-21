using Arad.Portal.DataLayer.Entities.General.Email;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.Domain
{
    public class Domain : BaseEntity
    {
        public Domain()
        {
            Prices = new();
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

        public List<Price> Prices { get; set; }

        public List<ProviderDetail> DomainPaymentProviders { get; set; }

        public InvoiceNumberProcedure InvoiceNumberProcedure { get; set; }

        /// <summary>
        /// if InvoiceNumberProcedure=CustomFromMyInstance owner should fill this prop
        /// otherwise main domain will generate the invoice number for this domain
        /// </summary>
        public string InvoiceNumberInitializer { get; set; }
    }


    public class ProviderDetail
    {
        public string ProviderId { get; set; }

        public List<Parameter> DomainValueProvider { get; set; }
    }
    public class Parameter
    {
        public string Key { get; set; }

        public int Value { get; set; }
    }

    public enum InvoiceNumberProcedure
    {
        FromMainDomain,
        CustomFromMyInstance
    }
}
