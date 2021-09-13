using Arad.Portal.DataLayer.Contracts.General.Permission;
using Arad.Portal.DataLayer.Contracts.General.User;
using Arad.Portal.DataLayer.Entities.General.Permission;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.General.Permission;
using Arad.Portal.DataLayer.Repositories.General.User.Mongo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
    public class InnerMenuViewComponent : ViewComponent
    {
        
        public  IViewComponentResult Invoke(MenuLinkModel model)
        {
            return  View(model);
        }
    }
}
