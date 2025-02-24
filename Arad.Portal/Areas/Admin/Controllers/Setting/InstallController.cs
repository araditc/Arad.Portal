using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Menu;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Currency;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SliderModule;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.BasicData;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Content;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.ContentCategory;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Models.Shared.User;
using System.Threading;

using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Models.Shared.DesignStructure;
using static Arad.Portal.DataLayer.Models.Shared.Enums;
using Serilog;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Arad.Portal.DataLayer.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Helpers.Shared;

namespace Arad.Portal.Areas.Admin.Controllers.Setting;

[Area("Admin")]
public class InstallController : Controller
{
    private readonly IDomainRepository _domainRepository;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;
    private readonly IMenuRepository _menuRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IContentCategoryRepository _contentCategoryRepository;
    private readonly IContentRepository _contentRepository;

    private readonly ISliderRepository _sliderRepository;
    private readonly ControllerHelper _controllerHelper;

    private IHostApplicationLifetime ApplicationLifetime { get; set; }
    private IWebHostEnvironment _environment;
    private AppSetting _appSetting = new();


    public InstallController(IDomainRepository domainRepository, IMenuRepository menuRepository,
                             UserManager<ApplicationUser> userManager, IHttpContextAccessor accessor,
                             ILanguageRepository lanRepository, ICurrencyRepository curRepository, IUserRepository userRepository,
                             IMapper mapper, ILogger logger, IWebHostEnvironment environment, IConfiguration config,
                             ISliderRepository sliderRepository, ControllerHelper controllerHelper,
                             IBasicDataRepository basicRepository, IHostApplicationLifetime applicationLifeTime,
                             IContentCategoryRepository categoryRepository, IContentRepository contentRepository)
    {
        _domainRepository = domainRepository;
        _userManager = userManager;
        _contentCategoryRepository = categoryRepository;
        _contentRepository = contentRepository;
        _menuRepository = menuRepository;
        _mapper = mapper;
        _logger = logger;
        _languageRepository = lanRepository;
        _currencyRepository = curRepository;
        _userRepository = userRepository;
        _environment = environment;
        _sliderRepository = sliderRepository;
        _controllerHelper = controllerHelper;
        ApplicationLifetime = applicationLifeTime;
    }
    public IActionResult Index()
    {
        InstallModel model = new();
        string appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        string appSettingJson = System.IO.File.ReadAllText(appSettingsPath);
        _appSetting = Newtonsoft.Json.JsonConvert.DeserializeObject<AppSetting>(appSettingJson);
        //  var _appSetting = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build().Get<AppSetting>();

        _appSetting.DatabaseConfig.ConnectionString = "mongodb://localhost:27017/TestArad";
        _appSetting.DatabaseConfig.DbName = "TestArad";

        Domain defaultDomainResult = _domainRepository.FirstOrDefault(c => c.IsDefault == true);
        Domain domainEntity = _mapper.Map<Domain>(defaultDomainResult);
        Language language = _languageRepository.FirstOrDefault(c => c.Symbol == CultureInfo.CurrentCulture.Name);
        if (defaultDomainResult != null)
        {
            model.DomainName = domainEntity.DomainName;
            model.DomainId = domainEntity.Id;
            model.Title = domainEntity.Titles.FirstOrDefault(c => c.LanguageId == language.Id)?.Name;
            model.IsShop = domainEntity.IsShop;
            model.IsMultiLinguals = domainEntity.IsMultiLinguals;
            model.CurrencyId = domainEntity.DefaultCurrencyId;
        }
        ApplicationUser sysAccountUser = _userManager.Users.FirstOrDefault(u => u.IsSystemAccount);
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

        List<Currency> currencyList = _currencyRepository.GetList(c => c.IsActive == true);
        List<SelectListModel> result = [];
        result.AddRange(from int i in Enum.GetValues(typeof(EmailEncryptionType)) let name = Enum.GetName(typeof(EmailEncryptionType), i) select new SelectListModel() { Text = name, Value = i.ToString() });

        ViewBag.EncryptionType = result;
        ViewBag.CurrencyList = currencyList;
        List<DataLayer.Entities.General.Language.Language> res = _languageRepository.GetList(c => c.IsActive == true);
        // res.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "-1" });
        ViewBag.LangList = res;


        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> SaveData([FromForm] InstallModel model, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);
        string appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        string appSettingJson = await System.IO.File.ReadAllTextAsync(appSettingsPath, cancellationToken);
        _appSetting = Newtonsoft.Json.JsonConvert.DeserializeObject<AppSetting>(appSettingJson);
        try
        {
            #region user
            ApplicationUser dbUser = await _userManager.FindByIdAsync(model.UserId);

            dbUser.UserName = model.UserName;
            dbUser.Profile = new()
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
            dbUser.Domains =
            [
                new() { DomainId = model.DomainId, IsOwner = true, DomainName = model.DomainName }
            ];

            await _userRepository.UpdateAsync(c => c.Id == dbUser.Id, m => m.UserName, dbUser.UserName, cancellationToken);
            await _userRepository.UpdateAsync(c => c.Id == dbUser.Id, m => m.Profile, dbUser.Profile, cancellationToken);
            await _userRepository.UpdateAsync(c => c.Id == dbUser.Id, m => m.CreatorId, dbUser.CreatorId, cancellationToken);
            await _userRepository.UpdateAsync(c => c.Id == dbUser.Id, m => m.CreatorUserName, dbUser.CreatorUserName, cancellationToken);
            await _userRepository.UpdateAsync(c => c.Id == dbUser.Id, m => m.PhoneNumber, dbUser.PhoneNumber, cancellationToken);
            await _userRepository.UpdateAsync(c => c.Id == dbUser.Id, m => m.IsSiteUser, dbUser.IsSiteUser, cancellationToken);
            await _userRepository.UpdateAsync(c => c.Id == dbUser.Id, m => m.Domains, dbUser.Domains, cancellationToken);
            string token = await _userManager.GeneratePasswordResetTokenAsync(dbUser);
            IdentityResult changePass = await _userManager.ResetPasswordAsync(dbUser, token, model.Password);
            #endregion

            #region domain
            List<MultiLingualProperty> titles = [];
            MultiLingualProperty m = new() { Name = model.Title };
            titles.Add(m);
            Domain domain = new()
            {
                DomainName = model.DomainName,
                Id = model.DomainId,
                Titles = titles,
                CreationDate = DateTime.Now,
                DefaultCurrencyId = model.CurrencyId,
                DefaultLanguageId = model.DefaultLanguageId,
                IsShop = model.IsShop,
                IsDefault = true,
                IsActive = true,
                IsMultiLinguals = model.IsMultiLinguals,
                OwnerUserId = model.UserId,

                //var smtp = new SendMessage()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    SMTPServer = erver = model.SMTPAccount.Server,
                //    Provider = model.SMTPAccount.DisplayName,
                //    Port = model.SMTPAccount.ServerPort,
                //    Password = model.SMTPAccount.SMTPAuthPassword,
                //    UserName = model.SMTPAccount.SMTPAuthUsername
                //};
                Prices = []
            };

            await _domainRepository.UpdateAsync(domain, cancellationToken);
            #endregion domain

            //#region appsetting
            //_appSetting.SmsEndPointConfig.Endpoint = model.SmsEndpoint;
            //_appSetting.SmsEndPointConfig.UserName = model.SmsUserName;
            //_appSetting.SmsEndPointConfig.Password = model.SmsPassword;
            //_appSetting.SmsEndPointConfig.Company = model.SmsCompany;
            //_appSetting.SmsEndPointConfig.TokenEndpoint = model.TokenEndpoint;
            //_appSetting.SmsEndPointConfig.TokenUserName = model.TokenUserName;
            //_appSetting.SmsEndPointConfig.TokenPassword = model.TokenPassword;
            //_appSetting.SmsEndPointConfig.LineNumber = model.SmsLineNumber;

            //_appSetting.DatabaseConfig.ConnectionString = model.ConnectionString;
            //_appSetting.LocalStaticFileStorage = model.LocalStaticFileStorage;
            //_appSetting.LogConfiguration.LogFileDirectory = model.LogFileDirectory;
            //_appSetting.LocalStaticFileShown = domain.DomainName;

            //_appSetting.IsFirstRun = false.ToString();
            //await System.IO.File.WriteAllTextAsync(appSettingsPath, Newtonsoft.Json.JsonConvert.SerializeObject(_appSetting, Newtonsoft.Json.Formatting.Indented));
            //#endregion

            #region HasDefaulthomePage
            if (model.HasDefaultHomeTemplate)
            {
                using StreamReader cat = new(Path.Combine(_environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "ContentCategory.json"));
                string catJson = await cat.ReadToEndAsync(cancellationToken);
                List<DataLayer.Entities.General.ContentCategory.ContentCategory> categories = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataLayer.Entities.General.ContentCategory.ContentCategory>>(catJson);

                if (categories.Any())
                {
                    await _contentCategoryRepository.InsertAsync(categories, cancellationToken);
                }


                using StreamReader r = new(Path.Combine(_environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "Content.json"));
                string json = await r.ReadToEndAsync(cancellationToken);
                List<DataLayer.Entities.General.Content.Content> contents = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataLayer.Entities.General.Content.Content>>(json);

                if (contents.Any())
                {
                    await _contentRepository.InsertAsync(contents, cancellationToken);
                }

                using StreamReader menuStreamReader = new(Path.Combine(_environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "Menu.json"));
                string menuJson = await menuStreamReader.ReadToEndAsync(cancellationToken);
                List<DataLayer.Entities.General.Menu.Menu> menus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataLayer.Entities.General.Menu.Menu>>(menuJson);

                if (menus.Any())
                {
                    await _menuRepository.InsertAsync(menus, cancellationToken);
                }

                using StreamReader slider = new(Path.Combine(_environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "Slider.json"));
                string sliderJson = await slider.ReadToEndAsync(cancellationToken);
                DataLayer.Entities.General.SliderModule.Slider sliderEntity = Newtonsoft.Json.JsonConvert.DeserializeObject<DataLayer.Entities.General.SliderModule.Slider>(sliderJson);

                if (sliderEntity != null)
                {
                    await _sliderRepository.InsertAsync(sliderEntity, cancellationToken);
                }

                //var entity = _basicRepository.UpdateDefaultDomainLastId(11);

                //sample Images
                string localStorage = model.LocalStaticFileStorage;
                string sourceImageDirectory = Path.Combine(_environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "DefImages");

                //Copy all the files & Replaces any files with the same name
                if (!Directory.Exists(Path.Combine(localStorage, "ckEditorDomainImages")))
                {
                    Directory.CreateDirectory(Path.Combine(localStorage, "ckEditorDomainImages"));
                }
                if (!Directory.Exists(Path.Combine(localStorage, "Images\\Contents")))
                {
                    Directory.CreateDirectory(Path.Combine(localStorage, "Images\\Contents"));
                }
                if (!Directory.Exists(Path.Combine(localStorage, "Images\\SliderModule")))
                {
                    Directory.CreateDirectory(Path.Combine(localStorage, "Images\\SliderModule"));
                }
                string[] dirs = Directory.GetFiles(sourceImageDirectory, "*.*", SearchOption.AllDirectories);
                foreach (string path in Directory.GetFiles(sourceImageDirectory, "*.*", SearchOption.AllDirectories))
                {
                    string targetPath = "";
                    if (path.Contains("ckEditorDomainImages"))
                    {

                        targetPath = Path.Combine(localStorage, "ckEditorDomainImages", Path.GetFileName(path));
                    }
                    else if (path.Contains("Images\\Contents"))
                    {
                        targetPath = Path.Combine(localStorage, "Images\\Contents", Path.GetFileName(path));
                    }
                    else if (path.Contains("Images\\SliderModule"))
                    {
                        targetPath = Path.Combine(localStorage, "Images\\SliderModule", Path.GetFileName(path));
                    }
                    System.IO.File.Copy(path, path.Replace(path, targetPath), true);
                }

                using StreamReader dynamicPageStr = new(Path.Combine(_environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "DefaultHomePage.json"));
                string dynamicJson = await dynamicPageStr.ReadToEndAsync(cancellationToken);
                PageDesignContent design = Newtonsoft.Json.JsonConvert.DeserializeObject<PageDesignContent>(dynamicJson);

                if (design != null)
                {
                    await _domainRepository.InsertAsync(domain, cancellationToken);
                }

            }
            #endregion

            ApplicationLifetime.StopApplication();


            //result = new JsonResult(new { Status = "Success", Message = Language.GetString("AlertAndMessage_OperarationDoneSuccessfully") });
            result = new(new { Status = "success", Message = "application stop successfully" });

        }
        catch (Exception e)
        {
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(InstallController)}/{nameof(SaveData)}");
            result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorInSaving") });
        }

        return result;

    }


}