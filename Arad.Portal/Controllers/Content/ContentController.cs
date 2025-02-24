using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Content;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Arad.Portal.DataLayer.Models.Shared;

using System.Collections.Specialized;
using System.Web;
using System.Collections.Generic;
using System.Threading;
using AutoMapper;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Comment;
using Arad.Portal.DataLayer.Entities.General.Comment;
using Arad.Portal.DataLayer.Models.Shared.DesignStructure;
using Arad.Portal.Models.Shared.Content;
using Arad.Portal.Controllers.Base;
using Arad.Portal.DataLayer.Entities.General.ContentCategory;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.ContentCategory;
using Arad.Portal.Helpers.Shared;

using Microsoft.Extensions.Configuration;

namespace Arad.Portal.Controllers.Content;

public class ContentController(
    IContentRepository contentRepository,
    IContentCategoryRepository contentCategoryRepository,
    IDomainRepository domainRepository,
    ILanguageRepository lanRepository,
    UserManager<ApplicationUser> userManager,
    ICommentRepository commentRepository,
    IHttpContextAccessor accessor,
    MinioHelper minioHelper,
    Serilog.ILogger logger,
    ControllerHelper controllerHelper,
    IConfiguration configuration,
    IMapper mapper)
    : BaseController(accessor, domainRepository, lanRepository)
{

    [Route("{language?}/blog")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ApplicationUser userDb = new();
        if (User.Identity is { IsAuthenticated: true })
        {
            userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        }

        ViewData["DomainTitle"] = DomainTitle;
        ViewData["PageTitle"] = UtilityLanguage.GetString("design_Articles");
        Domain domainEntity = controllerHelper.FetchDomainByName(DomainName, false).ReturnValue;
        string lanId;
        {
            string lanSymbol = CultureInfo.CurrentCulture.Name;
            lanId = controllerHelper.FetchLanguageBySymbol(lanSymbol);
        }

        //var cookieName = CookieRequestCultureProvider.DefaultCookieName;
        PagedItems<ContentGlance> result = new();
        string queryString = Request.QueryString.ToString();
        NameValueCollection filter = HttpUtility.ParseQueryString(queryString);

        if (string.IsNullOrWhiteSpace(filter["page"]))
        {
            filter.Set("page", "1");
        }
        if (string.IsNullOrWhiteSpace(filter["pageSize"]))
        {
            filter.Set("pageSize", "5");
        }
        int page = Convert.ToInt32(filter["page"]);
        int pageSize = Convert.ToInt32(filter["pageSize"]);

        try
        {
            long totalCount = await contentRepository.GetCountAsync(c => c.AssociatedDomainId == domainEntity.Id && c.LanguageId == lanId && c.IsDeleted == false, cancellationToken);
            List<ContentGlance> contentList = (await contentRepository.GetListAsync(c => c.AssociatedDomainId == domainEntity.Id && c.LanguageId == lanId && c.IsDeleted == false, cancellationToken))
                                              .Select(c =>
                                                          new ContentGlance
                                                          {
                                                              TotalScore = c.TotalScore,
                                                              ScoredCount = c.ScoredCount,
                                                              VisitCount = c.VisitCount,
                                                              CategoryName = c.ContentCategoryName,
                                                              ContentCategoryId = c.ContentCategoryId,
                                                              Id = c.Id,
                                                              ContentProviderName = c.ContentProviderName,
                                                              Images = c.Images,
                                                              SubTitle = c.SubTitle,
                                                              TagKeywords = c.TagKeywords,
                                                              Title = c.Title,
                                                              UrlFriend = c.UrlFriend,
                                                              ContentCode = c.ContentCode
                                                          }).ToList();

            result.Items = contentList;
            result.CurrentPage = page;
            result.ItemsCount = totalCount;
            result.PageSize = pageSize;
            result.QueryString = queryString;

            foreach (ContentGlance item in result.Items)
            {
                foreach (Image image in item.Images)
                {
                    if (!image.IsMain)
                    {
                        continue;
                    }

                    CultureInfo current = new("en-US")
                    {
                        DateTimeFormat = new()
                        {
                            Calendar = new GregorianCalendar()
                        }
                    };
                    Thread.CurrentThread.CurrentCulture = current;
                    Domain domain = controllerHelper.GetCurrentUserDomain();
                    string objectName = $"{domain.Id}/{image.ImageId}/{image.FileName.Replace(':', '-')}";
                    (bool success, byte[] imageData) = await minioHelper.GetObject("contentimage", objectName);
                    if (success)
                    {
                        image.Content = Convert.ToBase64String(imageData);
                    }
                    item.DesiredImageUrl = image.Content;
                }
            }
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
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ContentController)}/{nameof(Index)}");
        }

        return View("Index", result);
    }

    [AllowAnonymous]
    [Route("{language?}/Articles")]
    public IActionResult Articles()
    {

        Result<Domain> result = controllerHelper.FetchDomainByName(DomainName, false);

        if (HttpContext.Request.Path.Value == null)
        {
            return View(new MainPageContentPart());
        }

        string lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
        string lanId = controllerHelper.FetchLanguageBySymbol(lanIcon);
        ViewData["DomainTitle"] = DomainTitle;
        ViewData["PageTitle"] = UtilityLanguage.GetString("design_Articles");

        if (result.Succeeded)
        {
            if (!result.ReturnValue.IsMultiLinguals) //single language
            {
                string lan = result.ReturnValue.DefaultLanguageId;
                DataLayer.Entities.General.Language.Language lanEntity = controllerHelper.FetchLanguage(lan);
                Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                                        CookieRequestCultureProvider.MakeCookieValue(new(lanEntity.Symbol))
                                        , new()
                                        {
                                            Expires = DateTimeOffset.Now.AddYears(1),
                                            Domain = result.ReturnValue.DomainName
                                        });
            }


            if (result.ReturnValue.BlogPageDesign.Any(c => c.LanguageId == lanId))
            {
                PageDesignContent m = result.ReturnValue.BlogPageDesign.FirstOrDefault(c => c.LanguageId == lanId);

                if (m != null)
                {
                    return View(m.MainPageContainerPart);
                }
            }
            else
            {
                return View(new MainPageContentPart());
            }
        }
        else
        {
            return View(new MainPageContentPart());
        }

        return View(new MainPageContentPart());
    }

    [Route("{language}/{category}/{slug}", Order = 100)]
    public async Task<IActionResult> Details(string language ,string slug, string category,  CancellationToken cancellationToken)
    {

        ViewData["DomainTitle"] = DomainTitle;
    
        string userId = HttpContext.User.Identity is { IsAuthenticated: true } ? User.GetUserId() : "";
        string lanIcon = "";
        if (accessor.HttpContext == null)
        {
            return Redirect($"~/{lanIcon}/ExceptionHandler/PageNotFound");
        }

        if (accessor.HttpContext.Request.Path.Value != null)
        {
            lanIcon = accessor.HttpContext.Request.Path.Value.Split("/")[1];
        }

        ContentDto contentDto = new();
        DataLayer.Entities.General.Content.Content contentEntity;
        controllerHelper.GetCurrentDomainName();
        Result<Domain> domain = controllerHelper.FetchDomainByName(DomainName, true);



        if (long.TryParse(slug, out long codeNumber))
        {
            contentEntity = await contentRepository.FirstOrDefaultAsync(c => c.ContentCode == codeNumber && !c.IsDeleted && c.AssociatedDomainId == domain.ReturnValue.Id, cancellationToken);
        }
        else
        {
            contentEntity = await contentRepository.FirstOrDefaultAsync(c => c.UrlFriend == $"/{category.ToLower()}/{slug.ToLower()}" && !c.IsDeleted && c.AssociatedDomainId == domain.ReturnValue.Id, cancellationToken);
        }

        if (contentEntity != null)
        {
            contentDto = mapper.Map<ContentDto>(contentEntity);
            ContentCategory contentCategory = await contentCategoryRepository.FirstOrDefaultAsync(c => c.Id == contentEntity.ContentCategoryId, cancellationToken);

            List<Comment> comments = await commentRepository.GetListAsync(c => c.ReferenceId == "c*" + contentEntity.Id && c.ReferenceType == ReferenceType.Content, cancellationToken);
            if (User.Identity is { IsAuthenticated: true })
            {
                ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
                contentDto.Comments = controllerHelper.CreateNestedTreeComment(comments, userDb.Id);
            }
        }

        if (contentEntity != null)
        {
            EntityRate r = DataLayer.Helpers.Utilities.ConvertPopularityRate(contentEntity.TotalScore, contentEntity.ScoredCount);
            contentDto.LikeRate = r.LikeRate;
            contentDto.DisikeRate = r.DisikeRate;
            contentDto.HalfLikeRate = r.HalfLikeRate;
        }


        foreach (Image item in contentDto.Images)
        {
            if (item.ImageTemplateTypes.Contains(Enums.ImageTemplateType.ContentSlider))
            {
                CultureInfo current = new("en-US")
                {
                    DateTimeFormat = new()
                    {
                        Calendar = new GregorianCalendar()
                    }
                };
                Thread.CurrentThread.CurrentCulture = current;

                string objectName = $"{domain.ReturnValue.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
                (bool success, byte[] imageData) = await minioHelper.GetObject("contentimage", objectName);
                if (success)
                {
                    item.Content = Convert.ToBase64String(imageData);
                }
            }
        }

        //if (contentDto.Images.Count > 0 && contentDto.IsSliderShowing)
        //{
        //    foreach (Image item in contentDto.Images)
        //    {
        //        if (item.ImageTemplateTypes.Contains(Enums.ImageTemplateType.ContentSlider)) ;
        //        {
        //            CultureInfo current = new("en-US")
        //            {
        //                DateTimeFormat = new()
        //                {
        //                    Calendar = new GregorianCalendar()
        //                }
        //            };
        //            Thread.CurrentThread.CurrentCulture = current;

        //            string objectName = $"{domain.ReturnValue.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
        //            (bool success, byte[] imageData) = await minioHelper.GetObject("contentimage", objectName);
        //            if (success)
        //            {
        //                item.Content = Convert.ToBase64String(imageData);
        //            }

        //        }
        //    }
        //}

        ViewData["Tags"] = string.Join(",", contentDto.TagKeywords);
        ViewData["SeoTitle"] = contentDto.SeoTitle;
        ViewData["SeoDesc"] = contentDto.SeoDescription;
        ViewBag.Slug = slug;
        ViewBag.Category = category;

        {
            Result updateVisitCount = new();
            DataLayer.Entities.General.Content.Content entity = await contentRepository.FirstOrDefaultAsync(c => c.Id == contentDto.Id, cancellationToken);
            if (entity != null)
            {
                entity.VisitCount += 1;
                Result<DataLayer.Entities.General.Content.Content> updateResult = await contentRepository.UpdateAsync(entity, cancellationToken);
                if (updateResult.Succeeded)
                {
                    updateVisitCount.Succeeded = true;
                    updateVisitCount.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    updateVisitCount.Message = ConstMessages.GeneralError;
                }
            }
            else
            {
                updateVisitCount.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
            }
            if (contentDto.IsSidebarContentsShowing)
            {
                List<ContentGlance> sidebars = controllerHelper.GetContentInCategory(DataLayer.Entities.General.DesignStructure.ProductOrContentType.Newest, contentDto.ContentCategoryId);
                sidebars.RemoveAll(c => c.Id == contentDto.Id);
                foreach (ContentGlance item in sidebars)
                {
                    CultureInfo current = new("en-US")
                    {
                        DateTimeFormat = new()
                        {
                            Calendar = new GregorianCalendar()
                        }
                    };
                    Thread.CurrentThread.CurrentCulture = current;
                    bool bucketResult = await minioHelper.MakeBucket("contentimage");

                    if (bucketResult)
                    {


                        if (item.Images.Any(i => i.ImageTemplateTypes.Contains(Enums.ImageTemplateType.ContentSideBar)))
                        {
                            Image mainImage = item.Images.FirstOrDefault(i => i.ImageTemplateTypes.Contains(Enums.ImageTemplateType.ContentSideBar));
                            string objectName = $"{domain.ReturnValue.Id}/{mainImage.ImageId}/{mainImage.FileName.Replace(':', '-')}";
                            (bool success, byte[] imageData) = await minioHelper.GetObject("contentimage", objectName);
                            item.DesiredImageUrl = success ? Convert.ToBase64String(imageData) : "";
                        }
                        else if (item.Images.Any(c => c.IsMain))
                        {
                            Image mainImage = item.Images.FirstOrDefault(c => c.IsMain);
                            string objectName = $"{domain.ReturnValue.Id}/{mainImage.ImageId}/{mainImage.FileName.Replace(':', '-')}";
                            (bool success, byte[] imageData) = await minioHelper.GetObject("contentimage", objectName);
                            item.DesiredImageUrl = success ? Convert.ToBase64String(imageData) : "";
                        }
                    }
                }
                DataLayer.Entities.General.Language.Language defLang = controllerHelper.GetDefaultLanguage();
                CultureInfo current2 = new(defLang.Symbol)
                {
                    DateTimeFormat = new()
                    {
                        Calendar = new GregorianCalendar()
                    }
                };
                Thread.CurrentThread.CurrentCulture = current2;
                ViewBag.Sidbars = sidebars;
            }
            if (HttpContext.User.Identity is { IsAuthenticated: true })
            {
                #region check cookiepart for loggedUser
                string userProductRateCookieName = $"{userId}_cc{contentDto.Id}";
                if (HttpContext.Request.Cookies[userProductRateCookieName] != null)
                {
                    ViewBag.HasRateBefore = true;
                    ViewBag.PreRate = HttpContext.Request.Cookies[userProductRateCookieName];
                }
                else
                {
                    ViewBag.HasRateBefore = false;
                }
                #endregion
                #region FavoriteList
                //List<UserFavorites> userFavoriteList = controllerHelper.GetUserFavoriteList(userId, FavoriteType.Content);
                //ViewBag.Like = userFavoriteList != null && userFavoriteList.Any(f => f.EntityId == contentDto.Id);
                #endregion
            }
            ViewData["PageTitle"] = contentDto.Title;
            ViewBag.FontFamily = configuration["SiteSettings:FontFamily"] ?? "Arial, sans-serif";
            ViewBag.LanIcon = lanIcon;
            return View(contentDto);
        }

    }
}