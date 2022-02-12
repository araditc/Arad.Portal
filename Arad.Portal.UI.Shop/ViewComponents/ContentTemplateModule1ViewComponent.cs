using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class ContentTemplateModule1ViewComponent : ViewComponent
    {
        public ContentTemplateModule1ViewComponent()
        {
                
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
