using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Contracts.Shop.ProductUnit;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Contracts.Shop.ProductSpecificationGroup;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Drawing;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Entities.General.User;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductUnitRepository _unitRepository;
        private readonly IProductGroupRepository _productGroupRepository;
        private readonly IProductSpecGroupRepository _specGroupRepository;
        private readonly IPermissionView _permissionViewManager;
        private readonly ILanguageRepository _lanRepository;
        private readonly ICurrencyRepository _curRepository;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string imageSize = "";
        public ProductController(UserManager<ApplicationUser> userManager,
            IProductRepository productRepository, IPermissionView permissionView,
            ILanguageRepository languageRepository, IProductGroupRepository productGroupRepository,
            ICurrencyRepository currencyRepository, IProductUnitRepository unitRepository,
            IProductSpecGroupRepository specGroupRepository,
            IHttpContextAccessor accessor, IConfiguration configuration)
        {
            _productRepository = productRepository;
            _configuration = configuration;
            _permissionViewManager = permissionView;
            _lanRepository = languageRepository;
            _curRepository = currencyRepository;
            _productGroupRepository = productGroupRepository;
            _unitRepository = unitRepository;
            _specGroupRepository = specGroupRepository;
            _httpContextAccessor = accessor;
            _userManager = userManager;
            imageSize = _configuration["ProductImageSize:Size"];
        }
        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<ProductViewModel> result = new PagedItems<ProductViewModel>();
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            ViewBag.Permissions = dicKey;
            try
            {
                result = await _productRepository.List(Request.QueryString.ToString());
                var defLangId = _lanRepository.GetDefaultLanguage().LanguageId;
                ViewBag.DefLangId = defLangId;
                ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
               
                var groupList = _productGroupRepository.GetAlActiveProductGroup(defLangId);
                ViewBag.ProductGroupList = groupList;
               
                var unitList = _unitRepository.GetAllActiveProductUnit(defLangId);
                ViewBag.ProductUnitList = unitList;
            }
            catch (Exception)
            {
            }
            return View(result);
        }


        [HttpPost]
        public IActionResult  SaveProductImage(string file)
        {
            JsonResult output;
            try
            {
                if (!Directory.Exists("~/imgs/Products/temp"))
                {
                    Directory.CreateDirectory("~/imgs/Products/temp");
                };
                var temporaryFileName = $"{DateTime.Now.Ticks}.jpg";
                var path = $"~/imgs/Products/temp/{temporaryFileName}";
                System.IO.File.Create(path);
                System.IO.File.WriteAllText(path, file);
                output =  Json(new { status = "Succeed", path = path });
            }
            catch (Exception)
            {
                output = Json(new { status = "Error", path = string.Empty });
            }
            return output;
        }

        public string ResizeImage(string filePath, int desiredHeight)
        {
            var base64String = System.IO.File.ReadAllText(filePath);
            byte[] byteArray = Convert.FromBase64String(base64String);
            System.Drawing.Image img;
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                img = System.Drawing.Image.FromStream(ms);
            }

            double ratio = (double)desiredHeight / img.Height;
            int newWidth = (int)(img.Width * ratio);
            int newHeight = (int)(img.Height * ratio);
            Bitmap bitMapImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(bitMapImage))
            {
                g.DrawImage(img, 0, 0, newWidth, newHeight);
            }
            img.Dispose();
            //return newImage;

            byte[] byteImage;
            using (MemoryStream ms = new MemoryStream())
            {
                bitMapImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byteImage = ms.ToArray();
            }
            return Convert.ToBase64String(byteImage);
        }
        public async Task<IActionResult> AddEdit(string id = "")
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userDB = await _userManager.FindByIdAsync(currentUserId);
            if(userDB.IsSystemAccount)
            {
                var vendorList = await _userManager.GetUsersForClaimAsync(new Claim("AppRole", "True"));
                ViewBag.Vendors = vendorList.ToList().Select(_=>new SelectListModel()
                {
                    Text = _.Profile.FullName,
                    Value = _.Id
                });
            }
            else
            {
                ViewBag.Vendors = "-1";
            }
            var model = new ProductOutputDTO();
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = await _productRepository.ProductFetch(id);
            }

            var lan = _lanRepository.GetDefaultLanguage();

            var specGroupList = _specGroupRepository.AllActiveSpecificationGroup(lan.LanguageId);
            specGroupList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.SpecificationGroupList = specGroupList;

            var groupList = _productGroupRepository.GetAlActiveProductGroup(lan.LanguageId);
            groupList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.ProductGroupList = groupList;

            var currencyList = _curRepository.GetAllActiveCurrency();
            ViewBag.CurrencyList = currencyList;
            ViewBag.DefCurrency = _curRepository.GetDefaultCurrency().ReturnValue.CurrencyId;

            ViewBag.LangId = lan.LanguageId;
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();

            var unitList = _unitRepository.GetAllActiveProductUnit(lan.LanguageId);
            unitList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.ProductUnitList = unitList;


            ViewBag.PicSize = imageSize;
            return View(model);

        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            RepositoryOperationResult opResult = await _productRepository.DeleteProduct(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Restore(string id)
        {
            JsonResult result;
            try
            {

                var res = await _productRepository.Restore(id);
                if (res.Succeeded)
                {
                    result = new JsonResult(new
                    {
                        Status = "success",
                        Message = Language.GetString("AlertAndMessage_EditionDoneSuccessfully")
                    });
                }
                else
                {
                    result = new JsonResult(new
                    {
                        Status = "error",
                        Message = Language.GetString("AlertAndMessage_TryLator")
                    });
                }
            }
            catch (Exception ex)
            {
                result = new JsonResult(new
                {
                    Status = "error",
                    Message = Language.GetString("AlertAndMessage_TryLator")
                });
            }
            return result;
        }

    }
}
