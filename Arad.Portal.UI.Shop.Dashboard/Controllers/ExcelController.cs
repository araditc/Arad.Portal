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

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class ExcelController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ImportProductFromExcel()
        {
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ImportProductFromExcel(FilterWordsDto dto)
        //{
        //    if (Request.Form.Files.Count != 1)
        //    {
        //        ViewBag.OperationResult = new OperationResult { Message = Language.GetString("FileImportExport_SelectFilesNotSelected"), Succeeded = false };
        //        return View();
        //    }

        //    string filePath = "";
        //    string[] extension = { ".xls", ".xlsx" };

        //    foreach (IFormFile formFile in Request.Form.Files)
        //    {
        //        if (formFile is not { Length: > 0 })
        //        {
        //            continue;
        //        }

        //        if (!extension.Any(e => e.Equals(Path.GetExtension(formFile.FileName))))
        //        {
        //            ViewBag.OperationResult = new OperationResult { Message = Language.GetString("FileImportExport_InvalidContentType"), Succeeded = false };
        //            return View();
        //        }

        //        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(formFile.FileName)}";
        //        filePath = $"{Startup.RequestMainPath}\\{fileName}";

        //        await using FileStream inputStream = new(filePath, FileMode.Create);
        //        await formFile.CopyToAsync(inputStream);
        //        byte[] array = new byte[inputStream.Length];
        //        inputStream.Seek(0, SeekOrigin.Begin);
        //        inputStream.Read(array, 0, array.Length);
        //        inputStream.Close();
        //    }

        //    List<string> words = new();
        //    using (XLWorkbook wb = new(filePath))
        //    {
        //        foreach (IXLWorksheet ws in wb.Worksheets)
        //        {
        //            IXLColumn col = ws.FirstColumnUsed();
        //            words = col.AsRange().CellsUsed().AsEnumerable().Select(x => x.Value.ToString()).ToList();
        //        }
        //    }

        //    if (!words.Any())
        //    {
        //        ViewBag.OperationResult = new OperationResult { Message = Language.GetString("FileImportExport_FileEmpty"), Succeeded = false };
        //        return View(dto);
        //    }

        //    int successCount = 0;
        //    int failedList = 0;

        //    try
        //    {
        //        if (User.IsSystemAccount())
        //        {
        //            await SaveFilterWords();
        //        }
        //        else
        //        {
        //            await SaveUserFilterWords();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Fatal(e.Message);
        //        return StatusCode(500, new { Message = ConstMessages.ErrorInSaving });
        //    }

        //    async Task SaveFilterWords()
        //    {
        //        ConcurrentDictionary<string, FilterWords> successList = new();
        //        ConcurrentDictionary<string, FilterWords> storedWords = new();

        //        foreach (string currentWords in words)
        //        {
        //            try
        //            {
        //                if (string.IsNullOrEmpty(currentWords))
        //                {
        //                    continue;
        //                }

        //                Stopwatch t1 = Stopwatch.StartNew();
        //                storedWords.TryGetValue(currentWords, out FilterWords foundInStoredNumbers);
        //                t1.Stop();

        //                if (foundInStoredNumbers != null)
        //                {
        //                    storedWords.TryRemove(currentWords, out FilterWords _);

        //                    successList.TryAdd(currentWords,
        //                                       new()
        //                                       {
        //                                           CreationDate = DateTime.Now,
        //                                           CreatorId = User.GetUserId(),
        //                                           CreatorUserName = User.GetUserName(),
        //                                           Words = currentWords,
        //                                           Id = ObjectId.GenerateNewId(DateTime.Now).ToString(),
        //                                           IsActive = true
        //                                       });

        //                    continue;
        //                }

        //                successList.TryAdd(currentWords,
        //                                   new()
        //                                   {
        //                                       CreationDate = DateTime.Now,
        //                                       CreatorId = User.GetUserId(),
        //                                       CreatorUserName = User.GetUserName(),
        //                                       Words = currentWords,
        //                                       Id = ObjectId.GenerateNewId(DateTime.Now).ToString(),
        //                                       IsActive = true
        //                                   });
        //                successCount++;
        //            }
        //            catch
        //            {
        //                failedList++;
        //            }
        //        }

        //        foreach ((string key, FilterWords item) in storedWords)
        //        {
        //            successList.TryAdd(key, item);
        //        }

        //        if (System.IO.File.Exists(filePath))
        //        {
        //            System.IO.File.Delete(filePath);
        //        }

        //        if (successList.Any())
        //        {
        //            await _filterWordsRepository.SaveList(successList.Select(x => x.Value).ToList());
        //        }
        //    }

        //    async Task SaveUserFilterWords()
        //    {
        //        ConcurrentDictionary<string, UserFilterWords> successList = new();
        //        ConcurrentDictionary<string, UserFilterWords> storedWords = new();

        //        foreach (string currentWords in words)
        //        {
        //            try
        //            {
        //                if (string.IsNullOrEmpty(currentWords))
        //                {
        //                    continue;
        //                }

        //                Stopwatch t1 = Stopwatch.StartNew();
        //                storedWords.TryGetValue(currentWords, out UserFilterWords foundInStoredNumbers);
        //                t1.Stop();

        //                if (foundInStoredNumbers != null)
        //                {
        //                    storedWords.TryRemove(currentWords, out UserFilterWords _);

        //                    successList.TryAdd(currentWords,
        //                                       new()
        //                                       {
        //                                           CreationDate = DateTime.Now,
        //                                           CreatorId = User.GetUserId(),
        //                                           CreatorUserName = User.GetUserName(),
        //                                           Words = currentWords,
        //                                           Id = ObjectId.GenerateNewId(DateTime.Now).ToString(),
        //                                           IsActive = true
        //                                       });

        //                    continue;
        //                }

        //                successList.TryAdd(currentWords,
        //                                   new()
        //                                   {
        //                                       CreationDate = DateTime.Now,
        //                                       CreatorId = User.GetUserId(),
        //                                       CreatorUserName = User.GetUserName(),
        //                                       Words = currentWords,
        //                                       Id = ObjectId.GenerateNewId(DateTime.Now).ToString(),
        //                                       IsActive = true
        //                                   });
        //                successCount++;
        //            }
        //            catch
        //            {
        //                failedList++;
        //            }
        //        }

        //        foreach ((string key, UserFilterWords item) in storedWords)
        //        {
        //            successList.TryAdd(key, item);
        //        }

        //        if (System.IO.File.Exists(filePath))
        //        {
        //            System.IO.File.Delete(filePath);
        //        }

        //        if (successList.Any())
        //        {
        //            await _userFilterWordsRepository.SaveList(successList.Select(x => x.Value).ToList());
        //        }
        //    }

        //    ViewBag.OperationResult = new OperationResult { Message = string.Format(GetString("FileImportExport_ResultMessage"), successCount, failedList), Succeeded = true, Url = Url.Action("List") };
        //    return View(dto);
        //}
    }
}
