using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Contracts.Shop.Transaction;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.Transaction;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Transaction;
using Arad.Portal.GeneralLibrary.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class OrderController : Controller
    {

        private readonly ITransactionRepository _transactionRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public OrderController(ITransactionRepository transactionRepository, IDomainRepository domainRepository, UserManager<ApplicationUser> userManager, IHttpContextAccessor accessor, IProductRepository productRepository)
        {
            _transactionRepository = transactionRepository;
            _domainRepository = domainRepository;
            _userManager = userManager;
            _httpContextAccessor = accessor;
            _productRepository = productRepository;
        }


        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<TransactionGlanceAdminView> result = new PagedItems<TransactionGlanceAdminView>();
            try
            {
                var querystring = Request.QueryString.ToString();

                var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userEntity = await _userManager.FindByIdAsync(currentUserId);
                if (!userEntity.IsSystemAccount)
                {
                    var domainId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("RelatedDomain"))?.Value;
                    if (!string.IsNullOrWhiteSpace(Request.QueryString.ToString()))
                    {
                        querystring += $"&domainId={domainId}";
                    }
                    else
                    {
                        querystring = $"?domainId={domainId}";
                    }
                }
                result = await _transactionRepository.GetSiteAdminTransactionList(querystring);
                ViewBag.OrderStatusList = _transactionRepository.GetAllOrderStatusType();
                ViewBag.PaymentStageList = _transactionRepository.GetAllPaymentStageList();

            }
            catch (Exception)
            {
            }
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var currentUserId = HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value;
            List<KeyValuePair<string, long>> productCodeList = new List<KeyValuePair<string, long>>();
            var model = _transactionRepository.FetchById(id);
            var cnt = 0;
            foreach (var item in model.SubInvoices)
            {
                cnt += item.ParchasePerSeller.Products.Count;
                foreach (var pro in item.ParchasePerSeller.Products)
                {
                    var code  = _productRepository.GetProductCode(pro.ProductId);
                    productCodeList.Add(new KeyValuePair<string, long>(pro.ProductId, code));
                }
            }
            var user =await _userManager.FindByIdAsync(model.CreatorUserId);
            ViewBag.UserFullName = user.Profile.FullName;
            ViewBag.OrderItemCount = cnt;
            ViewBag.CodeList = productCodeList;
            ViewBag.PaymentDate = DateTime.Parse(model.AdditionalData.FirstOrDefault(_ => _.Key == "CreationDate").Value);
            return View(model);
        }

        [HttpGet]
        public IActionResult ChangeStatus(string id)
        {
            var orderStatusList = _transactionRepository.GetAllOrderStatusType();
            var model = _transactionRepository.FetchById(id);
            ViewBag.traId = id;
            ViewBag.OrderStatusList = orderStatusList;
            return View(model.OrderStatus);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus([FromBody]OrderChangeStatus model)
        {
            var res = await _transactionRepository.ChangeOrderStatus(model.TransactionId, model.OrderStatus);
            return Json(res ? new { Status = "Success", Message = Language.GetString("AlertAndMessage_OperationDoneSuccessfully") }
                    : new { Status = "Error", Message = Language.GetString("AlertAndMessage_OperationFailed") });

        }
    }
}
