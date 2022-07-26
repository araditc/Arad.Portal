using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
using Microsoft.AspNetCore.Http;
using System.IO;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Models.Product;
using Microsoft.Extensions.Configuration;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
using System.Security.Claims;
using Arad.Portal.DataLayer.Contracts.General.Language;
using System.IO.Compression;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class ExcelController : Controller
    {
        private readonly IConfiguration _configuration;
        private IWebHostEnvironment _Environment;
        private readonly IProductRepository _productRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly IProductGroupRepository _productGroupRepository;
        public ExcelController(IConfiguration configuration,
            IProductRepository productRepository,
            IWebHostEnvironment env,
            IProductGroupRepository groupRepository,
            ILanguageRepository languageRepository)
        {
            _configuration = configuration;
            _productRepository = productRepository;
            _productGroupRepository = groupRepository;
            _languageRepository = languageRepository;
            _Environment = env;
        }
      
        [HttpGet]
        public  ActionResult GetTemplate()
        {
            ViewBag.LangList = _languageRepository.GetAllActiveLanguage();
            return View();
        }

        [HttpGet]
        public async Task<FileResult> DownloadTemplate(string languageId)
        {
            var lanEntity = _languageRepository.FetchLanguage(languageId);
            var lanSymbol = lanEntity.Symbol;

            var filePath =  Path.Combine(_Environment.WebRootPath, $"assets/{lanSymbol}_ProductImportTemplate.xlsx");
            // Calling the ReadAllBytes() function
            byte[] fileContent = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileContent,
                 "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                 "Template.xlsx");
        }
        [HttpGet]
        public async Task<IActionResult> ImportProductFromExcel()
        {
            var model = new ProductImportPage();

            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var lan = _languageRepository.GetDefaultLanguage(currentUserId);
            var groupList = await _productGroupRepository.GetAlActiveProductGroup(lan.LanguageId, currentUserId);
            //groupList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.ProductGroupList = groupList;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportProductFromExcel([FromForm]ProductImportPage model)
        {
            var res = new ProductImportPage();
            var result = new DataLayer.Models.Shared.Result();
            var lst = new List<ProductExcelImport>();
            if (model.ProductsExcelFile == null)
            {
                ViewBag.OperationResult = new OperationResult 
                { Message = Language.GetString("FileImportExport_NoFileSelected"), 
                    Succeeded = false 
                };
                return View(res);
            }

            #region ImageSection
            var imageFormFile = model.ProductImages;
            string tempUnzipFolderPath = "";
            if (model.ProductImages != null)
            {
                var tempFolderPath = Path.Combine(_configuration["LocalStaticFileStorage"], "Temp", imageFormFile.FileName).Replace("\\","/");
                tempUnzipFolderPath = Path.Combine(_configuration["LocalStaticFileStorage"], "Temp").Replace("\\", "/");
                using (FileStream inputStream = new(tempFolderPath, FileMode.Create))
                {
                    await imageFormFile.CopyToAsync(inputStream);
                    byte[] array = new byte[inputStream.Length];
                    inputStream.Seek(0, SeekOrigin.Begin);
                    inputStream.Read(array, 0, array.Length);
                    inputStream.Close();
                }
                var extractedFolderPath = Path.Combine(tempUnzipFolderPath, Path.GetFileNameWithoutExtension(imageFormFile.FileName)).Replace("\\", "/");
                if (Directory.Exists(extractedFolderPath))
                {
                    Directory.Delete(extractedFolderPath, true);
                }
                ZipFile.ExtractToDirectory(tempFolderPath, tempUnzipFolderPath);
                tempUnzipFolderPath = extractedFolderPath;
            }

            #endregion 
            string[] extension = { ".xls", ".xlsx" };
            string filePath = "";
            string productImagePath = Path.Combine(_configuration["LocalStaticFileStorage"] , "images/Products").Replace("\\", "/");
            //foreach (IFormFile formFile in Request.Form.Files)
            //{
            var formFile = model.ProductsExcelFile;
            if (formFile is { Length: > 0 })
            {
                if (!extension.Any(e => e.Equals(Path.GetExtension(formFile.FileName))))
                {
                    ViewBag.OperationResult = new OperationResult { Message = Language.GetString("FileImportExport_InvalidContentType"), Succeeded = false };
                    return View();
                }

                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(formFile.FileName)}";
                filePath = Path.Combine(_configuration["LocalStaticFileStorage"], "Excel/Products", fileName);


                await using FileStream inputStream = new(filePath, FileMode.Create);
                await formFile.CopyToAsync(inputStream);
                byte[] array = new byte[inputStream.Length];
                inputStream.Seek(0, SeekOrigin.Begin);
                inputStream.Read(array, 0, array.Length);
                inputStream.Close();

                using (XLWorkbook wb = new(filePath))
                {
                    var ws = wb.Worksheet("Sheet1");
                    var titleRow = ws.FirstRowUsed();
                    //var usedColsInRow = titleRow.RowUsed();
                    var productRow = titleRow.RowBelow();
                    int rowNumber = 2;
                    while (!productRow.IsEmpty())
                    {
                        var dto = new ProductExcelImport();
                        
                        dto.ProductName = productRow.Cell("A").GetString();
                        
                        dto.Inventory = productRow.Cell("B").GetValue<int>();
                        dto.ProductUnit = productRow.Cell("C").GetString();
                        dto.IsPublishOnMainDomain = productRow.Cell("D").GetString() == "بله";
                        dto.ShowInLackOfInventory = productRow.Cell("E").GetString() == "بله";
                        dto.UniqueCode = productRow.Cell("F").GetString();
                        dto.SeoTitle = productRow.Cell("G").GetString();
                        dto.SeoDescription = productRow.Cell("H").GetString();
                        dto.Price = productRow.Cell("I").GetValue<long>();
                        dto.TagKeywords = productRow.Cell("J").GetString();
                        
                        if(Directory.Exists(tempUnzipFolderPath))
                        {
                            string sourceFilePath = "";
                            if(System.IO.File.Exists(Path.Combine(tempUnzipFolderPath, $"{rowNumber}.jpg").Replace("\\","/")))
                            {
                                sourceFilePath = Path.Combine(tempUnzipFolderPath, $"{rowNumber}.jpg").Replace("\\", "/");
                                var imageId = Guid.NewGuid().ToString();
                                
                                var filePathForDestinationCopy = Path.Combine(productImagePath, $"{imageId}.jpg").Replace("\\", "/");
                                System.IO.File.Copy(sourceFilePath, filePathForDestinationCopy);
                                dto.ProductImage = new Image()
                                {
                                    ImageId = imageId,
                                    Url = $"images/products/{imageId}.jpg",
                                    IsMain = true
                                };
                            }
                        }
                        lst.Add(dto);

                        productRow = productRow.RowBelow();
                    }
                }

                result = await _productRepository.ImportFromExcel(lst);
            }
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var lan = _languageRepository.GetDefaultLanguage(currentUserId);
            var groupList = await _productGroupRepository.GetAlActiveProductGroup(lan.LanguageId, currentUserId);
            groupList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.ProductGroupList = groupList;

            ViewBag.OperationResult = new OperationResult { Message = Language.GetString("FileImportExport_ResultMessage"),
                                                            Succeeded = result.Succeeded, 
                                                          };
            
            return View(model);
        }
    }
}
