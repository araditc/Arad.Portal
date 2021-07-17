using Arad.Portal.DataLayer.Models.Permission;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
    public class InnerPermissions : ViewComponent
    {
        public IViewComponentResult Invoke(ListPermissions model)
        {
            return View(model);
        }
    }
}
