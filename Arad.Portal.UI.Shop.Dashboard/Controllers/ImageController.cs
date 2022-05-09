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
using Microsoft.AspNetCore.Authorization;
using System.Collections;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class ImageController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly string _localStorage;
        public ImageController(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _config = configuration;
            _localStorage = _config["LocalStaticFileStorage"];
        }

        [HttpPost]
        [Route("Image/ContentImageUpload")]
        public IActionResult ContentUploadImage(IFormFile upload)
        {
            if (upload.Length <= 0) return null;
            // < 512 Kb
            if (upload.Length >= 524288) return null;

            var filename = Guid.NewGuid() + Path.GetExtension(upload.FileName).ToLower();
            var directory = Path.Combine(_localStorage, "ckEditorContentImages").Replace("\\", "/");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var path = Path.Combine(_localStorage, "ckEditorContentImages", filename).Replace("\\", "/");
            using (var stream = new FileStream(path, FileMode.Create))
            {
                upload.CopyTo(stream);
            }
            var url = $"{"/ckEditorContentImages/"}{filename}";
            return Json(new { uploaded = true, url });
        }

        [HttpPost]
        [Route("Image/ProductImageUpload")]
        public IActionResult ProductImageUpload(IFormFile upload)
        {
            if (upload.Length <= 0) return null;
            // < 512 Kb
            if (upload.Length >= 524288) return null;

            var filename = Guid.NewGuid() + Path.GetExtension(upload.FileName).ToLower();
            var directory = Path.Combine(_localStorage, "ckEditorProductImages").Replace("\\", "/");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var path = Path.Combine(_localStorage, "ckEditorProductImages", filename).Replace("\\", "/");
            using (var stream = new FileStream(path, FileMode.Create))
            {
                upload.CopyTo(stream);
            }
            var url = $"/ckEditorProductImages/{filename}";
            
            return Json(new { uploaded = true, url });
        }
       
        [HttpGet]
        public IActionResult ContentFileBrows()
        {
            var dir = new DirectoryInfo(Path.Combine(_localStorage, "ckEditorContentImages").Replace("\\", "/"));

            List<string> MultipleFilters = new List<string>()
            {
                "*.png",
                "*.jpeg",
                "*.jpg"
            };
            ArrayList alFiles = new ArrayList();
            // for each filter find mathing file names
            foreach (string FileFilter in MultipleFilters)
            {
                // add found file names to array list
                alFiles.AddRange(dir.GetFiles(FileFilter));
            }

            ViewBag.fileInfos = alFiles;
            ViewBag.Location = _localStorage;
            return View();
        }

        [HttpGet]
        public IActionResult ProductFileBrows()
        {
            var dir = new DirectoryInfo(Path.Combine(_localStorage, "ckEditorProductImages").Replace("\\", "/"));

            List<string> MultipleFilters = new List<string>()
            {
                "*.png",
                "*.jpeg",
                "*.jpg"
            };
            ArrayList alFiles = new ArrayList();
            // for each filter find mathing file names
            foreach (string FileFilter in MultipleFilters)
            {
                // add found file names to array list
                alFiles.AddRange(dir.GetFiles(FileFilter));
            }

            ViewBag.fileInfos = alFiles;
            ViewBag.Location = _localStorage;
            return View();
        }

        
    }
}