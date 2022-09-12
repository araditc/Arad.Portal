using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class GeneralSearchViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _accessor;
        public GeneralSearchViewComponent(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }
        public IViewComponentResult Invoke(string filter="")
        {
            DataLayer.Models.Shared.Filter obj = new DataLayer.Models.Shared.Filter();
            obj.Keyword = filter;
            string lanIcon;

            if (CultureInfo.CurrentCulture.Name != null)
            {
                lanIcon = CultureInfo.CurrentCulture.Name.ToLower();
            }
            else
            {
                lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1].ToLower();
            }
            ViewBag.LanIcon = lanIcon;
            return View(obj);
        }
    }
}
