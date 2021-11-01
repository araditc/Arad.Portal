using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Authorization
{
    public interface IPermissionView
    {
        Task<Dictionary<string, bool>> PermissionsViewGet(HttpContext HttpContext);
    }
}
