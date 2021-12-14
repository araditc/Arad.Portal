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
        public BasketController(IHttpContextAccessor accessor,
            IShoppingCartRepository shoppingCartRepository):base(accessor)
        {
            _shoppingCartRepository = shoppingCartRepository;   
        }


        [HttpPost]
        public async Task<IActionResult> AddProToBasket([FromQuery]string productId, [FromQuery]int cnt)
        {
            var res = await _shoppingCartRepository.AddOrChangeProductToUserCart(productId, cnt);
            return Json(new { status = res.Succeeded ? "Success" : "Error", 
                message = res.Succeeded ? Language.GetString("AlertAndMessage_ProductCountInCart") : res.Message
            });

        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
