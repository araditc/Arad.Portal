using Arad.Portal.DataLayer.Contracts.Shop.Transaction;
using Arad.Portal.UI.Shop.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Arad.Portal.DataLayer.Entities.Shop.Transaction;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Newtonsoft.Json;
using Arad.Portal.DataLayer.Models.Transaction;
using System.Net.Http;
using System.Text;
using System.Net.Mime;
using Arad.Portal.DataLayer.Models.PSPs.Saman;
using Microsoft.AspNetCore.Authorization;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class AAAController : BaseController
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly HttpClientHelper _httpClientHelper;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _accessor;
        private readonly IDomainRepository _domainRepository;
        public AAAController(ITransactionRepository transactionRepository,
            IHttpContextAccessor accessor,
            IDomainRepository domainRepository,
            HttpClientHelper httpClientHelper, IConfiguration configuration):base(accessor)
        {
            _domainRepository = domainRepository;
            MethodInfo method = typeof(XmlSerializer)
                .GetMethod("set_Mode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            method.Invoke(null, new object[] { 1 });

            _transactionRepository = transactionRepository;
            _httpClientHelper = httpClientHelper;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetToken(string reservationNumber)
        {
            Transaction transaction = _transactionRepository.FetchByIdentifierToken(reservationNumber);
            if (transaction == null || transaction.BasicData.Stage != Enums.PaymentStage.Initialized)
            {
                TempData["Psp"] = "Saman";
                TempData["PaymentErrorMessage"] = "تراکنش مورد نظر یافت نشد.";
                return RedirectToAction("PaymentError", "Ipg");
            }

            var senderModel = new GetTokenRequestModel() 
            {
                ResNum = reservationNumber,
                Action = "token",
                TotalAmount = transaction.FinalPriceToPay,
                RedirectURL = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/Saman/Verify"
            };
           
            var domainName = base.DomainName;
            var domainEntity = _domainRepository.FetchDomainByName(domainName);
            SamanModel samanModel = null;
            try
            {
                
                var samanData =
                   domainEntity.DomainPaymentProviders.FirstOrDefault(_ => _.PspType == Enums.PspType.Saman);

                samanModel = JsonConvert.DeserializeObject<SamanModel>(samanData.DomainValueProvider);
                senderModel.TerminalId = samanModel.TerminalId;
            }
            catch (Exception e)
            {
                TempData["Psp"] = "Saman";
                TempData["PaymentErrorMessage"] = "پارامترهای درگاه پرداخت سامان یافت نشد.";
                return RedirectToAction("PaymentError", "Ipg");
            }


            var httpClient = _httpClientHelper.GetClient();
            var serializedObj = JsonConvert.SerializeObject(senderModel);
            var content = new StringContent(serializedObj, Encoding.UTF8, MediaTypeNames.Application.Json);
            httpClient.BaseAddress = new Uri(samanModel.BaseAddress);

            transaction.EventsData.Add(new EventData()
            {
                JsonContent = serializedObj,
                ActionDateTime = DateTime.Now,
                ActionType = PspActions.ClientTokenRequest,
                additionalData = PspActions.ClientTokenRequest.GetDescription()
            });

            var response = await httpClient.PostAsync(samanModel.TokenEndPoint, content);
            var tokenResponseTime = DateTime.Now;
            var serializedTokenResponse = await response.Content.ReadAsStringAsync();

            transaction.EventsData.Add(new EventData()
            {
                JsonContent = serializedTokenResponse,
                ActionDateTime = tokenResponseTime,
                ActionType = PspActions.PspTokenResponse,
                additionalData = PspActions.PspTokenResponse.GetDescription()
            });
            await _transactionRepository.UpdateTransaction(transaction);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var tokenResponse =
                        JsonConvert.DeserializeObject<GetTokenResponseModel>(serializedTokenResponse);

                    if (tokenResponse.Status == 1)
                    {
                        transaction.BasicData.Stage = Enums.PaymentStage.RedirectToIPG;
                        
                        await _transactionRepository.UpdateTransaction(transaction);

                        return Redirect(System.IO.Path.Combine(samanModel.BaseAddress, samanModel.GatewayEndPoint) + "?token=" +
                                        tokenResponse.Token);
                    }
                    else
                    {
                        transaction.EventsData.FirstOrDefault(_ => _.ActionType == PspActions.PspTokenResponse).additionalData =
                        $"token error desc : {tokenResponse.ErrorDesc}, errorCode: {tokenResponse.ErrorCode}";
                        await _transactionRepository.UpdateTransaction(transaction);
                        //Log.Error($"token error desc : {tokenResponse.ErrorDesc}, errorCode: {tokenResponse.ErrorCode}");
                        TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
                        TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";
                        return RedirectToAction("PaymentError", "Ipg");
                    }
                    
                }
                catch (Exception e)
                {
                    await _transactionRepository.UpdateTransaction(transaction);
                    //Log.Error($"overal error : {e.Message}");
                    TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
                    TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";
                    return RedirectToAction("PaymentError", "Ipg");
                }
            }
            //Log.Error($"token error statusCode : {response.StatusCode}");
            TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
            TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";
            return RedirectToAction("PaymentError", "Ipg");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Verify()
        {
            var callbackTime = DateTime.Now;
            var initialData = new IpgOutputModel()
            {
                Amount = Request.Form["Amount"].ToString(),
                RefNum = Request.Form["RefNum"].ToString(),
                State = Request.Form["State"].ToString(),
                HashedCardNumber = Request.Form["HashedCardNumber"].ToString(),
                MID = Request.Form["MID"].ToString(),
                ResNum = Request.Form["ResNum"].ToString(),
                SecurePan = Request.Form["SecurePan"].ToString(),
                Status = Request.Form["Status"].ToString(),
                TerminalId = Request.Form["TerminalId"].ToString(),
                Wage = Request.Form["Wage"].ToString()
            };

            var transaction = _paymentRepository.GetTransactionByInvoiceNumber(initialData.ResNum);
            if (transaction == null)
            {
                TempData["Psp"] = "Saman";
                TempData["PaymentErrorMessage"] = "تراکنش مورد نظر یافت نشد.";
                return RedirectToAction("PaymentError", "Ipg");
            }

            transaction.EventsData.Add(new EventData()
            {
                ActionDateTime = callbackTime,
                ActionDescription = PspActions.PspSendCallback.GetDescription(),
                ActionType = PspActions.PspSendCallback,
                ActionDataJson = JsonConvert.SerializeObject(initialData)
            });

            if (initialData.Status == "2" && initialData.State == "OK")
            {
                if (transaction.BasicData.Stage != Enums.PaymentStage.RedirectToIPG)
                {
                    TempData["Psp"] = "Saman";
                    TempData["PaymentErrorMessage"] = "تراکنش تکراری.";
                    return RedirectToAction("PaymentError", "Ipg");
                }


                transaction.BasicData.Stage = Enums.PaymentStage.DoneButNotConfirmed;
                await _paymentRepository.UpdateTransaction(transaction);

                var baseAddress = _samanPspBasAddress;
                var verifyEndPoint = _samanVerifyEndPoint;

                var inputModel = new IpgInputModel()
                {
                    RefNum = initialData.RefNum,
                    TerminalNumber = Convert.ToInt32(initialData.TerminalId)
                };

                var serializedInput = JsonConvert.SerializeObject(inputModel);
                var inputContent = new StringContent(serializedInput, Encoding.UTF8, MediaTypeNames.Application.Json);

                transaction.EventsData.Add(new EventData()
                {
                    ActionDataJson = serializedInput,
                    ActionDateTime = DateTime.Now,
                    ActionDescription = PspActions.ClientVerifyRequest.GetDescription(),
                    ActionType = PspActions.ClientVerifyRequest
                });

                var client = _httpClientHelper.GetClient();
                client.BaseAddress = new Uri(baseAddress);
                var response = await client.PostAsync(verifyEndPoint, inputContent);
                var stringResponse = await response.Content.ReadAsStringAsync();

                transaction.EventsData.Add(new EventData()
                {
                    ActionDataJson = stringResponse,
                    ActionDateTime = DateTime.Now,
                    ActionDescription = PspActions.PspVerifyResponse.GetDescription(),
                    ActionType = PspActions.PspVerifyResponse
                });

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var verifyResponse =
                            JsonConvert.DeserializeObject<IpgVerifyOutputModel>(stringResponse);
                        var paid = Convert.ToInt32(transaction.ApportionData.TotalAmountPaid);

                        if (verifyResponse.Success && verifyResponse.ResultCode >= 0 && verifyResponse.TransactionDetail.OrginalAmount == paid)
                        {

                            transaction.BasicData.Stage = Enums.PaymentStage.DoneAndConfirmed;
                            transaction.BasicData.ReferenceId = verifyResponse.TransactionDetail.RefNum;

                            await _paymentRepository.UpdateTransaction(transaction);

                            var order = _paymentRepository.GetOrderByUniqueId(transaction.BasicData.UniqueIdInOrder);
                            order.ShopParameters.FirstOrDefault(c => c.UniqueId == transaction.BasicData.UniqueIdInOrder)
                                .IsPaid = true;

                            await _paymentRepository.UpdateOrder(order);

                            TempData["ReferenceNumber"] = verifyResponse.TransactionDetail.RefNum;
                            TempData["Psp"] = "Saman";
                            TempData["InvoiceNumber"] = initialData.ResNum;
                            TempData["OrderId"] = order.OrderId;

                            return RedirectToAction("PaymentSuccess", "Ipg");
                        }
                        else
                        {
                            reversing transaction.

                            transaction.BasicData.Stage = Enums.PaymentStage.Failed;
                            transaction.BasicData.ReferenceId = verifyResponse.TransactionDetail.RefNum;

                            transaction.EventsData.Add(new EventData()
                            {
                                ActionDateTime = DateTime.Now,
                                ActionDataJson = serializedInput,
                                ActionType = PspActions.ClientRequestReverseTransaction,
                                ActionDescription = PspActions.ClientRequestReverseTransaction.GetDescription()
                            });

                            await _paymentRepository.UpdateTransaction(transaction);

                            await client.PostAsync(_configuration["Ipg:Saman:ReverseEndPoint"], inputContent);

                            TempData["Psp"] = "Saman";
                            TempData["PaymentErrorMessage"] = "خطا در تایید تراکنش. تراکنش تایید نشد.";
                            return RedirectToAction("PaymentError", "Ipg");
                        }
                    }
                    catch (Exception e)
                    {
                        await _paymentRepository.UpdateTransaction(transaction);
                    }
                }
            }

            //error in payment
            transaction.BasicData.Stage = Enums.PaymentStage.Failed;
            await _paymentRepository.UpdateTransaction(transaction);

            TempData["Psp"] = "Saman";
            TempData["PaymentErrorMessage"] = "خطای درگاه بعد از پرداخت.";
            return RedirectToAction("PaymentError", "Ipg");
        }



    }





    public class SamanController : Controller
        {
            //private readonly IPaymentRepository _paymentRepository;
            //private readonly IBankRepository _bankRepository;
            //private readonly HttpClientHelper _httpClientHelper;
            //private readonly IConfiguration _configuration;
            //private readonly string _samanPspBasAddress;
            //private readonly string _samanVerifyEndPoint;
            //public SamanController(IPaymentRepository paymentRepository, IBankRepository bankRepository, HttpClientHelper httpClientHelper, IConfiguration configuration)
            //{
            //    _paymentRepository = paymentRepository;
            //    _bankRepository = bankRepository;
            //    _httpClientHelper = httpClientHelper;
            //    _configuration = configuration;
            //    _samanPspBasAddress = _configuration["Ipg:Saman:BaseAddress"];
            //    _samanVerifyEndPoint = _configuration["Ipg:Saman:VerifyEndPoint"];
            //}

            //[HttpGet]
            //public async Task<IActionResult> HandlePayRequest(string identifierToken)
            //{
            //    Transaction transaction = _paymentRepository.GetTransactionByIdentifierToken(identifierToken);
            //    if (transaction == null || transaction.BasicData.Stage != Enums.PaymentStage.Initialized)
            //    {
            //        TempData["Psp"] = "Saman";
            //        TempData["PaymentErrorMessage"] = "تراکنش مورد نظر یافت نشد.";
            //        return RedirectToAction("PaymentError", "Ipg");
            //    }

            //    List<IBANInfo> ibans = new List<IBANInfo>();
            //    foreach (var item in transaction.ApportionData.ProductApportions)
            //    {
            //        IBANInfo info = new IBANInfo()
            //        {
            //            Amount = Convert.ToInt64(item.Apportion),
            //            IBAN = item.Iban,
            //            PurchaseID = item.PaymentIdentifier
            //        };

            //        ibans.Add(info);
            //    }

            //    ibans.Add(new IBANInfo()
            //    {
            //        Amount = Convert.ToInt64(transaction.ApportionData.CommissionApportion.Apportion),
            //        IBAN = transaction.ApportionData.CommissionApportion.Iban,
            //        PurchaseID = transaction.ApportionData.CommissionApportion.PaymentIdentifier
            //    });

            //    var terminal = _bankRepository.GetTerminalById(transaction.BasicData.TerminalId);
            //    if (terminal is null)
            //    {
            //        TempData["Psp"] = "Saman";
            //        TempData["PaymentErrorMessage"] = "ترمینال پرداخت یافت نشد.";
            //        return RedirectToAction("PaymentError", "Ipg");
            //    }

            //    var samanData = terminal.ServiceProviders.FirstOrDefault(c => c.Type == Enums.PSPType.Saman);
            //    if (samanData is null)
            //    {
            //        TempData["Psp"] = "Saman";
            //        TempData["PaymentErrorMessage"] = "پارامترهای درگاه پرداخت سامان یافت نشد.";
            //        return RedirectToAction("PaymentError", "Ipg");
            //    }

            //    var terminalIdData = samanData.Parameters.FirstOrDefault(c => c.Key == "Terminal Id");
            //    if (terminalIdData is null)
            //    {
            //        TempData["Psp"] = "Saman";
            //        TempData["PaymentErrorMessage"] = "شماره ترمینال درگاه یافت نشد.";
            //        return RedirectToAction("PaymentError", "Ipg");
            //    }

            //    var client = _httpClientHelper.GetClient();
            //    var baseAddress = _configuration["Ipg:Saman:BaseAddress"];
            //    var tokenEndPoint = _configuration["Ipg:Saman:TokenEndPoint"];
            //    var gatewayEndpoint = _configuration["Ipg:Saman:GatewayEndPoint"];
            //    var tokenReq = new TokenRequest()
            //    {
            //        Amount = ibans.Sum(x => x.Amount),
            //        Action = "token",
            //        RedirectUrl = $"{Request.Scheme}://{Request.Host}/Saman/Verify",
            //        ResNum = DateTime.Now.Ticks.ToString(),
            //        SettlementIBANInfo = ibans.ToArray(),
            //        TerminalId = terminalIdData.Value
            //    };

            //    var serializedObj = JsonConvert.SerializeObject(tokenReq);
            //    var content = new StringContent(serializedObj, Encoding.UTF8, MediaTypeNames.Application.Json);
            //    client.BaseAddress = new Uri(baseAddress);

            //    transaction.EventsData.Add(new EventData()
            //    {
            //        ActionDataJson = serializedObj,
            //        ActionDateTime = DateTime.Now,
            //        ActionType = PspActions.ClientTokenRequest,
            //        ActionDescription = PspActions.ClientTokenRequest.GetDescription()
            //    });

            //    var response = await client.PostAsync(tokenEndPoint, content);
            //    var tokenResponseTime = DateTime.Now;
            //    var serializedTokenResponse = await response.Content.ReadAsStringAsync();

            //    transaction.EventsData.Add(new EventData()
            //    {
            //        ActionDataJson = serializedTokenResponse,
            //        ActionDateTime = tokenResponseTime,
            //        ActionType = PspActions.PspTokenResponse,
            //        ActionDescription = PspActions.PspTokenResponse.GetDescription()
            //    });



            //    if (response.IsSuccessStatusCode)
            //    {
            //        try
            //        {
            //            var tokenResponse =
            //                JsonConvert.DeserializeObject<TokenResponse>(serializedTokenResponse);

            //            if (tokenResponse.Status == 1)
            //            {
            //                transaction.BasicData.Stage = Enums.PaymentStage.RedirectToIPG;
            //                transaction.BasicData.InvoiceNumber = tokenReq.ResNum;
            //                await _paymentRepository.UpdateTransaction(transaction);

            //                return Redirect(Flurl.Url.Combine(baseAddress, gatewayEndpoint) + "?token=" +
            //                                tokenResponse.Token);
            //            }

            //            await _paymentRepository.UpdateTransaction(transaction);
            //            Log.Error($"token error desc : {tokenResponse.ErrorDesc}, errorCode: {tokenResponse.ErrorCode}");
            //            TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
            //            TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";
            //            return RedirectToAction("PaymentError", "Ipg");
            //        }
            //        catch (Exception e)
            //        {
            //            await _paymentRepository.UpdateTransaction(transaction);

            //            Log.Error($"overal error : {e.Message}");
            //            TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
            //            TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";
            //            return RedirectToAction("PaymentError", "Ipg");
            //        }
            //    }

            //    await _paymentRepository.UpdateTransaction(transaction);

            //    Log.Error($"token error statusCode : {response.StatusCode}");
            //    TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
            //    TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";
            //    return RedirectToAction("PaymentError", "Ipg");
            //}

           
        }

