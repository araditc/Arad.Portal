using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class ContentTemplateModule3ViewComponent : ViewComponent
    {
        public ContentTemplateModule3ViewComponent()
        {
                
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
