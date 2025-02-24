using Arad.Portal.GeneralLibrary.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Promotion;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using AutoMapper;
using System.Threading;

using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Setting;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Arad.Portal.Models.UI;
using Arad.Portal.Models.Shared.ShoppingCart;
using Arad.Portal.Models.Shared.Product;
using Arad.Portal.Controllers.Base;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Shared.User;
using Arad.Portal.Helpers.Shared;

namespace Arad.Portal.Controllers.Setting;

public class BasketController(
    IHttpContextAccessor accessor,
    IShoppingCartRepository shoppingCartRepository,
    UserManager<ApplicationUser> userManager,
    IProductRepository productRepository,
    IUserRepository userRepository,
    IPromotionRepository promotionRepository,
    IDomainRepository domainRepository,
    ILanguageRepository languageRepository,
    IMapper mapper,
    MinioHelper minioHelper,
    ControllerHelper controllerHelper)
    : BaseController(accessor, domainRepository, languageRepository)
{
    private readonly IDomainRepository _domainRepository = domainRepository;

    [HttpPost]
    public async ValueTask<IActionResult> AddProToBasket([FromBody] BasketModel model, CancellationToken cancellationToken)
    {
        string lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
        string productId = controllerHelper.FetchIdByCode(Convert.ToInt64(model.Code));
        if (User != null && User.Identity.IsAuthenticated)
        {
            Result<CartItemsCount> res = await controllerHelper.AddOrChangeProductToUserCart(productId, model.Count, model.SpecVals, model.CartDetailId, cancellationToken);
            ViewBag.BasketCount = res.ReturnValue.ItemsCount;
            return Json(new
            {
                status = res.Succeeded ? "Succeed" : "Error",
                message = res.Succeeded ? UtilityLanguage.GetString("AlertAndMessage_ProductCountInCart") : res.Message,
                cnt = res.ReturnValue.ItemsCount
            });
        }
        else
        {
            //return RedirectToAction("Login", "Account", new { returnUrl = "/basket/AddProToBasket" });
            return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + $"/{lanIcon}/Account/Login?returnUrl=/{lanIcon}/basket/AddProToBasket?code={model.Code}&cnt={model.Count}");
        }
    }

    [HttpPost]
    public async ValueTask<IActionResult> CheckCode([FromQuery] string code, [FromBody] NewVal model, CancellationToken cancellationToken)
    {
        Domain res = await _domainRepository.FirstAsync(c => c.DomainName == DomainName, cancellationToken);
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        Promotion? promotion = await promotionRepository.FirstOrDefaultAsync(c => c.CouponCode == code && c.IsActive & !c.IsDeleted, cancellationToken);

        UserCoupon? userCoupon = promotion?.UserCoupons.FirstOrDefault(c => c.UserIds.Contains(userDb.Id) && c.CouponCode == code);

        if (userCoupon != null)
        {
            userCoupon.UserIds.Remove(userDb.Id);
            promotion?.UserCoupons.Remove(userCoupon);
            promotion?.UserCoupons.Add(userCoupon);
            Result<Promotion>? result = new();
            if (promotion != null)
            {
                result = await promotionRepository.UpdateAsync(c => c.Id == promotion.Id, m => m.UserCoupons, promotion.UserCoupons, cancellationToken);
            }

            return Json(new
            {
                status = result.Succeeded ? "Succeed" : "Error",
                //   val = result.Succeeded ? result.ReturnValue.va : 0,
                Message = result.Succeeded ? ConstMessages.SuccessfullyDone : UtilityLanguage.GetString("AlertAndMessage_InvalidCode")
            });
        }
        else
        {
            return Json(new
            {
                status = "Error",
                Message = UtilityLanguage.GetString("AlertAndMessage_InvalidCode")
            });
        }
        //test
        //res.DomainId = "28d0433f-2bb6-4ef9-bad7-0a18a28d9004";
        //var result = _promotionRepository.CheckCode(currentUserId, code, res.Id, model.Price);
        //if (result.Succeeded)
        //{
        //    var shoppingCart = (await _shoppingCartRepository.FetchActiveUserShoppingCart(currentUserId, res.Id)).ReturnValue;
        //    var shoppingCartUpdateRes = await _shoppingCartRepository.ChangePriceWithCouponCode(shoppingCart.ShoppingCartId, code, model.Price, result.ReturnValue.Price);
        //    var removeRes = await _promotionRepository.RemoveUserFromUserCoupon(code, currentUserId, res.Id);

        //    return Json(new
        //    {
        //        status = result.Succeeded && shoppingCartUpdateRes.Succeeded ? "Succeed" : "Error",
        //        val = result.Succeeded ? result.ReturnValue.Price : 0,
        //        Message = result.Succeeded ? "" : Language.GetString("AlertAndMessage_InvalidCode")
        //    });
        //}


    }

    [HttpPost]
    public async ValueTask<IActionResult> RevertCode([FromQuery] string code, [FromBody] NewVal model, CancellationToken cancellationToken)
    {
        Domain res = await _domainRepository.FirstAsync(c => c.DomainName == DomainName, cancellationToken);
        string currentUserId = User.GetUserId();

        // var result = await _promotionRepository.RevertCodeForUser(currentUserId, code, res.Id, model.Price);
        return Json(new
        {
            //status = result.Succeeded ? "Succeed" : "Error",
            //val = result.ReturnValue.Price,
            //Message = result.Succeeded ? "" : Language.GetString("AlertAndMessage_InvalidCode")
        });

    }

    public IActionResult Reorder(string shoppingCartId)
    {
        //var res = _shoppingCartRepository.Reorder(shoppingCartId);

        string lanIcon = "";
        if (CultureInfo.CurrentCulture.Name != null)
        {
            lanIcon = CultureInfo.CurrentCulture.Name;
        }
        return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + $"/{lanIcon}/basket/get");
    }

    [HttpGet]
    public async ValueTask<IActionResult> Get(CancellationToken cancellationToken)
    {
        string? lanIcon = HttpContext.Request.Path.Value?.Split("/")[1];

        if (lanIcon != null)
        {
            ViewBag.LanIcon = lanIcon;
        }
        
        if (User.Identity is { IsAuthenticated: true })
        {
            ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
            Domain? domainEntity = controllerHelper.GetCurrentUserDomain();

            if (lanIcon != null)
            {
                string languageId = controllerHelper.FetchLanguageBySymbol(lanIcon);
                ViewBag.CompanyName = domainEntity.Titles.FirstOrDefault(c => c.LanguageId == languageId).Name!;
            }

          
            if (domainEntity != null)
            {
                ShoppingCart model = await shoppingCartRepository.FirstOrDefaultAsync(c => c.CreatorUserId == userDb.Id && c.AssociatedDomainId == domainEntity.Id, cancellationToken);
                ShoppingCartDto shoppingCartDto = new();
                if (model == null)
                {
                    return View(shoppingCartDto);
                } 
                shoppingCartDto = mapper.Map<ShoppingCartDto>(model);

                foreach (ShoppingCartDetailDto item2 in shoppingCartDto.Details.SelectMany(item => item.Products))
                {
                    List<DataLayer.Entities.Shop.Product.Product> product = await productRepository.GetListAsync(c => c.Id == item2.ProductId, cancellationToken);

                    foreach (DataLayer.Entities.Shop.Product.Product item3 in product)
                    {
                        Image? image = item3.Images.FirstOrDefault(c => c.IsMain);
                        CultureInfo current = new("en-US") { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
                        Thread.CurrentThread.CurrentCulture = current;

                        Domain domain = controllerHelper.GetCurrentUserDomain();
                        string objectName = "";
                        if (image != null)
                        {
                             objectName = $"{domain.Id}/{image.ImageId}/{image.FileName.Replace(':', '-')}";
                        }
                        (bool success, byte[] imageData) = await minioHelper.GetObject("productimage", objectName);

                        if (success)
                        {
                            if (image != null)
                            {
                                image.Content = Convert.ToBase64String(imageData);
                            }
                        }

                        if (image != null)
                        {
                            item2.ProductImage = image;
                        }
                    }
                }

                DataLayer.Entities.General.Language.Language defLang = controllerHelper.GetDefaultLanguage();
                CultureInfo current2 = new(defLang.Symbol) { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
                Thread.CurrentThread.CurrentCulture = current2;

                return View(shoppingCartDto);
            }
            else
            {
                ShoppingCartDto model = new();

                return View(model);
            }
        }
        else
        {
            return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + $"/{lanIcon}/Account/Login?returnUr=/{lanIcon}/basket/get");
        }

    }
    [HttpGet]
    //parId is userCartId and ID is shoppingCartDetailID
    public async ValueTask<IActionResult> DeleteItemFromCart(string parId, string id, CancellationToken cancellationToken)
    {
        string lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
        if (User != null && User.Identity.IsAuthenticated)
        {
            Result<ShoppingCart> res = await shoppingCartRepository.DeleteAsync(parId, cancellationToken);
            return Json(new
            {
                status = res.Succeeded ? "Succeed" : "Error",
                message = res.Succeeded ? res.Message : UtilityLanguage.GetString("AlertAndMessage_DeleteError")
            });
        }
        else
        {
            return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + $"/{lanIcon}/Account/Login?returnUr=/{lanIcon}/basket/get");
        }
    }



    [HttpGet]
    public async ValueTask<IActionResult> SendInfo(CancellationToken cancellationToken)
    {
        string domainName = DomainName;
        Domain domain = await _domainRepository.FirstAsync(c => c.DomainName == DomainName, cancellationToken);
        string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        ShoppingCart shoppingCart = await shoppingCartRepository.FirstOrDefaultAsync(c => c.CreatorUserId == userId && c.IsActive == true && c.AssociatedDomainId == domain.Id, cancellationToken);
        ApplicationUser user = await userManager.FindByIdAsync(userId);
        ShoppingCartDto shoppingCartDto = mapper.Map<ShoppingCartDto>(shoppingCart);
        SendInfoPage model = new()
        {
            Addresses = user.Profile.Addresses.Where(a => a.AddressType == DataLayer.Models.Shared.User.AddressType.ShippingAddress).ToList(),
            CurrencySymbol = shoppingCartDto.ShoppingCartCulture.CurrencySymbol,
            TotalCost = !string.IsNullOrEmpty(shoppingCartDto.CouponCode) ?
                                                 shoppingCartDto.FinalPriceAfterCouponCode.Value.ToString() :
                                                 shoppingCartDto.FinalPriceForPay.ToString(),
            UserCartId = shoppingCartDto.Id
        };
        return View(model);

    }

    public async ValueTask<IActionResult> DeleteAddress(string addressId, CancellationToken cancellationToken)
    {
        string lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
        try
        {

            string userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ApplicationUser user = await userManager.FindByIdAsync(userId);
            if (User != null && User.Identity.IsAuthenticated)
            {
                List<Address> addresses = user.Profile.Addresses;
                Address item = addresses.SingleOrDefault(a => a.Id == addressId);
                if (item != null) addresses.Remove(item);

                Result<ApplicationUser> result = await userRepository.UpdateAsync(c => c.Id == user.Id, m => m.Profile.Addresses, addresses, cancellationToken);

                if (result.Succeeded)
                {

                    TempData["MessageResult"] = true;
                    return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + $"/{lanIcon}/Basket/SendInfo");
                }


                TempData["MessageResult"] = false;
                return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + $"/{lanIcon}/Basket/SendInfo");
            }
            else
            {
                return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + $"/{lanIcon}/Account/Login?returnUrl=/{lanIcon}/basket/DeleteAddress?addressId={addressId}");
            }
        }
        catch (Exception x)
        {
            TempData["MessageResult"] = false;
            return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + $"/{lanIcon}/Basket/SendInfo");

        }
    }


}