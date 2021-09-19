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

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class ImageContentController : Controller
    {
        private readonly IWebHostEnvironment _env;
        public ImageContentController(IWebHostEnvironment env)
        {
            _env = env;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("ImageContent/file-upload")]
        public IActionResult UploadImage(IFormFile upload)
        {
            if (upload.Length <= 0) return null;
            // < 512 Kb
            if (upload.Length >= 524288) return null;

            var filename = Guid.NewGuid() + Path.GetExtension(upload.FileName).ToLower();
            var directory = Path.Combine(_env.WebRootPath, "ckEditorContentImages");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var path = Path.Combine(_env.WebRootPath, "ckEditorContentImages", filename);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                upload.CopyTo(stream);
            }
            var url = $"{"/ckEditorContentImages/"}{filename}";
            return Json(new { uploaded = true, url });
        }
    }
}