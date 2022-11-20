using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.Shop.ShoppingCart;
using Arad.Portal.GeneralLibrary.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Models.ShoppingCart;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Contracts.Shop.Promotion;
using Arad.Portal.DataLayer.Models.Shared;
using System.Globalization;
using Arad.Portal.DataLayer.Models.Product;
using DocumentFormat.OpenXml.EMMA;

namespace Arad.Portal.UI.Shop.Controllers
{
   
    public class BasketController : BaseController
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDomainRepository _domainRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPromotionRepository _promotionRepository;
        public BasketController(IHttpContextAccessor accessor,
            IShoppingCartRepository shoppingCartRepository,
            UserManager<ApplicationUser> userManager,
            IPromotionRepository promotionRepository,
            IProductRepository productRepository,
            IDomainRepository domainRepository) : base(accessor, domainRepository)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _domainRepository = domainRepository;
            _userManager = userManager;
            _promotionRepository = promotionRepository;
            _productRepository = productRepository;
        }


        [HttpPost]
        public async Task<IActionResult> AddProToBasket([FromBody]BasketModel model)
        {
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            var productId = _productRepository.FetchIdByCode(Convert.ToInt64(model.Code));
            if (User != null && User.Identity.IsAuthenticated)
            {
                var res = await _shoppingCartRepository.AddOrChangeProductToUserCart(productId, model.Count, model.SpecVals, model.CartDetailId);
                ViewBag.BasketCount = res.ReturnValue.ItemsCount;
                return Json(new
                {
                    status = res.Succeeded ? "Succeed" : "Error",
                    message = res.Succeeded ? Language.GetString("AlertAndMessage_ProductCountInCart") : res.Message,
                    cnt = res.ReturnValue.ItemsCount
                });
            }
            else
            {
                //return RedirectToAction("Login", "Account", new { returnUrl = "/basket/AddProToBasket" });
                return Redirect($"~/{lanIcon}/Account/Login?returnUrl=/{lanIcon}/basket/AddProToBasket?code={model.Code}&cnt={model.Count}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckCode([FromQuery] string code, [FromBody] NewVal model)
        {
            var res = _domainRepository.FetchByName(this.DomainName, false).ReturnValue;
            var currentUserId = User.GetUserId();
            var result = _promotionRepository.CheckCode(currentUserId, code, res.DomainId, model.Price);
            if(result.Succeeded)
            {
                var shoppingCart = (await _shoppingCartRepository.FetchActiveUserShoppingCart(currentUserId, res.DomainId)).ReturnValue;
                var shoppingCartUpdateRes = await _shoppingCartRepository.ChangePriceWithCouponCode(shoppingCart.ShoppingCartId, code, model.Price, result.ReturnValue.Price);
                var removeRes = await _promotionRepository.RemoveUserFromUserCoupon(code, currentUserId, res.DomainId);

                return Json(new
                {
                    status = result.Succeeded && shoppingCartUpdateRes.Succeeded ? "Succeed" : "Error",
                    val = result.Succeeded ? result.ReturnValue.Price : 0,
                    Message = result.Succeeded ? "" : Language.GetString("AlertAndMessage_InvalidCode")
                });
            }

            return Json(new
            {
                status = "Error",
                val = 0,
                Message = result.Succeeded ? "" : Language.GetString("AlertAndMessage_InvalidCode")
            });
        }

        [HttpPost]
        public async Task<IActionResult> RevertCode([FromQuery]string code, [FromBody]NewVal model)
        {
            var res = _domainRepository.FetchByName(this.DomainName, false).ReturnValue;
            var currentUserId = User.GetUserId();

            var result = await _promotionRepository.RevertCodeForUser(currentUserId, code, res.DomainId, model.Price);
            return Json(new
            {
                status = "Error",
                val = 0,
                Message = result.Succeeded ? "" : Language.GetString("AlertAndMessage_InvalidCode")
            });

        }

        public IActionResult Reorder(string shoppingCartId)
        {
            var res = _shoppingCartRepository.Reorder(shoppingCartId);

            var lanIcon = "";
            if(CultureInfo.CurrentCulture.Name != null)
            {
                lanIcon = CultureInfo.CurrentCulture.Name;
            }
            return Redirect($"~/{lanIcon}/basket/get");
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            ViewBag.LanIcon = lanIcon;
            if (User != null && User.Identity.IsAuthenticated)
            {
                var domainName = base.DomainName;
                var currentUserId = User.GetUserId();
                var res = _domainRepository.FetchByName(domainName, false);
                if(res.Succeeded)
                {
                    var model = (await _shoppingCartRepository.FetchActiveUserShoppingCart(currentUserId, res.ReturnValue.DomainId)).ReturnValue;
                    return View(model);
                }else
                {
                    var model = new ShoppingCartDTO();
                    return View(model);
                }
                
            }
            else
            {
                return Redirect($"~/{lanIcon}/Account/Login?returnUr=/{lanIcon}/basket/get");
            }

        }
        [HttpGet]
        //parId is userCartId and Id is shoppingcartDetailId
        public async Task<IActionResult> DeleteItemFromCart(string parId, string id)
        {
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            if (User != null && User.Identity.IsAuthenticated)
            {
                var res = await _shoppingCartRepository.DeleteShoppingCartItem(parId, id);
                return Json(new
                {
                    status = res.Succeeded ? "Succeed" : "Error",
                    message = res.Succeeded ? res.Message : Language.GetString("AlertAndMessage_DeleteError") 
                });
            }else
            {
                return Redirect($"~/{lanIcon}/Account/Login?returnUr=/{lanIcon}/basket/get");
            }
        }



        [HttpGet]
        public async Task<IActionResult> SendInfo()
        {
            var domainName = base.DomainName;
            var domainDTO = _domainRepository.FetchByName(domainName,false);
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var shoppingCart = await _shoppingCartRepository.FetchActiveUserShoppingCart(userId, domainDTO.ReturnValue.DomainId);
            var user = await _userManager.FindByIdAsync(userId);

            var model = new DataLayer.Models.Shared.SendInfoPage()
            {
                Addresses = user.Profile.Addresses.Where(_=>_.AddressType == DataLayer.Models.User.AddressType.ShippingAddress).ToList(),
                CurrencySymbol = shoppingCart.ReturnValue.ShoppingCartCulture.CurrencySymbol,
                TotalCost = !string.IsNullOrEmpty(shoppingCart.ReturnValue.CouponCode) ? 
                        shoppingCart.ReturnValue.FinalPriceAfterCouponCode.Value.ToString() : 
                        shoppingCart.ReturnValue.FinalPriceForPay.ToString(),
                UserCartId = shoppingCart.ReturnValue.ShoppingCartId
            };
            return View(model);
           
        }

        public async Task<IActionResult> DeleteAddress(string addressId)
        {
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            try
            {
               
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = await _userManager.FindByIdAsync(userId);
                if (User != null && User.Identity.IsAuthenticated)
                {
                    var addresses = user.Profile.Addresses;
                    var item = addresses.SingleOrDefault(a => a.Id == addressId);
                    if (item != null) addresses.Remove(item);

                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {

                        TempData["MessageResult"] = true;
                        return Redirect($"/{lanIcon}/Basket/SendInfo");
                    }


                    TempData["MessageResult"] = false;
                    return Redirect($"/{lanIcon}/Basket/SendInfo");
                }else
                {
                    return Redirect($"~/{lanIcon}/Account/Login?returnUrl=/{lanIcon}/basket/DeleteAddress?addressId={addressId}");
                }
            }
            catch (Exception x)
            {
                TempData["MessageResult"] = false;
                return Redirect($"/{lanIcon}/Basket/SendInfo");
                
            }
        }


    }
}
