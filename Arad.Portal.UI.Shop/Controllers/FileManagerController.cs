using Arad.Portal.UI.Shop.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Arad.Portal.DataLayer.Contracts.General.Domain;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class FileManagerController : BaseController
    {
        private readonly IConfiguration _configuration;
        public FileManagerController(IConfiguration configuration, 
            IHttpContextAccessor accessor, IDomainRepository  domRepository):base(accessor, domRepository)
        {
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("{language}/ckEditorContentImages/{**slug}")]
        public IActionResult GetCkEditorContentImages(string slug)
        {
            var path = $"/ckEditorContentImages/{slug}";
            (byte[] fileContents, string mimeType) = ImageFunctions.GetImageWithActualSize(path, _configuration["LocalStaticFileStorage"]);
            return File(fileContents, mimeType);
        }

        [HttpGet]
        [Route("{language}/images/DomainDesign/{**slug}")]
        public IActionResult GetDomainDesignImages(string slug)
        {
            var path = $"/images/DomainDesign/{slug}";
            (byte[] fileContents, string mimeType) = ImageFunctions.GetImageWithActualSize(path, _configuration["LocalStaticFileStorage"]);
            return File(fileContents, mimeType);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{language}/ckEditorProductImages/{**slug}")]
        public IActionResult GetCKEditorProductImages(string slug)
        {
            var path = $"/ckEditorProductImages/{slug}";
            (byte[] fileContents, string mimeType) = ImageFunctions.GetImageWithActualSize(path, _configuration["LocalStaticFileStorage"]);
            return File(fileContents, mimeType);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{language}/ckEditorDomainImages/{**slug}")]
        public IActionResult GetCKEditorDomainImages(string slug)
        {
            var path = $"/ckEditorDomainImages/{slug}";
            (byte[] fileContents, string mimeType) = ImageFunctions.GetImageWithActualSize(path, _configuration["LocalStaticFileStorage"]);
            return File(fileContents, mimeType);
        }

        [Route("GetImage")]
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
                    finalPath = "/imgs/NoImage.png";
                }
                var fileName = Path.GetFileName(finalPath);
                var mimeType = ImageFunctions.GetMIMEType(fileName);
                byte[] fileContent = System.IO.File.ReadAllBytes(finalPath);
                return File(fileContent, mimeType);
            }
            else
            {
                finalPath = "/imgs/NoImage.png";
                var fileName = Path.GetFileName(finalPath);
                var mimeType = ImageFunctions.GetMIMEType(fileName);
                byte[] fileContent = System.IO.File.ReadAllBytes(finalPath);
                return File(fileContent, mimeType);
            }
           
        }

        [Route("GetScaledImage")]
        public IActionResult GetScaledImage(string path, int height)
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
                finalPath = Path.Combine(localStaticFileStorage, "images/imgs/NoImage.png").Replace("\\", "/");
            }
            var fileName = Path.GetFileName(finalPath);
            var mimeType = ImageFunctions.GetMIMEType(fileName);
            byte[] fileContent = ImageFunctions.GetResizedImage(finalPath, height);
            return File(fileContent, mimeType);
        }


        [Route("GetScaledImageOnWidth")]
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
                finalPath = Path.Combine(localStaticFileStorage, "images/imgs/NoImage.png").Replace("\\", "/");
            }
            var fileName = Path.GetFileName(finalPath);
            var mimeType = ImageFunctions.GetMIMEType(fileName);
            byte[] fileContent = ImageFunctions.GetResizedImageBasedOnWidth(finalPath, width);
            return File(fileContent, mimeType);
        }

        
    }
}
