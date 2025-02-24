using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using System;
using Arad.Portal.Controllers.Base;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;

namespace Arad.Portal.Controllers.Setting;

public class FileManagerController : BaseController
{
    private readonly IConfiguration _configuration;
    public FileManagerController(IConfiguration configuration,
                                 IHttpContextAccessor accessor, ILanguageRepository languageRepository, IDomainRepository domRepository, ControllerHelper controllerHelper) : base(accessor, domRepository,languageRepository)
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
        string path = $"/ckEditorContentImages/{slug}";
        (byte[] fileContents, string mimeType) = ImageFunctions.GetImageWithActualSize(path, _configuration["LocalStaticFileStorage"]);
        return File(fileContents, mimeType);
    }

    [HttpGet]
    [Route("{language}/images/DomainDesign/{**slug}")]
    public IActionResult GetDomainDesignImages(string slug)
    {
        string path = $"/images/DomainDesign/{slug}";
        (byte[] fileContents, string mimeType) = ImageFunctions.GetImageWithActualSize(path, _configuration["LocalStaticFileStorage"]);
        return File(fileContents, mimeType);
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("{language}/ckEditorProductImages/{**slug}")]
    public IActionResult GetCkEditorProductImages(string slug)
    {
        string path = $"/ckEditorProductImages/{slug}";
        (byte[] fileContents, string mimeType) = ImageFunctions.GetImageWithActualSize(path, _configuration["LocalStaticFileStorage"]);
        return File(fileContents, mimeType);
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("{language}/ckEditorDomainImages/{**slug}")]
    public IActionResult GetCkEditorDomainImages(string slug)
    {
        string path = $"/ckEditorDomainImages/{slug}";
        (byte[] fileContents, string mimeType) = ImageFunctions.GetImageWithActualSize(path, _configuration["LocalStaticFileStorage"]);
        return File(fileContents, mimeType);
    }

    [Route("GetImage")]
    public IActionResult GetImage(string path)
    {

        string localStaticFileStorage = _configuration["LocalStaticFileStorage"];
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
            string fileName = Path.GetFileName(finalPath);
            string mimeType = ImageFunctions.GetMimeType(fileName);
            byte[] fileContent = System.IO.File.ReadAllBytes(finalPath);
            return File(fileContent, mimeType);
        }
        else
        {
            finalPath = "/imgs/NoImage.png";
            string fileName = Path.GetFileName(finalPath);
            string mimeType = ImageFunctions.GetMimeType(fileName);
            byte[] fileContent = System.IO.File.ReadAllBytes(finalPath);
            return File(fileContent, mimeType);
        }

    }

    [Route("GetScaledImage")]
    public IActionResult GetScaledImage(string path, int height)
    {
        string finalPath = "";
        string localStaticFileStorage = _configuration["LocalStaticFileStorage"];
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
        string fileName = Path.GetFileName(finalPath);
        string mimeType = ImageFunctions.GetMimeType(fileName);
        byte[] fileContent = ImageFunctions.GetResizedImage(finalPath, height);
        return File(fileContent, mimeType);
    }


    [Route("GetScaledImageOnWidth")]
    public IActionResult GetScaledImageOnWidth(string path, int width)
    {
        try
        {
            string finalPath = "";
            string localStaticFileStorage = _configuration["LocalStaticFileStorage"];
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
            string fileName = Path.GetFileName(finalPath);
            string mimeType = ImageFunctions.GetMimeType(fileName);
            byte[] fileContent = ImageFunctions.GetResizedImageBasedOnWidth(finalPath, width);
            if (fileContent == null)
            {
                return null;
            }
            return File(fileContent, mimeType);
        }
        catch (Exception ex)
        {

            throw;
        }

    }


}