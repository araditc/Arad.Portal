using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.Transaction;
using Arad.Portal.GeneralLibrary.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Transaction;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using System.Threading;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Helpers;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using System.Globalization;
using System.Collections.Generic;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Setting;
using Arad.Portal.DataLayer.Models.Shared.User;
using Arad.Portal.Models.UI;
using Arad.Portal.Controllers.Base;
using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Entities.Shop.Setting;
using Arad.Portal.Helpers.UI;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;

namespace Arad.Portal.Controllers.Payment;

public class PaymentController(
    IProductRepository productRepository,
    IHttpContextAccessor accessor,
    UserManager<ApplicationUser> userManager,
    ITransactionRepository transationRepository,
    SharedRuntimeData sharedRuntimeData,
    IDomainRepository domRepository,
    IShoppingCartRepository shoppingCartRepository,
    ILanguageRepository languageRepository,
    CreateNotification createNotification,
    ControllerHelper controllerHelper)
    : BaseController(accessor, domRepository, languageRepository)
{
    private readonly IDomainRepository _domRepository = domRepository;
    private readonly ILanguageRepository _languageRepository = languageRepository;

    public async ValueTask<IActionResult> InitializePay([FromBody] PaymentModel model, CancellationToken cancellationToken)
    {
        if (User != null && User.Identity.IsAuthenticated)
        {
            bool isUserCartShoppingValidation = true;
            ShoppingCart entity = await shoppingCartRepository.FirstOrDefaultAsync(c => c.Id == model.UserCartId, cancellationToken);
            foreach (PurchasePerSeller seller in entity.Details)
            {
                foreach (ShoppingCartDetail product in seller.Products)
                {
                    //check the quantity and price if no quantity and
                    //price change shoppingCart is ready to go to paymentGateway
                    DataLayer.Entities.Shop.Product.Product proEntity = await productRepository.FirstOrDefaultAsync(p => p.Id == product.ProductId, cancellationToken);
                    InventoryDetail inventoryDetail = controllerHelper.FindProductSpecValuesRecord(product.ProductId, product.ProductSpecValues);

                    if (!proEntity.IsDeleted &&
                        inventoryDetail != null &&
                        inventoryDetail.Count >= product.OrderCount &&
                        controllerHelper.GetCurrentPrice(proEntity) == product.PricePerUnit)
                    {
                        continue;
                    }

                    isUserCartShoppingValidation = false;
                    break;
                }
            }
            if (isUserCartShoppingValidation)
            {
                Result<ShoppingCart> result = new() { ReturnValue = new() };
                ShoppingCart shoppingCartModel = new();
                ShoppingCart userCartEntity = await shoppingCartRepository.FirstOrDefaultAsync(c => c.Id == model.UserCartId, cancellationToken);
                if (userCartEntity != null)
                {
                    shoppingCartModel.ShoppingCartCulture = userCartEntity.ShoppingCartCulture;
                    shoppingCartModel.Id = userCartEntity.Id;
                    shoppingCartModel.AssociatedDomainId = userCartEntity.AssociatedDomainId;
                    shoppingCartModel.CreatorUserId = userCartEntity.CreatorUserId;
                    result.ReturnValue.Details = [];

                    //each time we fetch cart shopping data should be updated in it
                    foreach (PurchasePerSeller sellerPurchase in userCartEntity.Details)
                    {
                        PurchasePerSeller purchasePerSeller = new();
                        decimal sellerFactor = 0;
                        purchasePerSeller.SellerId = sellerPurchase.SellerId;
                        purchasePerSeller.SellerUserName = sellerPurchase.SellerUserName;
                        purchasePerSeller.ShippingTypeId = sellerPurchase.ShippingTypeId;

                        // update shippingExpense if seller change it
                        ApplicationUser sellerEntity = await userManager.FindByIdAsync(sellerPurchase.SellerId);

                        sellerFactor += sellerPurchase.ShippingExpense;
                        foreach (ShoppingCartDetail pro in sellerPurchase.Products)
                        {
                            string productId = pro.ProductId;
                            DataLayer.Entities.Shop.Product.Product productEntity = await productRepository.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
                            InventoryDetail inventoryDetail = controllerHelper.FindProductSpecValuesRecord(pro.ProductId, pro.ProductSpecValues);

                            if (inventoryDetail != null && productEntity.IsActive
                                                        && !productEntity.IsDeleted && inventoryDetail.Count > 0)
                            {
                                long activePriceValue = controllerHelper.GetCurrentPrice(productEntity);
                                DiscountModel discountPerUnit = await controllerHelper.GetCurrentDiscountPerUnit(productEntity, activePriceValue);

                                pro.PricePerUnit = activePriceValue;
                                pro.DiscountPricePerUnit = discountPerUnit.DiscountPerUnit;
                                pro.TotalAmountToPay = (activePriceValue - discountPerUnit.DiscountPerUnit) * pro.OrderCount;
                                sellerFactor += pro.TotalAmountToPay;

                                //check inventory if it is less than orderCount then change our order Count to our inventory
                                if (pro.OrderCount > inventoryDetail.Count)
                                {
                                    pro.OrderCount = inventoryDetail.Count;
                                }
                            }
                            else
                            if (inventoryDetail != null && (productEntity.IsDeleted || inventoryDetail.Count == 0))
                            {
                                pro.OrderCount = 0;
                                pro.DiscountPricePerUnit = 0;
                                pro.PricePerUnit = 0;
                                pro.TotalAmountToPay = 0;
                            }

                            purchasePerSeller.Products.Add(pro);
                        }
                        purchasePerSeller.TotalDetailsAmountWithShipping = sellerFactor;
                        shoppingCartModel.FinalPriceToPay += sellerFactor;
                        shoppingCartModel.Details.Add(purchasePerSeller);
                    }
                    shoppingCartModel.CouponCode = userCartEntity.CouponCode;
                    shoppingCartModel.FinalPriceAfterCouponCode = userCartEntity.FinalPriceAfterCouponCode;
                    //model.FinalPriceForPay = finalPaymentPrice;
                    Result<ShoppingCart> updateResult = await shoppingCartRepository.UpdateAsync(shoppingCartModel, cancellationToken);
                    if (updateResult.Succeeded)
                    {
                        result.Succeeded = true;
                        result.Message = ConstMessages.SuccessfullyDone;
                        result.ReturnValue = shoppingCartModel;
                    }
                    else
                    {
                        result.Message = ConstMessages.GeneralError;
                    }
                }
                else
                {
                    result.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
                }
                if (!result.Succeeded)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }
                    Result subtractUserCartOrderCntFromInventory = new();
                    ApplicationUser adminUser = userManager.Users.FirstOrDefault(u => u.Domains.Any(a => a.IsOwner && a.DomainId == result.ReturnValue.AssociatedDomainId));
                    string lanId = (await _languageRepository.FirstOrDefaultAsync(l => l.Symbol.ToLower() == CultureInfo.CurrentCulture.Name.ToLower(), cancellationToken)).Id;
                    try
                    {
                        foreach (ShoppingCartDetail product in result.ReturnValue.Details.SelectMany(seller => seller.Products))
                        {
                            DataLayer.Entities.Shop.Product.Product productEntity = await productRepository.FirstOrDefaultAsync(p => p.Id == product.ProductId, cancellationToken);
                            InventoryDetail inventoryDetail = controllerHelper.FindProductSpecValuesRecord(product.ProductId, product.ProductSpecValues);

                            if (inventoryDetail == null)
                            {
                                continue;
                            }

                            {
                                productEntity.Inventory.FirstOrDefault(d => d.SpecValuesId == inventoryDetail.SpecValuesId)!.Count -= product.OrderCount;
                                await productRepository.UpdateAsync(p => p.Id == product.ProductId, m => m, productEntity, cancellationToken);

                                if (productEntity.Inventory == null || productEntity.Inventory.Sum(d => d.Count) > productEntity.MinimumCount)
                                {
                                    continue;
                                }

                                {
                                    string productName = productEntity.MultiLingualProperties.Any(p => p.LanguageId == lanId) ?
                                                             productEntity.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId)?.Name :
                                                             productEntity.MultiLingualProperties.FirstOrDefault()?.Name;
                                    await createNotification.NotifySiteAdminForLackOfInventory("NotifyProductMinimumCount", productName, adminUser, NotificationType.Sms, cancellationToken);
                                }
                            }
                        }

                        subtractUserCartOrderCntFromInventory.Succeeded = true;
                    }
                    catch (Exception ex)
                    {
                        subtractUserCartOrderCntFromInventory.Succeeded = false;
                        subtractUserCartOrderCntFromInventory.Message = UtilityLanguage.GetString("AlertAndMessage_ExceptionOccured");
                    }

                    PspType psp = Enum.Parse<PspType>(model.PspType);

                    if (!subtractUserCartOrderCntFromInventory.Succeeded)
                    {
                        return Json(UtilityLanguage.GetString("AlertAndMessage_InternalServerErrorMessage"));
                    }

                    {
                        ApplicationUser userEntity = await userManager.FindByIdAsync(User.GetUserId());
                        Transaction transaction = new() { Id = Guid.NewGuid().ToString(), ShoppingCartId = model.UserCartId };
                        Result<Domain> domainEntity = controllerHelper.FetchDomainByName(DomainName, false);
                        if (domainEntity.Succeeded)
                        {
                            if (!domainEntity.ReturnValue.IsDefault)
                            {
                                if (domainEntity.ReturnValue.InvoiceNumberProcedure == InvoiceNumberProcedure.FromMainDomain)
                                {
                                    transaction.MainInvoiceNumber = Guid.NewGuid().ToString();
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(domainEntity.ReturnValue.LastInvoiceNumber))
                                    {
                                        transaction.MainInvoiceNumber = Convert.ToInt64(domainEntity.ReturnValue.LastInvoiceNumber) +
                                                                        domainEntity.ReturnValue.IncreasementValue.ToString();
                                    }
                                    else
                                    {
                                        transaction.MainInvoiceNumber = domainEntity.ReturnValue.InvoiceNumberInitializer;
                                    }

                                    domainEntity.ReturnValue.LastInvoiceNumber = transaction.MainInvoiceNumber;
                                    await _domRepository.UpdateAsync(domainEntity.ReturnValue, cancellationToken);
                                }
                            }
                            else
                            {
                                transaction.MainInvoiceNumber = Guid.NewGuid().ToString();
                            }

                        }


                        transaction.FinalPriceToPay = string.IsNullOrWhiteSpace(result.ReturnValue.CouponCode) ? result.ReturnValue.FinalPriceToPay : result.ReturnValue.FinalPriceAfterCouponCode.Value;

                        transaction.CustomerData = new()
                                                   {
                                                       UserId = userEntity.Id,
                                                       UserName = userEntity.UserName,
                                                       UserFullName = userEntity.Profile.FullName ?? "",
                                                       ShippingAddressId = !string.IsNullOrWhiteSpace(model.Address) ? model.Address : userEntity.Profile.Addresses.Any(a => a.AddressType == AddressType.ShippingAddress) ?
                                                                               userEntity.Profile.Addresses.FirstOrDefault(a => a.AddressType == AddressType.ShippingAddress).Id : ""
                                                   };
                        transaction.BasicData = new()
                                                {
                                                    //PaymentId = Guid.NewGuid().ToString(),
                                                    CreationDateTime = DateTime.Now,
                                                    Stage = PaymentStage.Initialized,
                                                    PspType = psp,
                                                    ShoppinCartId = result.ReturnValue.Id,
                                                    ReservationNumber = $"{GetLocalIPAddress()}{DateTime.Now.Ticks}"
                                                };

                        string lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
                        foreach (InvoicePerSeller obj in result.ReturnValue.Details.Select(invoice => new InvoicePerSeller()
                                                                                                      {
                                                                                                          PurchasePerSeller = invoice,
                                                                                                          SellerInvoiceId = Guid.NewGuid().ToString(),
                                                                                                          SettlementInfo = new()
                                                                                                      }))
                        {
                            transaction.SubInvoices.Add(obj);
                        }

                        await transationRepository.InsertAsync(transaction, cancellationToken);
                        //delete current active use shopping cart after transaction inserted
                        if (userCartEntity != null)
                        {
                            userCartEntity.IsDeleted = true;
                            userCartEntity.IsActive = false;
                            Result<ShoppingCart> updateResult = await shoppingCartRepository.DeleteAsync(userCartEntity, cancellationToken);
                            if (updateResult.Succeeded)
                            {
                                result.Succeeded = true;
                                result.Message = ConstMessages.SuccessfullyDone;
                            }
                            else
                            {
                                result.Message = ConstMessages.GeneralError;

                            }
                        }
                        else
                        {
                            result.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
                        }
                        TransactionItems modelToStoreInSharedData = new();
                        Transaction transactionEntity = await transationRepository.FirstOrDefaultAsync(t => t.Id == transaction.Id, cancellationToken);
                        foreach (ProductOrder obj in from seller in transactionEntity.SubInvoices from pro in seller.PurchasePerSeller.Products select new ProductOrder
                                                                                                                                                       {
                                                                                                                                                           ProductId = pro.ProductId,
                                                                                                                                                           OrderCount = pro.OrderCount
                                                                                                                                                       })
                        {
                            modelToStoreInSharedData.Orders.Add(obj);
                        }
                        modelToStoreInSharedData.CreatedDate = DateTime.Now;
                        sharedRuntimeData.AddToPayingOrders($"ar_{transaction.Id}", modelToStoreInSharedData);

                        string id = Helpers.UI.Utilities.Base64Encode(transaction.BasicData.ReservationNumber);
                        string redirectAddress =
                            $"/{lanIcon}/{psp}/GetToken?reservationNumber={id}";

                        return Redirect(redirectAddress);
                    }
            }

            return Json("ShoppingCart is invalid");
        }
        else
        {
            return Json("user is not authenticated.");
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
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString().Replace(".", "");
            }
        }
        return "0001112345";
    }
}