using Arad.Portal.UI.Shop.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class FileManagerController : BaseController
    {
        private readonly IConfiguration _configuration;
        public FileManagerController(IConfiguration configuration, IHttpContextAccessor accessor):base(accessor)
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
            string finalPath;
            if(!string.IsNullOrWhiteSpace(path))
            {
                if (path.StartsWith("/"))
                    path = path[1..];
                finalPath = Path.Combine(localStaticFileStorage, path).Replace("\\", "/");

                if (!System.IO.File.Exists(finalPath))
                {
                    finalPath = Path.Combine(localStaticFileStorage, "images/imgs/NoImage.png").Replace("\\", "/");
                }
                var fileName = Path.GetFileName(finalPath);
                var mimeType = ImageFunctions.GetMIMEType(fileName);
                byte[] fileContent = System.IO.File.ReadAllBytes(finalPath);
                return File(fileContent, mimeType);
            }
            else
            {
                finalPath = Path.Combine(localStaticFileStorage, "images/imgs/NoImage.png").Replace("\\", "/");
                var fileName = Path.GetFileName(finalPath);
                var mimeType = ImageFunctions.GetMIMEType(fileName);
                byte[] fileContent = System.IO.File.ReadAllBytes(finalPath);
                return File(fileContent, mimeType);
            }
           
        }

        public IActionResult GetScaledImage(string path, int height)
        {
            string finalPath = "";
            var localStaticFileStorage = _configuration["LocalStaticFileStorage"];
            if(!string.IsNullOrWhiteSpace(path))
            {
                if (path.StartsWith("/"))
                    path = path[1..];
                finalPath = Path.Combine(localStaticFileStorage, path).Replace("\\", "/");
            }
           
            if (string.IsNullOrWhiteSpace(finalPath) || !System.IO.File.Exists(finalPath))
            {
                finalPath = "/images/imgs/NoImage.png";
            }
            var fileName = Path.GetFileName(finalPath);
            var mimeType = ImageFunctions.GetMIMEType(fileName);
            byte[] fileContent = ImageFunctions.GetResizedImage(finalPath, height);
            return File(fileContent, mimeType);
        }


        public IActionResult GetScaledImageOnWidth(string path, int width)
        {
            string finalPath = "";
            var localStaticFileStorage = _configuration["LocalStaticFileStorage"];
            if (!string.IsNullOrWhiteSpace(path))
            {
                if (path.StartsWith("/"))
                    path = path[1..];
                finalPath = Path.Combine(localStaticFileStorage, path).Replace("\\", "/");
            }

            if (string.IsNullOrWhiteSpace(finalPath) || !System.IO.File.Exists(finalPath))
            {
                finalPath = "/images/imgs/NoImage.png";
            }
            var fileName = Path.GetFileName(finalPath);
            var mimeType = ImageFunctions.GetMIMEType(fileName);
            byte[] fileContent = ImageFunctions.GetResizedImageBasedOnWidth(finalPath, width);
            return File(fileContent, mimeType);
        }

        
    }
}
