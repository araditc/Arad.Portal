using System;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.ContentCategory;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Entities.General.ContentCategory;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Content;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

using Arad.Portal.Models.UI;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Controllers.Base;
using Arad.Portal.DataLayer.Entities.General.Domain;

namespace Arad.Portal.Controllers.Content;


public class ContentCategoryController(
    IContentCategoryRepository categoryRepository,
    IDomainRepository domainRepository,
    IHttpContextAccessor accessor,
    IContentRepository contentRepository,
    MinioHelper minioHelper,
    ILanguageRepository languageRepository,
    ControllerHelper controllerHelper)
    : BaseController(accessor, domainRepository,languageRepository)
{

    [Route("{language}/category")]
    public async ValueTask<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["DomainTitle"] = DomainTitle;
        ViewData["PageTitle"] = GeneralLibrary.Utilities.UtilityLanguage.GetString("design_ArticleCategories");
        Domain? domain = await domainRepository.FirstOrDefaultAsync(c => c.IsDefault && c.IsActive && !c.IsDeleted, cancellationToken);
        List<ContentCategory>? categories = await categoryRepository.GetListAsync(c => c.IsActive && !c.IsDeleted && c.AssociatedDomainId == domain.Id, cancellationToken);
        return View(categories);
    }
    [Route("{language}/{slug}", Order = 100)]
    public async ValueTask<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        Domain domainEntity = controllerHelper.FetchDomainByName(DomainName, true).ReturnValue;
        CommonViewModel model = new();
        if (accessor.HttpContext == null)
        {
            return View(model);
        }

        if (string.IsNullOrEmpty(slug))
        {
            return View(model);
        }
        string path = accessor.HttpContext.Request.Path.ToString();
        string lanIcon = path.Split("/")[1].ToLower();
        ViewData["DomainTitle"] = DomainTitle;
        ViewData["PageTitle"] = slug.Replace("-", " ");
        string langId = controllerHelper.FetchLanguageBySymbol(lanIcon);
        ViewData["CurLangId"] = langId;

        string id = "";
        ContentCategory entity;
        if (long.TryParse(slug, out long codeNumber))
        {
            entity = await categoryRepository.FirstOrDefaultAsync(c => c.CategoryCode == codeNumber && !c.IsDeleted, cancellationToken);
        }
        else
        {
            var slug2 = slug.ToLower();
            entity = await categoryRepository.FirstOrDefaultAsync(c => c.CategoryNames.Any(a => a.UrlFriend == $"/{slug2}") && !c.IsDeleted, cancellationToken);
        }

        if (entity != null)
        {
            id = entity.Id;
        }
        if (!string.IsNullOrEmpty(id))
        {
            CategorySection catSection = new() { ContentCategoryId = id, CountToSkip = 0, CountToTake = 4, DefaultLanguageId = langId };

            IQueryable<string> lst = (await contentRepository.GetAllAsync(cancellationToken)).AsQueryable().Where(c => !c.IsDeleted && c.AssociatedDomainId == domainEntity.Id).GroupBy(c => c.ContentCategoryId).Select(a => a.Key);
            List<string> finalList = [];
            foreach (string catId in lst)
            {
                finalList.Add(catId);
                ContentCategory entity2 = await categoryRepository.FirstOrDefaultAsync(c => c.Id == catId, cancellationToken);

                if (entity2 == null)
                {
                    continue;
                }

                {
                    ContentCategory tmp = entity2;
                    while (tmp != null && !string.IsNullOrEmpty(tmp.ParentCategoryId))
                    {
                        finalList.Add(tmp.ParentCategoryId);
                        tmp = await categoryRepository.AnyAsync(c => c.Id == tmp.ParentCategoryId, cancellationToken) ?
                                  await categoryRepository.FirstOrDefaultAsync(c => c.Id == tmp.ParentCategoryId, cancellationToken) : null;
                    }
                }
            }
            catSection.CategoriesWithContent = finalList;

            CultureInfo current = new("en-US")
                                  {
                                      DateTimeFormat = new()
                                                       {
                                                           Calendar = new GregorianCalendar()
                                                       }
                                  };
            Thread.CurrentThread.CurrentCulture = current;
            controllerHelper.GetCurrentDomainName();
            Result<Domain> domain = controllerHelper.FetchDomainByName(DomainName, true);

            foreach (var item in entity.ContentPhotosSlider)
            {
                string objectName = $"{domain.ReturnValue.Id}/{item.ImageId}/{item.FileName.Replace(':', '-')}";
                (bool success, byte[] imageData) = await minioHelper.GetObject("contentcategoryimage", objectName);

                if (success)
                {
                    item.Content = Convert.ToBase64String(imageData);
                }

                catSection.Images = new List<Image>();
                catSection.Images.Add(item);
            }

            model.CategorySection = catSection;
           ContentsInCategorySection contentSection = new()
                                                       {
                                                           ContentCategoryId = id,
                                                           CountToSkip = 0,
                                                           CountToTake = 4,
                                                           DefaultLanguageId = langId
                                                       };
            model.ContentsInCategorySection = contentSection;
            ViewBag.Slug = slug;
            return View(model);
        }
        else
        {
            return Redirect($"~/{lanIcon}/ExceptionHandler/PageNotFound");
        }
    }

    [HttpPost]
    public IActionResult GetMyCategoryVc(CategorySection categorySection)
    {
        return ViewComponent("ContentCategorySection", categorySection);
    }

    public IActionResult GetContentsInCategoryVc(ContentsInCategorySection contentsInCategory)
    {
        return ViewComponent("ContentsInCategorySection", contentsInCategory);
    }
}