using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Configuration;

using static Arad.Portal.DataLayer.Models.Shared.Enums;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.DesignStructure;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SliderModule;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.BasicData;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.User;
using AutoMapper;
using Arad.Portal.DataLayer.Models.Shared.DesignStructure;
using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using System.Threading;

using Microsoft.AspNetCore.Hosting;
using System.Collections.Specialized;
using System.Web;

using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Serilog;
using System.Security.Claims;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.BasicData;
using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Models.Shared.Counter;
using Arad.Portal.DataLayer.Models.Shared.ProgressBar;
using Arad.Portal.DataLayer.Models.Shared.FAQ;
using Arad.Portal.DataLayer.Models.Shared.IconBlock;
using Arad.Portal.DataLayer.Models.Shared.PictureWithLabel;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;

using SixLabors.ImageSharp;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.Currency;
using Arad.Portal.Models.Shared.DesignStructure;
using Arad.Portal.Models.Shared.Domain;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Admin;
using Arad.Portal.Helpers.Admin;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using Module = Arad.Portal.DataLayer.Entities.General.DesignStructure.Module;
using static Lucene.Net.Util.Fst.Util;
using Humanizer;

using Image = SixLabors.ImageSharp.Image;
using System.Drawing.Imaging;
using System.Drawing;
using SharpCompress.Common;
using System.Collections;
using DocumentFormat.OpenXml.EMMA;
using System.Security.AccessControl;
using System.Net.NetworkInformation;

using Arad.Portal.DataLayer.Models.Shared.ContactUs;
using Arad.Portal.DataLayer.Models.Shared.SocialMedia;

namespace Arad.Portal.Areas.Admin.Controllers.Setting;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class DomainController(
    IDomainRepository domainRepository,
    UserManager<ApplicationUser> userManager,
    IModuleRepository moduleRepository,
    IBasicDataRepository basicDataRepository,
    IModificationRepository modificationRepository,
    ISliderRepository sliderRepository,
    IConfiguration configuration,
    IWebHostEnvironment webHostEnvironment,
    IUserRepository userRepository,
    IMapper mapper,
    ControllerHelper controllerHelper,
    MinioHelper minioHelper,
    ILogger logger) : Controller
{

    [HttpGet]
    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {

        PagedItems<DomainViewModel> result = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        string queryString = Request.QueryString.ToString();
        try
        {

            NameValueCollection filter = HttpUtility.ParseQueryString(queryString);

            if (string.IsNullOrWhiteSpace(filter["page"]))
            {
                filter.Set("page", "1");
            }

            if (string.IsNullOrWhiteSpace(filter["PageSize"]))
            {
                filter.Set("PageSize", "20");
            }

            int page = Convert.ToInt32(filter["page"]);
            int pageSize = Convert.ToInt32(filter["PageSize"]);
            long totalCount;
            Domain domain = new();
            List<DomainViewModel> list = [];
            if (userDb.IsSystemAccount)
            {
                totalCount = await domainRepository.GetCountAsync(c => true, cancellationToken);
                list = (await domainRepository.GetAllAsync(cancellationToken)).AsQueryable().Take(pageSize).Select(d => new DomainViewModel
                {
                    DomainId = d.Id,
                    DomainName = d.DomainName,
                    Prices = d.Prices,
                    OwnerUserId = d.OwnerUserId,
                    OwnerUserName = d.OwnerUserName,
                    DefaultLanguageId = d.DefaultLanguageId,
                    DefaultLanguageName = d.DefaultLanguageName,
                    IsDefault = d.IsDefault,
                    IsDeleted = d.IsDeleted,
                    DefaultCurrencyId = d.DefaultCurrencyId,
                    DefaultCurrencyName = d.DefaultCurrencyName

                }).ToList();
            }
            else
            {
                domain = controllerHelper.GetCurrentUserDomain();
                totalCount = await domainRepository.GetCountAsync(c => c.CreatorUserId == userDb.Id, cancellationToken);
                list = (await domainRepository.GetAllAsync(cancellationToken)).AsQueryable().Where(d => d.OwnerUserId == domain.OwnerUserId).Skip((page - 1) * pageSize)
                                                                              .Take(pageSize).Select(d => new DomainViewModel
                                                                              {
                                                                                  DomainId = d.Id,
                                                                                  DomainName = d.DomainName,
                                                                                  Prices = d.Prices,
                                                                                  OwnerUserId = d.OwnerUserId,
                                                                                  OwnerUserName = d.OwnerUserName,
                                                                                  DefaultLanguageId = d.DefaultLanguageId,
                                                                                  DefaultLanguageName = d.DefaultLanguageName,
                                                                                  IsDefault = d.IsDefault,
                                                                                  IsDeleted = d.IsDeleted,
                                                                                  DefaultCurrencyId = d.DefaultCurrencyId,
                                                                                  DefaultCurrencyName = d.DefaultCurrencyName

                                                                              }).ToList();
            }



            result.CurrentPage = page;
            result.Items = list;
            result.ItemsCount = totalCount;
            result.PageSize = pageSize;
            result.QueryString = queryString;

        }
        catch (Exception e)
        {
            result.CurrentPage = 1;
            result.Items = [];
            result.ItemsCount = 0;
            result.PageSize = 10;
            result.QueryString = queryString;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(DomainController)}/{nameof(List)}");
        }
        ViewBag.DefLangId = controllerHelper.GetDefaultLanguage().Id;
        ViewBag.LangList = controllerHelper.GetAllActiveLanguage();
        ViewBag.IsSystemAccount = userDb.IsSystemAccount;
        return View(result);
    }

    public async ValueTask<IActionResult> AddEdit(string id, CancellationToken cancellationToken)
    {
        Domain model = new();
        DomainModel domainDto;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        List<SelectListModel> vendors = [];
        if (userDb.IsSystemAccount)
        {
            List<ApplicationUser> vendorList = await userRepository.GetListAsync(c => c.IsVendor == true, cancellationToken);
            vendorList = vendorList.Where(u => !u.Domains.Any(a => a.IsOwner) || u.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId == id).ToList();
            vendors = vendorList.Select(u => new SelectListModel
            {
                Text = u.Profile.FirstName + " " + u.Profile.LastName,
                Value = u.Id.ToString()
            }).ToList();
            vendors.Insert(0, new() { Text = UtilityLanguage.GetString("AlertAndMessage_Choose"), Value = "-1" });
            ViewBag.Vendors = vendors;
        }
        else
        {
            List<ApplicationUser> vendorList = await userRepository.GetListAsync(c => c.IsVendor == true, cancellationToken);
            vendorList = vendorList.Where(u => !u.Domains.Any(a => a.IsOwner) || u.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId == id).ToList();
            vendors = vendorList.Select(u => new SelectListModel
            {
                Text = u.Profile.FirstName + " " + u.Profile.LastName,
                Value = u.Id.ToString()
            }).ToList();
            vendors.Insert(0, new() { Text = UtilityLanguage.GetString("AlertAndMessage_Choose"), Value = "-1" });
            ViewBag.Vendors = vendors;
        }
        List<SelectListModel> slmProvider = [];
        slmProvider.AddRange(from int i in Enum.GetValues(typeof(PspType)) let name = Enum.GetName(typeof(PspType), i) select new SelectListModel { Text = name, Value = i.ToString() });

        slmProvider.Insert(0, new()
        {
            Text = UtilityLanguage.GetString("Choose"),
            Value = "-1"
        });
        ViewBag.Providers = slmProvider;

        if (!string.IsNullOrWhiteSpace(id))
        {
            model = controllerHelper.FetchDomain(id).ReturnValue;
            domainDto = mapper.Map<DomainModel>(model);
        }

        DataLayer.Entities.General.Language.Language lan = controllerHelper.GetDefaultLanguage();
        ViewBag.LangId = lan.Id;

        ViewBag.LangList = controllerHelper.GetAllActiveLanguage();

        List<BasicData> shippingTypeList = controllerHelper.GetBasicDataList("ShippingType", true, true, cancellationToken);
        List<BasicDataModel> shippingTypeListDto = mapper.Map<List<BasicDataModel>>(shippingTypeList);
        ViewBag.ShippingTypeList = shippingTypeListDto;

        ViewBag.CurrencyList = controllerHelper.GetAllActiveCurrency();
        DataLayer.Models.Shared.Result<Currency> defCurrency = controllerHelper.GetDefaultCurrency();
        CurrencyDto defCurrencyDto = mapper.Map<CurrencyDto>(defCurrency.ReturnValue);
        ViewBag.DefCurrency = defCurrencyDto;

        List<SelectListModel> activeLanguages = controllerHelper.GetAllActiveLanguage();
        List<SelectListModel> lst = [];
        lst.AddRange(activeLanguages.Select(item => controllerHelper.FetchLanguage(item.Value)).Select(languageEntity => new SelectListModel { Text = languageEntity.Symbol, Value = languageEntity.Id }));

        ViewBag.Activelanguages = lst;
        List<SelectListModel> result = [];
        result.AddRange(from int i in Enum.GetValues(typeof(InvoiceNumberProcedure)) let name = Enum.GetName(typeof(InvoiceNumberProcedure), i) select new SelectListModel { Text = name, Value = i.ToString() });

        result.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });
        ViewBag.IsSystemAccount = userDb.IsSystemAccount;
        ViewBag.InvoiceNumberEnum = result;
        model.LogoImage ??= new();

        if (model.FavicoImage == null)
        {
            model.FavicoImage = new();
        }
        else
        {
            Domain domain = controllerHelper.GetCurrentUserDomain();
            //string objectName = $"{domain.ReturnValue.DomainName[7..].Replace(':', '-')}/{model.LogoImage.ImageId}/{model.LogoImage.FileName.Replace(':', '-')}";
            //(bool success, byte[] imageData) = await minioHelper.GetObject("domainimage", objectName);
            //if (success)
            //{
            //    model.LogoImage.Content = Convert.ToBase64String(imageData);
            //}
            DataLayer.Entities.General.Language.Language defLang = controllerHelper.GetDefaultLanguage();
            CultureInfo current2 = new(defLang.Symbol)
            {
                DateTimeFormat = new()
                {
                    Calendar = new GregorianCalendar()
                }
            };
            Thread.CurrentThread.CurrentCulture = current2;
        }
        domainDto = mapper.Map<DomainModel>(model);

        return View(domainDto);
    }

    [HttpPost]
    public async ValueTask<IActionResult> SavePageContent([FromBody] DomainPageModel dto, CancellationToken cancellationToken)
    {
        Result res = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        DataLayer.Models.Shared.Result<Domain> result = new();
        try
        {
            if (dto == null)
            {
                return Json(new { Status = "Error", Message = "Error occured" });
            }
            Domain domainEntity = await domainRepository.FirstOrDefaultAsync(d => d.Id == dto.DomainId, cancellationToken);
            domainEntity.IsAnchor = dto.IsAnchor;
            PageDesignContent obj = new()
            {
                LanguageId = dto.LanguageId,
                HeaderPart = dto.HeaderPart,
                FooterPart = dto.FooterPart,
                MainPageContainerPart = dto.MainPageContainerPart
            };

            switch (dto.PageType)
            {
                case PageType.HomePage:
                    if (domainEntity.HomePageDesign.Any(c => c.LanguageId == dto.LanguageId))
                    {
                        PageDesignContent? home = domainEntity.HomePageDesign.FirstOrDefault(c => c.LanguageId == dto.LanguageId);

                        if (home != null)
                        {
                            home.HeaderPart = obj.HeaderPart;
                            home.FooterPart = obj.FooterPart;
                            home.MainPageContainerPart = obj.MainPageContainerPart;
                        }
                    }
                    else
                    {
                        domainEntity.HomePageDesign.Add(obj);
                    }
                    break;
                case PageType.BlogPage:
                    if (domainEntity.BlogPageDesign.Any(c => c.LanguageId == dto.LanguageId))
                    {
                        PageDesignContent blog = domainEntity.BlogPageDesign.FirstOrDefault(c => c.LanguageId == dto.LanguageId);

                        if (blog != null)
                        {
                            blog.HeaderPart = obj.HeaderPart;
                            blog.FooterPart = obj.FooterPart;
                            blog.MainPageContainerPart = obj.MainPageContainerPart;
                        }
                    }
                    else
                    {
                        domainEntity.BlogPageDesign.Add(obj);
                    }
                    break;
                case PageType.ProductPage:
                    if (domainEntity.ProductPageDesign.Any(c => c.LanguageId == dto.LanguageId))
                    {
                        PageDesignContent pro = domainEntity.ProductPageDesign.FirstOrDefault(c => c.LanguageId == dto.LanguageId);

                        if (pro != null)
                        {
                            pro.HeaderPart = obj.HeaderPart;
                            pro.FooterPart = obj.FooterPart;
                            pro.MainPageContainerPart = obj.MainPageContainerPart;
                        }
                    }
                    else
                    {
                        domainEntity.ProductPageDesign.Add(obj);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            result = await domainRepository.UpdateAsync(domainEntity, cancellationToken);
            if (result.Succeeded)
            {
                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Update,
                    CollectionType = CollectionType.Domain,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = domainEntity.Id
                };
                DataLayer.Models.Shared.Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},saving page content with {dto.DomainId} id done successfully");
                res.Succeeded = true;
                res.Message = ConstMessages.SuccessfullyDone;
            }
            else
            {
                res.Message = ConstMessages.ErrorInSaving;
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(DomainController)}/{nameof(SavePageContent)}");
            res.Message = ConstMessages.ExceptionOccured;
        }

        return Json(res.Succeeded ? new { Status = "Success", result.Message }
                        : new { Status = "Error", res.Message });
    }

    [HttpPost]
    public async ValueTask<IActionResult> Add([FromBody] DomainModel dto, CancellationToken cancellationToken)
    {
        JsonResult result;
        DataLayer.Models.Shared.Result<Domain> saveResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            if (!ModelState.IsValid)
            {
                List<AjaxValidationErrorModel> errors = [];

                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry modelStateVal = ModelState[modelStateKey];

                    if (modelStateVal != null)
                    {
                        errors.AddRange(modelStateVal.Errors
                                                     .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                    }
                }
                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                dto.Id = Guid.NewGuid().ToString();

                foreach (ProviderDetailDto item in dto.DomainPaymentProviders)
                {
                    item.PspType = (PspType)Enum.Parse(typeof(PspType), item.Type);
                }
                foreach (PriceDto item in dto.Prices)
                {
                    DataLayer.Models.Shared.Result<Currency> cur = controllerHelper.FetchCurrency(item.CurrencyId);

                    item.PriceId = Guid.NewGuid().ToString();
                    item.Symbol = cur.ReturnValue.Symbol;
                    item.Prefix = cur.ReturnValue.Symbol;
                    item.SDate = item.StartDate.Split(" ")[0].ToEnglishDate();
                }

                if (dto.Prices.Any(p => p.IsActive = false))
                {
                    PriceDto incorrectBoundary = dto.Prices.FirstOrDefault(p => p.IsActive = false);
                    PriceDto activePrice = dto.Prices.FirstOrDefault(p => p.IsActive && p.EDate == null);

                    if (incorrectBoundary != null)
                    {
                        if (activePrice is { SDate: not null })
                        {
                            incorrectBoundary.EDate = activePrice.SDate.Value.AddDays(-1);
                        }
                    }
                }
                int i = 1;
                foreach (BasicData basicObject in from lanId in dto.SupportedLangId
                                                  let language = controllerHelper.FetchLanguage(lanId)
                                                  select new BasicData
                                                  {
                                                      AssociatedDomainId = dto.Id,
                                                      Id = Guid.NewGuid().ToString(),
                                                      GroupKey = "SupportedCultures",
                                                      Value = lanId,
                                                      Text = language.Symbol,
                                                      Order = i,
                                                      CreationDate = DateTime.UtcNow,
                                                      IsActive = true,
                                                      CreatorUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                                                  })
                {
                    await controllerHelper.InsertNewRecord(basicObject, cancellationToken);
                    i++;
                }

                dto.LogoImage.FileName = "RandomContentImage.png";

                dto.FavicoImage.FileName = "RandomFavicon.ico";


                DataLayer.Entities.General.Language.Language defLang = controllerHelper.GetDefaultLanguage();
                CultureInfo current2 = new(defLang.Symbol)
                {
                    DateTimeFormat = new()
                    {
                        Calendar = new GregorianCalendar()
                    }
                };
                Thread.CurrentThread.CurrentCulture = current2;


                if (await domainRepository.AnyAsync(d => d.DomainName == dto.DomainName, cancellationToken))
                {
                    saveResult.Message = ConstMessages.DuplicateField;
                }
                else
                {
                    #region UploadImages
                    CultureInfo current = new("en-US")
                    {
                        DateTimeFormat = new()
                        {
                            Calendar = new GregorianCalendar()
                        }
                    };
                    Thread.CurrentThread.CurrentCulture = current;
                    bool bucketResult = await minioHelper.MakeBucket("domainimage");
                    if (!string.IsNullOrEmpty(dto.LogoImage.Content))
                    {
                        MemoryStream msLogo = new();
                        byte[] logoBytes = Convert.FromBase64String(dto.LogoImage.Content.Replace("data:image/png;base64,", ""));
                        Image logoImage = Image.Load(logoBytes);
                        await logoImage.SaveAsPngAsync(msLogo, cancellationToken);
                        msLogo.Seek(0, SeekOrigin.Begin);
                        if (string.IsNullOrEmpty(dto.LogoImage.ImageId))
                        {
                            dto.LogoImage.ImageId = Guid.NewGuid().ToString();
                        }
                        string logoObjectName = $"{dto.Id}/{dto.LogoImage.ImageId}/{dto.LogoImage.FileName.Replace(':', '-')}";
                        await minioHelper.Upload("domainimage", logoObjectName, msLogo, "image/png", msLogo.Length);
                    }

                    if (!string.IsNullOrEmpty(dto.FavicoImage.Content))
                    {
                        MemoryStream msFavicon = new();
                        byte[] faviconBytes = Convert.FromBase64String(dto.FavicoImage.Content.Replace("data:image/x-icon;base64,", ""));
                        using (MemoryStream icoStream = new MemoryStream(faviconBytes))
                        {
                            using (Icon icon = new Icon(icoStream))
                            {
                                Bitmap bitmap = icon.ToBitmap();
                                bitmap.Save(msFavicon, ImageFormat.Png); // Convert ICO to PNG format
                            }
                        }
                        msFavicon.Seek(0, SeekOrigin.Begin);
                        if (string.IsNullOrEmpty(dto.FavicoImage.ImageId))
                        {
                            dto.FavicoImage.ImageId = Guid.NewGuid().ToString();
                        }
                        string faviconObjectName = $"{dto.Id}/{dto.FavicoImage.ImageId}/{dto.FavicoImage.FileName.Replace(':', '-')}";
                        await minioHelper.Upload("domainimage", faviconObjectName, msFavicon, "image/png", msFavicon.Length); // Save as PNG
                    }
                    #endregion

                    Domain equivalentEntity = mapper.Map<Domain>(dto);
                    //equivalentEntity.DomainId = Guid.NewGuid().ToString();
                    foreach (PriceDto price in dto.Prices)
                    {
                        //price.PriceId = Guid.NewGuid().ToString();
                    }
                    #region Prices
                    equivalentEntity.Prices = [];
                    foreach (PriceDto price in dto.Prices.OrderBy(p => p.SDate))
                    {
                        if (price.IsActive && string.IsNullOrWhiteSpace(price.EndDate))//price is valid from client
                        {
                            if (equivalentEntity.Prices.Any(x => x.CurrencyId == price.CurrencyId && x.EndDate != null && x.IsActive))
                            {
                                Price exist = equivalentEntity.Prices.FirstOrDefault(x => x.CurrencyId == price.CurrencyId && x.EndDate != null && x.IsActive);
                                if (exist != null)
                                {
                                    exist.IsActive = false;
                                    exist.EndDate = DateTime.UtcNow;
                                }
                            }
                        }

                        Price p = new()
                        {
                            PriceId = !string.IsNullOrWhiteSpace(price.PriceId) ? price.PriceId : Guid.NewGuid().ToString(),
                            CurrencyId = price.CurrencyId,
                            CurrencyName = price.CurrencyName,
                            IsActive = string.IsNullOrWhiteSpace(price.PriceId) || price.IsActive,
                            Prefix = price.Prefix,
                            PriceValue = price.PriceValue,
                            StartDate = price.StartDate.Split(" ")[0].ToEnglishDate().ToUniversalTime(),
                            EndDate = !string.IsNullOrWhiteSpace(price.EndDate) ?
                                                    price.EndDate.Split(" ")[0].ToEnglishDate().ToUniversalTime() : null
                        };
                        equivalentEntity.Prices.Add(p);
                    }
                    #endregion


                    if (dto.IsShop)
                    {
                        equivalentEntity.InvoiceNumberProcedure = (InvoiceNumberProcedure)Convert.ToInt32(dto.InvoiceNumberProcedure);
                    }

                    equivalentEntity.CreationDate = DateTime.Now;
                    equivalentEntity.CreatorUserId = userDb.Id;
                    equivalentEntity.CreatorUserName = userDb.UserName;
                    equivalentEntity.IsActive = true;
                    equivalentEntity.DefaultCurrencyId = controllerHelper.GetDefaultCurrency().ReturnValue.Id;


                    saveResult = await domainRepository.InsertAsync(equivalentEntity, cancellationToken);

                    if (saveResult.Succeeded)
                    {

                        #region DomainOwner user update
                        ApplicationUser ownerUser = await userManager.FindByIdAsync(dto.OwnerUserId);
                        if (ownerUser != null && ownerUser.Domains.All(d => d.DomainId != dto.Id))
                        {
                            ownerUser.Domains.Add(new() { DomainId = dto.Id, DomainName = dto.DomainName, IsOwner = true });
                        }
                        else
                        {
                            if (ownerUser != null)
                            {
                                ownerUser.Domains.FirstOrDefault(d => d.DomainId == dto.Id)!.IsOwner = true;
                            }
                        }

                        if (ownerUser != null)
                        {
                            await userRepository.UpdateAsync(u => u.Id == ownerUser.Id, m => m.Domains, ownerUser.Domains, cancellationToken);
                        }
                        #endregion

                   


                        Modification modification = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ActionTypes = ActionTypes.Insert,
                            CollectionType = CollectionType.Domain,
                            Ip = controllerHelper.GetUserIpAddress(),
                            ModifierId = userDb.Id,
                            ModifierUserName = userDb.UserName,
                            ModifyDateTime = DateTime.Now,
                            RecordId = equivalentEntity.Id
                        };
                        DataLayer.Models.Shared.Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                        logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},adding domain with {dto.Id} id and {dto.DomainName} done successfully");
                        saveResult.Message = ConstMessages.SuccessfullyDone;
                    }
                    else
                    {
                        saveResult.Message = ConstMessages.ErrorInSaving;
                    }
                }
            }
        }
        catch (Exception e)
        {
            saveResult.Succeeded = false;
            saveResult.Message = ConstMessages.ExceptionOccured;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(DomainController)}/{nameof(Add)}");
        }
        result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                          : new { Status = "Error", saveResult.Message });
        return result;
    }

    [HttpGet]
    public IActionResult PageDesign(string domainId, PageType pageType)
    {
        DataLayer.Models.Shared.Result<Domain> domainEntity = controllerHelper.FetchDomain(domainId);
        ViewBag.PageType = pageType;
        List<PageDesignContent> finalModel = [];
        ViewBag.DomainId = domainId;
        ViewBag.Url = configuration["LocalStaticFileShown"];
        List<SelectListModel> lanList = controllerHelper.GetAllActiveLanguage();
        lanList.Insert(0, new() { Text = UtilityLanguage.GetString("AlertAndMessage_Choose"), Value = "-1" });
        ViewBag.LangList = lanList;
        ViewBag.ImageRatio = controllerHelper.GetAllImageRatio();
        ViewBag.IsAnchor = domainEntity.ReturnValue.IsAnchor;
        string imageSize = configuration["ProductImageSize:Size"];
        ViewBag.PicSize = imageSize;
        switch (pageType)
        {
            case PageType.HomePage:
                foreach (PageDesignContent design in domainEntity.ReturnValue.HomePageDesign.Where(design => string.IsNullOrWhiteSpace(design.LanguageName)))
                {
                    design.LanguageName = controllerHelper.FetchLanguage(design.LanguageId).LanguageName;
                }
                finalModel = domainEntity.ReturnValue.HomePageDesign;
                break;
            case PageType.ProductPage:
                foreach (PageDesignContent design in domainEntity.ReturnValue.ProductPageDesign.Where(design => string.IsNullOrWhiteSpace(design.LanguageName)))
                {
                    design.LanguageName = controllerHelper.FetchLanguage(design.LanguageId).LanguageName;
                }
                finalModel = domainEntity.ReturnValue.ProductPageDesign;
                break;

            case PageType.BlogPage:
                foreach (PageDesignContent design in domainEntity.ReturnValue.BlogPageDesign.Where(design => string.IsNullOrWhiteSpace(design.LanguageName)))
                {
                    design.LanguageName = controllerHelper.FetchLanguage(design.LanguageId).LanguageName;
                }
                finalModel = domainEntity.ReturnValue.BlogPageDesign;

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(pageType), pageType, null);
            
        }

        if (finalModel.Count != 0)
        {

            if (finalModel.First().HeaderPart.BgImage != null)
            {
                if (finalModel.First().HeaderPart.BgImage.ImageId != null)
                {
                    ValueTask<BgImage> bgImage = GetImage(finalModel.First().HeaderPart.BgImage);
                    finalModel.First().HeaderPart.BgImage.Base64ImageContent = bgImage.Result.Base64ImageContent;
                }
            }

            if (finalModel.First().HeaderPart.CustomizedContent != null)
            {
                foreach (RowContent? item in finalModel.First().HeaderPart.CustomizedContent)
                {
                    if (item.BgImage != null)
                    {
                        if (item.BgImage.ImageId != null)
                        {
                            item.BGType = BGType.Image;
                            ValueTask<BgImage> bgImage = GetImage(item.BgImage);
                            item.BgImage.Base64ImageContent = bgImage.Result.Base64ImageContent;
                        }
                    }
                }
            }

            if (finalModel.First().MainPageContainerPart.BgImage != null)
            {
                if (finalModel.First().MainPageContainerPart.BgImage.ImageId != null)
                {
                    ValueTask<BgImage> bgImage = GetImage(finalModel.First().MainPageContainerPart.BgImage);
                    finalModel.First().MainPageContainerPart.BgImage.Base64ImageContent = bgImage.Result.Base64ImageContent;
                }
            }

            if (finalModel.First().MainPageContainerPart.RowContents != null)
            {
                foreach (RowContent? item in finalModel.First().MainPageContainerPart.RowContents)
                {
                    if (item.BgImage != null)
                    {
                        if (item.BgImage.ImageId != null)
                        {
                            item.BGType = BGType.Image;
                            ValueTask<BgImage> bgImage = GetImage(item.BgImage);
                            item.BgImage.Base64ImageContent = bgImage.Result.Base64ImageContent;
                        }
                    }
                }
            }

            if (finalModel.First().FooterPart.BgImage != null)
            {
                if (finalModel.First().FooterPart.BgImage.ImageId != null)
                {
                    ValueTask<BgImage> bgImage = GetImage(finalModel.First().FooterPart.BgImage);
                    finalModel.First().FooterPart.BgImage.Base64ImageContent = bgImage.Result.Base64ImageContent;
                }
            }

            if (finalModel.First().FooterPart.CustomizedContent != null)
            {
                foreach (RowContent? item in finalModel.First().FooterPart.CustomizedContent)
                {
                    if (item.BgImage != null)
                    {
                        if (item.BgImage.ImageId != null)
                        {
                            item.BGType = BGType.Image;
                            ValueTask<BgImage> bgImage = GetImage(item.BgImage);
                            item.BgImage.Base64ImageContent = bgImage.Result.Base64ImageContent;
                        }
                    }
                }
            }
        }

        return View("~/Areas/Admin/Views/Domain/PrimaryTemplateDesignPage.cshtml", finalModel);
    }

    public async ValueTask<BgImage> GetImage(BgImage image)
    {
        CultureInfo current = new("en-US")
                              {
                                  DateTimeFormat = new()
                                                   {
                                                       Calendar = new GregorianCalendar()
                                                   }
                              };
        Thread.CurrentThread.CurrentCulture = current;
        Domain domain = controllerHelper.GetCurrentUserDomain();
        string objectName = $"{domain.Id}/{image.ImageId}/{image.ImageFileName.Replace(':', '-')}";
        (bool success, byte[] imageData) = await minioHelper.GetObject("templatedesignbgimage", objectName);
        if (success)
        {
            image.Base64ImageContent = "data:image/png;base64," + Convert.ToBase64String(imageData);
        }
        return image;
    }

    [HttpPost]
    public IActionResult StoreDesignPreview([FromBody] TemplateDesign model)
    {
        string key = Guid.NewGuid().ToString();
        HttpContext.Session.SetComplexData(key, model);

        return Json(new { key });

    }

    [HttpGet]
    public IActionResult DesignPreview(string key)
    {

        TemplateDesign data = HttpContext.Session.GetComplexData<TemplateDesign>(key);

        DataLayer.Entities.General.Language.Language lanEntity = controllerHelper.FetchLanguage(data.LanguageId);
        data.LangSymbol = lanEntity.Symbol.Substring(0, 2);

        return View("PrimaryTemplatePreview", data);
    }

    #region GetModulesViewComponents
    [HttpGet]
    public IActionResult GetProductModuleViewComponent(ProductOrContentType productType, ProductTemplateDesign selectionTemplate,
                                                       int count, DataLayer.Entities.General.SliderModule.TransActionType loadAnimation, LoadAnimationType loadAnimationType, string di)
    {
        ModuleParameters moduleParameters = new()
        {
            ProductOrContentType = productType,
            ProductTemplateDesign = selectionTemplate,
            Count = count,
            LoadAnimation = loadAnimation,
            LoadAnimationType = loadAnimationType,
            DomainId = di
        };
        return ViewComponent("SpecialProduct", moduleParameters);
    }

    [HttpGet]
    public IActionResult GetLoginProfileModuleViewComponent(string domainId, bool isShop)
    {
        return ViewComponent("LoginProfile", new { domainId, isShop });
    }

    [HttpGet]
    public IActionResult GetMultiLingualModuleViewComponent()
    {
        return ViewComponent("MultiLingual");
    }

    [HttpGet]
    public IActionResult GetGeneralSearchViewComponent()
    {
        return ViewComponent("GeneralSearch");
    }


    [HttpPost]
    public IActionResult GetContentModuleViewComponent(ProductOrContentType contentType,
                                                       ContentTemplateDesign selectionTemplate,
                                                       int? count,
                                                       DataLayer.Entities.General.SliderModule.TransActionType loadAnimation,
                                                       LoadAnimationType loadAnimationType,
                                                       SelectionType selectionType,
                                                       string catId,
                                                       string di)
    {

        ModuleParameters moduleParameters = new()
        {
            ProductOrContentType = contentType,
            ContentTemplateDesign = selectionTemplate,
            Count = count,
            LoadAnimation = loadAnimation,
            LoadAnimationType = loadAnimationType,
            SelectionType = selectionType,
            CatId = catId,
            DomainId = di
        };
        return ViewComponent("ContentTemplates", new { moduleParameters }
        );
    }

    [HttpGet]
    public IActionResult GetSliderViewComponent(string sliderId, SliderType sliderType)
    {
        return ViewComponent("Slider", new { sliderId, sliderType });
    }

    [HttpPost]
    public IActionResult GetSocialMediaViewComponent([FromBody] List<SocialMedia> socialMedias)
    {
        ModuleParameters moduleParameters = new() { SocialMedias = socialMedias };

        return ViewComponent("SocialMedia", new { moduleParameters });
    }

    [HttpPost]
    public IActionResult GetCounterViewComponent([FromBody] List<Counter> counters)
    {
        return ViewComponent("Counter", new { counters });
    }

    [HttpPost]
    public async ValueTask<IActionResult> GetPictureWithLabelViewComponent([FromBody] List<PictureWithLabel> pictureWithLabels, CancellationToken cancellationToken)
    {
        foreach (PictureWithLabel item in pictureWithLabels)
        {
            CultureInfo current = new("en-US")
            {
                DateTimeFormat = new()
                {
                    Calendar = new GregorianCalendar()
                }
            };
            Thread.CurrentThread.CurrentCulture = current;
            MemoryStream ms = new();
            bool bucketResult = await minioHelper.MakeBucket("pagedesign");
            byte[] byteArray = Convert.FromBase64String(item.Image.Content);
            Image logoImage = Image.Load(byteArray);
            await logoImage.SaveAsPngAsync(ms, cancellationToken: cancellationToken);
            ms.Seek(0, SeekOrigin.Begin);
            Domain domain = controllerHelper.GetCurrentUserDomain();

            string objectName = $"PictureWithLabels/{domain.Id}/{item.Image.ImageId}/RandomPictureWithLabels.png";
            await minioHelper.Upload("pagedesign", objectName, ms, "image/png", ms.Length);
        }

        return ViewComponent("PictureWithLabel", new { pictureWithLabels });
    }

    [HttpPost]
    public IActionResult GetFAQViewComponent([FromBody] List<FAQItem> faqItems)
    {
        return ViewComponent("FAQ", new { faqItems });
    }

    [HttpPost]
    public IActionResult GetIconBlocksViewComponent([FromBody] List<IconBlock> iconBlocks)
    {
        return ViewComponent("IconBlocks", new { iconBlocks });
    }

    [HttpPost]
    public IActionResult GetProgressBarsViewComponent([FromBody] List<ProgressBar> progressBars)
    {
        return ViewComponent("ProgressBars", new { progressBars });
    }
    public IActionResult GetStoreMenuViewComponent()
    {
        return ViewComponent("StoreMenu");
    }

    [HttpPost]
    public IActionResult GetContactUsViewComponent([FromBody] List<ContactUs> contactUsList)
    {
        return ViewComponent("ContactUs", contactUsList);
    }

    #endregion GetModulesViewComponents

    public async ValueTask<IActionResult> GetSpecificModule(string moduleName, string id, int colCount, string rn, string cn, string sec, string langId, CancellationToken cancellationToken)
    {
        Module module = await moduleRepository.FirstOrDefaultAsync(c => c.ModuleName == moduleName, cancellationToken);

        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        string viewName = $"_{moduleName}.cshtml";
        string imageTemplatePath = webHostEnvironment.WebRootPath;
        List<SelectListModel> productOrContentTypes = [];
        foreach (int i in Enum.GetValues(typeof(ProductOrContentType)))
        {
            string name = Enum.GetName(typeof(ProductOrContentType), i);
            SelectListModel obj = new()
            {
                Text = UtilityLanguage.GetString($"Enum_{name}"),
                Value = i.ToString()
            };
            productOrContentTypes.Add(obj);
        }
        ViewBag.ProductOrContentTypeList = productOrContentTypes;
        ViewBag.DomainId = id;

        switch (moduleName.ToLower())
        {
            case "productlist":
                List<SelectImageListModel> productTemplateList = (
                                                                     from int i in Enum.GetValues(typeof(ProductTemplateDesign))
                                                                     select Enum.GetName(typeof(ProductTemplateDesign), i) into name
                                                                     select new SelectImageListModel()
                                                                     {
                                                                         Text = name,
                                                                         Value = name,
                                                                         ImageUrl = $"/Template/Product/{name}.jpg" // Constructing a web URL path
                                                                     }
                                                                 ).ToList();

                ViewBag.ProductTemplateList = productTemplateList;
                ViewBag.TransactionType = controllerHelper.GetAllTransactionType();
                ViewBag.LoadAnimationType = controllerHelper.GetAllLoadAnimationType();
                break;
            case "contentlist":
                List<SelectImageListModel> contentTemplateDesigns = (from int i in Enum.GetValues(typeof(ContentTemplateDesign)) let name = Enum.GetName(typeof(ContentTemplateDesign), i) select new SelectImageListModel() { Text = name, Value = i.ToString() }).ToList();

                foreach (SelectImageListModel item in contentTemplateDesigns)
                {
                    // Construct the web URL path instead of using Path.Combine
                    item.ImageUrl = item.Text.ToLower() != "forth"
                                        ? $"/Template/Content/{item.Text}.jpg"
                                        : (CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                                               ? "/Template/Content/Forth-rtl.jpg"
                                               : "/Template/Content/Forth-ltr.jpg");
                }
                ViewBag.ContentTemplateList = contentTemplateDesigns;

                ViewBag.TransactionType = controllerHelper.GetAllTransactionType();

                ViewBag.LoadAnimationType = controllerHelper.GetAllLoadAnimationType();

                List<SelectListModel> selectionType = (from int i in Enum.GetValues(typeof(SelectionType)) let name = Enum.GetName(typeof(SelectionType), i) select new SelectListModel() { Text = UtilityLanguage.GetString($"Enum_{name}"), Value = i.ToString() }).ToList();
                selectionType.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });
                ViewBag.SelectionType = selectionType;

                ViewBag.CategoryList = controllerHelper.AllActiveContentCategory(langId, userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId);

                ViewBag.ContentList = controllerHelper.GetContentsList(id, "");
                break;
            case "imagetextslider":
                // Slider List
                var sliderList = (await sliderRepository.GetListAsync(s => s.IsActive && !s.IsDeleted && s.AssociatedDomainId == id, cancellationToken))
                                 ?.Select(s => new SelectListModel()
                                               {
                                                   Text = s.Title,
                                                   Value = s.Id.ToString()
                                               }).ToList();

                if (sliderList == null || !sliderList.Any())
                {
                    sliderList = new List<SelectListModel> { new() { Text = UtilityLanguage.GetString("NoData"), Value = "-1" } };
                }

                ViewBag.SliderList = sliderList;

                // Slider Type
                var sliderType = new List<SelectListModel>();

                foreach (int i in Enum.GetValues(typeof(SliderType)))
                {
                    string name = Enum.GetName(typeof(SliderType), i) ?? $"Unknown ({i})";
                    SelectListModel obj = new() { Text = name, Value = i.ToString() };
                    sliderType.Add(obj);
                }

                // Add default "Choose" option
                sliderType.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });

                ViewBag.SliderType = sliderType;
                break;
            case "horizantalstoremenu":
                break;
            case "generalsearch":
                break;
            case "advertisement":
                break;
            case "socialmedia":
                break;
            case "faq":
                break;
            case "progressbars":
                break;
            case "iconblocks":
                break;
            case "picturewithlabel":
                break;
            case "counter":
                break;
            case "contactus":
                break;
        }

        ViewBag.ColCnt = colCount;

        ViewBag.RowNumber = rn;
        ViewBag.ColNumber = cn;
        ViewBag.Section = sec;
        return PartialView($"~/Areas/Admin/Views/Domain/{viewName}", new ModuleWithParametersValue() { });
    }

    [HttpPost]
    public IActionResult SanitizeContent([FromBody] string html)
    {
        bool res = HtmlSanitizer.SanitizeHtml(html);
        return Json(new { isvalid = res });
    }

    [HttpPost]
    public IActionResult GetProviderParams([FromQuery] int pspVal)
    {
        string htmlResult = "";
        PspType psp = (PspType)pspVal;
        switch (psp)
        {
            //case PspType.IranKish:
            //    htmlResult = GenerateForm(typeof(IrankishModel).GetProperties());
            //    break;
            case PspType.Saman:
                htmlResult = GenerateForm(typeof(SamanModel).GetProperties());
                break;
                //case PspType.Parsian:
                //    htmlResult = GenerateForm(typeof(ParsianModel).GetProperties());
                //    break;
        }
        return Content(htmlResult);
    }

    private string GenerateForm(PropertyInfo[] properties)
    {
        string finalHtml = "";
        foreach (PropertyInfo prop in properties)
        {
            finalHtml += $"<div class='form-group col-md-3'><label class='form-label' for='{prop.Name}'>{prop.Name}</label><br/><input type='text' id='{prop.Name}' class='form-control gatewayPar ltr' value='' /></div>";
        }

        return finalHtml;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        DataLayer.Models.Shared.Result<Domain> upResult = new();
        try
        {
            DataLayer.Models.Shared.Result<Domain> domain = controllerHelper.FetchDomain(id);
            if (domain == null)
            {
                result = new(new
                {
                    Status = "error",
                    Message = UtilityLanguage.GetString("AlertAndMessage_EntityNotFound")
                });
            }
            else
            {
                upResult = await domainRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);
                if (upResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Restore,
                        CollectionType = CollectionType.Domain,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = domain.ReturnValue.Id
                    };
                    DataLayer.Models.Shared.Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},restoring domain with {id} id and {domain.ReturnValue.DomainName} done successfully");
                    upResult.Message = ConstMessages.SuccessfullyDone;
                    result = new(new
                    {
                        Status = "success",
                        Message = UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully")
                    });
                }
                else
                {
                    upResult.Message = ConstMessages.ErrorInSaving;
                    result = new(new
                    {
                        Status = "error",
                        Message = UtilityLanguage.GetString("AlertAndMessage_TryLater")
                    });
                }
            }
        }
        catch (Exception e)
        {
            upResult.Message = ConstMessages.ExceptionOccured;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(DomainController)}/{nameof(Restore)}");
            result = new(new
            {
                Status = "error",
                Message = UtilityLanguage.GetString("AlertAndMessage_TryLater")
            });
        }
        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Edit([FromBody] DomainModel dto, CancellationToken cancellationToken)
    {
        Result opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        Domain domain = controllerHelper.GetCurrentUserDomain();
        try
        {
            if (!ModelState.IsValid)
            {
                List<AjaxValidationErrorModel> errors = [];
                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry modelStateVal = ModelState[modelStateKey];
                    if (modelStateVal != null)
                    {
                        errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel
                        {
                            Key = modelStateKey,
                            ErrorMessage = error.ErrorMessage
                        }));
                    }
                }
                return Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                if (!userDb.IsSystemAccount)
                {
                    List<Price>? prices = mapper.Map<List<Price>>(dto.Prices);

                    if (dto.DomainName != domain.DomainName || dto.OwnerUserId != domain.OwnerUserId || dto.OwnerUserName != domain.OwnerUserName)
                    {
                        logger.Information($"user which is not SuperAdmin wants to change some essential data stack trace: {nameof(DomainController)}/{nameof(Edit)}");

                        return Json(new { Status = "error", ConstMessages.ExceptionOccured });
                    }
                }

                if (string.IsNullOrEmpty(dto.LogoImage.ImageId))
                {
                    dto.LogoImage.ImageId = Guid.NewGuid().ToString();
                }

                if (string.IsNullOrEmpty(dto.FavicoImage.ImageId))
                {
                    dto.FavicoImage.ImageId = Guid.NewGuid().ToString();
                }
                dto.LogoImage.FileName = "RandomContentImage.png";
                dto.FavicoImage.FileName = "RandomFavicon.ico";

                opResult = await controllerHelper.EditDomain(dto, cancellationToken);
                if (opResult.Succeeded)
                {

                    #region UploadImages
                    CultureInfo current = new("en-US")
                                          {
                                              DateTimeFormat = new()
                                                               {
                                                                   Calendar = new GregorianCalendar()
                                                               }
                                          };
                    Thread.CurrentThread.CurrentCulture = current;

                    MemoryStream msLogo = new();
                    MemoryStream msFavicon = new();

                    bool bucketResult = await minioHelper.MakeBucket("domainimage");

                    byte[] logoBytes = Convert.FromBase64String(dto.LogoImage.Content.Replace("data:image/png;base64,", ""));
                    byte[] faviconBytes = Convert.FromBase64String(dto.FavicoImage.Content.Replace("data:image/x-icon;base64,", ""));

                    
                    Image logoImage = Image.Load(logoBytes);
                    await logoImage.SaveAsPngAsync(msLogo, cancellationToken);
                    msLogo.Seek(0, SeekOrigin.Begin);

                    
                    using (MemoryStream icoStream = new(faviconBytes))
                    {
                        using (Icon icon = new(icoStream))
                        {
                            Bitmap bitmap = icon.ToBitmap();
                            bitmap.Save(msFavicon, ImageFormat.Png); 
                        }
                    }
                    msFavicon.Seek(0, SeekOrigin.Begin);

 


                    string logoObjectName = $"{domain.Id}/{dto.LogoImage.ImageId}/{dto.LogoImage.FileName.Replace(':', '-')}";
                    string faviconObjectName = $"{domain.Id}/{dto.FavicoImage.ImageId}/{dto.FavicoImage.FileName.Replace(':', '-')}";

                    await minioHelper.Upload("domainimage", logoObjectName, msLogo, "image/png", msLogo.Length);
                    await minioHelper.Upload("domainimage", faviconObjectName, msFavicon, "image/png", msFavicon.Length); // Save as PNG
                    #endregion

                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Update,
                        CollectionType = CollectionType.Domain,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = dto.Id
                    };
                    await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, editing domain with {dto.Id} id and {dto.DomainName} done successfully");
                    opResult.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    opResult.Message = ConstMessages.ErrorInSaving;
                }
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occurred. stack trace: {nameof(DomainController)}/{nameof(Edit)}");
            opResult.Succeeded = false;
            opResult.Message = ConstMessages.ExceptionOccured;
        }

        JsonResult result = Json(opResult.Succeeded ? new { Status = "Success", opResult.Message } : new { Status = "Error", opResult.Message });
        return result;
    }
    [HttpGet]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        DataLayer.Models.Shared.Result<Domain> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        DataLayer.Models.Shared.Result<Domain> domain = controllerHelper.FetchDomain(id);
        if (domain.ReturnValue == null)
        {
            opResult.Message = ConstMessages.ObjectNotFound;
        }
        else
        {
            try
            {
                opResult = await domainRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, true, cancellationToken);
                if (opResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Delete,
                        CollectionType = CollectionType.Domain,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = domain.ReturnValue.Id
                    };
                    DataLayer.Models.Shared.Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},deleting domain with {domain.ReturnValue.Id} id and {domain.ReturnValue.DomainName} done successfully");
                    opResult.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    opResult.Message = ConstMessages.ErrorInSaving;
                }
            }
            catch (Exception e)
            {
                opResult.Message = ConstMessages.ExceptionOccured;
                logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(DomainController)}/{nameof(Delete)}");
            }
        }
        return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }

    public IActionResult GetRelatedColsTemplateWidths(int count)
    {
        JsonResult result;
        List<SelectListModel> res = [];
        switch (count)
        {
            case 1:
                res = [];

                foreach (int i in Enum.GetValues(typeof(OneColsTemplateWidth)))
                {
                    string name = Enum.GetName(typeof(OneColsTemplateWidth), i);
                    SelectListModel obj = new()
                    {
                        Text = name,
                        Value = i.ToString()
                    };
                    res.Add(obj);
                }
                res.Insert(0, new()
                {
                    Text = UtilityLanguage.GetString("Choose"),
                    Value = "-1"
                });
                break;
            case 2:
                res = [];
                res.AddRange(from int i in Enum.GetValues(typeof(TwoColsTemplateWidth)) let name = Enum.GetName(typeof(TwoColsTemplateWidth), i) select new SelectListModel() { Text = name, Value = i.ToString() });

                res.Insert(0, new()
                {
                    Text = UtilityLanguage.GetString("Choose"),
                    Value = "-1"
                });
                break;
            case 3:
                res = [];
                res.AddRange(from int i in Enum.GetValues(typeof(ThreeColsTemplateWidth)) let name = Enum.GetName(typeof(ThreeColsTemplateWidth), i) select new SelectListModel() { Text = name, Value = i.ToString() });

                res.Insert(0, new()
                {
                    Text = UtilityLanguage.GetString("Choose"),
                    Value = "-1"
                });
                break;
            case 4:
                res = [];
                res.AddRange(from int i in Enum.GetValues(typeof(FourColsTemplateWidth)) let name = Enum.GetName(typeof(FourColsTemplateWidth), i) select new SelectListModel() { Text = name, Value = i.ToString() });

                res.Insert(0, new()
                {
                    Text = UtilityLanguage.GetString("Choose"),
                    Value = "-1"
                });
                break;
            case 5:
                res = [];
                res.AddRange(from int i in Enum.GetValues(typeof(FiveColsTemplateWidth)) let name = Enum.GetName(typeof(FiveColsTemplateWidth), i) select new SelectListModel() { Text = name, Value = i.ToString() });

                res.Insert(0, new()
                {
                    Text = UtilityLanguage.GetString("Choose"),
                    Value = "-1"
                });
                break;
            case 6:
                res = [];
                res.AddRange(from int i in Enum.GetValues(typeof(SixColsTemplateWidth)) let name = Enum.GetName(typeof(SixColsTemplateWidth), i) select new SelectListModel() { Text = name, Value = i.ToString() });

                res.Insert(0, new()
                {
                    Text = UtilityLanguage.GetString("Choose"),
                    Value = "-1"
                });
                break;
            case 8:
                res = [];
                res.AddRange(from int i in Enum.GetValues(typeof(EightColsTemplateWidth)) let name = Enum.GetName(typeof(EightColsTemplateWidth), i) select new SelectListModel() { Text = name, Value = i.ToString() });

                res.Insert(0, new()
                              {
                                  Text = UtilityLanguage.GetString("Choose"),
                                  Value = "-1"
                              });
                break;
        }

        if (res.Any())
        {
            result = new(new { Status = "success", Data = res });
        }
        else
        {
            result = new(new { Status = "notFound", Message = "" });
        }

        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> SaveBackgroundImageToServer([FromBody] BgImage model, CancellationToken cancellationToken)
    {
        CultureInfo current = new("en-US")
        {
            DateTimeFormat = new()
            {
                Calendar = new GregorianCalendar()
            }
        };
        Thread.CurrentThread.CurrentCulture = current;
        MemoryStream ms = new();
        bool bucketResult = await minioHelper.MakeBucket("templatedesignbgimage");
        Domain domain = controllerHelper.GetCurrentUserDomain();

        if (bucketResult)
        {
            byte[] bytes;
            string imageFormat;
            string base64Prefix;

            if (model.Base64ImageContent.Contains("data:image/png;base64,"))
            {
                imageFormat = "image/png";
                base64Prefix = "data:image/png;base64,";
                bytes = Convert.FromBase64String(model.Base64ImageContent.Replace(base64Prefix, ""));
                model.ImageFileName = "RandomBgImage.png";
                Image image = Image.Load(bytes);
                await image.SaveAsPngAsync(ms, cancellationToken);
            }
            else
            {
                imageFormat = "image/jpeg";
                base64Prefix = "data:image/jpeg;base64,";
                bytes = Convert.FromBase64String(model.Base64ImageContent.Replace(base64Prefix, ""));
                model.ImageFileName = "RandomBgImage.jpg";
                Image image = Image.Load(bytes);
                await image.SaveAsJpegAsync(ms, cancellationToken);
            }

            ms.Seek(0, SeekOrigin.Begin);

            if (string.IsNullOrEmpty(model.ImageId))
            {
                model.ImageId = Guid.NewGuid().ToString();
            }
            string objectName = $"{domain.Id}/{model.ImageId}/{model.ImageFileName.Replace(':', '-')}";

            bool isSave = await minioHelper.Upload("templatedesignbgimage", objectName, ms, imageFormat, ms.Length);
            if (isSave)
            {
                logger.Information($"Image with {model.ImageId} ID in template design bgImage saved correctly.");
            }
            else
            {
                logger.Error($"Image not saved correctly. Stack trace: {nameof(DomainController)}/{nameof(SaveBackgroundImageToServer)}");
            }

            // Get the image data for returning to the client
            (bool success, byte[] imageData) = await minioHelper.GetObject("templatedesignbgimage", objectName);
            if (success)
            {
                model.Base64ImageContent = base64Prefix + Convert.ToBase64String(imageData);
            }
        }
        else
        {
            logger.Error($"Error in making bucket. Stack trace: {nameof(DomainController)}/{nameof(SaveBackgroundImageToServer)}");
        }

        JsonResult result = Json(new { status = "success", bgImage = model, selectedRowGuid = model.SelectedRowGuid, section = model.SelectedSection });
        return result;
    }

    [HttpGet]
    [Route("images/DomainDesign/{**slug}")]
    public IActionResult GetDomainDesignImages(string slug)
    {
        string path = $"/images/DomainDesign/{slug}";
        (byte[] fileContents, string mimeType) = ImageFunctions.GetImageWithActualSize(path, configuration["LocalStaticFileStorage"]);
        return File(fileContents, mimeType);
    }

    [HttpPost]
    public async ValueTask<IActionResult> GetRowWithSelectedColumns([FromBody] RowSelectedColumnsModel obj, CancellationToken cancellationToken)
    {
        List<SelectListModel> moduleList = (await moduleRepository
                                                .GetAllAsync(cancellationToken))
                                           .Select(m => new SelectListModel
                                           {
                                               Value = m.Id.ToString(),
                                               Text = m.ModuleName
                                           }).ToList();
        moduleList.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });
        ViewBag.ModuleList = moduleList;

        string imageTemplatePath = webHostEnvironment.WebRootPath;
        Domain domainEntity = controllerHelper.FetchDomain(obj.DomainId).ReturnValue;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        string viewName = obj.Count switch
        {
            1 => "_OneColumn.cshtml",
            2 => "_TwoColumns.cshtml",
            3 => "_ThreeColumns.cshtml",
            4 => "_FourColumns.cshtml",
            5 => "_FiveColumns.cshtml",
            6 => "_SixColumns.cshtml",
            8 => "_EightColumns.cshtml",
            _ => ""
        };

        List<SelectListModel> productOrContentList = [];
        productOrContentList.AddRange(from int i in Enum.GetValues(typeof(ProductOrContentType)) let name = Enum.GetName(typeof(ProductOrContentType), i) select new SelectListModel { Text = UtilityLanguage.GetString($"Enum_{name}"), Value = i.ToString() });

        ViewBag.ProductOrContentTypeList = productOrContentList;

        List<SelectImageListModel> productTemplateList = [];
        productTemplateList.AddRange(from int i in Enum.GetValues(typeof(ProductTemplateDesign)) select Enum.GetName(typeof(ProductTemplateDesign), i) into name select new SelectImageListModel { Text = name, Value = name, ImageUrl = $"template/{name}.jpg" });

        ViewBag.ProductTemplateList = productTemplateList;

        ViewBag.TransactionType = controllerHelper.GetAllTransactionType();

        ViewBag.LoadAnimationType = controllerHelper.GetAllLoadAnimationType();

        List<SelectImageListModel> contentTemplateDesignList = [];
        contentTemplateDesignList.AddRange(
            from int i in Enum.GetValues(typeof(ContentTemplateDesign))
            let name = Enum.GetName(typeof(ContentTemplateDesign), i)
            select new SelectImageListModel() { Text = name, Value = i.ToString() });

        foreach (SelectImageListModel item in contentTemplateDesignList)
        {
            // Construct the web URL path instead of using Path.Combine
            item.ImageUrl = item.Text.ToLower() != "forth"
                                ? $"/Template/Content/{item.Text}.jpg"
                                : (CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                                       ? "/Template/Content/Forth-rtl.jpg"
                                       : "/Template/Content/Forth-ltr.jpg");
        }
        ViewBag.ContentTemplateList = contentTemplateDesignList;

        List<SelectListModel> slmSelectionType = [];
        slmSelectionType.AddRange(from int i in Enum.GetValues(typeof(SelectionType)) let name = Enum.GetName(typeof(SelectionType), i) select new SelectListModel() { Text = UtilityLanguage.GetString($"Enum_{name}"), Value = i.ToString() });

        slmSelectionType.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });
        ViewBag.SelectionType = slmSelectionType;

        ViewBag.CategoryList = controllerHelper.AllActiveContentCategory(domainEntity.DefaultLanguageId, userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId);

        ViewBag.ContentList = controllerHelper.GetContentsList(obj.DomainId, "");

        List<SelectListModel> sliderList = (await sliderRepository.GetListAsync(s => s.IsActive && !s.IsDeleted && s.AssociatedDomainId == domainEntity.Id, cancellationToken))
                                           .Select(s => new SelectListModel
                                           {
                                               Text = s.Title,
                                               Value = s.Id.ToString()
                                           }).ToList();
        ViewBag.SliderList = sliderList;
        var sliderType = new List<SelectListModel>();

        foreach (int i in Enum.GetValues(typeof(SliderType)))
        {
            string name = Enum.GetName(typeof(SliderType), i) ?? $"Unknown ({i})";
            SelectListModel selectListModel = new() { Text = name, Value = i.ToString() };
            sliderType.Add(selectListModel);
        }
        sliderType.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });

        ViewBag.SliderType = sliderType;

        ViewBag.ColWidth = obj.ColumnWidth;

        ViewBag.RowNumber = obj.RowNumber;

        ViewBag.DomainId = obj.DomainId;

        ViewBag.Guid = obj.RowGuid;

        return PartialView($"~/Areas/Admin/Views/Domain/{viewName}", obj.RowData);
    }

}