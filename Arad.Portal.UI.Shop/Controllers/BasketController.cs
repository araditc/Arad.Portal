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

namespace Arad.Portal.UI.Shop.Controllers
{
    [Authorize(Policy = "Role")]
    public class BasketController : BaseController
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IDomainRepository _domainRepository;
        public BasketController(IHttpContextAccessor accessor,
            IShoppingCartRepository shoppingCartRepository,
            IDomainRepository domainRepository) : base(accessor)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _domainRepository = domainRepository;
        }


        [HttpGet]
        public async Task<IActionResult> AddProToBasket([FromQuery]string productId, [FromQuery]int cnt)
        {
            if(User != null && User.Identity.IsAuthenticated)
            {
                var res = await _shoppingCartRepository.AddOrChangeProductToUserCart(productId, cnt);
                return Json(new
                {
                    status = res.Succeeded ? "Success" : "Error",
                    message = res.Succeeded ? Language.GetString("AlertAndMessage_ProductCountInCart") : res.Message,
                    cnt = res.ReturnValue.ItemsCount
                });
            }
            else
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/basket/AddProToBasket" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if(User != null && User.Identity.IsAuthenticated)
            {
                var domainName = base.DomainName;
                var currentUserId = base.CurrentUserId;
                var domain = _domainRepository.FetchByName(domainName);
                var model =(await _shoppingCartRepository.FetchActiveUserShoppingCart(currentUserId, domain.ReturnValue.DomainId)).ReturnValue;

                return View(model);
            }
            else
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/basket/get" });
            }

        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
