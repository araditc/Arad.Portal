using Arad.Portal.DataLayer.Contracts.General.Content;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using Arad.Portal.DataLayer.Models.Content;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
   
    public class ContentTemplatesViewComponent : ViewComponent
    {
        private readonly IContentRepository _contentRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILanguageRepository _lanRepository;
        public ContentTemplatesViewComponent(IContentRepository contentRepository,
            IHttpContextAccessor accessor, ILanguageRepository lanRepository)
        {
                _contentRepository = contentRepository;
                _accessor = accessor;
               _lanRepository = lanRepository;
        }
        public IViewComponentResult Invoke(ProductOrContentType contentType, ContentTemplateDesign selectionTemplate, int? count)
        {
            var defaultCulture = _accessor.HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            List<ContentGlance> lst = new List<ContentGlance>();
            var defLangSymbol = defaultCulture.Split("|")[0][2..];
            CultureInfo currentCultureInfo = new(defLangSymbol, false);
            var langId = _lanRepository.FetchBySymbol(defLangSymbol);
            ViewBag.CurLangId = langId;
            int cnt = 0;
            switch(selectionTemplate)
            {
                case ContentTemplateDesign.First:
                    cnt = 6;
                    break;
                case ContentTemplateDesign.Second:
                    cnt = 4;
                    break;
                case ContentTemplateDesign.Third:
                    cnt = 8;
                    break;
                case ContentTemplateDesign.Forth :
                case ContentTemplateDesign.Fifth :
                    cnt = count.Value;
                    break;
                
            }
            lst = _contentRepository.GetSpecialContent(selectionTemplate, cnt, langId, contentType);
            return selectionTemplate switch
            {
                ContentTemplateDesign.First => View("First", lst),
                ContentTemplateDesign.Second => View("Second", lst),
                ContentTemplateDesign.Third => View("Third", lst),
                ContentTemplateDesign.Forth => View("Forth", lst),
                ContentTemplateDesign.Fifth => View("Fifth", lst),
                _ => View(lst),
            };
        }
    }
}
