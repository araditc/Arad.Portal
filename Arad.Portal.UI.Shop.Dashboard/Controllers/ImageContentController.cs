using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Imageflow.Fluent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.FileProviders;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class ImageContentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}