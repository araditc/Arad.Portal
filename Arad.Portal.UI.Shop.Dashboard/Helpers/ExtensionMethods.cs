using Arad.Portal.DataLayer.Contracts.General.Permission;
using Arad.Portal.DataLayer.Contracts.General.Role;
using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.User;
using Arad.Portal.DataLayer.Repositories.General.Permission.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Role.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Newtonsoft.Json;
using Arad.Portal.DataLayer.Entities.General.Permission;
using Arad.Portal.DataLayer.Models.Role;
using Arad.Portal.DataLayer.Entities.General.Role;
using Arad.Portal.DataLayer.Repositories.General.MessageTemplate.Mongo;
using Arad.Portal.DataLayer.Contracts.General.MessageTemplate;
using Arad.Portal.DataLayer.Entities.General.MessageTemplate;
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Repositories.General.Currency.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.BasicData;
using Arad.Portal.DataLayer.Repositories.General.BasicData.Mongo;
using Arad.Portal.DataLayer.Contracts.General.Services;
using Arad.Portal.DataLayer.Repositories.General.Service.Mongo;
using Arad.Portal.DataLayer.Repositories.General.CountryParts.Mongo;
using Arad.Portal.DataLayer.Contracts.General.CountryParts;
using Arad.Portal.DataLayer.Contracts.General.DesignStructure;
using Arad.Portal.DataLayer.Repositories.General.DesignStructure.Mongo;
using Microsoft.AspNetCore.Http;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{
    public static class ExtensionMethods
    {
        public static void UseSeedDatabase(this IApplicationBuilder app, string applicationPath)
        {
            using var scope = app.ApplicationServices.CreateScope();

            #region Language
            var languageRepository =
                (LanguageRepository)scope.ServiceProvider.GetService(typeof(ILanguageRepository));
            if (!languageRepository.HasAny())
            {
                var lan = new DataLayer.Entities.General.Language.Language()
                {
                    //LanguageId = Guid.NewGuid().ToString(),
                    LanguageId = "9cbda53a-f5ca-47df-a648-44de1c06c75c",
                    Direction = DataLayer.Entities.General.Language.Direction.ltr,
                    IsActive = true,
                    LanguageName = "English",
                    Symbol = "en-US",
                    IsDefault = false
                };
                languageRepository.InsertOne(lan);
                var lanFa = new DataLayer.Entities.General.Language.Language()
                {
                    //LanguageId = Guid.NewGuid().ToString(),
                    LanguageId = "0f0815fb-5fca-470c-bbfd-4d8c162de05a",
                    Direction = DataLayer.Entities.General.Language.Direction.rtl,
                    IsActive = true,
                    LanguageName = "فارسی",
                    Symbol = "fa-IR",
                    IsDefault = true
                };
                languageRepository.InsertOne(lanFa);
            }
            #endregion Language

            #region currency
            var currencyRepository =
                 (CurrencyRepository)scope.ServiceProvider.GetService(typeof(ICurrencyRepository));
            if (!currencyRepository.HasAny())
            {
                var currencyDef = new DataLayer.Entities.General.Currency.Currency
                {
                    //CurrencyId = Guid.NewGuid().ToString(),
                    CurrencyId = "f6a41b2d-3ed5-412b-b511-68498a7b62f3",
                    CurrencyName = "ریال",
                    Prefix = "IRR",
                    Symbol = "ریال",
                    IsDefault = true,
                    IsActive = true
                };
                currencyRepository.InsertOne(currencyDef);

                var currency = new DataLayer.Entities.General.Currency.Currency
                {
                    //CurrencyId = Guid.NewGuid().ToString(),
                    CurrencyId = "9c3138e3-95a0-4b4d-ac2a-07354a090927",
                    CurrencyName = "American Dollar",
                    Prefix = "USD",
                    Symbol = "$",
                    IsDefault = false,
                    IsActive = true
                };
                currencyRepository.InsertOne(currency);
            }
            #endregion currency

            #region permission
            var permissionRepository =
                (PermissionRepository)scope.ServiceProvider.GetService(typeof(IPermissionRepository));

            if (!permissionRepository.HasAny())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "permissions.json"));
                string json = r.ReadToEnd();
                List<Permission> items = JsonConvert.DeserializeObject<List<Permission>>(json);

                if (items.Any())
                {
                    permissionRepository.InsertMany(items).Wait();
                }
            }
            #endregion permission


            #region country, state, city
            var countryRepository =
                (CountryRepository)scope.ServiceProvider.GetService(typeof(ICountryRepository));

            if (!countryRepository.Countries.AsQueryable().Any())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "Countries.json"));
                string json = r.ReadToEnd();
                List<DataLayer.Entities.General.Country.Country> countries = JsonConvert.DeserializeObject<List<DataLayer.Entities.General.Country.Country>>(json);

                if (countries.Any())
                {
                    countryRepository.Countries.InsertMany(countries);
                }
            }
            #endregion country, state, city

            #region messageTemplate
            var messageTemplateRepository =
                (MessageTemplateRepository)scope.ServiceProvider.GetService(typeof(IMessageTemplateRepository));

            if (!messageTemplateRepository.HasAny())
            {
                using StreamReader r = new(Path.Combine(applicationPath, "SeedData", "MessageTemplate.json"));
                string json = r.ReadToEnd();
                List<MessageTemplate> messageTemplates = JsonConvert.DeserializeObject<List<MessageTemplate>>(json);

                if (messageTemplates != null && messageTemplates.Any())
                {
                    foreach (MessageTemplate messageTemplate in messageTemplates)
                    {
                        messageTemplate.CreationDate = DateTime.Now;
                        messageTemplate.CreatorUserId = "ba63fb8b-3a2d-4efb-8be2-710fa21f68fa";
                        messageTemplate.CreatorUserName = "superAdmin";
                    }
                    messageTemplateRepository.InsertMany(messageTemplates);
                }
            }
            #endregion

            #region providers diffrenet Types
            var providerRepository =
              (ProviderRepository)scope.ServiceProvider.GetService(typeof(IProviderRepository));
            if (providerRepository.GetProvidersPerType(DataLayer.Entities.General.Service.ProviderType.Payment).Count() == 0)
            {
                var parsianGateway = new DataLayer.Entities.General.Service.Provider()
                {
                    ProviderId = Guid.NewGuid().ToString(),
                    CreationDate = DateTime.Now,
                    IsActive = true,
                    Template = "{'BaseUrl','UserName','PINCode', 'TerminalId'}",
                    ProviderType = DataLayer.Entities.General.Service.ProviderType.Payment,
                    ProviderName = "Parsian",
                    AssociatedDomainId = "d24ceebd-c587-4a02-a201-3ad5a9345daf"
                };
                providerRepository.InsertOne(parsianGateway);

                var iranKishGateway = new DataLayer.Entities.General.Service.Provider()
                {
                    ProviderId = Guid.NewGuid().ToString(),
                    CreationDate = DateTime.Now,
                    IsActive = true,
                    Template = "{'BaseUrl','UserName', 'Password', 'MerchantId','TerminalId','AcceptorId', 'AccountIban', 'Sha1'}",
                    ProviderType = DataLayer.Entities.General.Service.ProviderType.Payment,
                    ProviderName = "IranKish",
                    AssociatedDomainId = "d24ceebd-c587-4a02-a201-3ad5a9345daf"
                };
                providerRepository.InsertOne(iranKishGateway);

                var samanGateway = new DataLayer.Entities.General.Service.Provider()
                {
                    ProviderId = Guid.NewGuid().ToString(),
                    CreationDate = DateTime.Now,
                    IsActive = true,
                    Template = "{'BaseAddress', 'TokenEndPoint', 'GatewayEndPoint', 'VerifyEndpoint', 'Password', 'TerminalId'}",
                    ProviderType = DataLayer.Entities.General.Service.ProviderType.Payment,
                    ProviderName = "Saman",
                    AssociatedDomainId = "d24ceebd-c587-4a02-a201-3ad5a9345daf"
                };
                providerRepository.InsertOne(samanGateway);
            }
            #endregion


            #region Module
            var moduleRepository =
            (ModuleRepository)scope.ServiceProvider.GetService(typeof(IModuleRepository));
            if (!moduleRepository.HasAnyModule())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "Modules.json"));
                string json = r.ReadToEnd();
                List<DataLayer.Entities.General.DesignStructure.Module> modules = JsonConvert.DeserializeObject<List<DataLayer.Entities.General.DesignStructure.Module>>(json);

                if (modules.Any())
                {
                    moduleRepository.Modules.InsertMany(modules);
                }
            }
            #endregion

        }

        public static T GetComplexData<T>(this ISession session, string key)
        {
            var data = session.GetString(key);
            if (data == null)
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(data);
        }

        public static void SetComplexData(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
    }
}
