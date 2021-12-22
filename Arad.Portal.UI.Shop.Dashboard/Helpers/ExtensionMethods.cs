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
using Arad.Portal.DataLayer.Entities.General.City;
using Arad.Portal.DataLayer.Entities.General.State;
using Arad.Portal.DataLayer.Entities.General.County;
using Arad.Portal.DataLayer.Entities.General.District;
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

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{
    public static class ExtensionMethods
    {
        public static void UseSeedDatabase(this IApplicationBuilder app, string applicationPath)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var userManager =
                (UserManager<ApplicationUser>)scope.ServiceProvider
                    .GetService(typeof(UserManager<ApplicationUser>));

            if (userManager != null && !userManager.Users.Any())
            {
                var user = new ApplicationUser()
                {
                    //???
                    //just for testing
                    //Id = Guid.NewGuid().ToString(),
                    Id = "ba63fb8b-3a2d-4efb-8be2-710fa21f68fa",
                    UserName = "SuperAdmin",
                    Claims = new List<IdentityUserClaim<string>>()
                    {

                        new IdentityUserClaim<string>()
                        {
                            ClaimType = "AppRole",
                            ClaimValue = true.ToString()
                        },
                        new IdentityUserClaim<string>()
                        {
                            ClaimType = "RelatedDomain",
                            ClaimValue = "f4dd87ab-cf2f-4711-9e1e-64735aa364de"
                        }
                    },
                    PhoneNumber = "989309910790",
                    IsActive = true,
                    IsSystemAccount = true,
                    Profile = new Profile()
                    {
                        Gender = Gender.Women,
                        FatherName = "نام پدر",
                        FirstName = "ادمین",
                        LastName = "شماره یک",
                        FullName = "ادمین" + " " + "شماره یک",
                        BirthDate = new DateTime(1987, 8, 26).ToUniversalTime(),
                        NationalId = "1292086734",
                        CompanyName = "Arad",
                        UserType = UserType.Admin
                    },
                    Addresses = new List<Address>(),
                    CreationDate = DateTime.UtcNow
                };

                userManager.CreateAsync(user, "Sa@12345").Wait();
            }


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


            var roleRepository =
                (RoleRepository)scope.ServiceProvider.GetService(typeof(IRoleRepository));

            if (!roleRepository.States.AsQueryable().Any())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "States.json"));
                string json = r.ReadToEnd();
                List<State> states = JsonConvert.DeserializeObject<List<State>>(json);

                if (states.Any())
                {
                    roleRepository.States.InsertMany(states);
                }
            }

            if (!roleRepository.Counties.AsQueryable().Any())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "States.json"));
                string json = r.ReadToEnd();
                List<County> counties = JsonConvert.DeserializeObject<List<County>>(json);

                if (counties.Any())
                {
                    roleRepository.Counties.InsertMany(counties);
                }
            }

            if (!roleRepository.Districts.AsQueryable().Any())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "States.json"));
                string json = r.ReadToEnd();
                List<District> districts = JsonConvert.DeserializeObject<List<District>>(json);

                if (districts.Any())
                {
                    roleRepository.Districts.InsertMany(districts);
                }
            }

            if (!roleRepository.Cities.AsQueryable().Any())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "States.json"));
                string json = r.ReadToEnd();
                List<City> cities = JsonConvert.DeserializeObject<List<City>>(json);

                if (cities.Any())
                {
                    roleRepository.Cities.InsertMany(cities);
                }
            }

            //Role
            if (!roleRepository.HasAny())
            {
                var role = new Role()
                {
                    RoleId = Guid.NewGuid().ToString(),
                    RoleName = "سوپر ادمین",
                    CreationDate = DateTime.UtcNow,
                    IsActive = true,
                    PermissionIds = permissionRepository.GetAllPermissionIds()
                };
                var role2 = new Role()
                {
                    RoleId = Guid.NewGuid().ToString(),
                    RoleName = "مدیریت کاربران",
                    CreationDate = DateTime.UtcNow,
                    IsActive = true
                };
                var role3 = new Role()
                {
                    RoleId = Guid.NewGuid().ToString(),
                    RoleName = "مشتری",
                    CreationDate = DateTime.UtcNow,
                    IsActive = true
                };
                List<Role> items = new List<Role>();
                items.Add(role);
                items.Add(role2);
                items.Add(role3);

                if (items.Any())
                {
                    roleRepository.InsertMany(items).Wait();
                }
            }

            //domain
            var domainRepository =
               (DomainRepository)scope.ServiceProvider.GetService(typeof(IDomainRepository));
            if (!domainRepository.HasAny())
            {
                var dom = new Domain()
                {
                    //TODO change these two lines
                    //this is for developing
                    // DomainId = Guid.NewGuid().ToString(),
                    DomainId = "d24ceebd-c587-4a02-a201-3ad5a9345daf",
                    DomainName = "https://localhost:3214",//store
                    OwnerUserName = "superadmin",
                    IsActive = true,
                    DefaultLanguageId = "0f0815fb-5fca-470c-bbfd-4d8c162de05a",
                    DefaultLanguageName = "فارسی",
                    DefaultLangSymbol = "fa-IR",
                    DefaultCurrencyId = "f6a41b2d-3ed5-412b-b511-68498a7b62f3",
                    DefaultCurrencyName = "ریال",
                    IsDefault = true,
                    CreationDate = DateTime.Now,
                    SMTPAccount = new DataLayer.Entities.General.Email.SMTP()
                    {
                        EmailAddress = "azizi@arad-itc.com",
                        Encryption = DataLayer.Models.Shared.Enums.EmailEncryptionType.None,
                        IgnoreSSLWarning = true,
                        IsDefault = true,
                        Server = "https://mail.arad-itc.org/",
                        SMTPAuthPassword = "Sa15861136664",
                        SMTPAuthUsername = "azizi",
                        ServerPort = 465
                    }
                };
                var dom2 = new Domain()
                {
                    //DomainId = Guid.NewGuid().ToString(),
                    DomainId = "28d0433f-2bb6-4ef9-bad7-0a18a28d9004",
                    DomainName = "https://localhost:17951",//dashboard
                    OwnerUserName = "superadmin",
                    IsActive = true,
                    DefaultLanguageId = "0f0815fb-5fca-470c-bbfd-4d8c162de05a",
                    DefaultLanguageName = "فارسی",
                    DefaultLangSymbol = "fa-IR",
                    DefaultCurrencyId = "f6a41b2d-3ed5-412b-b511-68498a7b62f3",
                    DefaultCurrencyName = "ریال",
                    IsDefault = true,
                    CreationDate = DateTime.Now,
                    SMTPAccount = new DataLayer.Entities.General.Email.SMTP()
                    {
                        EmailAddress = "azizi@arad-itc.com",
                        Encryption = DataLayer.Models.Shared.Enums.EmailEncryptionType.None,
                        IgnoreSSLWarning = true,
                        IsDefault = true,
                        Server = "https://mail.arad-itc.org/",
                        SMTPAuthPassword = "Sa15861136664",
                        SMTPAuthUsername = "azizi",
                        ServerPort = 465
                    }
                };
                var dom3 = new Domain()
                {
                    //DomainId = Guid.NewGuid().ToString(),
                    DomainId = "94a444a8-c002-4ca8-a353-612f2d7249fb",
                    DomainName = "https://www.hajibazar.com",
                    OwnerUserName = "superadmin",
                    IsActive = true,
                    DefaultLanguageId = "0f0815fb-5fca-470c-bbfd-4d8c162de05a",
                    DefaultLanguageName = "فارسی",
                    DefaultLangSymbol = "fa-IR",
                    DefaultCurrencyId = "f6a41b2d-3ed5-412b-b511-68498a7b62f3",
                    DefaultCurrencyName = "ریال",
                    IsDefault = true,
                    CreationDate = DateTime.Now,
                };

                domainRepository.InsertMany(new List<Domain>() { dom, dom2, dom3 }).Wait();
            }

            //MessageTemplate
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

            //Language
            var languageRepository =
                (LanguageRepository)scope.ServiceProvider.GetService(typeof(ILanguageRepository));
            if (!languageRepository.HasAny())
            {
                var lan = new DataLayer.Entities.General.Language.Language()
                {
                    LanguageId = Guid.NewGuid().ToString(),
                    Direction = DataLayer.Entities.General.Language.Direction.ltr,
                    IsActive = true,
                    LanguageName = "English",
                    Symbol = "en-US",
                    IsDefault = false
                };
                languageRepository.InsertOne(lan);
                var lanFa = new DataLayer.Entities.General.Language.Language()
                {
                    LanguageId = Guid.NewGuid().ToString(),
                    Direction = DataLayer.Entities.General.Language.Direction.rtl,
                    IsActive = true,
                    LanguageName = "فارسی",
                    Symbol = "fa-IR",
                    IsDefault = true
                };
                languageRepository.InsertOne(lanFa);
            }


            //Currency
            var currencyRepository =
                 (CurrencyRepository)scope.ServiceProvider.GetService(typeof(ICurrencyRepository));
            if (!currencyRepository.HasAny())
            {
                var currencyDef = new DataLayer.Entities.General.Currency.Currency
                {
                    CurrencyId = Guid.NewGuid().ToString(),
                    CurrencyName = "ریال",
                    Prefix = "IRR",
                    Symbol = "ریال",
                    IsDefault = true,
                    IsActive = true
                };
                currencyRepository.InsertOne(currencyDef);


                var currency = new DataLayer.Entities.General.Currency.Currency
                {
                    CurrencyId = Guid.NewGuid().ToString(),
                    CurrencyName = "American Dollar",
                    Prefix = "USD",
                    Symbol = "$",
                    IsDefault = false,
                    IsActive = true
                };
                currencyRepository.InsertOne(currency);
            }

            //BasicData
            var basicDataRepository =
                 (BasicDataRepository)scope.ServiceProvider.GetService(typeof(IBasicDataRepository));
            if (!basicDataRepository.HasLastID())
            {
                var def = new DataLayer.Entities.General.BasicData.BasicData
                {
                    BasicDataId = Guid.NewGuid().ToString(),
                    GroupKey = "lastid",
                    Text = "0",
                    Value = "0",
                    Order = 1
                };
                basicDataRepository.InsertOne(def);
            }
            if (!basicDataRepository.HasShippingType())
            {
                var post = new DataLayer.Entities.General.BasicData.BasicData
                {
                    BasicDataId = Guid.NewGuid().ToString(),
                    GroupKey = "ShippingType",
                    Text = "Post",
                    Value = "1",
                    Order = 1
                };
                basicDataRepository.InsertOne(post);


                var courier = new DataLayer.Entities.General.BasicData.BasicData
                {
                    BasicDataId = Guid.NewGuid().ToString(),
                    GroupKey = "ShippingType",
                    Text = "Courier",
                    Value = "2",
                    Order = 2

                };
                basicDataRepository.InsertOne(courier);
            }

            //providers diffrenet Types

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
                    ProviderName = "درگاه  پرداخت پارسیان",
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
                    ProviderName = "درگاه  پرداخت ایران کیش",
                    AssociatedDomainId = "d24ceebd-c587-4a02-a201-3ad5a9345daf"
                };
                providerRepository.InsertOne(iranKishGateway);

                var samanGateway = new DataLayer.Entities.General.Service.Provider()
                {
                    ProviderId = Guid.NewGuid().ToString(),
                    CreationDate = DateTime.Now,
                    IsActive = true,
                    Template = "{'BaseAddress', 'TokenEndPoint', 'GatewayEndPoint', 'VerifyEndpoint'}",
                    ProviderType = DataLayer.Entities.General.Service.ProviderType.Payment,
                    ProviderName = "درگاه  پرداخت ایران کیش",
                    AssociatedDomainId = "d24ceebd-c587-4a02-a201-3ad5a9345daf"
                };
                providerRepository.InsertOne(samanGateway);

            }

        }
    }
}
