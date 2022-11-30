﻿using Arad.Portal.DataLayer.Contracts.General.Content;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using Arad.Portal.DataLayer.Entities.General.SliderModule;
using Arad.Portal.DataLayer.Models.Content;
using Arad.Portal.DataLayer.Models.DesignStructure;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
   
    public class ContentTemplatesViewComponent : ViewComponent
    {
        private readonly IContentRepository _contentRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILanguageRepository _lanRepository;
        public ContentTemplatesViewComponent(IContentRepository contentRepository,
            IHttpContextAccessor accessor, ILanguageRepository lanRepository, IWebHostEnvironment env)
        {
                _contentRepository = contentRepository;
                _accessor = accessor;
                _lanRepository = lanRepository;
                _environment = env;
        }
        //public IViewComponentResult Invoke(ProductOrContentType contentType, ContentTemplateDesign selectionTemplate, 
        //    int count, TransActionType loadAnimation, LoadAnimationType loadAnimationType)
        public IViewComponentResult Invoke(ModuleParameters moduleParameters)
        {
            var defaultCulture = _accessor.HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            List<ContentGlance> lst = new List<ContentGlance>();
            var defLangSymbol = defaultCulture.Split("|")[0][2..];
            CultureInfo currentCultureInfo = new(defLangSymbol, false);
            var langId = _lanRepository.FetchBySymbol(defLangSymbol);
            ViewBag.CurLangId = langId;

            ViewBag.LoadAnimation = moduleParameters.LoadAnimation;
            ViewBag.LoadAnimationType = moduleParameters.LoadAnimationType;
            
            lst = _contentRepository.GetSpecialContent(moduleParameters.Count.Value, 
                moduleParameters.ProductOrContentType.Value, moduleParameters.SelectionType.Value, 
                moduleParameters.CatId, moduleParameters.SelectedIds, _environment.IsDevelopment(), moduleParameters.DomainId);
            switch (moduleParameters.ContentTemplateDesign)
            {
                case ContentTemplateDesign.First:
                    foreach (var item in lst)
                    {
                       if(item.Images.Any(_=>_.ImageRatio == ImageRatio.Square))
                        {
                            item.DesiredImageUrl = item.Images
                                .FirstOrDefault(_ => _.ImageRatio == ImageRatio.Square).Url;
                        }
                        else
                        {
                            item.DesiredImageUrl = "/imgs/NoImage.png";
                        }
                    }
                    return View("First", lst);
                case ContentTemplateDesign.Second:
                    if (lst.Count() == 4)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (i == 0 || i == 3)
                            {
                                if (lst[i].Images.Any(_ => _.ImageRatio == ImageRatio.TwoToOne))
                                {
                                    lst[i].DesiredImageUrl = lst[i].Images
                                        .FirstOrDefault(_ => _.ImageRatio == ImageRatio.TwoToOne).Url;
                                }
                                else
                                {
                                    lst[i].DesiredImageUrl = "/imgs/NoImage21.jpg";
                                }
                            }
                            else
                            {
                                if (lst[i].Images.Any(_ => _.ImageRatio == ImageRatio.FourToOne))
                                {
                                    lst[i].DesiredImageUrl = lst[i].Images
                                        .FirstOrDefault(_ => _.ImageRatio == ImageRatio.FourToOne).Url;
                                }
                                else
                                {
                                    lst[i].DesiredImageUrl = "/imgs/NoImage41.jpg";
                                }
                            }
                        }
                    }
                    return View("Second", lst);
                   
                case ContentTemplateDesign.Third:
                    if (lst.Count() == 8)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            if (i < 4)
                            {
                                if (lst[i].Images.Any(_ => _.ImageRatio == ImageRatio.TwoToOne))
                                {
                                    lst[i].DesiredImageUrl = lst[i].Images
                                        .FirstOrDefault(_ => _.ImageRatio == ImageRatio.TwoToOne).Url;
                                }
                                else
                                {
                                    lst[i].DesiredImageUrl = "/imgs/NoImage21.jpg";
                                }
                            }
                            else
                            {
                                if (lst[i].Images.Any(_ => _.ImageRatio == ImageRatio.Square))
                                {
                                    lst[i].DesiredImageUrl = lst[i].Images
                                        .FirstOrDefault(_ => _.ImageRatio == ImageRatio.Square).Url;
                                }
                                else
                                {
                                    lst[i].DesiredImageUrl = "/imgs/NoImage.png";
                                }
                            }
                        }
                    }
                    return View("Third", lst);
                case ContentTemplateDesign.Forth:
                    foreach (var item in lst)
                    {
                        if (item.Images.Any(_ => _.ImageRatio == ImageRatio.Square))
                        {
                            item.DesiredImageUrl = item.Images
                                .FirstOrDefault(_ => _.ImageRatio == ImageRatio.Square).Url;
                        }
                        else
                        {
                            item.DesiredImageUrl = "/imgs/NoImage.png";
                        }
                    }
                    return View("Forth", lst);
                case ContentTemplateDesign.Fifth:
                    return View("Fifth", lst);
                case ContentTemplateDesign.SliderWithSubtitle:
                    foreach (var item in lst)
                    {
                        if (item.Images.Any(_ => _.ImageRatio == ImageRatio.Square))
                        {
                            item.DesiredImageUrl = item.Images
                                .FirstOrDefault(_ => _.ImageRatio == ImageRatio.Square).Url;
                        }
                        else
                        {
                            item.DesiredImageUrl = "/imgs/NoImage.png";
                        }
                    }
                    return View("SliderWithSubtitle", lst);
                default:
                    return View(lst);
            }
            
        }
    }
}
