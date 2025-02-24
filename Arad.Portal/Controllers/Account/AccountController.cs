using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using System.Threading;

using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Models.Shared.Product;
using Arad.Portal.Models.Shared.User;
using Arad.Portal.Helpers.Shared;

using Language = Arad.Portal.DataLayer.Entities.General.Language.Language;

namespace Arad.Portal.Controllers.Account;

[Authorize(Policy = "Role")]

public class AccountController(
    IMapper mapper,
    ControllerHelper controllerHelper,
    MinioHelper minioHelper)
    : Controller
{

    [HttpGet]
    public IActionResult AccessDenied(string returnUrl)
    {
        return View();
    }

    [HttpGet]
    public IActionResult PageOrItemNotFound()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult UnAuthorize()
    {
        return View();
    }

    [HttpGet]
    public async ValueTask<IActionResult> Favorites(string type, string keyword = "")
    {
        List<UserFavoritesDto> list = [];
        Domain domainRes = controllerHelper.GetCurrentUserDomain();
        FavoriteType favType = type.ToLower() == "product" ? FavoriteType.Product : FavoriteType.Content;
        string userId = User.GetUserId();
        List<UserFavorites> lst = controllerHelper.GetUserFavoriteList(userId, favType);
        string lanId;
        {
            string lanSymbol = CultureInfo.CurrentCulture.Name;
            lanId = controllerHelper.FetchLanguageBySymbol(lanSymbol);
        }
        ViewBag.Type = type;
        if (lst != null)
        {
            foreach (UserFavorites item in lst)
            {
                UserFavoritesDto obj = mapper.Map<UserFavoritesDto>(item);
                string bucketName = "";
                List<Image> images;

                if (type.ToLower() == "product")
                {
                    ProductOutputDto res = await controllerHelper.ProductFetch(item.EntityId);
                    images = res.Images;
                    bucketName = "productimage";
                    obj.Name = res.MultiLingualProperties.Any(p => p.LanguageId == lanId) ?
                                   res.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId)?.Name : res.MultiLingualProperties.FirstOrDefault()?.Name;
                }
                else
                {
                    DataLayer.Entities.General.Content.Content res = await controllerHelper.ContentFetch(item.EntityId);
                    images = res.Images;
                    bucketName = "contentimage";
                    obj.Name = res.Title;
                }
                if (obj.Name != null && !string.IsNullOrWhiteSpace(keyword) && !obj.Name.Contains(keyword))
                {
                    continue;
                }
                Image mainImage = images.FirstOrDefault(i => i.IsMain) ?? images.FirstOrDefault(i => i.ImageRatio == ImageRatio.Square);

                if (mainImage == null)
                {
                    obj.ImagePath = "";
                    obj.NoImage = true;
                }
                if (mainImage != null)
                {
                    CultureInfo current = new("en-US")
                                          {
                                              DateTimeFormat = new()
                                                               {
                                                                   Calendar = new GregorianCalendar()
                                                               }
                                          };
                    Thread.CurrentThread.CurrentCulture = current;
                    mainImage = images.FirstOrDefault(i => i.IsMain);
                    string objectName = $"{domainRes.Id}/{mainImage.ImageId}/{mainImage.FileName.Replace(':', '-')}";
                    (bool success, byte[] imageData) = await minioHelper.GetObject(bucketName, objectName);
                    if (success)
                    {
                        mainImage.Content = Convert.ToBase64String(imageData);
                    }
                }
                obj.ImagePath = mainImage.Content;
                list.Add(obj);
            }
        }
        Language defLang = controllerHelper.GetDefaultLanguage();
        CultureInfo current2 = new(defLang.Symbol)
                               {
                                   DateTimeFormat = new()
                                                    {
                                                        Calendar = new GregorianCalendar()
                                                    }
                               };
        Thread.CurrentThread.CurrentCulture = current2;
        ViewBag.Type = type;
        return View(list);
    }
}