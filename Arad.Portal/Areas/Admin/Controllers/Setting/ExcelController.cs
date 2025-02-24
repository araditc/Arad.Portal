using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.BasicData;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.Helpers.Admin;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared.Language;
using Arad.Portal.Models.Shared.Product;
using Arad.Portal.Models.Shared.Setting;

using AutoMapper;

using ClosedXML.Excel;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

using Serilog;

using SixLabors.ImageSharp;

using KeyVal = Arad.Portal.Models.Admin.KeyVal;

namespace Arad.Portal.Areas.Admin.Controllers.Setting;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class ExcelController(
    IConfiguration configuration,
    IWebHostEnvironment webHostEnvironment,
    ILanguageRepository languageRepository,
    MinioHelper minioHelper,
    IModificationRepository modificationRepository,
    ControllerHelper controllerHelper,
    ILogger logger,
    IMapper mapper) : Controller
{
    [HttpGet]
    public ActionResult GetTemplate()
    {
        ViewBag.LangList = controllerHelper.GetAllActiveLanguage();

        return View();
    }

    [HttpGet]
    public async ValueTask<FileResult> DownloadTemplate(string languageId, CancellationToken cancellationToken)
    {
        Language lanEntity = controllerHelper.FetchLanguage(languageId);
        string lanSymbol = lanEntity.Symbol;

        string filePath = Path.Combine(webHostEnvironment.WebRootPath, $"assets/{lanSymbol}_ProductImportTemplate.xlsx");

        // Calling the ReadAllBytes() function
        byte[] fileContent = await System.IO.File.ReadAllBytesAsync(filePath, cancellationToken);

        return File(fileContent,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Template.xlsx");
    }

    [HttpGet]
    public async ValueTask<IActionResult> ImportProductFromExcel()
    {
        ProductImportPage model = new();
        Language lan = controllerHelper.GetDefaultLanguage();
        List<SelectListModel> groupList = controllerHelper.GetAllActiveProductGroup(lan.Id);
        ViewBag.ProductGroupList = groupList;

        return await Task.FromResult<IActionResult>(View(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> ImportProductFromExcel([FromForm] ProductImportPage model, CancellationToken cancellationToken)
    {
        ProductImportPage res = new();
        Result result = new();
        List<ProductExcelImport> lst = [];

        if (model.ProductsExcelFile == null)
        {
            ViewBag.OperationResult = new OperationResult
            {
                Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("FileImportExport_NoFileSelected"),
                Succeeded = false
            };
            return View(res);
        }

        #region ImageSection
        IFormFile imageFormFile = model.ProductImages;
        string tempUnzipFolderPath = "";

        if (model.ProductImages != null)
        {
            string tempFolderPath = Path.Combine(configuration["LocalStaticFileStorage"], "Temp", imageFormFile.FileName).Replace("\\", "/");
            tempUnzipFolderPath = Path.Combine(configuration["LocalStaticFileStorage"], "Temp").Replace("\\", "/");

            // Create temp directory if it doesn't exist
            if (!Directory.Exists(tempUnzipFolderPath))
            {
                Directory.CreateDirectory(tempUnzipFolderPath);
            }

            // Save the uploaded image file to the temp folder
            await using (FileStream inputStream = new(tempFolderPath, FileMode.Create))
            {
                await imageFormFile.CopyToAsync(inputStream, cancellationToken);
            }

            // Unzip the image folder
            string extractedFolderPath = Path.Combine(tempUnzipFolderPath, Path.GetFileNameWithoutExtension(imageFormFile.FileName)).Replace("\\", "/");
            if (Directory.Exists(extractedFolderPath))
            {
                Directory.Delete(extractedFolderPath, true);
            }

            ZipFile.ExtractToDirectory(tempFolderPath, tempUnzipFolderPath);
            tempUnzipFolderPath = extractedFolderPath;
        }
        #endregion

        string[] extension = [".xls", ".xlsx"];
        string productImagePath = Path.Combine(configuration["LocalStaticFileStorage"], "images/Products").Replace("\\", "/");

        // Handle the Excel file
        IFormFile formFile = model.ProductsExcelFile;
        if (formFile is { Length: > 0 })
        {
            if (!extension.Any(e => e.Equals(Path.GetExtension(formFile.FileName))))
            {
                ViewBag.OperationResult = new OperationResult { Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("FileImportExport_InvalidContentType"), Succeeded = false };
                return View();
            }

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(formFile.FileName)}";
            string filePath = Path.Combine(configuration["LocalStaticFileStorage"], "Excel/Products", fileName);

            // Create directory if it doesn't exist
            string excelProductDirectory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(excelProductDirectory))
            {
                if (excelProductDirectory != null)
                {
                    Directory.CreateDirectory(excelProductDirectory);
                }
            }

            // Save the Excel file
            await using (FileStream inputStream = new(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(inputStream, cancellationToken);
            }

            // The file stream is closed at this point, so now open it with `XLWorkbook`
            using (XLWorkbook wb = new(filePath))
            {
                IXLWorksheet ws = wb.Worksheet("Sheet1");
                IXLRow titleRow = ws.FirstRowUsed();
                IXLRow productRow = titleRow.RowBelow();
                int rowNumber = 2;

                while (!productRow.IsEmpty())
                {
                    ProductExcelImport dto = new()
                    {
                        ProductName = productRow.Cell("A").GetString(),
                        ProductUnit = productRow.Cell("B").GetString(),
                        IsPublishOnMainDomain = productRow.Cell("C").GetString() == GeneralLibrary.Utilities.UtilityLanguage.GetString("btn_Confirm"),
                        ShowInLackOfInventory = productRow.Cell("D").GetString() == GeneralLibrary.Utilities.UtilityLanguage.GetString("btn_Confirm"),
                        UniqueCode = productRow.Cell("E").GetString(),
                        SeoTitle = productRow.Cell("F").GetString(),
                        SeoDescription = productRow.Cell("G").GetString(),
                        Price = productRow.Cell("H").GetValue<long>(),
                        TagKeywords = productRow.Cell("I").GetString()
                    };

                    // Image processing here
                    string sourceFilePath = Path.Combine(tempUnzipFolderPath, $"{rowNumber}.jpg").Replace("\\", "/");
                    if (System.IO.File.Exists(sourceFilePath))
                    {
                        string imageId = Guid.NewGuid().ToString();

                        // Ensure the product image directory exists
                        if (!Directory.Exists(productImagePath))
                        {
                            Directory.CreateDirectory(productImagePath);
                        }

                        // Copy the image file to the destination folder
                        string destinationFilePath = Path.Combine(productImagePath, $"{imageId}.jpg").Replace("\\", "/");
                        System.IO.File.Copy(sourceFilePath, destinationFilePath);

                        // Save image metadata
                        dto.ProductImage = new()
                        {
                            ImageId = imageId,
                            Url = destinationFilePath,
                            IsMain = true,
                            FileName = $"{imageId}.jpg",
                        };
                        MemoryStream ms = new();
                        bool bucketResult = await minioHelper.MakeBucket("productimage");

                        if (bucketResult)
                        {
                            SixLabors.ImageSharp.Image image = await SixLabors.ImageSharp.Image.LoadAsync(destinationFilePath, cancellationToken);
                            await image.SaveAsPngAsync(ms, cancellationToken);
                            ms.Seek(0, SeekOrigin.Begin);

                            Domain domain = controllerHelper.GetCurrentUserDomain();

                            string objectName = $"{domain.Id}/{dto.ProductImage.ImageId}/{dto.ProductImage.FileName.Replace(':', '-')}";
                            bool isSave = await minioHelper.Upload("productimage", objectName, ms, "image/png", ms.Length);
                            
                            if (isSave)
                            {
                                logger.Information($"image with {dto.ProductImage.ImageId} id saved correctly");
                            }
                            else
                            {
                                logger.Error($"image not save correctly. stack trace: {nameof(ExcelController)}/{nameof(ImportProductFromExcel)}");
                            }
                        }
                        else
                        {
                            logger.Error($"something is wrong in making bucket stack trace: {nameof(ExcelController)}/{nameof(ImportProductFromExcel)}");
                        }

                    }

                    lst.Add(dto);
                    productRow = productRow.RowBelow();
                    rowNumber++;
                }
            }


            // Perform the product import operation
            result = await controllerHelper.ImportFromExcel(lst, cancellationToken);
        }

        Language lan = controllerHelper.GetDefaultLanguage();
        List<SelectListModel> groupList = controllerHelper.GetAllActiveProductGroup(lan.Id);
        groupList.Insert(0, new() { Text = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_Choose"), Value = "" });
        ViewBag.ProductGroupList = groupList;

        ViewBag.OperationResult = new OperationResult
        {
            Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("FileImportExport_ResultMessage"),
            Succeeded = result.Succeeded
        };

        return View(model);
    }

    [HttpGet]
    public async ValueTask<IActionResult> AddExtraLanguage(CancellationToken cancellationToken)
    {
        LanguageDictionaryModel model = new();
        ApplicationUser user = await controllerHelper.GetCurrentUser(cancellationToken);

        if (!user.IsSystemAccount)
        {
            return NotFound();
        }

        List<SelectListModel> slm = (await languageRepository.GetListAsync(_ => true, cancellationToken)).Select(l => new SelectListModel { Text = l.LanguageName, Value = l.Id.ToString() }).ToList();
        ViewBag.Languages = slm;

        return View(model);

    }

    /// <summary>
    ///     this method can only Access by system account
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> AddExtraLanguage([FromForm] LanguageDictionaryModel model, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        LanguageDictionaryModel res = new();

        if (model.LanguageUploadFile == null)
        {
            ViewBag.OperationResult = new OperationResult { Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("FileImportExport_NoFileSelected"), Succeeded = false };

            return View(res);
        }

        try
        {
            IFormFile formFile = model.LanguageUploadFile;
            List<KeyVal> lst = [];
            string[] extension = [".xls", ".xlsx"];

            if (formFile is { Length: > 0 })
            {
                if (!extension.Any(e => e.Equals(Path.GetExtension(formFile.FileName))))
                {
                    ViewBag.OperationResult = new OperationResult { Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("FileImportExport_InvalidContentType"), Succeeded = false };

                    return View();
                }

                Language lanEntity = controllerHelper.FetchLanguage(model.LanguageId);
                mapper.Map<LanguageDto>(lanEntity);
                string lanSymbolName = lanEntity.Symbol.Contains("-") ? lanEntity.Symbol.Split("-")[0] : lanEntity.Symbol;
                string jsonFileName = Path.Combine(configuration["DictionaryFolderPath"] ?? string.Empty, $"Dictionaries/{lanSymbolName}.json");
                string excelFilePath = Path.Combine(configuration["LocalStaticFileStorage"] ?? string.Empty, "Temp", "LanguageTemp.xlsx");

                await using FileStream inputStream = new(excelFilePath, FileMode.Create);
                await formFile.CopyToAsync(inputStream, cancellationToken);
                byte[] array = new byte[inputStream.Length];
                inputStream.Seek(0, SeekOrigin.Begin);
                await inputStream.ReadAsync(array, 0, array.Length, cancellationToken);
                inputStream.Close();

                using (XLWorkbook wb = new(excelFilePath))
                {
                    IXLWorksheet ws = wb.Worksheet("Sheet1");
                    IXLRow titleRow = ws.FirstRowUsed();
                    IXLRow exelRow = titleRow.RowBelow();

                    while (!exelRow.IsEmpty())
                    {
                        KeyVal obj = new() { Key = exelRow.Cell("A").GetString(), Val = exelRow.Cell("B").GetString() };

                        lst.Add(obj);

                        exelRow = exelRow.RowBelow();
                    }
                }

                string json = JsonConvert.SerializeObject(lst);

                if (!System.IO.File.Exists(jsonFileName))
                {
                    FileStream fs = System.IO.File.Create(jsonFileName);
                    fs.Close();
                }

                await System.IO.File.WriteAllTextAsync(jsonFileName, json, cancellationToken);

                //delete uploaded excel file
                System.IO.File.Delete(excelFilePath);

                //make the current Language Active
                lanEntity.IsActive = true;
                Result result = new();

                Language availableEntity = await languageRepository.FirstOrDefaultAsync(l => l.Id == lanEntity.Id, cancellationToken);

                if (availableEntity != null)
                {
                    lanEntity.CreationDate = availableEntity.CreationDate;
                    lanEntity.CreatorUserId = availableEntity.CreatorUserId;
                    lanEntity.CreatorUserName = availableEntity.CreatorUserName;

                    Result<Language> updateResult = await languageRepository.UpdateAsync(lanEntity, cancellationToken);

                    if (updateResult.Succeeded)
                    {
                        Modification modification = new()
                                                    {
                                                        Id = Guid.NewGuid().ToString(),
                                                        ActionTypes = ActionTypes.Insert,
                                                        CollectionType = CollectionType.Excel,
                                                        Ip = controllerHelper.GetUserIpAddress(),
                                                        ModifierId = userDb.Id,
                                                        ModifierUserName = userDb.UserName,
                                                        ModifyDateTime = DateTime.Now,
                                                        RecordId = lanEntity.Id
                                                    };
                        Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                        result.Succeeded = true;
                        result.Message = ConstMessages.SuccessfullyDone;
                    }
                    else
                    {
                        result.Succeeded = false;
                        result.Message = ConstMessages.ErrorInSaving;
                    }
                }

                //add this culture to supported cultures
                List<BasicData> list = controllerHelper.GetBasicDataList("SupportedCultures", false, true, cancellationToken);
                string lastValue = list.Max(d => d.Value);

                string domainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;
                await controllerHelper.InsertNewRecord(new()
                                                       {
                                                           Id = Guid.NewGuid().ToString(),
                                                           GroupKey = "SupportedCultures",
                                                           Value = !string.IsNullOrWhiteSpace(lastValue) ? (Convert.ToInt32(lastValue) + 1).ToString() : "1",
                                                           Text = lanEntity.Symbol,
                                                           AssociatedDomainId = domainId,
                                                           Order = !string.IsNullOrWhiteSpace(lastValue) ? Convert.ToInt32(lastValue) + 1 : 1
                                                       },
                                                       cancellationToken);

                ViewBag.OperationResult = new OperationResult { Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("FileImportExport_ResultMessage"), Succeeded = result.Succeeded };
            }

            GeneralLibrary.Utilities.UtilityLanguage.ReloadDictionary();
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ExcelController)}/{nameof(AddExtraLanguage)}");
            ViewBag.OperationResult = new OperationResult { Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_ErrorOccurrence"), Succeeded = false };
        }

        List<SelectListModel> slm = (await languageRepository.GetListAsync(_ => true, cancellationToken)).Select(l => new SelectListModel { Text = l.LanguageName, Value = l.Id.ToString() }).ToList();
        ViewBag.Languages = slm;

        return View(model);
    }

    [HttpGet]
    public async ValueTask<FileResult> GetLanguageTemplate(CancellationToken cancellationToken)
    {
        string filePath = Path.Combine(configuration["DictionaryFolderPath"] ?? string.Empty, "Dictionaries/DictionaryTemplate.xlsx");

        // Calling the ReadAllBytes() function
        byte[] fileContent = await System.IO.File.ReadAllBytesAsync(filePath, cancellationToken);

        return File(fileContent,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Template.xlsx");
    }
}