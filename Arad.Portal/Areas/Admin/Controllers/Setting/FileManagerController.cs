using Arad.Portal.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.General.Domain;
using DocumentFormat.OpenXml.EMMA;

namespace Arad.Portal.Areas.Admin.Controllers.Setting;

[Authorize(Policy = "Role")]
[Area("Admin")]
[Route("Admin/File")]
public class FileManagerController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, MinioHelper minioHelper, ControllerHelper controllerHelper) : Controller
{
    // Constructor

    [AllowAnonymous]
    [HttpGet("ckEditorContentImages/{**slug}")]
    public IActionResult GetCkEditorContentImages(string slug)
    {
        string path = $"/ckEditorContentImages/{slug}";
        (byte[] fileContents, string mimeType) = GetImageWithActualSize(path);
        return File(fileContents, mimeType);
    }

    [AllowAnonymous]
    [HttpGet("ckEditorProductImages/{**slug}")]
    public IActionResult GetCkEditorProductImages(string slug)
    {
        string path = $"/ckEditorProductImages/{slug}";
        (byte[] fileContents, string mimeType) = GetImageWithActualSize(path);
        return File(fileContents, mimeType);
    }

    [AllowAnonymous]
    [HttpGet("ckEditorDomainImages/{**slug}")]
    public IActionResult GetCkEditorDomainImages(string slug)
    {
        string path = $"/ckEditorDomainImages/{slug}";
        (byte[] fileContents, string mimeType) = GetImageWithActualSize(path);
        return File(fileContents, mimeType);
    }

    private (byte[], string) GetImageWithActualSize(string path)
    {
        string localStaticFileStorage = configuration["LocalStaticFileStorage"];
        string finalPath;

        if (!string.IsNullOrWhiteSpace(path))
        {
            if (path.StartsWith("/"))
            {
                path = path[1..];
            }

            finalPath = Path.Combine(localStaticFileStorage, path).Replace("\\", "/");

            if (!System.IO.File.Exists(finalPath))
            {
                finalPath = Path.Combine(webHostEnvironment.WebRootPath, "imgs/NoImage.png");
            }
        }
        else
        {
            finalPath = Path.Combine(webHostEnvironment.WebRootPath, "imgs/NoImage.png");
        }

        string fileName = Path.GetFileName(finalPath);
        string mimeType = ImageFunctions.GetMimeType(fileName);
        byte[] fileContent = System.IO.File.ReadAllBytes(finalPath);
        return (fileContent, mimeType);
    }

    [HttpPost("UploadImage")]
    public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string contentId)
    {
        CultureInfo current = new("en-US")
                              {
                                  DateTimeFormat = new()
                                                   {
                                                       Calendar = new GregorianCalendar()
                                                   }
                              };
        Thread.CurrentThread.CurrentCulture = current;

        Domain domain = controllerHelper.GetCurrentUserDomain();
        //foreach (var file in files)
        //{
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        if (string.IsNullOrWhiteSpace(contentId))
        {
            return BadRequest("Content ID is required.");
        }

        bool isBucketExist = await minioHelper.MakeBucket("texteditorimage");
        const string FILE_NAME = "RandomTextEditorImageName.png";
        string objectName = $"{domain.Id}/{contentId}/{FILE_NAME}";
        bool isUpload = await minioHelper.Upload(file, "texteditorimage", objectName );
        
        string fileUrl = "";

        if (!isUpload)
        {
            return Ok(new { location = fileUrl });
        }

        (bool, string) getObject = await minioHelper.GetObjectUrl2("texteditorimage", objectName);

        if (getObject.Item1)
        {
            fileUrl = getObject.Item2;
        }

        return Ok(new { location = fileUrl });
    }
}