using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        public IActionResult ContentImageUpload(IFormFile upload)
        {
            if (upload.Length <= 0) return null;
            // < 512 Kb
            if (upload.Length >= 524288) return null;

            var filename = Guid.NewGuid() + Path.GetExtension(upload.FileName).ToLower();
            //var path = Path.Combine(Directory.GetCurrentDirectory(),
            //   _env.WebRootPath, "CkEditor/Content");
            //if (!Directory.Exists(path))
            //{
            //    DirectoryInfo di = Directory.CreateDirectory(path);
            //}
            //$"/{fileName}"
            //path = Path.Combine(path, filename);
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
            //var url = "/CkEditor/Content/" + filename;
            var url = $"/ckEditorContentImages/{filename}";
            return Json(new { uploaded = true, url = url });
           // return new JsonResult(new { uploaded = 1, fileName = filename, url = url, error = "The file is too big." });
        }

        [HttpPost]
        public IActionResult ProductImageUpload(IFormFile upload)
        {
            if (upload.Length <= 0) return null;
            // < 512 Kb
            if (upload.Length >= 524288) return null;

            var filename = Guid.NewGuid() + Path.GetExtension(upload.FileName).ToLower();
            //var path = Path.Combine(Directory.GetCurrentDirectory(),
            //   _env.WebRootPath, "CkEditor/Product");
            //if (!Directory.Exists(path))
            //{
            //    DirectoryInfo di = Directory.CreateDirectory(path);
            //}
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
            //var url = "/CkEditor/Product/" + filename;
            var url = $"/ckEditorProductImages/{filename}";
            
            return Json(new { uploaded = true, url });
        }

        [HttpPost]
        public IActionResult DomainImageUpload(IFormFile upload)
        {
            if (upload.Length <= 0) return null;
            // < 512 Kb
            if (upload.Length >= 524288) return null;

            var filename = Guid.NewGuid() + Path.GetExtension(upload.FileName).ToLower();
            //var path = Path.Combine(Directory.GetCurrentDirectory(),
            //   _env.WebRootPath, "CkEditor/Domain");

            //if (!Directory.Exists(path))
            //{
            //    DirectoryInfo di = Directory.CreateDirectory(path);
            //}

            var directory = Path.Combine(_localStorage, "ckEditorDomainImages").Replace("\\", "/");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var path = Path.Combine(_localStorage, "ckEditorDomainImages", filename).Replace("\\", "/");
            using (var stream = new FileStream(path, FileMode.Create))
            {
                upload.CopyTo(stream);
            }

            var url = $"/ckEditorDomainImages/" + filename;
            // var url = $"{"/ckEditorContentImages/"}{filename}";
            return Json(new { uploaded = true, url = url });
            
        }

        [HttpGet]
        public IActionResult ContentFileBrows()
        {
            //var dir = new DirectoryInfo(Path.Combine(_localStorage, "ckEditorContentImages").Replace("\\", "/"));
            var dir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(),
               _env.WebRootPath, "CkEditor/Content"));

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
            //var dir = new DirectoryInfo(Path.Combine(_localStorage, "ckEditorProductImages").Replace("\\", "/"));
            var dir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(),
              _env.WebRootPath, "CkEditor/Product"));

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
        public IActionResult DomainFileBrows()
        {
            var dir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(),
              _env.WebRootPath, "CkEditor/Domain"));

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