﻿using Arad.Portal.DataLayer.Contracts.General.Content;
using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Menu;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Repositories.General.User.Mongo;
using MongoDB.Driver;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.Currency;
using System.Collections.Generic;
using System.Security.Claims;
using Arad.Portal.DataLayer.Models.User;
using System.IO;
using MongoDB.Bson.IO;
using System.Xml;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Models.Setting;
using Arad.Portal.DataLayer.Models.Product;
using SharpCompress.Common;
using static Lucene.Net.Util.Fst.Util;
using ClosedXML.Excel;
using Flurl.Util;
using DocumentFormat.OpenXml.Drawing.Charts;
using Arad.Portal.GeneralLibrary.Utilities;
using KeyVal = Arad.Portal.DataLayer.Models.Shared.KeyVal;
using Arad.Portal.DataLayer.Contracts.General.BasicData;
using AspNetCore.Identity.MongoDbCore.Models;
using IApplicationLifetime = Microsoft.Extensions.Hosting.IApplicationLifetime;
using Microsoft.Extensions.Hosting.Internal;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class InstallController : BaseController
    {
        private readonly IDomainRepository _domainRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IMenuRepository _menuRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IContentCategoryRepository _contentCategoryRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly IBasicDataRepository _basicRepository;
        private IApplicationLifetime _ApplicationLifetime { get; set; }
        private IWebHostEnvironment _Environment;
        private AppSetting _appSetting = new AppSetting();
        private bool isReadyToStart = false;
       
        public InstallController(IDomainRepository domainRepository, IMenuRepository menuRepository, 
                                 UserManager<ApplicationUser> userManager, IHttpContextAccessor accessor,
                                 ILanguageRepository lanRepository, ICurrencyRepository curRepository,
                                 IMapper mapper, IWebHostEnvironment environment, IConfiguration config,
                                 IBasicDataRepository basicRepository, IApplicationLifetime applicationLifeTime,
                                 IContentCategoryRepository categoryRepository, IContentRepository contentRepository ):base(accessor, domainRepository)
        {
            _domainRepository = domainRepository;
            _userManager = userManager;
            _contentCategoryRepository = categoryRepository;
            _contentRepository = contentRepository;
            _menuRepository = menuRepository;
            _mapper = mapper;
            _languageRepository = lanRepository;
            _currencyRepository = curRepository;
            _accessor = accessor;
            _Environment = environment;
            _configuration = config;
            _basicRepository = basicRepository;
            _ApplicationLifetime = applicationLifeTime;
        }
        public IActionResult Index()
        {
            var model = new InstallModel();
            _appSetting = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build().Get<AppSetting>();

            _appSetting.DatabaseConfig.ConnectionString = "mongodb://localhost:27017/AradPortalTest";
            _appSetting.DatabaseConfig.DbName = "AradPortalTest";

            var defaultDomainResult = _domainRepository.FetchDefaultDomain();
            var domainEntity = _mapper.Map<Domain>(defaultDomainResult.ReturnValue);
            if (defaultDomainResult.Succeeded)
            {
                model.DomainName = domainEntity.DomainName;
                model.DomainId = domainEntity.DomainId;
                model.Title = domainEntity.Title;
                model.IsShop = domainEntity.IsShop;
                model.IsMultiLinguals = domainEntity.IsMultiLinguals;
                model.CurrencyId = domainEntity.DefaultCurrencyId;
            }
            //var sysAccountUser = _userManager.Users.FirstOrDefault(_ => _.IsSystemAccount);
            //if(sysAccountUser != null)
            //{
            //    model.UserId = sysAccountUser.Id;
            //    model.FirstName = sysAccountUser.Profile.FirstName;
            //    model.LastName = sysAccountUser.Profile.LastName;
            //    model.UserName = sysAccountUser.UserName;
            //}
            var currencyList = _currencyRepository.GetAllActiveCurrency();
            var encryptionTypes = _domainRepository.GetAllEmailEncryptionType();
            ViewBag.EncryptionType = encryptionTypes;
            ViewBag.CurrencyList = currencyList;
            var res = _languageRepository.GetAllActiveLanguage();
            res.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "-1" });
            ViewBag.LangList = res;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveData([FromForm] InstallModel model)
        {
            JsonResult result;
            try
            {
               
                #region domain
                var domain = new Domain()
                {
                    DomainName = model.DomainName,
                    DomainId = Guid.NewGuid().ToString(),
                    Title = model.Title,
                    CreationDate = DateTime.Now,
                    DefaultCurrencyId = model.CurrencyId,
                    DefaultLanguageId = model.DefaultLanguageId,
                    IsShop = model.IsShop,
                    IsMultiLinguals = model.IsMultiLinguals
                };

                domain.SMTPAccount = new DataLayer.Entities.General.Email.SMTP()
                {
                    SMTPId = Guid.NewGuid().ToString(),
                    Server = model.SMTPAccount.Server,
                    EmailAddress = model.SMTPAccount.EmailAddress,
                    DisplayName = model.SMTPAccount.DisplayName,
                    Encryption = model.SMTPAccount.Encryption,
                    IgnoreSSLWarning = true,
                    IsDefault = true,
                    ServerPort = model.SMTPAccount.ServerPort,
                    SMTPAuthPassword = model.SMTPAccount.SMTPAuthPassword,
                    SMTPAuthUsername = model.SMTPAccount.SMTPAuthUsername
                };
               
                domain.Prices = new();
                _domainRepository.InsertOne(domain);
                #endregion domain

                #region user
               
                ApplicationUser user = new()
                {
                    UserName = model.UserName,
                    IsSystemAccount = true,
                    Id = Guid.NewGuid().ToString(),
                    
                    Profile = new DataLayer.Models.User.Profile() { UserType = UserType.Admin, DefaultCurrencyId = model.CurrencyId, DefaultLanguageId = model.DefaultLanguageId },
                    IsDeleted = false,
                    CreatorId = Guid.NewGuid().ToString(),
                    CreatorUserName = model.UserName,
                    CreationDate = DateTime.Now,
                    IsActive = true,
                    PhoneNumber = model.FullMobile,
                    PhoneNumberConfirmed = true,
                    Modifications = new(),
                    IsSiteUser = true
                };

                #region Set claim
                List<MongoClaim> claims = new()
                {
                    new() { Type = ClaimTypes.GivenName, Value = model.FullMobile },
                    new() { Type = "IsActive", Value = true.ToString() },
                    new() { Type = "IsSystemAccount", Value = false.ToString() }
                };
                #endregion
                user.Claims = claims;
                user.Domains.Add(new() { DomainId = domain.DomainId, IsOwner = true, DomainName = domain.DomainName });
                IdentityResult insertResult = await _userManager.CreateAsync(user, model.Password);
                #endregion

                #region update domain
                var domainDto = _domainRepository.FetchDomain(domain.DomainId);
                domainDto.ReturnValue.OwnerUserId = user.Id;
                var res = _domainRepository.EditDomain(domainDto.ReturnValue);
                #endregion

                #region appsetting
                _appSetting.SendSmsConfig.AradVas_Number = model.AradVas_Number;
                _appSetting.SendSmsConfig.AradVas_UserName = model.AradVas_UserName;
                _appSetting.SendSmsConfig.AradVas_Domain = model.AradVas_Domain;
                _appSetting.SendSmsConfig.AradVas_Password = model.AradVas_Password;
                _appSetting.SendSmsConfig.AradVas_Link_1 = model.AradVas_Link_1;

                _appSetting.DatabaseConfig.ConnectionString = model.ConnectionString;
                _appSetting.LocalStaticFileStorage = model.LocalStaticFileStorage;
                _appSetting.LogConfiguration.LogFileDirectory = model.LogFileDirectory;
                _appSetting.LocalStaticFileShown = domain.DomainName;

                _appSetting.IsFirstRun = false.ToString();

                string appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                await System.IO.File.WriteAllTextAsync(appSettingsPath, Newtonsoft.Json.JsonConvert.SerializeObject(_appSetting, Newtonsoft.Json.Formatting.Indented));
                #endregion

                _ApplicationLifetime.StopApplication();
                

                result = new JsonResult(new { Status = "Success", Message = Language.GetString("AlertAndMessage_OperarationDoneSuccessfully") });

            }
            catch (Exception)
            {
                result = new JsonResult(new { Status = "Error", Message = Language.GetString("AlertAndMessage_ErrorInSaving") });
            }

            return result;
            
        }


      
    }
}
