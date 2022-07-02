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

        [AllowAnonymous]
        [HttpGet]
        [Route("ckEditorContentImages/{**slug}")]
        public IActionResult GetCkEditorContentImages(string slug)
        {
            var path = $"/ckEditorContentImages/{slug}";
            (byte[] fileContents, string mimeType) = GetImageWithActualSize(path);
            return File(fileContents, mimeType);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ckEditorProductImages/{**slug}")]
        public IActionResult GetCKEditorProductImages(string slug)
        {
            var path = $"/ckEditorProductImages/{slug}";
            (byte[] fileContents, string mimeType) = GetImageWithActualSize(path);
            return File(fileContents, mimeType);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ckEditorDomainImages/{**slug}")]
        public IActionResult GetCKEditorDomainImages(string slug)
        {
            var path = $"/ckEditorDomainImages/{slug}";
            (byte[] fileContents, string mimeType) = GetImageWithActualSize(path);
            return File(fileContents, mimeType);
        }

        private (byte[], string) GetImageWithActualSize(string path)
        {
            var localStaticFileStorage = _configuration["LocalStaticFileStorage"];
            string finalPath;
            if (!string.IsNullOrWhiteSpace(path))
            {
                if (path.StartsWith("/"))
                    path = path[1..];
                finalPath = Path.Combine(localStaticFileStorage, path).Replace("\\", "/");

                if (!System.IO.File.Exists(finalPath))
                {
                    finalPath = "/imgs/NoImage.png";
                }
                var fileName = Path.GetFileName(finalPath);
                var mimeType = ImageFunctions.GetMIMEType(fileName);
                byte[] fileContent = System.IO.File.ReadAllBytes(finalPath);
                return (fileContent, mimeType);
            }
            else
            {
                finalPath = "/imgs/NoImage.png";
                var fileName = Path.GetFileName(finalPath);
                var mimeType = ImageFunctions.GetMIMEType(fileName);
                byte[] fileContent = System.IO.File.ReadAllBytes(finalPath);
                return (fileContent, mimeType);
            }
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

        public IActionResult GetScaledImage(string path, int height)
        {
            var localStaticFileStorage = _configuration["LocalStaticFileStorage"];
            if (path.StartsWith("/"))
                path = path[1..];
            var finalPath = Path.Combine(localStaticFileStorage, path).Replace("\\", "/");
            if (!System.IO.File.Exists(finalPath))
            {
                finalPath = "/imgs/NoImage.png";
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
                finalPath = Path.Combine(localStaticFileStorage, "imgs/NoImage.png").Replace("\\", "/");
            }
            var fileName = Path.GetFileName(finalPath);
            var mimeType = ImageFunctions.GetMIMEType(fileName);
            byte[] fileContent = ImageFunctions.GetResizedImageBasedOnWidth(finalPath, width);
            return File(fileContent, mimeType);
        }
    }
}
