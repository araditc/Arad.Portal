﻿using Arad.Portal.DataLayer.Contracts.General.Domain;
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
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager,
            IDomainRepository domainRepository) : base(accessor, env)
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
                    status = res.Succeeded ? "Success" : "Error",
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
            if (User != null && User.Identity.IsAuthenticated)
            {
                var domainName = base.DomainName;
                var currentUserId = HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value;
                var domain = _domainRepository.FetchByName(domainName);
               
                var model =(await _shoppingCartRepository.FetchActiveUserShoppingCart(currentUserId, domain.ReturnValue.DomainId)).ReturnValue;

                return View(model);
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
            var domainDTO = _domainRepository.FetchByName(domainName);
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var shoppingCart =await _shoppingCartRepository.FetchActiveUserShoppingCart(userId, domainDTO.ReturnValue.DomainId);
            var user = await _userManager.FindByIdAsync(userId);

            var model = new DataLayer.Models.Shared.SendInfoPage()
            {
                Addresses = user.Profile.Addresses,
                CurrencySymbol = shoppingCart.ReturnValue.ShoppingCartCulture.CurrencySymbol,
                TotalCost = Math.Round(shoppingCart.ReturnValue.FinalPriceForPay).ToString()
            };
            return View(model);
        }

           


        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
