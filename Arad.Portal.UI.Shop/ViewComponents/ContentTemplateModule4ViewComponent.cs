using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class ContentTemplateModule4ViewComponent : ViewComponent
    {
        public ContentTemplateModule4ViewComponent()
        {
                
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
