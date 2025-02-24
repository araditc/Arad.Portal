using System.Collections.Generic;

using Arad.Portal.DataLayer.Models.Shared.ContactUs;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Arad.Portal.Areas.Shared.ViewComponents
{
    public class ContactUsViewComponent(IConfiguration configuration) : ViewComponent
    {
        public IViewComponentResult Invoke(List<ContactUs> contactUsList)
        {
            ViewBag.FontFamily = configuration["SiteSettings:FontFamily"];
            return View("~/Areas/Shared/Views/Shared/Components/ContactUs/Default.cshtml", contactUsList);
        }
    }
}
