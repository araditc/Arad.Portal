﻿using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Contracts.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Contracts.Shop.Transaction;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.Transaction;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using static Arad.Portal.DataLayer.Models.Shared.Enums;
using System.Security.Claims;

namespace Arad.Portal.UI.Shop.Controllers
{
    [Authorize(Policy = "Role")]
    public class PaymentController : BaseController
    {
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly SharedRuntimeData _sharedRuntimeData;

        public PaymentController(IProductRepository productRepository, IHttpContextAccessor accessor,
            UserManager<ApplicationUser> userManager, ITransactionRepository transationRepository,
            SharedRuntimeData sharedRuntimeData,
            IWebHostEnvironment env,
            IMapper mapper, IShoppingCartRepository shoppingCartRepository):base(accessor, env)
        {  
            _productRepository = productRepository;
            _userManager = userManager;
            _transactionRepository = transationRepository;
            _mapper = mapper;
            _shoppingCartRepository = shoppingCartRepository;
            _sharedRuntimeData = sharedRuntimeData;
        }


        [HttpPost]
        public async Task<IActionResult> InitializePay([FromBody] DataLayer.Models.Shared.PaymentModel model)
        {
            if(_shoppingCartRepository.UserCartShoppingValidation(model.UserCartId))
            {
                var result = await _shoppingCartRepository.FetchUserShoppingCart(model.UserCartId);
                if (!result.Succeeded)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }
                else
                {
                    var subtractResult = await _shoppingCartRepository.SubtractUserCartOrderCntFromInventory(result.ReturnValue);

                    PspType psp = Enum.Parse<PspType>(model.PspType);
                    if (subtractResult.Succeeded)
                    {
                        var userEntity = await _userManager.FindByIdAsync(HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value);
                        var transaction = new Transaction();
                        transaction.TransactionId = Guid.NewGuid().ToString();
                        transaction.MainInvoiceNumber = Guid.NewGuid().ToString();
                        transaction.FinalPriceToPay = result.ReturnValue.FinalPriceToPay;
                        transaction.CustomerData = new CustomerData()
                        {
                            UserId = userEntity.Id,
                            UserName = userEntity.UserName,
                            UserFullName = userEntity.Profile.FullName ?? ""
                        };
                        transaction.BasicData = new PaymentGatewayData()
                        {
                            //PaymentId = Guid.NewGuid().ToString(),
                            CreationDateTime = DateTime.Now,
                            Stage = PaymentStage.Initialized,
                            PspType = psp,
                            ShoppinCartId = result.ReturnValue.ShoppingCartId,
                            ReservationNumber = $"{GetLocalIPAddress()}{DateTime.Now.Ticks}"
                        };

                        // var transaction = new Transaction()
                        //{
                        //    TransactionId = Guid.NewGuid().ToString(),
                        //    //???
                        //    MainInvoiceNumber = Guid.NewGuid().ToString(),
                        //    FinalPriceToPay = result.ReturnValue.FinalPriceToPay,
                        //    CustomerData = new CustomerData()
                        //    {
                        //        UserId = HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value,
                        //        UserName = HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Name).Value,
                        //        UserFullName = userEntity.Profile.FullName??""
                        //    },
                        //    BasicData = new PaymentGatewayData()
                        //    {
                        //        //PaymentId = Guid.NewGuid().ToString(),
                        //        CreationDateTime = DateTime.Now,
                        //        Stage = PaymentStage.Initialized,
                        //        PspType = (PspType)model.PspTypeId,
                        //        ShoppinCartId = result.ReturnValue.ShoppingCartId,
                        //        ReservationNumber =  $"{GetLocalIPAddress()}{DateTime.Now.Ticks}"
                        //    }
                        //};
                        var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
                        foreach (var invoice in result.ReturnValue.Details)
                        {
                            var obj = new InvoicePerSeller()
                            {
                                ParchasePerSeller = invoice,
                                SellerInvoiceId = Guid.NewGuid().ToString(),
                                SettlementInfo = new SettlementInfo()
                            };
                            transaction.SubInvoices.Add(obj);
                        }

                        await _transactionRepository.InsertTransaction(transaction);
                        var modelToStoreInSharedData = _transactionRepository.CreateTransactionItemsModel(transaction.TransactionId);
                        _sharedRuntimeData.AddToPayingOrders($"ar_{transaction.TransactionId}", modelToStoreInSharedData);

                        var id = Utilities.Base64Encode(transaction.BasicData.ReservationNumber);
                        var redirectAddress =
                            $"/{lanIcon}/{psp}/GetToken?reservationNumber={id}";

                        return Redirect(redirectAddress);
                    }
                    else
                    {
                        return Json(Language.GetString("AlertAndMessage_InternalServerErrorMessage"));
                    }
                }
            }
            else
            {
                return Json("ShoppingCart is invalid");
            }
        }

        public IActionResult PaymentError()
        {
            ViewBag.Psp = TempData["Psp"];
            ViewBag.Message = TempData["PaymentErrorMessage"];
            return View();
        }

        public IActionResult PaymentSuccess()
        {
            ViewBag.Psp = TempData["Psp"];
            ViewBag.ReferenceNumber = TempData["ReferenceNumber"];
            ViewBag.InvoiceNumber = TempData["InvoiceNumber"];
            ViewBag.OrderId = TempData["OrderId"];

            return View();
        }

        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString().Replace(".", "");
                }
            }
            return "0001112345";
        }
    }
}
