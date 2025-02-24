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

using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.BasicData;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Content;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.ContentCategory;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Currency;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Menu;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SliderModule;
using Arad.Portal.DataLayer.Models.Shared.DesignStructure;
using Arad.Portal.DataLayer.Models.Shared.User;
using System.Threading;

using static Arad.Portal.DataLayer.Models.Shared.Enums;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Arad.Portal.Models.Shared;
using Arad.Portal.Controllers.Base;
using Arad.Portal.DataLayer.Shared;
using Arad.Portal.Helpers.Shared;

namespace Arad.Portal.UIControllers.Setting;

public class InstallController(
    IDomainRepository domainRepository,
    IMenuRepository menuRepository,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor accessor,
    ILanguageRepository lanRepository,
    ICurrencyRepository curRepository,
    IMapper mapper,
    IWebHostEnvironment environment,
    IConfiguration config,
    ISliderRepository sliderRepository,
    IUserRepository userRepository,
    IBasicDataRepository basicRepository,
    IHostApplicationLifetime applicationLifeTime,
    IContentCategoryRepository categoryRepository,
    IContentRepository contentRepository,
    ControllerHelper controllerHelper)
    : BaseController(accessor, domainRepository,lanRepository)
{
    private readonly IDomainRepository _domainRepository = domainRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IConfiguration _configuration = config;
    private readonly ILanguageRepository _languageRepository = lanRepository;
    private readonly ICurrencyRepository _currencyRepository = curRepository;
    private readonly IHttpContextAccessor _accessor = accessor;
    private readonly IBasicDataRepository _basicRepository = basicRepository;

    private IHostApplicationLifetime ApplicationLifetime { get; set; } = applicationLifeTime;

    private AppSetting _appSetting = new();

    public IActionResult Index()
    {
        InstallModel model = new();
        string appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        string appSettingJson = System.IO.File.ReadAllText(appSettingsPath);
        _appSetting = Newtonsoft.Json.JsonConvert.DeserializeObject<AppSetting>(appSettingJson);

        _appSetting.DatabaseConfig.ConnectionString = "mongodb://localhost:27017/TestArad";
        _appSetting.DatabaseConfig.DbName = "TestArad";

        Domain defaultDomainResult = controllerHelper.GetCurrentUserDomain();
        if (defaultDomainResult != null)
        {
            model.DomainName = defaultDomainResult.DomainName;
            model.DomainId = defaultDomainResult.Id;
            model.IsShop = defaultDomainResult.IsShop;
            model.IsMultiLinguals = defaultDomainResult.IsMultiLinguals;
            model.CurrencyId = defaultDomainResult.DefaultCurrencyId;
        }
        ApplicationUser sysAccountUser = userManager.Users.FirstOrDefault(u => u.IsSystemAccount);
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

        List<SelectListModel> slm = [];
        foreach (int i in Enum.GetValues(typeof(EmailEncryptionType)))
        {
            string name = Enum.GetName(typeof(EmailEncryptionType), i);
            SelectListModel obj = new()
                                  {
                                      Text = name,
                                      Value = i.ToString()
                                  };
            slm.Add(obj);
        }
        ViewBag.EncryptionType = slm;

        List<SelectListModel> currencyList = controllerHelper.GetAllActiveCurrency();
        ViewBag.CurrencyList = currencyList;
        List<SelectListModel> res = controllerHelper.GetAllActiveLanguage();
        res.Insert(0, new() { Text = UtilityLanguage.GetString("AlertAndMessage_Choose"), Value = "-1" });
        ViewBag.LangList = res;


        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> SaveData([FromForm] InstallModel model, CancellationToken cancellationToken)
    {
        JsonResult result;
        string appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        string appSettingJson = await System.IO.File.ReadAllTextAsync(appSettingsPath, cancellationToken);
        _appSetting = Newtonsoft.Json.JsonConvert.DeserializeObject<AppSetting>(appSettingJson);
        try
        {
            #region user
            ApplicationUser dbUser = await userManager.FindByIdAsync(model.UserId);

            if (dbUser != null)
            {
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
                dbUser.PhoneNumber = model.FullMobile.Replace("+", "");
                dbUser.CreatorId = model.UserId;
                dbUser.CreatorUserName = model.UserName;
                dbUser.PhoneNumber = dbUser.PhoneNumber;
                dbUser.IsSiteUser = true;
                dbUser.Domains =
                [
                    new() { DomainId = model.DomainId, IsOwner = true, DomainName = model.DomainName }
                ];

                await userRepository.UpdateAsync(c => c.Id == dbUser.Id, m => m.UserName, dbUser.UserName, cancellationToken);
                await userRepository.UpdateAsync(c => c.Id == dbUser.Id, m => m.Profile, dbUser.Profile, cancellationToken);
                await userRepository.UpdateAsync(c => c.Id == dbUser.Id, m => m.CreatorId, dbUser.CreatorId, cancellationToken);
                await userRepository.UpdateAsync(c => c.Id == dbUser.Id, m => m.CreatorUserName, dbUser.CreatorUserName, cancellationToken);
                await userRepository.UpdateAsync(c => c.Id == dbUser.Id, m => m.PhoneNumber, dbUser.PhoneNumber, cancellationToken);
                await userRepository.UpdateAsync(c => c.Id == dbUser.Id, m => m.IsSiteUser, dbUser.IsSiteUser, cancellationToken);
                await userRepository.UpdateAsync(c => c.Id == dbUser.Id, m => m.Domains, dbUser.Domains, cancellationToken);
                string token = await userManager.GeneratePasswordResetTokenAsync(dbUser);
                IdentityResult changePass = await userManager.ResetPasswordAsync(dbUser, token, model.Password);
            }
            #endregion

            #region domain
            Domain domain = new()
                            {
                                DomainName = model.DomainName,
                                Id = model.DomainId,
                                CreationDate = DateTime.Now,
                                DefaultCurrencyId = model.CurrencyId,
                                DefaultLanguageId = model.DefaultLanguageId,
                                IsShop = model.IsShop,
                                IsDefault = true,
                                IsActive = true,
                                IsMultiLinguals = model.IsMultiLinguals,
                                OwnerUserId = model.UserId,

                                //domain.SMTPAccount = new DataLayer.Entities.General.Email.SMTP()
                                //{
                                //    Id = Guid.NewGuid().ToString(),
                                //    Server = model.SMTPAccount.Server,
                                //    EmailAddress = model.SMTPAccount.EmailAddress,
                                //    DisplayName = model.SMTPAccount.DisplayName,
                                //    Encryption = model.SMTPAccount.Encryption,
                                //    IgnoreSSLWarning = true,
                                //    IsDefault = true,
                                //    ServerPort = model.SMTPAccount.ServerPort,
                                //    SMTPAuthPassword = model.SMTPAccount.SMTPAuthPassword,
                                //    SMTPAuthUsername = model.SMTPAccount.SMTPAuthUsername
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


            //await System.IO.File.WriteAllTextAsync(appSettingsPath, Newtonsoft.Json.JsonConvert.SerializeObject(_appSetting, Newtonsoft.Json.Formatting.Indented), cancellationToken);
            //#endregion

            #region HasDefaulthomePage
            if (model.HasDefaultHomeTemplate)
            {
                using StreamReader cat = new(Path.Combine(environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "ContentCategory.json"));
                string catJson = await cat.ReadToEndAsync(cancellationToken);
                List<DataLayer.Entities.General.ContentCategory.ContentCategory> categories = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataLayer.Entities.General.ContentCategory.ContentCategory>>(catJson);

                if (categories.Any())
                {
                    await categoryRepository.InsertAsync(categories, cancellationToken);
                }


                using StreamReader r = new(Path.Combine(environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "Content.json"));
                string json = await r.ReadToEndAsync(cancellationToken);
                List<DataLayer.Entities.General.Content.Content> contents = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataLayer.Entities.General.Content.Content>>(json);

                if (contents.Any())
                {
                    await contentRepository.InsertAsync(contents, cancellationToken);
                }

                using StreamReader menustr = new(Path.Combine(environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "Menu.json"));
                string menuJson = await menustr.ReadToEndAsync(cancellationToken);
                List<DataLayer.Entities.General.Menu.Menu> menus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataLayer.Entities.General.Menu.Menu>>(menuJson);

                if (menus.Any())
                {
                    await menuRepository.InsertAsync(menus, cancellationToken);
                }

                using StreamReader slider = new(Path.Combine(environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "Slider.json"));
                string sliderJson = await slider.ReadToEndAsync(cancellationToken);
                DataLayer.Entities.General.SliderModule.Slider sliderEntity = Newtonsoft.Json.JsonConvert.DeserializeObject<DataLayer.Entities.General.SliderModule.Slider>(sliderJson);

                if (sliderEntity != null)
                {
                    await sliderRepository.InsertAsync(sliderEntity, cancellationToken);
                }

                //     var entity = _basicRepository.UpdateDefaultDomainLastId(11);

                //sample Images
                string localStorage = model.LocalStaticFileStorage;
                string sourceImageDirectory = Path.Combine(environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "DefImages");

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

                using StreamReader dynamicPageStr = new(Path.Combine(environment.ContentRootPath, "SeedData", "DefaultdynamicHomePage", "DefaultHomePage.json"));
                string dynamicJson = await dynamicPageStr.ReadToEndAsync(cancellationToken);
                PageDesignContent design = Newtonsoft.Json.JsonConvert.DeserializeObject<PageDesignContent>(dynamicJson);



            }
            #endregion

            ApplicationLifetime.StopApplication();

            result = new(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_RestartApplication") });

        }
        catch (Exception)
        {
            result = new(new { Status = "Error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorInSaving") });
        }

        return result;

    }



}