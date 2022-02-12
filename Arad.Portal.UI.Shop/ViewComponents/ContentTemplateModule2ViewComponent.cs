using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class ContentTemplateModule2ViewComponent : ViewComponent
    {
        public ContentTemplateModule2ViewComponent()
        {
                
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
