using Arad.Portal.DataLayer.Contracts.Shop.Transaction;
using Arad.Portal.UI.Shop.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Arad.Portal.DataLayer.Entities.Shop.Transaction;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Net.Mime;
using Arad.Portal.DataLayer.Models.PSPs.Saman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class SamanController : BaseController
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly HttpClientHelper _httpClientHelper;
        private readonly IConfiguration _configuration;
        private readonly SharedRuntimeData _sharedRuntimeData;
        private readonly IHttpContextAccessor _accessor;
        private readonly IDomainRepository _domainRepository;
        private SamanModel _samanModel = null;
        public SamanController(ITransactionRepository transactionRepository,
            IHttpContextAccessor accessor,
            IDomainRepository domainRepository,
             SharedRuntimeData sharedRuntimeData,
             IWebHostEnvironment env,
            HttpClientHelper httpClientHelper, IConfiguration configuration) : base(accessor, env)
        {
            _domainRepository = domainRepository;
            //MethodInfo method = typeof(XmlSerializer)
            //    .GetMethod("set_Mode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            //method.Invoke(null, new object[] { 1 });
            _transactionRepository = transactionRepository;
            _httpClientHelper = httpClientHelper;
            _configuration = configuration;
            _sharedRuntimeData = sharedRuntimeData;
            _accessor = accessor;
        }

        [HttpGet]
        public async Task<IActionResult> GetToken(string reservationNumber)
        {
            reservationNumber = Utilities.Base64Decode(reservationNumber);
            Transaction transaction = _transactionRepository.FetchByIdentifierToken(reservationNumber);
            if (transaction == null || transaction.BasicData.Stage != Enums.PaymentStage.Initialized)
            {
                TempData["Psp"] = "Saman";
                TempData["PaymentErrorMessage"] = "تراکنش مورد نظر یافت نشد.";
                return RedirectToAction("PaymentError", "Payment");
            }

            var senderModel = new GetTokenRequestModel()
            {
                ResNum = reservationNumber,
                Action = "token",
                Amount = transaction.FinalPriceToPay,
                RedirectURL = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/fa-ir/Saman/Verify"
            };

            var domainName = base.DomainName;
            var domainEntity = _domainRepository.FetchDomainByName(domainName);
           
            try
            {

                var samanData =
                   domainEntity.DomainPaymentProviders.FirstOrDefault(_ => _.PspType == Enums.PspType.Saman);

                _samanModel = JsonConvert.DeserializeObject<SamanModel>(samanData.DomainValueProvider);
                senderModel.TerminalId = _samanModel.TerminalId;
            }
            catch (Exception e)
            {
                TempData["Psp"] = "Saman";
                TempData["PaymentErrorMessage"] = "پارامترهای درگاه پرداخت سامان یافت نشد.";
                return RedirectToAction("PaymentError", "Payment");
            }


            var httpClient = _httpClientHelper.GetClient();
            var serializedObj = JsonConvert.SerializeObject(senderModel);
            var content = new StringContent(serializedObj, Encoding.UTF8, MediaTypeNames.Application.Json);
            httpClient.BaseAddress = new Uri(_samanModel.BaseAddress);

            transaction.EventsData.Add(new EventData()
            {
                JsonContent = serializedObj,
                ActionDateTime = DateTime.Now,
                ActionType = PspActions.ClientTokenRequest,
                additionalData = PspActions.ClientTokenRequest.GetDescription()
            });




            //???
            //httpResponseMessage.EnsureSuccessStatusCode();
            string serializedTokenResponse = "";
            HttpResponseMessage httpResponseMessage;
            try
            {
                httpResponseMessage = await httpClient.PostAsync(_samanModel.TokenEndPoint, content);
                serializedTokenResponse = await httpResponseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {

                throw;
            }
            var tokenResponseTime = DateTime.Now;
            

            transaction.EventsData.Add(new EventData()
            {
                JsonContent = serializedTokenResponse,
                ActionDateTime = tokenResponseTime,
                ActionType = PspActions.PspTokenResponse,
                additionalData = PspActions.PspTokenResponse.GetDescription()
            });
            await _transactionRepository.UpdateTransaction(transaction);
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                try
                {
                    var tokenResponse =
                        JsonConvert.DeserializeObject<GetTokenResponseModel>(serializedTokenResponse);

                    if (tokenResponse.Status == 1)
                    {
                        transaction.BasicData.Stage = Enums.PaymentStage.RedirectToIPG;

                        await _transactionRepository.UpdateTransaction(transaction);

                        var path = Flurl.Url.Combine(_samanModel.BaseAddress, _samanModel.GatewayEndPoint) + "?token=" + tokenResponse.Token;

                        return Redirect(path);
                        //try
                        //{
                        //     await httpClient.GetAsync(_samanModel.GatewayEndPoint+"?token="+ tokenResponse.Token);
                        //}
                        //catch (Exception ex)
                        //{

                        //    throw;
                        //}

                       // return Redirect(path);
                    }
                    else
                    {
                        transaction.EventsData.FirstOrDefault(_ => _.ActionType == PspActions.PspTokenResponse).additionalData =
                        $"token error desc : {tokenResponse.ErrorDesc}, errorCode: {tokenResponse.ErrorCode}";
                        await _transactionRepository.UpdateTransaction(transaction);
                        //Log.Error($"token error desc : {tokenResponse.ErrorDesc}, errorCode: {tokenResponse.ErrorCode}");
                        TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
                        TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";
                        return RedirectToAction("PaymentError", "Payment");
                    }

                }
                catch (Exception ex)
                {
                    await _transactionRepository.UpdateTransaction(transaction);
                    //Log.Error($"overal error : {e.Message}");
                    TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
                    TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";
                    return RedirectToAction("PaymentError", "Payment");
                }
            }
            //Log.Error($"token error statusCode : {response.StatusCode}");
            TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
            TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";
            return RedirectToAction("PaymentError", "Payment");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Verify()
        {
            Transaction transaction = null;
            try
            {
                var callbackTime = DateTime.Now;
                var initialData = new GatewayResponseModel()
                {
                    Amount = Convert.ToInt64(Request.Form["Amount"].ToString()),
                    State = Request.Form["State"].ToString(),
                    Status = Convert.ToInt32(Request.Form["Status"].ToString()),
                    HashedCardNumber = Request.Form["HashedCardNumber"].ToString(),
                    MID = Request.Form["MID"].ToString(),
                    ResNum = Request.Form["ResNum"].ToString(),
                    SecurePan = Request.Form["SecurePan"].ToString(),
                    TerminalId = Request.Form["TerminalId"].ToString(),
                    Wage = Convert.ToInt64(Request.Form["Wage"].ToString()),
                    RRN = Request.Form["RRN"].ToString(),
                    TraceNo = Request.Form["TraceNo"].ToString()
                };

                transaction = _transactionRepository.FetchByIdentifierToken(initialData.ResNum);
                if (transaction == null)
                {
                    TempData["Psp"] = "Saman";
                    TempData["PaymentErrorMessage"] = "تراکنش مورد نظر یافت نشد.";
                    await _sharedRuntimeData.DeleteDataWithRollBack(transaction.TransactionId);
                    return RedirectToAction("PaymentError", "Payment");
                }

                if (!string.IsNullOrWhiteSpace(Request.Form["RefNum"]))
                {
                    initialData.RefNum = Request.Form["RefNum"].ToString();

                    #region checking for uniqueness of referencenumber
                    if(initialData.State == "OK")
                    {
                        bool isUnique = _transactionRepository.IsRefNumberUnique(initialData.RefNum, Enums.PspType.Saman);
                        transaction.EventsData.Add(new EventData()
                        {
                            JsonContent = JsonConvert.SerializeObject(initialData),
                            ActionDateTime = callbackTime,
                            ActionType = PspActions.PspSendCallback,
                            additionalData = PspActions.PspSendCallback.GetDescription()
                        });
                        transaction.BasicData.ReferenceId = initialData.RefNum;
                        await _transactionRepository.UpdateTransaction(transaction);

                        if (!isUnique)
                        {
                            TempData["Psp"] = "Saman";
                            TempData["PaymentErrorMessage"] = "کد رهگیری تکراری میباشد.";
                            await _sharedRuntimeData.DeleteDataWithRollBack(transaction.TransactionId);
                            return RedirectToAction("PaymentError", "Payment");
                        }
                    }
                    
                    #endregion

                }
                else
                {
                    TempData["Psp"] = "Saman";
                    TempData["PaymentErrorMessage"] = "مشکلی در تراکنش توسط خریدار به وجود آمده است";
                    await _sharedRuntimeData.DeleteDataWithRollBack(transaction.TransactionId);
                    return RedirectToAction("PaymentError", "Payment");
                }


                if (initialData.Status == 2 && initialData.State == "OK")
                {
                    if (transaction.BasicData.Stage != Enums.PaymentStage.RedirectToIPG)
                    {
                        TempData["Psp"] = "Saman";
                        TempData["PaymentErrorMessage"] = "تراکنش تکراری.";
                        await _sharedRuntimeData.DeleteDataWithRollBack(transaction.TransactionId);
                        return RedirectToAction("PaymentError", "Payment");
                    }
                }
                if(initialData.State == "OK")
                {
                    transaction.BasicData.Stage = Enums.PaymentStage.DoneButNotConfirmed;
                    await _transactionRepository.UpdateTransaction(transaction);

                    //try to verify transaction
                    var verifyInputModel = new IPGInputModel()
                    {
                        ReferenceId = initialData.RefNum,
                        TerminalNumber = Convert.ToInt32(initialData.TerminalId)
                    };

                    var serializedInput = JsonConvert.SerializeObject(verifyInputModel);
                    var verifyContent = new StringContent(serializedInput, Encoding.UTF8, MediaTypeNames.Application.Json);

                    transaction.EventsData.Add(new EventData()
                    {
                        JsonContent = serializedInput,
                        ActionDateTime = DateTime.Now,
                        additionalData = PspActions.ClientVerifyRequest.GetDescription(),
                        ActionType = PspActions.ClientVerifyRequest
                    });
                    await _transactionRepository.UpdateTransaction(transaction);

                  
                    var client = _httpClientHelper.GetClient();
                    client.Timeout = TimeSpan.FromSeconds(60);
                    client.BaseAddress = new Uri(_samanModel.BaseAddress);
                  
                    //try several times to get response from verifyEndpoint
                    HttpResponseMessage response = null;
                    for (int i = 0; i < 10; i++)
                    {
                        try
                        {
                            response = await client.PostAsync(_samanModel.VerifyEndpoint, verifyContent);
                            //get result before timeout
                            break;
                        }
                        catch (Exception ex)
                        {
                            //request timeout and didnt get any result then if it is in boundary try again
                            continue;
                        }
                    }
                    if(response != null)
                    {
                        var verifyResponse = await response.Content.ReadAsStringAsync();
                        var verifyOutPutModel = JsonConvert.DeserializeObject<IPGOutputModel>(verifyResponse);

                        transaction.EventsData.Add(new EventData()
                        {
                            JsonContent = verifyResponse,
                            ActionDateTime = DateTime.Now,
                            additionalData = PspActions.PspVerifyResponse.GetDescription(),
                            ActionType = PspActions.PspVerifyResponse
                        });
                        await _transactionRepository.UpdateTransaction(transaction);
                        if (verifyOutPutModel.Success && verifyOutPutModel.ResultCode >= 0)
                        {
                            //check original amount and affected amount
                            if (verifyOutPutModel.VerifyInfo.OriginalAmount != verifyOutPutModel.VerifyInfo.AffectiveAmount)
                            {
                                #region درخواست برگشت تراکنش بعلت عدم تطابق مبالغ
                                var reverseResponse = await client.PostAsync(_samanModel.ReverseEndPoint, verifyContent);
                                var reverseResponseContent = await response.Content.ReadAsStringAsync();
                                //same as verifyoutputmodel
                                verifyOutPutModel = JsonConvert.DeserializeObject<IPGOutputModel>(reverseResponseContent);

                                transaction.EventsData.Add(new EventData()
                                {
                                    JsonContent = reverseResponseContent,
                                    ActionDateTime = DateTime.Now,
                                    additionalData = PspActions.ClientRequestReverseTransaction.GetDescription(),
                                    ActionType = PspActions.ClientRequestReverseTransaction
                                });

                                if (verifyOutPutModel.Success)
                                {
                                    transaction.EventsData.Add(new EventData()
                                    {
                                        JsonContent = "transaction successfully been reversed",
                                        ActionDateTime = DateTime.Now,
                                        additionalData = PspActions.PspResponseReverseTransaction.GetDescription(),
                                        ActionType = PspActions.PspResponseReverseTransaction
                                    });
                                    transaction.BasicData.Stage = Enums.PaymentStage.Failed;
                                    await _transactionRepository.UpdateTransaction(transaction);
                                    TempData["Psp"] = "Saman";
                                    await _sharedRuntimeData.DeleteDataWithRollBack(transaction.TransactionId);
                                    TempData["PaymentErrorMessage"] = "تراکنش بعلت عدم تطابق مبلغ قابل پرداخت و موجودی کسر شده از کارت برگشت داده شد و تا 72 ساعت مبلغ کسر شده به کارت شما بازگشت داده میشود.";
                                    return RedirectToAction("PaymentError", "Payment");
                                }
                                else
                                {
                                    transaction.EventsData.Add(new EventData()
                                    {
                                        JsonContent = "error happened to response of reversed transaction",
                                        ActionDateTime = DateTime.Now,
                                        additionalData = PspActions.PspResponseReverseTransaction.GetDescription(),
                                        ActionType = PspActions.PspResponseReverseTransaction
                                    });
                                    transaction.BasicData.Stage = Enums.PaymentStage.Failed;
                                    await _transactionRepository.UpdateTransaction(transaction);
                                    TempData["Psp"] = "Saman";
                                    await _sharedRuntimeData.DeleteDataWithRollBack(transaction.TransactionId);
                                    TempData["PaymentErrorMessage"] = "در برگشت تراکنش بعلت عدم تطابق مبلغ قایل پرداخت و مبلغ کسر شده از کارت خطایی بوجود آمده است لطفا با پشتیبانی تماس حاصل فرمایید.";
                                    return RedirectToAction("PaymentError", "Payment");

                                }
                                #endregion
                            }
                            else
                            {
                                transaction.EventsData.Add(new EventData()
                                {
                                    JsonContent = "Successfully been verifed by psp and amounts are equal",
                                    ActionDateTime = DateTime.Now,
                                    additionalData = PspActions.PspVerifyResponse.GetDescription(),
                                    ActionType = PspActions.PspVerifyResponse
                                });
                                transaction.BasicData.Stage = Enums.PaymentStage.DoneAndConfirmed;
                                transaction.BasicData.ReferenceId = verifyOutPutModel.VerifyInfo.RefNum;
                                await _transactionRepository.UpdateTransaction(transaction);

                                //رسید دیجیتالی
                                TempData["ReferenceNumber"] = verifyOutPutModel.VerifyInfo.RefNum;
                                TempData["Psp"] = "Saman";

                                TempData["InvoiceNumber"] = transaction.MainInvoiceNumber;
                                //شماره خرید
                                TempData["ReservationNumber"] = transaction.BasicData.ReservationNumber;
                                //شماره مرجع
                                TempData["RRN"] = verifyOutPutModel.VerifyInfo.RRN;
                                //کد رهگیری
                                TempData["StraceNo"] = verifyOutPutModel.VerifyInfo.StraceNo;
                                _sharedRuntimeData.DeleteDataWithoutRollBack(transaction.TransactionId);
                                return RedirectToAction("PaymentSuccess", "Payment");
                            }
                        }
                        else
                        {
                            transaction.EventsData.Add(new EventData()
                            {
                                JsonContent = "Psp failed to verify Transaction",
                                ActionDateTime = DateTime.Now,
                                additionalData = PspActions.PspVerifyResponse.GetDescription(),
                                ActionType = PspActions.PspVerifyResponse
                            });
                            transaction.BasicData.Stage = Enums.PaymentStage.Failed;
                            await _transactionRepository.UpdateTransaction(transaction);
                            TempData["Psp"] = "Saman";
                            TempData["PaymentErrorMessage"] = "خطا در تایید تراکنش توسط درگاه";
                            await _sharedRuntimeData.DeleteDataWithRollBack(transaction.TransactionId);
                            return RedirectToAction("PaymentError", "Payment");

                        }
                    }
                    else //when response is null
                    {
                        TempData["Psp"] = "Saman";
                        TempData["PaymentErrorMessage"] = "پاسخی از درگاه برای تایید دریافت نشدودرصورت کسر موجودی تا 72 ساعت به حساب شما بازگشته میشود.";
                        await _sharedRuntimeData.DeleteDataWithRollBack(transaction.TransactionId);
                        return RedirectToAction("PaymentError", "Payment");
                    }
                }
                else //initialData.State is not ok
                {
                    TempData["Psp"] = "Saman";
                    TempData["PaymentErrorMessage"] = "خطای درگاه بعد از پرداخت";
                    await _sharedRuntimeData.DeleteDataWithRollBack(transaction.TransactionId);
                    return RedirectToAction("PaymentError", "Payment");
                }
            }
            catch (Exception ex)
            {
                transaction.BasicData.Stage = Enums.PaymentStage.Failed;
                await _transactionRepository.UpdateTransaction(transaction);
                TempData["Psp"] = "Saman";
                TempData["PaymentErrorMessage"] = "خطایی پس از انتقال از درگاه بوجود آمده است";
                await _sharedRuntimeData.DeleteDataWithRollBack(transaction.TransactionId);
                return RedirectToAction("PaymentError", "Payment");
            }
        }
          
    }
}



    

