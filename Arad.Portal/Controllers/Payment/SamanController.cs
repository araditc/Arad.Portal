using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Entities.Shop.Transaction;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Helpers;
using Utilities = Arad.Portal.Helpers.UI.Utilities;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Transaction;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using System.Threading;
using Arad.Portal.Helpers.UI;
using Arad.Portal.Models.Shared.PSPs.Saman;
using Arad.Portal.Models.Shared;
using Arad.Portal.Controllers.Base;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.Helpers.Shared;

namespace Arad.Portal.Controllers.Payment;

public class SamanController(
    ITransactionRepository transactionRepository,
    IHttpContextAccessor accessor,
    CreateNotification createNotification,
    IDomainRepository domainRepository,
    ILanguageRepository languageRepository,
    SharedRuntimeData sharedRuntimeData,
    UserManager<ApplicationUser> userManager,
    IProductRepository productRepository,
    HttpClientHelper httpClientHelper,
    IConfiguration configuration,
    ControllerHelper controllerHelper)
    : BaseController(accessor, domainRepository, languageRepository)
{
    private readonly IHttpContextAccessor _accessor = accessor;

    private SamanModel _samanModel = null;

    [HttpGet]
    public async ValueTask<IActionResult> GetToken(string reservationNumber, CancellationToken cancellationToken)
    {
        reservationNumber = Utilities.Base64Decode(reservationNumber);
        Transaction transaction = await transactionRepository.FirstOrDefaultAsync(c => c.BasicData.ReservationNumber == reservationNumber, cancellationToken);

        string lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
        string errorUrl = $"/{lanIcon}/Payment/PaymentError";

        if (HttpContext.Request.Path.Value == null)
        {
            return Json(new { status = "error", redirecturl = errorUrl });
        }

        {
          

            if (transaction == null || transaction.BasicData.Stage != Enums.PaymentStage.Initialized)
            {
                TempData["Psp"] = "Saman";
                TempData["PaymentErrorMessage"] = "تراکنش مورد نظر یافت نشد.";

                return Json(new { status = "error", redirecturl = errorUrl });
            }

            if (_accessor.HttpContext != null)
            {
                GetTokenRequestModel senderModel = new()
                                                   {
                                                       ResNum = reservationNumber,
                                                       Action = "token",
                                                       Amount = Convert.ToInt64(transaction.FinalPriceToPay),
                                                       RedirectURL = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/fa-ir/Saman/Verify"
                                                   };

                Result<Domain> domainEntity = controllerHelper.FetchDomainByName(DomainName, true);

                try
                {

                    ProviderDetail samanData =
                        domainEntity.ReturnValue.DomainPaymentProviders.FirstOrDefault(d => d.PspType == Enums.PspType.Saman);

                    if (samanData != null)
                    {
                        _samanModel = JsonConvert.DeserializeObject<SamanModel>(samanData.DomainValueProvider);
                        senderModel.TerminalId = _samanModel.TerminalId;
                    }
                    else
                    {
                        return Json(new { status = "error", redirecturl = errorUrl });
                    }
                }
                catch (Exception)
                {
                    TempData["Psp"] = "Saman";
                    TempData["PaymentErrorMessage"] = "پارامترهای درگاه پرداخت سامان یافت نشد.";

                    //return Json({"PaymentError", "Payment");
                    return Json(new { status = "error", redirecturl = errorUrl });
                }


                HttpClient httpClient = httpClientHelper.GetClient();
                string serializedObj = JsonConvert.SerializeObject(senderModel);
                StringContent content = new(serializedObj, Encoding.UTF8, MediaTypeNames.Application.Json);
                httpClient.BaseAddress = new(_samanModel.BaseAddress);

                transaction.EventsData.Add(new() { JsonContent = serializedObj, ActionDateTime = DateTime.Now, ActionType = PspActions.ClientTokenRequest, AdditionalData = PspActions.ClientTokenRequest.GetDescription() });

                string serializedTokenResponse = "";

                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(_samanModel.TokenEndPoint, content, cancellationToken);
                serializedTokenResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                DateTime tokenResponseTime = DateTime.Now;


                transaction.EventsData.Add(new()
                                           {
                                               JsonContent = serializedTokenResponse, ActionDateTime = tokenResponseTime, ActionType = PspActions.PspTokenResponse, AdditionalData = PspActions.PspTokenResponse.GetDescription()
                                           });
                await transactionRepository.UpdateAsync(c => c.Id == transaction.Id, m => m, transaction, cancellationToken);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    try
                    {
                        GetTokenResponseModel tokenResponse =
                            JsonConvert.DeserializeObject<GetTokenResponseModel>(serializedTokenResponse);

                        if (tokenResponse.Status == 1)
                        {
                            transaction.BasicData.Stage = Enums.PaymentStage.RedirectToIpg;

                            await transactionRepository.UpdateAsync(transaction, cancellationToken);
                            string path = $"https://sep.shaparak.ir/OnlinePG/SendToken?token={tokenResponse.Token}";

                            return Json(new { status = "success", redirecturl = path });


                        }
                        else
                        {
                            transaction.EventsData.FirstOrDefault(d => d.ActionType == PspActions.PspTokenResponse)!.AdditionalData =
                                $"token error desc : {tokenResponse.ErrorDesc}, errorCode: {tokenResponse.ErrorCode}";

                            await transactionRepository.UpdateAsync(transaction, cancellationToken);
                            TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
                            TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";

                            return Json(new { status = "error", redirecturl = errorUrl });
                        }

                    }
                    catch (Exception)
                    {

                        await transactionRepository.UpdateAsync(transaction, cancellationToken);

                        //Log.Error($"overall error : {e.Message}");
                        TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
                        TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";

                        return Json(new { status = "error", redirecturl = errorUrl });
                    }
                }
            }

            //Log.Error($"token error statusCode : {response.StatusCode}");
            TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
            TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";

            return Json(new { status = "error", redirecturl = errorUrl });
        }
    }

    [HttpPost]
    [AllowAnonymous]
    public async ValueTask<IActionResult> Verify(CancellationToken cancellationToken)
    {
        Transaction transaction = null;
        string lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
        string errorUrl = $"/{lanIcon}/Payment/PaymentError";
        string successUrl = $"/{lanIcon}/Payment/PaymentSuccess";

        if (HttpContext.Request.Path.Value == null)
        {
            return Redirect(errorUrl);
        }

        Result<Domain> domainEntity = controllerHelper.FetchDomainByName(DomainName, false);
        ProviderDetail samanData =
            domainEntity.ReturnValue.DomainPaymentProviders.FirstOrDefault(d => d.PspType == Enums.PspType.Saman);

        if (samanData != null)
        {
            _samanModel = JsonConvert.DeserializeObject<SamanModel>(samanData.DomainValueProvider);
        }

        //senderModel.TerminalId = _samanModel.TerminalId;
        try
        {
            DateTime callbackTime = DateTime.Now;
            GatewayResponseModel initialData = new()
                                               {
                                                   Amount = Convert.ToInt64(Request.Form["Amount"].ToString()),
                                                   State = Request.Form["State"].ToString(),
                                                   Status = Convert.ToInt32(Request.Form["Status"].ToString()),
                                                   HashedCardNumber = Request.Form["HashedCardNumber"].ToString(),
                                                   Mid = Request.Form["MID"].ToString(),
                                                   ResNum = Request.Form["ResNum"].ToString(),
                                                   SecurePan = Request.Form["SecurePan"].ToString(),
                                                   TerminalId = Request.Form["TerminalId"].ToString(),
                                                   Wage = !string.IsNullOrWhiteSpace(Request.Form["Wage"].ToString()) ? Convert.ToInt64(Request.Form["Wage"].ToString()) : 0,
                                                   Rrn = Request.Form["RRN"].ToString(),
                                                   TraceNo = Request.Form["TraceNo"].ToString()
                                               };

            transaction = await transactionRepository.FirstOrDefaultAsync(c => c.BasicData.ReservationNumber == initialData.ResNum, cancellationToken);

            if (transaction == null)
            {
                TempData["Psp"] = "Saman";
                TempData["PaymentErrorMessage"] = "تراکنش مورد نظر یافت نشد.";
                await sharedRuntimeData.DeleteDataWithRollBack(transaction.Id);

                return RedirectToAction("PaymentError", "Payment");
            }

            if (!string.IsNullOrWhiteSpace(Request.Form["RefNum"]))
            {
                initialData.RefNum = Request.Form["RefNum"].ToString();

                #region checking for uniqueness of referencenumber
                if (initialData.State == "OK")
                {
                    bool isUnique = await transactionRepository.AnyAsync(c => c.BasicData.ReferenceId == initialData.RefNum && c.BasicData.PspType == Enums.PspType.Saman && c.AssociatedDomainId == domainEntity.ReturnValue.Id,
                                                                         cancellationToken);

                    transaction.EventsData.Add(new()
                                               {
                                                   JsonContent = JsonConvert.SerializeObject(initialData),
                                                   ActionDateTime = callbackTime,
                                                   ActionType = PspActions.PspSendCallback,
                                                   AdditionalData = PspActions.PspSendCallback.GetDescription()
                                               });
                    transaction.BasicData.ReferenceId = initialData.RefNum;
                    await transactionRepository.UpdateAsync(transaction, cancellationToken);

                    if (!isUnique)
                    {
                        TempData["Psp"] = "Saman";
                        TempData["PaymentErrorMessage"] = "کد رهگیری تکراری میباشد.";
                        await sharedRuntimeData.DeleteDataWithRollBack(transaction.Id);

                        return RedirectToAction("PaymentError", "Payment");
                    }
                }

                #endregion

            }
            else
            {
                TempData["Psp"] = "Saman";
                TempData["PaymentErrorMessage"] = "مشکلی در تراکنش توسط خریدار به وجود آمده است";
                await sharedRuntimeData.DeleteDataWithRollBack(transaction.Id);

                return RedirectToAction("PaymentError", "Payment");
            }


            if (initialData.Status == 2 && initialData.State == "OK")
            {
                if (transaction.BasicData.Stage != Enums.PaymentStage.RedirectToIpg)
                {
                    TempData["Psp"] = "Saman";
                    TempData["PaymentErrorMessage"] = "تراکنش تکراری.";
                    await sharedRuntimeData.DeleteDataWithRollBack(transaction.Id);

                    return RedirectToAction("PaymentError", "Payment");
                }
            }

            if (initialData.State == "OK")
            {
                transaction.BasicData.Stage = Enums.PaymentStage.DoneButNotConfirmed;
                await transactionRepository.UpdateAsync(transaction, cancellationToken);

                //try to verify transaction
                IpgInputModel verifyInputModel = new()
                                                 {
                                                     RefNum = initialData.RefNum, TerminalNumber = Convert.ToInt32(initialData.TerminalId),

                                                     //TxnRandomSessionKey = Convert.ToInt32(DateTime.Now.Ticks.ToString().Substring(0,9))

                                                 };

                string serializedInput = JsonConvert.SerializeObject(verifyInputModel);
                StringContent verifyContent = new(serializedInput, Encoding.UTF8, MediaTypeNames.Application.Json);

                transaction.EventsData.Add(new() { JsonContent = serializedInput, ActionDateTime = DateTime.Now, AdditionalData = PspActions.ClientVerifyRequest.GetDescription(), ActionType = PspActions.ClientVerifyRequest });
                await transactionRepository.UpdateAsync(transaction, cancellationToken);


                HttpClient client = httpClientHelper.GetClient();
                client.Timeout = TimeSpan.FromSeconds(60);
                client.BaseAddress = new(_samanModel.BaseAddress);

                //try several times to get response from verifyEndpoint
                HttpResponseMessage response = null;

                for (int i = 0; i < 10; i++)
                {
                    try
                    {

                        response = await client.PostAsync(_samanModel.VerifyEndpoint, verifyContent, cancellationToken);

                        //get result before timeout
                        break;
                    }
                    catch (Exception ex)
                    {
                        //request timeout and didn't get any result then if it is in boundary try again
                        continue;
                    }
                }

                if (response != null)
                {
                    string verifyResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                    IpgOutputModel verifyOutPutModel = JsonConvert.DeserializeObject<IpgOutputModel>(verifyResponse);

                    transaction.EventsData.Add(new() { JsonContent = verifyResponse, ActionDateTime = DateTime.Now, AdditionalData = PspActions.PspVerifyResponse.GetDescription(), ActionType = PspActions.PspVerifyResponse });
                    await transactionRepository.UpdateAsync(transaction, cancellationToken);

                    if (verifyOutPutModel.Success && verifyOutPutModel.ResultCode >= 0)
                    {
                        //check original amount and affected amount
                        if (verifyOutPutModel.TransactionDetail.OriginalAmount != verifyOutPutModel.TransactionDetail.AffectiveAmount)
                        {
                            #region درخواست برگشت تراکنش بعلت عدم تطابق مبالغ
                            HttpResponseMessage reverseResponse = await client.PostAsync(_samanModel.ReverseEndPoint, verifyContent, cancellationToken);
                            string reverseResponseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                            //same as verify output model
                            verifyOutPutModel = JsonConvert.DeserializeObject<IpgOutputModel>(reverseResponseContent);

                            transaction.EventsData.Add(new()
                                                       {
                                                           JsonContent = reverseResponseContent,
                                                           ActionDateTime = DateTime.Now,
                                                           AdditionalData = PspActions.ClientRequestReverseTransaction.GetDescription(),
                                                           ActionType = PspActions.ClientRequestReverseTransaction
                                                       });

                            if (verifyOutPutModel.Success)
                            {
                                transaction.EventsData.Add(new()
                                                           {
                                                               JsonContent = "transaction successfully been reversed",
                                                               ActionDateTime = DateTime.Now,
                                                               AdditionalData = PspActions.PspResponseReverseTransaction.GetDescription(),
                                                               ActionType = PspActions.PspResponseReverseTransaction
                                                           });
                                transaction.BasicData.Stage = Enums.PaymentStage.Failed;

                                await transactionRepository.UpdateAsync(transaction, cancellationToken);
                                TempData["Psp"] = "Saman";
                                await sharedRuntimeData.DeleteDataWithRollBack(transaction.Id);
                                TempData["PaymentErrorMessage"] = "تراکنش بعلت عدم تطابق مبلغ قابل پرداخت و موجودی کسر شده از کارت برگشت داده شد و تا 72 ساعت مبلغ کسر شده به کارت شما بازگشت داده میشود.";

                                return Redirect(errorUrl);
                            }
                            else
                            {
                                transaction.EventsData.Add(new()
                                                           {
                                                               JsonContent = "error happened to response of reversed transaction",
                                                               ActionDateTime = DateTime.Now,
                                                               AdditionalData = PspActions.PspResponseReverseTransaction.GetDescription(),
                                                               ActionType = PspActions.PspResponseReverseTransaction
                                                           });
                                transaction.BasicData.Stage = Enums.PaymentStage.Failed;

                                await transactionRepository.UpdateAsync(transaction, cancellationToken);
                                TempData["Psp"] = "Saman";
                                await sharedRuntimeData.DeleteDataWithRollBack(transaction.Id);
                                TempData["PaymentErrorMessage"] = "در برگشت تراکنش بعلت عدم تطابق مبلغ قایل پرداخت و مبلغ کسر شده از کارت خطایی بوجود آمده است لطفا با پشتیبانی تماس حاصل فرمایید.";

                                return RedirectToAction("PaymentError", "Payment");

                            }
                            #endregion
                        }
                        else
                        {
                            transaction.EventsData.Add(new()
                                                       {
                                                           JsonContent = "Successfully been verified by psp and amounts are equal",
                                                           ActionDateTime = DateTime.Now,
                                                           AdditionalData = PspActions.PspVerifyResponse.GetDescription(),
                                                           ActionType = PspActions.PspVerifyResponse
                                                       });
                            transaction.AdditionalData.Add(new() { Key = "PaymentDate", Value = DateTime.Now.ToString() });
                            transaction.BasicData.Stage = Enums.PaymentStage.DoneAndConfirmed;
                            transaction.OrderStatus = OrderStatus.OrderRegitered;
                            transaction.BasicData.ReferenceId = verifyOutPutModel.TransactionDetail.RefNum;
                            await transactionRepository.UpdateAsync(transaction, cancellationToken);

                            //رسید دیجیتالی
                            TempData["ReferenceNumber"] = verifyOutPutModel.TransactionDetail.RefNum;
                            TempData["Psp"] = "Saman";

                            TempData["InvoiceNumber"] = transaction.MainInvoiceNumber;

                            //شماره خرید
                            TempData["ReservationNumber"] = transaction.BasicData.ReservationNumber;

                            //شماره مرجع
                            TempData["RRN"] = verifyOutPutModel.TransactionDetail.Rrn;

                            //کد رهگیری
                            TempData["StraceNo"] = verifyOutPutModel.TransactionDetail.StraceNo;

                            #region product download limitation
                            Result<Domain> domainRes = controllerHelper.FetchDomainByName(DomainName, false);

                            foreach (InvoicePerSeller sub in transaction.SubInvoices)
                            {
                                foreach (ShoppingCartDetail item in sub.PurchasePerSeller.Products)
                                {
                                    Result result = new();

                                    try
                                    {
                                        DataLayer.Entities.Shop.Product.Product productEntity = await productRepository.FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);

                                        if (productEntity is { ProductType: Enums.ProductType.File })
                                        {
                                            DownloadLimitation obj = new DownloadLimitation()
                                                                     {
                                                                         Id = Guid.NewGuid().ToString(),
                                                                         AssociatedDomainId = domainRes.ReturnValue.Id,
                                                                         CreatorUserId = transaction.CustomerData.UserId,
                                                                         CreationDate = DateTime.Now
                                                                     };

                                            switch (productEntity.DownloadLimitationType)
                                            {
                                                case Enums.DownloadLimitationType.TimeDuration:
                                                    obj.StartDate = DateTime.Now;

                                                    break;

                                                case Enums.DownloadLimitationType.DownloadCount:
                                                    obj.DownloadedCount = 0;

                                                    break;

                                                case Enums.DownloadLimitationType.TimeDurationWithCnt:
                                                    obj.StartDate = DateTime.Now;
                                                    obj.DownloadedCount = 0;

                                                    break;

                                                case Enums.DownloadLimitationType.NoLimitation:
                                                    break;

                                                default:
                                                    throw new ArgumentOutOfRangeException();
                                            }

                                            productEntity.DownloadLimitation = obj;
                                            await productRepository.InsertAsync(productEntity, cancellationToken);
                                        }

                                        result.Succeeded = true;
                                        result.Message = ConstMessages.SuccessfullyDone;
                                    }
                                    catch (Exception)
                                    {
                                        result.Succeeded = false;
                                        result.Message = ConstMessages.InternalServerErrorMessage;
                                    }
                                }
                            }
                            #endregion

                            #region sendNotification for customer and siteadmin
                            ApplicationUser customerUser = await userManager.FindByIdAsync(transaction.CustomerData.UserId);
                            ApplicationUser siteAdmin = userManager.Users
                                                                   .FirstOrDefault(u => u.Domains.Any(a => a.DomainId == domainRes.ReturnValue.Id));
                            await createNotification.NotifyNewOrder("UserRegisterNewOrder", customerUser, cancellationToken);
                            await createNotification.NotifyNewOrder("AdminRegisterNewOrder", siteAdmin, cancellationToken);
                            #endregion

                            sharedRuntimeData.DeleteDataWithoutRollBack(transaction.Id);

                            return Redirect(successUrl);
                        }
                    }
                    else
                    {
                        transaction.EventsData.Add(new()
                                                   {
                                                       JsonContent = "Psp failed to verify Transaction",
                                                       ActionDateTime = DateTime.Now,
                                                       AdditionalData = PspActions.PspVerifyResponse.GetDescription(),
                                                       ActionType = PspActions.PspVerifyResponse
                                                   });
                        transaction.BasicData.Stage = Enums.PaymentStage.Failed;

                        await transactionRepository.UpdateAsync(transaction, cancellationToken);
                        TempData["Psp"] = "Saman";
                        TempData["PaymentErrorMessage"] = "خطا در تایید تراکنش توسط درگاه";
                        await sharedRuntimeData.DeleteDataWithRollBack(transaction.Id);

                        return Redirect(errorUrl);

                    }

                    //return Redirect(errorUrl);
                }
                else //when response is null
                {
                    TempData["Psp"] = "Saman";
                    TempData["PaymentErrorMessage"] = "پاسخی از درگاه برای تایید دریافت نشدودرصورت کسر موجودی تا 72 ساعت به حساب شما بازگشته میشود.";
                    await sharedRuntimeData.DeleteDataWithRollBack(transaction.Id);

                    return Redirect(errorUrl);
                }
            }
            else //initialData.State is not ok
            {
                TempData["Psp"] = "Saman";
                TempData["PaymentErrorMessage"] = "خطای درگاه بعد از پرداخت";
                await sharedRuntimeData.DeleteDataWithRollBack(transaction.Id);

                return Redirect(errorUrl);
            }
        }
        catch (Exception ex)
        {
            transaction.BasicData.Stage = Enums.PaymentStage.Failed;
            await transactionRepository.UpdateAsync(transaction, cancellationToken);
            TempData["Psp"] = "Saman";
            TempData["PaymentErrorMessage"] = "خطایی پس از انتقال از درگاه بوجود آمده است";
            await sharedRuntimeData.DeleteDataWithRollBack(transaction.Id);

            return Redirect(errorUrl);
        }
    }
}