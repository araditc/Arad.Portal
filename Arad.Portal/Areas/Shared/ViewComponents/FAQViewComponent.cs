using System.Collections.Generic;

using Arad.Portal.DataLayer.Models.Shared.FAQ;

using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.Areas.Shared.ViewComponents
{
    public class FAQViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<FAQItem> faqItems)
        {
            FAQViewModel model = new() { FAQItems = faqItems };

            return View("~/Areas/Shared/Views/Shared/Components/FAQ/Default.cshtml", model);
        }
    }
}
