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

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class ExcelController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IProductRepository _productRepository;
        public ExcelController(IConfiguration configuration, IProductRepository productRepository)
        {
            _configuration = configuration;
            _productRepository = productRepository;
        }
      

        [HttpGet]
        public IActionResult ImportProductFromExcel()
        {
            var model = new ProductImportPage();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportProductFromExcel(ProductImportPage model)
        {
            var res = new ProductImportPage();
            var result = new DataLayer.Models.Shared.Result();
            var lst = new List<ProductExcelImport>();
            if (model.ProductsExcelFile == null)
            {
                ViewBag.OperationResult = new OperationResult { Message = Language.GetString("FileImportExport_NoFileSelected"), Succeeded = false };
                return View(res);
            }

            #region ImageSection
            var imageFormFile = model.ProductImages;
            string tempFolderPath = "";
            if (model.ProductImages != null)
            {
                tempFolderPath = Path.Combine(_configuration["LocalStaticFileStorage"], "Temp", imageFormFile.FileName);
                using (FileStream inputStream = new(tempFolderPath, FileMode.Create))
                {
                    await imageFormFile.CopyToAsync(inputStream);
                    byte[] array = new byte[inputStream.Length];
                    inputStream.Seek(0, SeekOrigin.Begin);
                    inputStream.Read(array, 0, array.Length);
                    inputStream.Close();
                }
            }

            #endregion 
            string[] extension = { ".xls", ".xlsx" };
            string filePath = "";
            string productImagePath = Path.Combine(_configuration["LocalStaticFileStorage"] , "images/Products");
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
                        dto.IsPublishOnMainDomain = productRow.Cell("D").GetString() == "بله" ? true : false;
                        dto.ShowInLackOfInventory = productRow.Cell("E").GetString() == "بله" ? true : false;
                        dto.UniqueCode = productRow.Cell("F").GetString();
                        dto.SeoTitle = productRow.Cell("G").GetString();
                        dto.SeoDescription = productRow.Cell("H").GetString();
                        dto.Price = productRow.Cell("I").GetValue<long>();
                        dto.TagKeywords = productRow.Cell("J").GetString();
                        
                        if(System.IO.File.Exists(tempFolderPath))
                        {
                            string sourceFilePath = "";
                            if(System.IO.File.Exists(Path.Combine(tempFolderPath, $"{rowNumber}.jpg")))
                            {
                                sourceFilePath = Path.Combine(tempFolderPath, $"{rowNumber}.jpg");
                                var imageId = Guid.NewGuid().ToString();
                                
                                var filePathForDestinationCopy = Path.Combine(productImagePath, $"{imageId}.jpg");
                                System.IO.File.Copy(sourceFilePath, filePathForDestinationCopy);
                                dto.ProductImage = new DataLayer.Models.Shared.Image()
                                {
                                    ImageId = imageId,
                                    Url = "images/products/{imageId}.jpg",
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

            ViewBag.OperationResult = new OperationResult { Message = Language.GetString("FileImportExport_ResultMessage"), Succeeded = res, Url = Url.Action("List") };
            return View();
        }
    }
}
