using Arad.Portal.UI.Shop.Dashboard.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class FileManagerController : Controller
    {
        private readonly IConfiguration _configuration;
        public FileManagerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index()

        {
            return View();
        }

        public IActionResult GetImage(string path)
        {
            var localStaticFileStorage = _configuration["LocalStaticFileStorage"];
            var finalPath = Path.Combine(localStaticFileStorage, path);
           
            if(!System.IO.File.Exists(finalPath))
            {
                finalPath =  "/imgs/NoImage.png";
            }
            var fileName = Path.GetFileName(finalPath);
            var mimeType = ImageFunctions.GetMIMEType(fileName);
            byte[] fileContent = System.IO.File.ReadAllBytes(finalPath);
            return File(fileContent, mimeType);
        }
    }
}
