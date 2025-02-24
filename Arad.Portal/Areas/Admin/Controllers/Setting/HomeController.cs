using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;

using Humanizer;
using Microsoft.AspNetCore.Hosting;

using Serilog;

namespace Arad.Portal.Areas.Admin.Controllers.Setting;

[Authorize(Policy = "Role")]
[Area("Admin")]

public class HomeController(ControllerHelper controllerHelper, MinioHelper minioHelper, IDomainRepository domainRepository, IWebHostEnvironment env) : Controller
{

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [AllowAnonymous]
    public async Task<IActionResult> GetFavicon(CancellationToken cancellationToken)
    {
        CultureInfo current = new("en-US")
                              {
                                  DateTimeFormat = new()
                                                   {
                                                       Calendar = new GregorianCalendar()
                                                   }
                              };
        Thread.CurrentThread.CurrentCulture = current;

        string domainName = controllerHelper.GetCurrentDomainName();
        Domain domain = await domainRepository.FirstOrDefaultAsync(c => c.DomainName == "https://" + domainName, cancellationToken);

        string objectName = "";
        if (!string.IsNullOrEmpty(domain.FavicoImage.ImageId))
        {
            objectName = $"{domain.Id}/{domain.FavicoImage.ImageId}/{domain.FavicoImage.FileName.Replace(':', '-')}";
        }
        string filePath = Path.Combine(env.WebRootPath, "imgs", "NoImage41.jpg");
        try
        {
            Log.Information($"objectName: {objectName}");
            (bool success, byte[] imageData) = await minioHelper.GetObject("domainimage", objectName);

            if (!success || imageData == null)
            {
                Log.Warning($"there is no favicon.ico objectName: {objectName}");
                return PhysicalFile(filePath, "image/jpg");
            }

            // Return the image data as a PNG file
            return File(imageData, "image/x-icon");
        }
        catch (Exception ex)
        {
            Log.Error($"exception with {ex.Message} occured. stack trace: {nameof(HomeController)}/{nameof(GetFavicon)}");
            return PhysicalFile(filePath, "image/jpg");
        }
    }

    [AllowAnonymous]
    public async Task<IActionResult> GetLogo(CancellationToken cancellationToken)
    {
        CultureInfo current = new("en-US")
                              {
                                  DateTimeFormat = new()
                                                   {
                                                       Calendar = new GregorianCalendar()
                                                   }
                              };
        Thread.CurrentThread.CurrentCulture = current;

        string domainName = controllerHelper.GetCurrentDomainName();
        Domain domain = await domainRepository.FirstOrDefaultAsync(c => c.DomainName == "https://" + domainName, cancellationToken);
        string objectName = "";
        if (!string.IsNullOrEmpty(domain.LogoImage.ImageId) && !string.IsNullOrEmpty(domain.LogoImage.FileName))
        {
            objectName = $"{domain.Id}/{domain.LogoImage.ImageId}/{domain.LogoImage.FileName.Replace(':', '-')}";
        }
        
        string filePath = Path.Combine(env.WebRootPath, "imgs", "NoImage41.jpg");
        try
        {
            (bool success, byte[] imageData) = await minioHelper.GetObject("domainimage", objectName);

            if (!success || imageData == null)
            {
                
                return PhysicalFile(filePath, "image/jpg");
            }

            return File(imageData, "image/png");
        }
        catch (Exception ex)
        {
            Log.Error($"exception with {ex.Message} occured. stack trace: {nameof(HomeController)}/{nameof(GetLogo)}");
            return PhysicalFile(filePath, "image/jpg");
        }
    }
}