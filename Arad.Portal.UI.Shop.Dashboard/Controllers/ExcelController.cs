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
using Arad.Portal.DataLayer.Models.Setting;
using KeyVal = Arad.Portal.DataLayer.Models.Shared.KeyVal;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Entities.General.User;
using static Lucene.Net.Util.Fst.Util;
using Arad.Portal.DataLayer.Contracts.General.BasicData;

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
        private readonly IBasicDataRepository _basicDataRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public ExcelController(IConfiguration configuration,
            IProductRepository productRepository,
            IWebHostEnvironment env,
            IBasicDataRepository basicDataRepository,
            IProductGroupRepository groupRepository,
            UserManager<ApplicationUser> userManager,
            ILanguageRepository languageRepository)
        {
            _configuration = configuration;
            _productRepository = productRepository;
            _productGroupRepository = groupRepository;
            _languageRepository = languageRepository;
            _Environment = env;
            _basicDataRepository = basicDataRepository;
            _userManager = userManager;
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
                        
                        //dto.Inventory = productRow.Cell("B").GetValue<int>();
                        dto.ProductUnit = productRow.Cell("B").GetString();
                        dto.IsPublishOnMainDomain = productRow.Cell("C").GetString() == Language.GetString("btn_Confirm");
                        dto.ShowInLackOfInventory = productRow.Cell("D").GetString() == Language.GetString("btn_Confirm");
                        dto.UniqueCode = productRow.Cell("E").GetString();
                        dto.SeoTitle = productRow.Cell("F").GetString();
                        dto.SeoDescription = productRow.Cell("G").GetString();
                        dto.Price = productRow.Cell("H").GetValue<long>();
                        dto.TagKeywords = productRow.Cell("I").GetString();
                        
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


        [HttpGet]
        /// <summary>
        /// this method can only Accessed by system account
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddExtraLanguage()
        {
            var model = new LanguageDictionaryModel();
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId);
            if(user.IsSystemAccount)
            {
                ViewBag.Languages = _languageRepository.GetAllLanguages();
                return View(model);
            }else
            {
                return Unauthorized();
            }
           
        }


        /// <summary>
        /// this method can only Accessed by system account
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExtraLanguage([FromForm] LanguageDictionaryModel model)
        {
            var res = new LanguageDictionaryModel();
            if (model.LanguageUploadFile == null)
            {
                ViewBag.OperationResult = new OperationResult
                {
                    Message = Language.GetString("FileImportExport_NoFileSelected"),
                    Succeeded = false
                };
                return View(res);
            }
            try
            {
                var formFile = model.LanguageUploadFile;
                List<KeyVal> lst = new List<KeyVal>();
                string[] extension = { ".xls", ".xlsx" };

                if (formFile is { Length: > 0 })
                {
                    if (!extension.Any(e => e.Equals(Path.GetExtension(formFile.FileName))))
                    {
                        ViewBag.OperationResult = new OperationResult { Message = Language.GetString("FileImportExport_InvalidContentType"), Succeeded = false };
                        return View();
                    }
                    var lanEntity = _languageRepository.FetchLanguage(model.LanguageId);

                    var lanSymbolName = lanEntity.Symbol.Contains("-") ? lanEntity.Symbol.Split("-")[0] : lanEntity.Symbol;
                    string jsonFileName = Path.Combine(_configuration["DictionaryFolderPath"], $"Dictionaries/{lanSymbolName}.json");
                    string excelFilePath = Path.Combine(_configuration["LocalStaticFileStorage"], "Temp", "LanguageTemp.xlsx");


                    await using FileStream inputStream = new(excelFilePath, FileMode.Create);
                    await formFile.CopyToAsync(inputStream);
                    byte[] array = new byte[inputStream.Length];
                    inputStream.Seek(0, SeekOrigin.Begin);
                    inputStream.Read(array, 0, array.Length);
                    inputStream.Close();

                    using (XLWorkbook wb = new(excelFilePath))
                    {
                        var ws = wb.Worksheet("Sheet1");
                        var titleRow = ws.FirstRowUsed();
                        var exelRow = titleRow.RowBelow();
                        while (!exelRow.IsEmpty())
                        {
                            var obj = new KeyVal();

                            obj.Key = exelRow.Cell("A").GetString();
                            obj.Val = exelRow.Cell("B").GetString();
                            lst.Add(obj);

                            exelRow = exelRow.RowBelow();
                        }
                    }
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(lst);
                    if (!System.IO.File.Exists(jsonFileName))
                    {
                        var fs = System.IO.File.Create(jsonFileName);
                        fs.Close();

                    }
                    System.IO.File.WriteAllText(jsonFileName, json);

                    //delete uploaded excel file
                    System.IO.File.Delete(excelFilePath);

                    //make the current Language Active
                    lanEntity.IsActive = true;
                    var result = await _languageRepository.EditLanguage(lanEntity);

                    //add this culture to supported cultures
                    var list = _basicDataRepository.GetList("SupportedCultures", false);
                    var lastValue = list.Max(_ => _.Value);
                    var domainId = "";
                    var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var userDb = await _userManager.FindByIdAsync(currentUserId);

                    domainId = userDb.Domains.FirstOrDefault(_ => _.IsOwner).DomainId;
                    await _basicDataRepository.InsertNewRecord(new DataLayer.Entities.General.BasicData.BasicData()
                    {
                        BasicDataId = Guid.NewGuid().ToString(),
                        GroupKey = "SupportedCultures",
                        Value = !string.IsNullOrWhiteSpace(lastValue) ? (Convert.ToInt32(lastValue) + 1).ToString() : "1",
                        Text = lanEntity.Symbol,
                        AssociatedDomainId = domainId,
                        Order = !string.IsNullOrWhiteSpace(lastValue) ? Convert.ToInt32(lastValue) + 1 : 1
                    });

                    ViewBag.OperationResult = new OperationResult
                    {
                        Message = Language.GetString("FileImportExport_ResultMessage"),
                        Succeeded = result.Succeeded,
                    };
                }
                Language.ReloadDictionary();
               
            }
            catch (Exception)
            {

                ViewBag.OperationResult = new OperationResult
                {
                    Message = Language.GetString("AlertAndMessage_ErrorOccurrence"),
                    Succeeded = false,
                };
            }
            ViewBag.Languages = _languageRepository.GetAllLanguages();
            return View(model);
        }

        [HttpGet]
        public async Task<FileResult> GetLanguageTemplate()
        {
            var filePath = Path.Combine(_configuration["DictionaryFolderPath"], $"Dictionaries/DictionaryTemplate.xlsx");
            // Calling the ReadAllBytes() function
            byte[] fileContent = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileContent,
                 "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                 "Template.xlsx");
        }
    }
}
