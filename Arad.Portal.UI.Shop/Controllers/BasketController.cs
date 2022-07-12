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

namespace Arad.Portal.UI.Shop.Controllers
{
    [Authorize(Policy = "Role")]
    public class BasketController : BaseController
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDomainRepository _domainRepository;
        public BasketController(IHttpContextAccessor accessor,
            IShoppingCartRepository shoppingCartRepository,
            UserManager<ApplicationUser> userManager,
            IDomainRepository domainRepository) : base(accessor, domainRepository)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _domainRepository = domainRepository;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> AddProToBasket([FromQuery]string productId, [FromQuery]int cnt)
        {
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            if (User != null && User.Identity.IsAuthenticated)
            {
                var res = await _shoppingCartRepository.AddOrChangeProductToUserCart(productId, cnt);
                @ViewBag.BasketCount = res.ReturnValue.ItemsCount;
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
                return Redirect($"~/{lanIcon}/Account/Login?returnUrl=/{lanIcon}/basket/AddProToBasket?productId={productId}&cnt={cnt}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            ViewBag.LanIcon = lanIcon;
            if (User != null && User.Identity.IsAuthenticated)
            {
                var domainName = base.DomainName;
                var currentUserId = HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value;
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
        public async Task<IActionResult> SendInfo()
        {
           
            //var items = HttpContext.Session.GetString("basket");
            var domainName = base.DomainName;
            var domainDTO = _domainRepository.FetchByName(domainName,false);
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var shoppingCart = await _shoppingCartRepository.FetchActiveUserShoppingCart(userId, domainDTO.ReturnValue.DomainId);
            var user = await _userManager.FindByIdAsync(userId);

            var model = new DataLayer.Models.Shared.SendInfoPage()
            {
                Addresses = user.Profile.Addresses.Where(_=>_.AddressType == DataLayer.Models.User.AddressType.ShippingAddress).ToList(),
                CurrencySymbol = shoppingCart.ReturnValue.ShoppingCartCulture.CurrencySymbol,
                TotalCost = Math.Round(shoppingCart.ReturnValue.FinalPriceForPay).ToString(),
                UserCartId = shoppingCart.ReturnValue.UserCartId
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
            }
            catch (Exception x)
            {
                TempData["MessageResult"] = false;
                return Redirect($"/{lanIcon}/Basket/SendInfo");
                
            }
        }




        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
