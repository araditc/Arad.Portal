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

namespace Arad.Portal.UI.Shop.Controllers
{
    public class SamanController : BaseController
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly HttpClientHelper _httpClientHelper;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _accessor;
        private readonly IDomainRepository _domainRepository;
        public SamanController(ITransactionRepository transactionRepository,
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
        public async Task<IActionResult> HandlePayRequest(string identifierToken)
        {
            Transaction transaction = _transactionRepository.FetchByIdentifierToken(identifierToken);
            if (transaction == null || transaction.BasicData.Stage != Enums.PaymentStage.Initialized)
            {
                TempData["Psp"] = "Saman";
                TempData["PaymentErrorMessage"] = "تراکنش مورد نظر یافت نشد.";
                return RedirectToAction("PaymentError", "Ipg");
            }

            var userName = "";
            var password = "";
            var merchantId = "";


            var domainName = base.DomainName;
            var domainEntity = _domainRepository.FetchDomainByName(domainName);
            
            try
            {
                SamanModel samanModel = null;
                var samanData =
                   domainEntity.DomainPaymentProviders.FirstOrDefault(_ => _.PspType == Enums.PspType.Saman);
               
                samanModel = JsonConvert.DeserializeObject<SamanModel>(samanData.DomainValueProvider);
                userName = samanModel.UserName;
                password = samanModel.Password;
                merchantId = samanModel.MerchantId;
               
            }
            catch (Exception e)
            {
                TempData["Psp"] = "Saman";
                TempData["PaymentErrorMessage"] = "پارامترهای درگاه پرداخت سامان یافت نشد.";
                return RedirectToAction("PaymentError", "Ipg");
            }


            var client = new TechnoIPGWSClient();

            string invoiceNumber = DateTime.Now.Ticks.ToString();
            string paymentId = "";

            var loginParam = new LoginParam()
            {
                UserName = userName,
                Password = password
            };

            transaction.EventsData.Add(new EventData()
            {
                ActionDataJson = JsonConvert.SerializeObject(loginParam),
                ActionDateTime = DateTime.Now,
                ActionType = PspActions.ClientRequestToLogin,
                ActionDescription = PspActions.ClientRequestToLogin.GetDescription()
            });

            await _paymentRepository.UpdateTransaction(transaction);

            try
            {

                MerchantLoginResponse loginResult = await client.MerchantLoginAsync(new MerchantLoginRequest(loginParam));

                var error = new SamanDtoResultApi()
                {
                    IsSuccess = false,
                    Message = Enums.ResponseCodes.CanNotConnectToPG.GetDescription(),
                    StatusCode = (int)Enums.ResponseCodes.CanNotConnectToPG
                };

                transaction.EventsData.Add(new EventData()
                {
                    ActionDataJson = JsonConvert.SerializeObject(loginResult?.@return),
                    ActionDateTime = DateTime.Now,
                    ActionType = PspActions.PspLoginResponse,
                    ActionDescription = PspActions.PspLoginResponse.GetDescription()
                });


                if (loginResult?.@return.Result == "erSucceed" &&
                    !string.IsNullOrEmpty(loginResult.@return.SessionId))
                {


                    var list = new List<EApportionmentAccount>();

                    foreach (var item in transaction.ApportionData.ProductApportions)
                    {
                        EApportionmentAccount info = new EApportionmentAccount()
                        {
                            Amount = Convert.ToDecimal(item.Apportion),
                            AccountIBAN = item.Iban,
                            AmountSpecified = true,
                            ApportionmentAccountType = item.IsMain ? enApportionmentAccountType.enMain : enApportionmentAccountType.enOther,
                            ApportionmentAccountTypeSpecified = true,
                            SettelmentPayID = item.PaymentIdentifier
                        };

                        list.Add(info);
                    }

                    list.Add(new EApportionmentAccount()
                    {
                        Amount = Convert.ToDecimal(transaction.ApportionData.CommissionApportion.Apportion),
                        AccountIBAN = transaction.ApportionData.CommissionApportion.Iban,
                        AmountSpecified = true,
                        ApportionmentAccountType = enApportionmentAccountType.enOther,
                        ApportionmentAccountTypeSpecified = true,
                        SettelmentPayID = "000000000000000000000000000000"
                    });

                    var param = new RequestParam
                    {
                        IsGovermentPay = true,
                        IsGovermentPaySpecified = true,
                        TransTypeSpecified = true,
                        AmountSpecified = true,
                        WSContext =
                            new WSContext()
                            {
                                UserId = userName,
                                Password = password,
                                SessionId = loginResult.@return.SessionId
                            },
                        ReserveNum = invoiceNumber,
                        TerminalId = terminalId,
                        Amount = list.Sum(x => x.Amount),
                        RedirectUrl = $"{Request.Scheme}://{Request.Host}/saman/verify",
                        MerchantId = merchantId,
                        ApportionmentAccountList = list.ToArray(),
                        TransType = enTransType.enGoods
                    };

                    transaction.EventsData.Add(new EventData()
                    {
                        ActionDataJson = JsonConvert.SerializeObject(param),
                        ActionDateTime = DateTime.Now,
                        ActionType = PspActions.ClientRequestGenerateTransactionDataToSign,
                        ActionDescription = PspActions.ClientRequestGenerateTransactionDataToSign.GetDescription()
                    });

                    await _paymentRepository.UpdateTransaction(transaction);

                    GenerateTransactionDataToSignResponse generateTransactionDataToSignResponse =
                        await client.GenerateTransactionDataToSignAsync(new GenerateTransactionDataToSignRequest(param));

                    transaction.EventsData.Add(new EventData()
                    {
                        ActionDataJson = JsonConvert.SerializeObject(generateTransactionDataToSignResponse?.@return),
                        ActionDateTime = DateTime.Now,
                        ActionType = PspActions.PspResponseGenerateTransactionDataToSign,
                        ActionDescription = PspActions.PspResponseGenerateTransactionDataToSign.GetDescription()
                    });

                    await _paymentRepository.UpdateTransaction(transaction);

                    if (!string.IsNullOrEmpty(generateTransactionDataToSignResponse?.@return.Result))
                    {
                        var generateSignedDataTokenParam = new GenerateSignedDataTokenParam()
                        {
                            UniqueId = generateTransactionDataToSignResponse.@return.UniqueId,
                            WSContext = new WSContext()
                            {
                                UserId = userName,
                                Password = password,
                                SessionId = loginResult.@return.SessionId
                            },

                            Signature = generateTransactionDataToSignResponse.@return.DataToSign
                        };

                        transaction.EventsData.Add(new EventData()
                        {
                            ActionDataJson = JsonConvert.SerializeObject(generateSignedDataTokenParam),
                            ActionDateTime = DateTime.Now,
                            ActionType = PspActions.ClientTokenRequest,
                            ActionDescription = PspActions.ClientTokenRequest.GetDescription()
                        });

                        await _paymentRepository.UpdateTransaction(transaction);

                        GenerateSignedDataTokenResponse generateSignedDataTokenResult =
                            await client.GenerateSignedDataTokenAsync(new GenerateSignedDataTokenRequest(generateSignedDataTokenParam));

                        transaction.EventsData.Add(new EventData()
                        {
                            ActionDataJson = JsonConvert.SerializeObject(generateSignedDataTokenResult?.@return),
                            ActionDateTime = DateTime.Now,
                            ActionType = PspActions.PspTokenResponse,
                            ActionDescription = PspActions.PspTokenResponse.GetDescription()
                        });

                        await _paymentRepository.UpdateTransaction(transaction);

                        if (generateSignedDataTokenResult?.@return.Result == "erSucceed" &&
                            !string.IsNullOrEmpty(generateSignedDataTokenResult.@return.Token))
                        {
                            transaction.BasicData.InvoiceNumber = invoiceNumber;
                            transaction.BasicData.Stage = Enums.PaymentStage.GenerateToken;

                            await _paymentRepository.UpdateTransaction(transaction);

                            return View("SamanPay", generateSignedDataTokenResult.@return.Token);
                        }
                    }
                }

                TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
                TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";
                return RedirectToAction("PaymentError", "Ipg");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Verify([FromForm] SamanResult result)
        {
            try
            {
                if (result.State.Equals("OK"))
                {
                    var transaction = _paymentRepository.GetTransactionByInvoiceNumber(result.ResNum);

                    if (transaction == null)
                    {
                        TempData["Psp"] = "Saman";
                        TempData["PaymentErrorMessage"] = "خطای تایید تراکنش : تراکنش یافت نشد.";
                        return RedirectToAction("PaymentError", "Ipg");
                    }

                    transaction.EventsData.Add(new EventData()
                    {
                        ActionDataJson = JsonConvert.SerializeObject(result),
                        ActionDateTime = DateTime.Now,
                        ActionType = PspActions.PspSendCallback,
                        ActionDescription = PspActions.PspSendCallback.GetDescription()
                    });

                    await _paymentRepository.UpdateTransaction(transaction);

                    var terId = transaction.BasicData.TerminalId;
                    var ter = _bankRepository.GetTerminalById(terId);
                    var provider = ter.ServiceProviders.FirstOrDefault(c => c.Type == transaction.BasicData.PspType);

                    if (provider.Type != Enums.PSPType.Saman)
                    {
                        TempData["Psp"] = "Saman";
                        TempData["PaymentErrorMessage"] = "خطای تایید تراکنش : تراکنش مربوط به درگاه سامان نمی باشد.";
                        return RedirectToAction("PaymentError", "Ipg");
                    }

                    var userName = provider.Parameters.FirstOrDefault(c => c.Key == "Username").Value;
                    var password = provider.Parameters.FirstOrDefault(c => c.Key == "Password").Value;

                    var originalAmount = $"{Convert.ToDecimal(transaction.ApportionData.TotalAmountPaid)}";

                    var client = new TechnoIPGWSClient();
                    var param = new VerifyMerchantTransParam()
                    {
                        RefNum = result.RefNum,
                        Token = result.token,
                        WSContext = new WSContext()
                        {
                            Password = password,
                            UserId = userName
                        }
                    };

                    if (transaction.BasicData.Stage == Enums.PaymentStage.DoneAndConfirmed)
                    {
                        TempData["Psp"] = "Saman";
                        TempData["PaymentErrorMessage"] = "خطای تایید تراکنش : تراکنش قبلا تایید شده است.";
                        return RedirectToAction("PaymentError", "Ipg");
                    }

                    transaction.BasicData.ReferenceId = result.CustomerRefNum;
                    transaction.BasicData.Stage = Enums.PaymentStage.DoneButNotConfirmed;
                    await _paymentRepository.UpdateTransaction(transaction);

                    transaction.EventsData.Add(new EventData()
                    {
                        ActionDataJson = JsonConvert.SerializeObject(param),
                        ActionDateTime = DateTime.Now,
                        ActionType = PspActions.ClientVerifyRequest,
                        ActionDescription = PspActions.ClientVerifyRequest.GetDescription()
                    });

                    await _paymentRepository.UpdateTransaction(transaction);


                    var verifyMerchantTrans = await client.VerifyMerchantTransAsync(new VerifyMerchantTransRequest(param));
                    DateTime callVerify = DateTime.Now;

                    transaction.EventsData.Add(new EventData()
                    {
                        ActionDataJson = JsonConvert.SerializeObject(verifyMerchantTrans?.@return),
                        ActionDateTime = callVerify,
                        ActionType = PspActions.PspVerifyResponse,
                        ActionDescription = PspActions.PspVerifyResponse.GetDescription()
                    });

                    await _paymentRepository.UpdateTransaction(transaction);

                    if (result.transactionAmount == originalAmount)
                    {
                        if (verifyMerchantTrans?.@return.Result == "erSucceed")
                        {
                            try
                            {
                                //sendSms
                                transaction.UserData.PayVerifySmsIsSent = false;

                            }
                            catch (Exception e)
                            {
                                transaction.UserData.PayVerifySmsIsSent = false;
                            }

                            transaction.BasicData.Stage = Enums.PaymentStage.DoneAndConfirmed;
                            await _paymentRepository.UpdateTransaction(transaction);

                            var order = _paymentRepository.GetOrderByCompleteNumber(transaction.BasicData.OrderId);

                            var shopParameter = order?.ShopParameters.FirstOrDefault(x =>
                                x.UniqueId == transaction.BasicData.UniqueIdInOrder);

                            if (shopParameter is not null)
                            {
                                shopParameter.IsPaid = true;
                                shopParameter.PaymentReferenceNumber = transaction.BasicData.ReferenceId;

                                await _paymentRepository.UpdateOrder(order);
                            }

                            TempData["Psp"] = "Saman";
                            TempData["ReferenceNumber"] = result.CustomerRefNum;
                            TempData["InvoiceNumber"] = result.ResNum;
                            TempData["OrderId"] = transaction.BasicData.OrderId;
                            return RedirectToAction("PaymentSuccess", "Ipg");
                        }

                        transaction.BasicData.Stage = Enums.PaymentStage.Failed;
                        await _paymentRepository.UpdateTransaction(transaction);

                        TempData["Psp"] = "Saman";
                        TempData["PaymentErrorMessage"] = "خطای تایید تراکنش : تراکنش تایید نشد.";
                        return RedirectToAction("PaymentError", "Ipg");
                    }

                    ReverseMerchantTransParam cancelTransParam = new ReverseMerchantTransParam()
                    {
                        Token = result.token,
                        RefNum = result.RefNum,
                        WSContext = new WSContext()
                        {
                            Password = userName,
                            UserId = password
                        }
                    };

                    transaction.EventsData.Add(new EventData()
                    {
                        ActionDataJson = JsonConvert.SerializeObject(cancelTransParam),
                        ActionDateTime = callVerify,
                        ActionType = PspActions.ClientRequestReverseTransaction,
                        ActionDescription = PspActions.ClientRequestReverseTransaction.GetDescription()
                    });

                    await _paymentRepository.UpdateTransaction(transaction);
                    var cancelTransactionResponse = await client.ReverseMerchantTransAsync(new ReverseMerchantTransRequest(cancelTransParam));

                    transaction.EventsData.Add(new EventData()
                    {
                        ActionDataJson = JsonConvert.SerializeObject(cancelTransactionResponse?.@return),
                        ActionDateTime = callVerify,
                        ActionType = PspActions.PspResponseReverseTransaction,
                        ActionDescription = PspActions.PspResponseReverseTransaction.GetDescription()
                    });

                    await _paymentRepository.UpdateTransaction(transaction);

                    Log.Fatal($"Error in payment before verification. invoice number :{result.ResNum}, state:{result.State}");
                    TempData["Psp"] = "Saman";
                    TempData["PaymentErrorMessage"] = "خطای درگاه بعد از پرداخت.";
                    return RedirectToAction("PaymentError", "Ipg");
                }
            }
            catch (Exception e)
            {
                //error in payment
                Log.Fatal($"Error in payment before verification. invoice number :{result.ResNum}, state:{result.State}");
                TempData["Psp"] = "Saman";
                TempData["PaymentErrorMessage"] = "خطای درگاه بعد از پرداخت.";
                return RedirectToAction("PaymentError", "Ipg");
            }

            Log.Fatal($"Error in payment before verification. invoice number :{result.ResNum}, state:{result.State}");
            TempData["Psp"] = "Saman";
            TempData["PaymentErrorMessage"] = "خطای درگاه بعد از پرداخت.";
            return RedirectToAction("PaymentError", "Ipg");
        }
    }
}


//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Net.Mime;
//using System.Text;
//using System.Threading.Tasks;
//using Arad.PaymentSystem.Dashboard.Entities;
//using Arad.PaymentSystem.Dashboard.Helpers;
//using Arad.paymentSystem.Dashboard.Models;
//using Arad.PaymentSystem.Dashboard.Models.Saman;
//using Arad.PaymentSystem.Dashboard.Repositories.Bank;
//using Arad.PaymentSystem.Dashboard.Repositories.Payment;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc.Formatters;
//using Microsoft.Extensions.Configuration;
//using Newtonsoft.Json;
//using Serilog;

//namespace Arad.PaymentSystem.Dashboard.Controllers
//{
//    [Authorize(Policy = "Role")]

//    public class SamanController : Controller
//    {
//        private readonly IPaymentRepository _paymentRepository;
//        private readonly IBankRepository _bankRepository;
//        private readonly HttpClientHelper _httpClientHelper;
//        private readonly IConfiguration _configuration;
//        private readonly string _samanPspBasAddress;
//        private readonly string _samanVerifyEndPoint;
//        public SamanController(IPaymentRepository paymentRepository, IBankRepository bankRepository, HttpClientHelper httpClientHelper, IConfiguration configuration)
//        {
//            _paymentRepository = paymentRepository;
//            _bankRepository = bankRepository;
//            _httpClientHelper = httpClientHelper;
//            _configuration = configuration;
//            _samanPspBasAddress = _configuration["Ipg:Saman:BaseAddress"];
//            _samanVerifyEndPoint = _configuration["Ipg:Saman:VerifyEndPoint"];
//        }

//        [HttpGet]
//        public async Task<IActionResult> HandlePayRequest(string identifierToken)
//        {
//            Transaction transaction = _paymentRepository.GetTransactionByIdentifierToken(identifierToken);
//            if (transaction == null || transaction.BasicData.Stage != Enums.PaymentStage.Initialized)
//            {
//                TempData["Psp"] = "Saman";
//                TempData["PaymentErrorMessage"] = "تراکنش مورد نظر یافت نشد.";
//                return RedirectToAction("PaymentError", "Ipg");
//            }

//            List<IBANInfo> ibans = new List<IBANInfo>();
//            foreach (var item in transaction.ApportionData.ProductApportions)
//            {
//                IBANInfo info = new IBANInfo()
//                {
//                    Amount = Convert.ToInt64(item.Apportion),
//                    IBAN = item.Iban,
//                    PurchaseID = item.PaymentIdentifier
//                };

//                ibans.Add(info);
//            }

//            ibans.Add(new IBANInfo()
//            {
//                Amount = Convert.ToInt64(transaction.ApportionData.CommissionApportion.Apportion),
//                IBAN = transaction.ApportionData.CommissionApportion.Iban,
//                PurchaseID = transaction.ApportionData.CommissionApportion.PaymentIdentifier
//            });

//            var terminal = _bankRepository.GetTerminalById(transaction.BasicData.TerminalId);
//            if (terminal is null)
//            {
//                TempData["Psp"] = "Saman";
//                TempData["PaymentErrorMessage"] = "ترمینال پرداخت یافت نشد.";
//                return RedirectToAction("PaymentError", "Ipg");
//            }

//            var samanData = terminal.ServiceProviders.FirstOrDefault(c => c.Type == Enums.PSPType.Saman);
//            if (samanData is null)
//            {
//                TempData["Psp"] = "Saman";
//                TempData["PaymentErrorMessage"] = "پارامترهای درگاه پرداخت سامان یافت نشد.";
//                return RedirectToAction("PaymentError", "Ipg");
//            }

//            var terminalIdData = samanData.Parameters.FirstOrDefault(c => c.Key == "Terminal Id");
//            if (terminalIdData is null)
//            {
//                TempData["Psp"] = "Saman";
//                TempData["PaymentErrorMessage"] = "شماره ترمینال درگاه یافت نشد.";
//                return RedirectToAction("PaymentError", "Ipg");
//            }

//            var client = _httpClientHelper.GetClient();
//            var baseAddress = _configuration["Ipg:Saman:BaseAddress"];
//            var tokenEndPoint = _configuration["Ipg:Saman:TokenEndPoint"];
//            var gatewayEndpoint = _configuration["Ipg:Saman:GatewayEndPoint"];
//            var tokenReq = new TokenRequest()
//            {
//                Amount = ibans.Sum(x => x.Amount),
//                Action = "token",
//                RedirectUrl = $"{Request.Scheme}://{Request.Host}/Saman/Verify",
//                ResNum = DateTime.Now.Ticks.ToString(),
//                SettlementIBANInfo = ibans.ToArray(),
//                TerminalId = terminalIdData.Value
//            };

//            var serializedObj = JsonConvert.SerializeObject(tokenReq);
//            var content = new StringContent(serializedObj, Encoding.UTF8, MediaTypeNames.Application.Json);
//            client.BaseAddress = new Uri(baseAddress);

//            transaction.EventsData.Add(new EventData()
//            {
//                ActionDataJson = serializedObj,
//                ActionDateTime = DateTime.Now,
//                ActionType = PspActions.ClientTokenRequest,
//                ActionDescription = PspActions.ClientTokenRequest.GetDescription()
//            });

//            var response = await client.PostAsync(tokenEndPoint, content);
//            var tokenResponseTime = DateTime.Now;
//            var serializedTokenResponse = await response.Content.ReadAsStringAsync();

//            transaction.EventsData.Add(new EventData()
//            {
//                ActionDataJson = serializedTokenResponse,
//                ActionDateTime = tokenResponseTime,
//                ActionType = PspActions.PspTokenResponse,
//                ActionDescription = PspActions.PspTokenResponse.GetDescription()
//            });



//            if (response.IsSuccessStatusCode)
//            {
//                try
//                {
//                    var tokenResponse =
//                        JsonConvert.DeserializeObject<TokenResponse>(serializedTokenResponse);

//                    if (tokenResponse.Status == 1)
//                    {
//                        transaction.BasicData.Stage = Enums.PaymentStage.RedirectToIPG;
//                        transaction.BasicData.InvoiceNumber = tokenReq.ResNum;
//                        await _paymentRepository.UpdateTransaction(transaction);

//                        return Redirect(Flurl.Url.Combine(baseAddress, gatewayEndpoint) + "?token=" +
//                                        tokenResponse.Token);
//                    }

//                    await _paymentRepository.UpdateTransaction(transaction);
//                    Log.Error($"token error desc : {tokenResponse.ErrorDesc}, errorCode: {tokenResponse.ErrorCode}");
//                    TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
//                    TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";
//                    return RedirectToAction("PaymentError", "Ipg");
//                }
//                catch (Exception e)
//                {
//                    await _paymentRepository.UpdateTransaction(transaction);

//                    Log.Error($"overal error : {e.Message}");
//                    TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
//                    TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";
//                    return RedirectToAction("PaymentError", "Ipg");
//                }
//            }

//            await _paymentRepository.UpdateTransaction(transaction);

//            Log.Error($"token error statusCode : {response.StatusCode}");
//            TempData["Psp"] = transaction.BasicData.PspType.GetDescription();
//            TempData["PaymentErrorMessage"] = "خطا در اتصال به درگاه.";
//            return RedirectToAction("PaymentError", "Ipg");
//        }

//        [HttpPost]
//        [AllowAnonymous]
//        public async Task<IActionResult> Verify()
//        {
//            var callbackTime = DateTime.Now;
//            var initialData = new IpgOutputModel()
//            {
//                Amount = Request.Form["Amount"].ToString(),
//                RefNum = Request.Form["RefNum"].ToString(),
//                State = Request.Form["State"].ToString(),
//                HashedCardNumber = Request.Form["HashedCardNumber"].ToString(),
//                MID = Request.Form["MID"].ToString(),
//                ResNum = Request.Form["ResNum"].ToString(),
//                SecurePan = Request.Form["SecurePan"].ToString(),
//                Status = Request.Form["Status"].ToString(),
//                TerminalId = Request.Form["TerminalId"].ToString(),
//                Wage = Request.Form["Wage"].ToString()
//            };

//            var transaction = _paymentRepository.GetTransactionByInvoiceNumber(initialData.ResNum);
//            if (transaction == null)
//            {
//                TempData["Psp"] = "Saman";
//                TempData["PaymentErrorMessage"] = "تراکنش مورد نظر یافت نشد.";
//                return RedirectToAction("PaymentError", "Ipg");
//            }

//            transaction.EventsData.Add(new EventData()
//            {
//                ActionDateTime = callbackTime,
//                ActionDescription = PspActions.PspSendCallback.GetDescription(),
//                ActionType = PspActions.PspSendCallback,
//                ActionDataJson = JsonConvert.SerializeObject(initialData)
//            });

//            if (initialData.Status == "2" && initialData.State == "OK")
//            {
//                if (transaction.BasicData.Stage != Enums.PaymentStage.RedirectToIPG)
//                {
//                    TempData["Psp"] = "Saman";
//                    TempData["PaymentErrorMessage"] = "تراکنش تکراری.";
//                    return RedirectToAction("PaymentError", "Ipg");
//                }


//                transaction.BasicData.Stage = Enums.PaymentStage.DoneButNotConfirmed;
//                await _paymentRepository.UpdateTransaction(transaction);

//                var baseAddress = _samanPspBasAddress;
//                var verifyEndPoint = _samanVerifyEndPoint;

//                var inputModel = new IpgInputModel()
//                {
//                    RefNum = initialData.RefNum,
//                    TerminalNumber = Convert.ToInt32(initialData.TerminalId)
//                };

//                var serializedInput = JsonConvert.SerializeObject(inputModel);
//                var inputContent = new StringContent(serializedInput, Encoding.UTF8, MediaTypeNames.Application.Json);

//                transaction.EventsData.Add(new EventData()
//                {
//                    ActionDataJson = serializedInput,
//                    ActionDateTime = DateTime.Now,
//                    ActionDescription = PspActions.ClientVerifyRequest.GetDescription(),
//                    ActionType = PspActions.ClientVerifyRequest
//                });

//                var client = _httpClientHelper.GetClient();
//                client.BaseAddress = new Uri(baseAddress);
//                var response = await client.PostAsync(verifyEndPoint, inputContent);
//                var stringResponse = await response.Content.ReadAsStringAsync();

//                transaction.EventsData.Add(new EventData()
//                {
//                    ActionDataJson = stringResponse,
//                    ActionDateTime = DateTime.Now,
//                    ActionDescription = PspActions.PspVerifyResponse.GetDescription(),
//                    ActionType = PspActions.PspVerifyResponse
//                });

//                if (response.IsSuccessStatusCode)
//                {
//                    try
//                    {
//                        var verifyResponse =
//                            JsonConvert.DeserializeObject<IpgVerifyOutputModel>(stringResponse);
//                        var paid = Convert.ToInt32(transaction.ApportionData.TotalAmountPaid);

//                        if (verifyResponse.Success && verifyResponse.ResultCode >= 0 && verifyResponse.TransactionDetail.OrginalAmount == paid)
//                        {

//                            transaction.BasicData.Stage = Enums.PaymentStage.DoneAndConfirmed;
//                            transaction.BasicData.ReferenceId = verifyResponse.TransactionDetail.RefNum;

//                            await _paymentRepository.UpdateTransaction(transaction);

//                            var order = _paymentRepository.GetOrderByUniqueId(transaction.BasicData.UniqueIdInOrder);
//                            order.ShopParameters.FirstOrDefault(c => c.UniqueId == transaction.BasicData.UniqueIdInOrder)
//                                .IsPaid = true;

//                            await _paymentRepository.UpdateOrder(order);

//                            TempData["ReferenceNumber"] = verifyResponse.TransactionDetail.RefNum;
//                            TempData["Psp"] = "Saman";
//                            TempData["InvoiceNumber"] = initialData.ResNum;
//                            TempData["OrderId"] = order.OrderId;

//                            return RedirectToAction("PaymentSuccess", "Ipg");
//                        }
//                        else
//                        {
//                            reversing transaction.

//                            transaction.BasicData.Stage = Enums.PaymentStage.Failed;
//                            transaction.BasicData.ReferenceId = verifyResponse.TransactionDetail.RefNum;

//                            transaction.EventsData.Add(new EventData()
//                            {
//                                ActionDateTime = DateTime.Now,
//                                ActionDataJson = serializedInput,
//                                ActionType = PspActions.ClientRequestReverseTransaction,
//                                ActionDescription = PspActions.ClientRequestReverseTransaction.GetDescription()
//                            });

//                            await _paymentRepository.UpdateTransaction(transaction);

//                            await client.PostAsync(_configuration["Ipg:Saman:ReverseEndPoint"], inputContent);

//                            TempData["Psp"] = "Saman";
//                            TempData["PaymentErrorMessage"] = "خطا در تایید تراکنش. تراکنش تایید نشد.";
//                            return RedirectToAction("PaymentError", "Ipg");
//                        }
//                    }
//                    catch (Exception e)
//                    {
//                        await _paymentRepository.UpdateTransaction(transaction);
//                    }
//                }
//            }

//            //error in payment
//            transaction.BasicData.Stage = Enums.PaymentStage.Failed;
//            await _paymentRepository.UpdateTransaction(transaction);

//            TempData["Psp"] = "Saman";
//            TempData["PaymentErrorMessage"] = "خطای درگاه بعد از پرداخت.";
//            return RedirectToAction("PaymentError", "Ipg");
//        }

//    }
//}
