using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
    public class AddUserComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("Default"); 

        }
    }
}

