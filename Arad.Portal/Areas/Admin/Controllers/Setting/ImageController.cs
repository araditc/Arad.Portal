using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.General.User;

using Serilog;
using Arad.Portal.Helpers.Shared;

namespace Arad.Portal.Areas.Admin.Controllers.Setting;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class ImageController : Controller
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly ControllerHelper _controllerHelper;
    private readonly string _localStorage;
    public ImageController(IWebHostEnvironment env, IConfiguration configuration, ILogger logger, ControllerHelper controllerHelper)
    {
        _env = env;
        _config = configuration;
        _logger = logger;
        _controllerHelper = controllerHelper;
        _localStorage = _config["LocalStaticFileStorage"];
    }

    [HttpPost]
    public async ValueTask<IActionResult> ContentImageUpload(IFormFile upload, CancellationToken cancellationToken)
    {
        if (upload is not { Length: > 0 and < 524288 })
        {
            return Json(new { uploaded = false });
        }

        string url = "";
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            string filename = Guid.NewGuid() + Path.GetExtension(upload.FileName).ToLower();
            string directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ckEditorContentImages").Replace("\\", "/");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string path = Path.Combine(directory, filename).Replace("\\", "/");

            await using (FileStream stream = new(path, FileMode.Create))
            {
                await upload.CopyToAsync(stream, cancellationToken);
            }
            url = $"/ckEditorContentImages/{filename}";
        }
        catch (Exception e)
        {
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occurred. Stack trace: {nameof(ImageController)}/{nameof(ContentImageUpload)}");
            return Json(new { uploaded = false });
        }

        return Json(new { location = url });
    }

    [HttpPost]
    public IActionResult ProductImageUpload(IFormFile upload)
    {
        Task<ApplicationUser> userDb = _controllerHelper.GetCurrentUser();
        string url = "";
        try
        {
            switch (upload.Length)
            {
                case <= 0:
                case >= 524288:
                    return null;
            }

            string filename = Guid.NewGuid() + Path.GetExtension(upload.FileName).ToLower();
            string directory = Path.Combine(_localStorage, "ckEditorProductImages").Replace("\\", "/");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string path = Path.Combine(_localStorage, "ckEditorProductImages", filename).Replace("\\", "/");
            using (FileStream stream = new(path, FileMode.Create))
            {
                upload.CopyTo(stream);
            }
            url = $"/ckEditorProductImages/{filename}";
        }
        catch (Exception e)
        {
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.Result.UserName} error {e.Message} occured. stack trace: {nameof(ImageController)}/{nameof(ProductImageUpload)}");
        }


        return Json(new { uploaded = true, url });
    }

    [HttpPost]
    public IActionResult DomainImageUpload(IFormFile upload)
    {
        Task<ApplicationUser> userDb = _controllerHelper.GetCurrentUser();
        string url = "";
        try
        {
            if (upload.Length <= 0) return null;
            if (upload.Length >= 524288) return null;

            string filename = Guid.NewGuid() + Path.GetExtension(upload.FileName).ToLower();
            string directory = Path.Combine(_localStorage, "ckEditorDomainImages").Replace("\\", "/");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string path = Path.Combine(_localStorage, "ckEditorDomainImages", filename).Replace("\\", "/");
            using (FileStream stream = new(path, FileMode.Create))
            {
                upload.CopyTo(stream);
            }

            url = $"/ckEditorDomainImages/" + filename;
        }
        catch (Exception e)
        {
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.Result.UserName} error {e.Message} occured. stack trace: {nameof(ImageController)} / {nameof(DomainImageUpload)}");
        }

        return Json(new { uploaded = true, url });

    }

    [HttpGet]
    public IActionResult ContentFileBrows()
    {
        DirectoryInfo dir = new(Path.Combine(Directory.GetCurrentDirectory(),
                                             _env.WebRootPath, "CkEditor/Content"));

        List<string> multipleFilters =
        [
            "*.png",
            "*.jpeg",
            "*.jpg"
        ];
        ArrayList alFiles = [];
        // for each filter find mathing file names
        foreach (string fileFilter in multipleFilters)
        {
            // add found file names to array list
            alFiles.AddRange(dir.GetFiles(fileFilter));
        }

        ViewBag.fileInfos = alFiles;
        ViewBag.Location = _localStorage;
        return View();
    }

    [HttpGet]
    public IActionResult ProductFileBrows()
    {
        DirectoryInfo dir = new(Path.Combine(Directory.GetCurrentDirectory(),
                                             _env.WebRootPath, "CkEditor/Product"));

        List<string> multipleFilters =
        [
            "*.png",
            "*.jpeg",
            "*.jpg"
        ];
        ArrayList alFiles = [];
        // for each filter find mathing file names
        foreach (string fileFilter in multipleFilters)
        {
            // add found file names to array list
            alFiles.AddRange(dir.GetFiles(fileFilter));
        }

        ViewBag.fileInfos = alFiles;
        ViewBag.Location = _localStorage;
        return View();
    }

    [HttpGet]
    public IActionResult DomainFileBrows()
    {
        DirectoryInfo dir = new(Path.Combine(Directory.GetCurrentDirectory(),
                                             _env.WebRootPath, "CkEditor/Domain"));

        List<string> multipleFilters =
        [
            "*.png",
            "*.jpeg",
            "*.jpg"
        ];
        ArrayList alFiles = [];
        // for each filter find mathing file names
        foreach (string fileFilter in multipleFilters)
        {
            // add found file names to array list
            alFiles.AddRange(dir.GetFiles(fileFilter));
        }

        ViewBag.fileInfos = alFiles;
        ViewBag.Location = _localStorage;
        return View();
    }


}