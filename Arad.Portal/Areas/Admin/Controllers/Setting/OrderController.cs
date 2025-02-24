using Arad.Portal.GeneralLibrary.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Transaction;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using System.Threading;
using Arad.Portal.DataLayer.Entities.Shop.Transaction;
using static Arad.Portal.DataLayer.Models.Shared.Enums;
using MongoDB.Driver;
using System.Collections.Specialized;
using System.Web;

using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;

using Serilog;
using Arad.Portal.Models.Shared.Transaction;
using Arad.Portal.Helpers.Shared;

namespace Arad.Portal.Areas.Admin.Controllers.Setting;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class OrderController(
    ITransactionRepository transactionRepository,
    UserManager<ApplicationUser> userManager,
    IProductRepository productRepository,
    ILogger logger,
    ControllerHelper controllerHelper)
    : Controller
{

    [HttpGet]
    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        PagedItems<TransactionGlanceAdminView> result = new();
        try
        {
            string querystring = Request.QueryString.ToString();
            ApplicationUser userEntity = await controllerHelper.GetCurrentUser(cancellationToken);
            if (!userEntity.IsSystemAccount)
            {
                string domainId = userEntity.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;
                if (!string.IsNullOrWhiteSpace(Request.QueryString.ToString()))
                {
                    querystring += $"&domainId={domainId}";
                }
                else
                {
                    querystring = $"?domainId={domainId}";
                }
            }

            FilterDefinitionBuilder<Transaction> traBuilder = new();

            try
            {
                NameValueCollection filter = HttpUtility.ParseQueryString(querystring);

                if (string.IsNullOrWhiteSpace(filter["page"]))
                {
                    filter.Set("page", "1");
                }

                if (string.IsNullOrWhiteSpace(filter["PageSize"]))
                {
                    filter.Set("PageSize", "20");
                }
                int page = Convert.ToInt32(filter["page"]);
                int pageSize = Convert.ToInt32(filter["PageSize"]);
                long totalCount = 0;
                if (!string.IsNullOrWhiteSpace(filter["domainId"]))
                {
                    totalCount = await transactionRepository.GetCountAsync(t => t.AssociatedDomainId.ToString() == filter["domainId"], cancellationToken);
                }
                else
                {
                    totalCount = await transactionRepository.GetCountAsync(_ => true, cancellationToken);
                }

                if (!string.IsNullOrWhiteSpace(filter["domainId"]))
                {
                    traBuilder.Eq(nameof(Transaction.AssociatedDomainId), filter["domainId"]);
                }

                List<TransactionGlanceAdminView> list = (await transactionRepository.GetListAsync(c => c.AssociatedDomainId.ToString() == filter["domainId"], cancellationToken))
                                                                             .Select(t => new TransactionGlanceAdminView
                                                                             {
                                                                                 MainInvoiceNumber = t.MainInvoiceNumber,
                                                                                 OrderStatus = t.OrderStatus,
                                                                                 PaymentDate = DateTime.Parse(t.AdditionalData.FirstOrDefault(p => p.Key == "CreationDate")?.Value),
                                                                                 PaymentStage = t.BasicData.Stage,
                                                                                 RegisteredDate = t.CreationDate,
                                                                                 TotalAmount = t.FinalPriceToPay,
                                                                                 TransactionId = t.Id,
                                                                                 UserId = t.CustomerData.UserId,
                                                                                 UserFullName = t.CustomerData.UserFullName,
                                                                                 UserName = t.CustomerData.UserName,
                                                                                 // OrderItemCount = _.SubInvoices.Sum(a=> a.ParchasePerSeller.Products.Count())
                                                                             }).ToList();/*.Sort(Builders<DataLayer.Entities.Shop.Transaction.Transaction>.Sort.Descending(a => DateTime.Parse(a.AdditionalData.FirstOrDefault(_ => _.Key == "CreationDate").Value))).Skip((page - 1) * pageSize).Limit(pageSize).ToList();*/

                result.Items = list;
                result.CurrentPage = page;
                result.ItemsCount = totalCount;
                result.PageSize = pageSize;
                result.QueryString = querystring;

            }
            catch (Exception)
            {
                result.CurrentPage = 1;
                result.Items = [];
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = querystring;
            }
            //result = await _transactionRepository.GetSiteAdminTransactionList(querystring);
            List<SelectListModel> slmStatusType = [];
            slmStatusType.AddRange(from int i in Enum.GetValues(typeof(OrderStatus)) let name = Enum.GetName(typeof(OrderStatus), i) select new SelectListModel() { Text = name, Value = i.ToString() });

            slmStatusType.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });

            ViewBag.OrderStatusList = slmStatusType;

            List<SelectListModel> slmPaymentStageList = [];
            foreach (int i in Enum.GetValues(typeof(PaymentStage)))
            {
                string name = Enum.GetName(typeof(PaymentStage), i);
                SelectListModel obj = new()
                {
                    Text = name,
                    Value = i.ToString()
                };
                slmPaymentStageList.Add(obj);
            }
            slmPaymentStageList.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });
            ViewBag.PaymentStageList = slmPaymentStageList;

        }
        catch (Exception)
        {
        }
        return View(result);
    }

    [HttpGet]
    public async ValueTask<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        List<KeyValuePair<string, long>> productCodeList = [];
        Transaction model = await transactionRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        int cnt = 0;
        foreach (InvoicePerSeller item in model.SubInvoices)
        {
            cnt += item.PurchasePerSeller.Products.Count;
            foreach (ShoppingCartDetail pro in item.PurchasePerSeller.Products)
            {
                long code = (await productRepository.FirstAsync(c => c.Id == pro.ProductId, cancellationToken)).ProductCode;
                productCodeList.Add(new(pro.ProductId, code));
            }
        }
        ApplicationUser user = await userManager.FindByIdAsync(model.CreatorUserId);

        if (user != null)
        {
            ViewBag.UserFullName = user.Profile.FullName;
        }

        ViewBag.OrderItemCount = cnt;
        ViewBag.CodeList = productCodeList;
        ViewBag.PaymentDate = DateTime.Parse(model.AdditionalData.FirstOrDefault(p => p.Key == "CreationDate")?.Value);
        return View(model);
    }

    [HttpGet]
    public async ValueTask<IActionResult> ChangeStatus(string id, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            List<SelectListModel> result = [];
            result.AddRange(from int i in Enum.GetValues(typeof(OrderStatus)) let name = Enum.GetName(typeof(OrderStatus), i) select new SelectListModel() { Text = name, Value = i.ToString() });

            result.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });
            Transaction model = await transactionRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            ViewBag.traId = id;
            ViewBag.OrderStatusList = result;
            return View(model.OrderStatus);
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(OrderController)}/{nameof(ChangeStatus)}");
        }
        return View();
    }

    [HttpPost]
    public async ValueTask<IActionResult> ChangeStatus([FromBody] OrderChangeStatus model, CancellationToken cancellationToken)
    {
        Result<Transaction> updateRes = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            Transaction entity = await transactionRepository.FirstOrDefaultAsync(t => t.Id == model.TransactionId, cancellationToken);
            if (entity != null)
            {
                entity.OrderStatus = model.OrderStatus;
                updateRes = await transactionRepository.UpdateAsync(entity, cancellationToken);
                if (updateRes.Succeeded)
                {
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},changing status with {entity.Id} id done successfully");
                    updateRes.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    updateRes.Message = ConstMessages.ErrorInSaving;
                }
            }
            else
            {
                updateRes.Succeeded = false;
                updateRes.Message = ConstMessages.ObjectNotFound;
            }
        }
        catch (Exception e)
        {
            updateRes.Succeeded = false;
            updateRes.Message = ConstMessages.ExceptionOccured;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(OrderController)}/{nameof(ChangeStatus)}");
        }

        return Json(updateRes.Succeeded ? new { Status = "Success", Message = UtilityLanguage.GetString("AlertAndMessage_OperationDoneSuccessfully") }
                        : new { Status = "Error", Message = UtilityLanguage.GetString("AlertAndMessage_OperationFailed") });

    }
}