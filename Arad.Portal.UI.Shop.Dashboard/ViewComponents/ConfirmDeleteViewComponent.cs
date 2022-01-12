using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
    public class ConfirmDeleteViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ConfirmDeleteDTO result) => View(result);
    }
}
