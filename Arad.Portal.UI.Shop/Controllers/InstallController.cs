using Arad.Portal.DataLayer.Contracts.General.Content;
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

using MongoDB.Driver;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.Currency;
using System.Collections.Generic;

using Arad.Portal.DataLayer.Models.User;
using System.IO;

using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

using Arad.Portal.GeneralLibrary.Utilities;

using Arad.Portal.DataLayer.Contracts.General.BasicData;

using Arad.Portal.DataLayer.Contracts.General.SliderModule;

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
        private readonly ISliderRepository _sliderRepository;
        private IHostApplicationLifetime _ApplicationLifetime { get; set; }
        private IWebHostEnvironment _Environment;
        private AppSetting _appSetting = new AppSetting();
       
       
        public InstallController(IDomainRepository domainRepository, IMenuRepository menuRepository, 
                                 UserManager<ApplicationUser> userManager, IHttpContextAccessor accessor,
                                 ILanguageRepository lanRepository, ICurrencyRepository curRepository,
                                 IMapper mapper, IWebHostEnvironment environment, IConfiguration config,
                                 ISliderRepository sliderRepository,
                                 IBasicDataRepository basicRepository, IHostApplicationLifetime applicationLifeTime,
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
            _sliderRepository = sliderRepository;
            _ApplicationLifetime = applicationLifeTime;
        }
        public IActionResult Index()
        {
            var model = new InstallModel();
            var appSettingsPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "appsettings.json");
            var appSettingJson = System.IO.File.ReadAllText(appSettingsPath);
            _appSetting = Newtonsoft.Json.JsonConvert.DeserializeObject<AppSetting>(appSettingJson);

            _appSetting.DatabaseConfig.ConnectionString = "mongodb://localhost:27017/TestArad";
            _appSetting.DatabaseConfig.DbName = "TestArad";

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
            var sysAccountUser = _userManager.Users.FirstOrDefault(_ => _.IsSystemAccount);
            if (sysAccountUser != null)
            {
                model.UserId = sysAccountUser.Id;
                model.FirstName = sysAccountUser.Profile.FirstName;
                model.LastName = sysAccountUser.Profile.LastName;
                model.UserName = sysAccountUser.UserName;
                model.PhoneNumber = sysAccountUser.PhoneNumber;
                model.DefaultLanguageId = sysAccountUser.Profile.DefaultLanguageId;
            }

            model.ConnectionString = _appSetting.DatabaseConfig.ConnectionString;

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
            var appSettingsPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "appsettings.json");
            var appSettingJson = System.IO.File.ReadAllText(appSettingsPath);
            _appSetting = Newtonsoft.Json.JsonConvert.DeserializeObject<AppSetting>(appSettingJson);
            try
            {
                #region user
                var dbUser = await _userManager.FindByIdAsync(model.UserId);

                dbUser.UserName = model.UserName;
                dbUser.Profile = new DataLayer.Models.User.Profile()

                {
                    UserType = UserType.Admin,
                    DefaultCurrencyId = model.CurrencyId,
                    DefaultLanguageId = model.DefaultLanguageId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    FullName = $"{model.FirstName} {model.LastName}"
                };
                dbUser.CreatorId = model.UserId;
                dbUser.CreatorUserName = model.UserName;
                dbUser.PhoneNumber = model.FullMobile.Replace("+", "");
                dbUser.IsSiteUser = true;
                dbUser.Domains = new();

                dbUser.Domains.Add(new() { DomainId = model.DomainId, IsOwner = true, DomainName = model.DomainName });
                IdentityResult updateResult = await _userManager.UpdateAsync(dbUser);
                var token = await _userManager.GeneratePasswordResetTokenAsync(dbUser);
                var changePass = await _userManager.ResetPasswordAsync(dbUser, token, model.Password);
                #endregion

                #region domain
                var domain = new Domain()
                {
                    DomainName = model.DomainName,
                    DomainId = model.DomainId,
                    Title = model.Title,
                    CreationDate = DateTime.Now,
                    DefaultCurrencyId = model.CurrencyId,
                    DefaultLanguageId = model.DefaultLanguageId,
                    IsShop = model.IsShop,
                    IsDefault = true,
                    IsActive = true,
                    IsMultiLinguals = model.IsMultiLinguals,
                    OwnerUserId = model.UserId
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
                await _domainRepository.DomainUpdate(domain);
                #endregion domain
               
                #region appsetting
                _appSetting.SmsEndPointConfig.Endpoint = model.SmsEndpoint;
                _appSetting.SmsEndPointConfig.UserName = model.SmsUserName;
                _appSetting.SmsEndPointConfig.Password = model.SmsPassword;
                _appSetting.SmsEndPointConfig.Company = model.SmsCompany;
                _appSetting.SmsEndPointConfig.TokenEndpoint = model.TokenEndpoint;
                _appSetting.SmsEndPointConfig.TokenUserName = model.TokenUserName;
                _appSetting.SmsEndPointConfig.TokenPassword = model.TokenPassword;
                _appSetting.SmsEndPointConfig.LineNumber = model.SmsLineNumber;

                _appSetting.DatabaseConfig.ConnectionString = model.ConnectionString;
                _appSetting.LocalStaticFileStorage = model.LocalStaticFileStorage;
                _appSetting.LogConfiguration.LogFileDirectory = model.LogFileDirectory;
                _appSetting.LocalStaticFileShown = domain.DomainName;

                _appSetting.IsFirstRun = false.ToString();

               
                await System.IO.File.WriteAllTextAsync(appSettingsPath, Newtonsoft.Json.JsonConvert.SerializeObject(_appSetting, Newtonsoft.Json.Formatting.Indented));
                #endregion

                #region HasDefaulthomePage
                if(model.HasDefaultHomeTemplate)
                {
                    using StreamReader cat = new StreamReader(Path.Combine(_Environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "ContentCategory.json"));
                    string catJson = cat.ReadToEnd();
                    List<DataLayer.Entities.General.ContentCategory.ContentCategory> categories = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataLayer.Entities.General.ContentCategory.ContentCategory>>(catJson);

                    if (categories.Any())
                    {
                        _contentCategoryRepository.InsertMany(categories);
                    }


                    using StreamReader r = new StreamReader(Path.Combine(_Environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "Content.json"));
                    string json = r.ReadToEnd();
                    List<DataLayer.Entities.General.Content.Content> contents = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataLayer.Entities.General.Content.Content>>(json);

                    if (contents.Any())
                    {
                        _contentRepository.InsertMany(contents);
                    }

                    using StreamReader menustr = new StreamReader(Path.Combine(_Environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "Menu.json"));
                    string menuJson = menustr.ReadToEnd();
                    List<DataLayer.Entities.General.Menu.Menu> menus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataLayer.Entities.General.Menu.Menu>>(menuJson);

                    if (menus.Any())
                    {
                        _menuRepository.InsertMany(menus);
                    }

                    using StreamReader slider = new StreamReader(Path.Combine(_Environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "Slider.json"));
                    string sliderJson = slider.ReadToEnd();
                    DataLayer.Entities.General.SliderModule.Slider sliderEntity = Newtonsoft.Json.JsonConvert.DeserializeObject<DataLayer.Entities.General.SliderModule.Slider>(sliderJson);

                    if (sliderEntity != null)
                    {
                        _sliderRepository.InsertOne(sliderEntity);
                    }

                    var entity = _basicRepository.UpdateDefaultDomainLastId(11);

                    //sample Images
                    var localStorage = model.LocalStaticFileStorage;
                    var SourceImageDirectory = Path.Combine(_Environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "DefImages");

                    //Copy all the files & Replaces any files with the same name
                    if(!Directory.Exists(Path.Combine(localStorage, "ckEditorDomainImages")))
                    {
                        Directory.CreateDirectory(Path.Combine(localStorage, "ckEditorDomainImages"));
                    }
                    if(!Directory.Exists(Path.Combine(localStorage, "Images\\Contents")))
                    {
                        Directory.CreateDirectory(Path.Combine(localStorage, "Images\\Contents"));
                    }
                    if(!Directory.Exists(Path.Combine(localStorage, "Images\\SliderModule")))
                    {
                        Directory.CreateDirectory(Path.Combine(localStorage, "Images\\SliderModule"));
                    }
                    var dirs = Directory.GetFiles(SourceImageDirectory, "*.*", SearchOption.AllDirectories);
                    foreach (string path in Directory.GetFiles(SourceImageDirectory, "*.*", SearchOption.AllDirectories))
                    {
                        string targetPath = string.Empty;
                       if(path.Contains("ckEditorDomainImages"))
                        {
                          
                            targetPath = Path.Combine(localStorage, "ckEditorDomainImages", Path.GetFileName(path));
                        }
                        else if(path.Contains("Images\\Contents"))
                        {
                            targetPath = Path.Combine(localStorage, "Images\\Contents", Path.GetFileName(path));
                        }
                        else if(path.Contains("Images\\SliderModule"))
                        {
                            targetPath = Path.Combine(localStorage, "Images\\SliderModule", Path.GetFileName(path));
                        }
                        System.IO.File.Copy(path, path.Replace(path, targetPath), true);
                    }

                    using StreamReader dynamicPageStr = new StreamReader(Path.Combine(_Environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "DefaultHomePage.json"));
                    string dynamicJson = dynamicPageStr.ReadToEnd();
                    DataLayer.Models.DesignStructure.PageDesignContent design = Newtonsoft.Json.JsonConvert.DeserializeObject<DataLayer.Models.DesignStructure.PageDesignContent>(dynamicJson);

                    if (design != null)
                    {
                       await  _domainRepository.AddSamplePagetoDomainEntity(domain, design);
                    }

                }
                #endregion

                _ApplicationLifetime.StopApplication();
             

                //result = new JsonResult(new { Status = "Success", Message = Language.GetString("AlertAndMessage_OperarationDoneSuccessfully") });
                result = new JsonResult(new { Status = "success", Message = Language.GetString("AlertAndMessage_RestartApplication") });

            }
            catch (Exception ex)
            {
                result = new JsonResult(new { Status = "Error", Message = Language.GetString("AlertAndMessage_ErrorInSaving") });
            }

            return result;
            
        }


      
    }
}
