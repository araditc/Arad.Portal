using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Arad.Portal.DataLayer.Entities.General.Currency;

using Newtonsoft.Json;
using Arad.Portal.DataLayer.Entities.General.Permission;
using Arad.Portal.DataLayer.Entities.General.MessageTemplate;
using Microsoft.AspNetCore.Http;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Service;
using Arad.Portal.DataLayer.Models.Shared.User;
using Arad.Portal.DataLayer.Repositories.Implementations.General.CountryParts.Mongo;
using Arad.Portal.DataLayer.Repositories.Implementations.General.Currency.Mongo;
using Arad.Portal.DataLayer.Repositories.Implementations.General.DesignStructure.Mongo;
using Arad.Portal.DataLayer.Repositories.Implementations.General.Domain.Mongo;
using Arad.Portal.DataLayer.Repositories.Implementations.General.Language.Mongo;
using Arad.Portal.DataLayer.Repositories.Implementations.General.MessageTemplate.Mongo;
using Arad.Portal.DataLayer.Repositories.Implementations.General.Permission.Mongo;
using Arad.Portal.DataLayer.Repositories.Implementations.General.Service.Mongo;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.CountryParts;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Currency;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.DesignStructure;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.MessageTemplate;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Permission;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Services;
using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.Helpers.Shared;

public static class ExtensionMethods
{
    public static void UseSeedDatabase(this IApplicationBuilder app, string applicationPath)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        #region Language
        LanguageRepository languageRepository =
            (LanguageRepository)scope.ServiceProvider.GetService(typeof(ILanguageRepository));
        if (!languageRepository.Any())
        {
            using StreamReader r = new(Path.Combine(applicationPath, "SeedData", "Languages.json"));
            string json = r.ReadToEnd();
            List<DataLayer.Entities.General.Language.Language> languages = JsonConvert.DeserializeObject<List<DataLayer.Entities.General.Language.Language>>(json);

            if (languages.Any())
            {
                _ = languageRepository.InsertAsync(languages);
            }
        }
        #endregion Language

        #region currency
        CurrencyRepository currencyRepository =
            (CurrencyRepository)scope.ServiceProvider.GetService(typeof(ICurrencyRepository));
        if (!currencyRepository.Any())
        {
            Currency currencyDef = new()
                                   {
                                       //CurrencyId = Guid.NewGuid().ToString(),
                                       Id = "f6a41b2d-3ed5-412b-b511-68498a7b62f3",
                                       CurrencyName = "ریال",
                                       Prefix = "IRR",
                                       Symbol = "ریال",
                                       IsDefault = true,
                                       IsActive = true
                                   };
            _ = currencyRepository.InsertAsync(currencyDef);

            Currency currency = new()
                                {
                                    //CurrencyId = Guid.NewGuid().ToString(),
                                    Id = "9c3138e3-95a0-4b4d-ac2a-07354a090927",
                                    CurrencyName = "American Dollar",
                                    Prefix = "USD",
                                    Symbol = "$",
                                    IsDefault = false,
                                    IsActive = true
                                };
            _ = currencyRepository.InsertAsync(currency);
        }
        #endregion currency

        #region permission
        PermissionRepository permissionRepository =
            (PermissionRepository)scope.ServiceProvider.GetService(typeof(IPermissionRepository));

        if (!permissionRepository.Any())
        {
            using StreamReader r = new(Path.Combine(applicationPath, "SeedData", "permissions.json"));
            string json = r.ReadToEnd();
            List<Permission> items = JsonConvert.DeserializeObject<List<Permission>>(json);

            if (items.Any())
            {
                foreach (Permission item in items)
                {
                    item.IsActive = true;
                }
                _ = permissionRepository.InsertAsync(items);
            }
        }
        #endregion permission

        #region Domain
        DomainRepository domainRepository =
            (DomainRepository)scope.ServiceProvider.GetService(typeof(IDomainRepository));
        if (!domainRepository.Any(c => c.IsDefault))
        {
            Domain domain = new()
                            {
                                Id = "1e336912-00e4-4a0a-bee6-20ce8ae49855",
                                DomainName = "http://aradportal.ir",
                                IsDefault = true,
                                IsActive = true,
                                DefaultCurrencyId = "f6a41b2d-3ed5-412b-b511-68498a7b62f3",
                                DefaultLanguageId = "0f0815fb-5fca-470c-bbfd-4d8c162de05a"
                            };
            _ = domainRepository.InsertAsync(domain);
        }
        #endregion

        #region user
        UserManager<ApplicationUser> userManager =
            (UserManager<ApplicationUser>)scope.ServiceProvider
                                               .GetService(typeof(UserManager<ApplicationUser>));

        if (userManager != null && !userManager.Users.Any())
        {
            ApplicationUser user = new()
                                   {
                                       //for testing
                                       //Id = Guid.NewGuid().ToString(),
                                       Id = "ba63fb8b-3a2d-4efb-8be2-710fa21f68fa",
                                       UserName = "SuperAdmin",
                                       PhoneNumber = "989309910790",
                                       IsActive = true,
                                       IsSystemAccount = true,
                                       UserRoleId = "0e1643a3-1212-485b-862f-45147532427c",

                                       Profile = new()
                                                 {
                                                     Gender = Gender.Female,
                                                     FatherName = "نام پدر",
                                                     FirstName = "ادمین",
                                                     LastName = "شماره یک",
                                                     FullName = "ادمین" + " " + "شماره یک",
                                                     NationalCode = "",
                                                     CompanyName = "myComapany",
                                                     DefaultLanguageId = "0f0815fb-5fca-470c-bbfd-4d8c162de05a",
                                                     DefaultCurrencyId = "f6a41b2d-3ed5-412b-b511-68498a7b62f3",
                                                     UserType = UserType.Admin
                                                 },
                                       CreationDate = DateTime.UtcNow
                                   };
            user.Domains.Add(new() { DomainId = "1e336912-00e4-4a0a-bee6-20ce8ae49855", DomainName = "https://localhost:3214", IsOwner = true });
            userManager.CreateAsync(user, "Sa@12345").Wait();
        }
        #endregion user

        #region country, state, city
        CountryRepository countryRepository =
            (CountryRepository)scope.ServiceProvider.GetService(typeof(ICountryRepository));

        if (!countryRepository.Any())
        {
            using StreamReader r = new(Path.Combine(applicationPath, "SeedData", "Countries.json"));
            string json = r.ReadToEnd();
            List<DataLayer.Entities.General.CountryParts.Country> countries = JsonConvert.DeserializeObject<List<DataLayer.Entities.General.CountryParts.Country>>(json);

            if (countries.Any())
            {
                _ = countryRepository.InsertAsync(countries);
            }
        }
        #endregion country, state, city

        #region messageTemplate
        MessageTemplateRepository messageTemplateRepository =
            (MessageTemplateRepository)scope.ServiceProvider.GetService(typeof(IMessageTemplateRepository));

        if (!messageTemplateRepository.Any())
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
                _ = messageTemplateRepository.InsertAsync(messageTemplates);
            }
        }
        #endregion

        #region providers diffrenet Types
        ProviderRepository providerRepository =
            (ProviderRepository)scope.ServiceProvider.GetService(typeof(IProviderRepository));
        List<SelectListModel> res = [];
        List<Provider> lst = providerRepository.GetList(p => p.ProviderType == ProviderType.Payment);

        res = lst.Select(p => new SelectListModel
                              {
                                  Text = p.ProviderName,
                                  Value = p.Id
                              }).ToList();

        res.Insert(0, new()
                      {
                          Value = "-1",
                          Text = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_Choose")
                      });
        if (res.Count() == 0)
        {
            //var parsianGateway = new DataLayer.Entities.General.Service.Provider()
            //{
            //    ProviderId = Guid.NewGuid().ToString(),
            //    CreationDate = DateTime.Now,
            //    IsActive = true,
            //    Template = "{'BaseUrl','UserName','PINCode', 'TerminalId'}",
            //    ProviderType = DataLayer.Entities.General.Service.ProviderType.Payment,
            //    ProviderName = "Parsian",
            //    AssociatedDomainId = "d24ceebd-c587-4a02-a201-3ad5a9345daf"
            //};
            //providerRepository.InsertOne(parsianGateway);

            //var iranKishGateway = new DataLayer.Entities.General.Service.Provider()
            //{
            //    ProviderId = Guid.NewGuid().ToString(),
            //    CreationDate = DateTime.Now,
            //    IsActive = true,
            //    Template = "{'BaseUrl','UserName', 'Password', 'MerchantId','TerminalId','AcceptorId', 'AccountIban', 'Sha1'}",
            //    ProviderType = DataLayer.Entities.General.Service.ProviderType.Payment,
            //    ProviderName = "IranKish",
            //    AssociatedDomainId = "d24ceebd-c587-4a02-a201-3ad5a9345daf"
            //};
            //providerRepository.InsertOne(iranKishGateway);

            Provider samanGateway = new()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        CreationDate = DateTime.Now,
                                        IsActive = true,
                                        Template = "{'BaseAddress', 'TokenEndPoint', 'GatewayEndPoint', 'VerifyEndpoint', 'Password', 'TerminalId'}",
                                        ProviderType = ProviderType.Payment,
                                        ProviderName = "Saman",
                                        AssociatedDomainId = "d24ceebd-c587-4a02-a201-3ad5a9345daf"
                                    };
            _ = providerRepository.InsertAsync(samanGateway);
        }
        #endregion

        #region Module
        ModuleRepository moduleRepository =
            (ModuleRepository)scope.ServiceProvider.GetService(typeof(IModuleRepository));
        if (!moduleRepository.Any())
        {
            using StreamReader r = new(Path.Combine(applicationPath, "SeedData", "Modules.json"));
            string json = r.ReadToEnd();
            List<DataLayer.Entities.General.DesignStructure.Module> modules = JsonConvert.DeserializeObject<List<DataLayer.Entities.General.DesignStructure.Module>>(json);

            if (modules.Any())
            {
                _ = moduleRepository.InsertAsync(modules);
            }
        }
        #endregion

    }

    public static T GetComplexData<T>(this ISession session, string key)
    {
        string data = session.GetString(key);
        return data == null ? default : JsonConvert.DeserializeObject<T>(data);
    }

    public static void SetComplexData(this ISession session, string key, object value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }
}