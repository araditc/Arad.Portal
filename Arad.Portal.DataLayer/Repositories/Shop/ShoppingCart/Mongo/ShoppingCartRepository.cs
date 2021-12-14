using Arad.Portal.DataLayer.Contracts.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.ShoppingCart;
using Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using AutoMapper;
using Arad.Portal.DataLayer.Repositories.Shop.Promotion.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Currency.Mongo;

namespace Arad.Portal.DataLayer.Repositories.Shop.ShoppingCart.Mongo
{
    public class ShoppingCartRepository : BaseRepository, IShoppingCartRepository
    {
        private readonly ShoppingCartContext _context;
        private readonly DomainContext _domainContext;
        private readonly LanguageContext _languageContext;
        private readonly CurrencyContext _currencyContext;
        private readonly ProductContext _productContext;
        private readonly PromotionContext _promotionContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public ShoppingCartRepository(IHttpContextAccessor httpContextAccessor,
            ShoppingCartContext context,
            ProductContext productContext,
            PromotionContext promotionContext,
            DomainContext domainContext,
            LanguageContext languageContext,
            CurrencyContext currencyContext,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
            : base(httpContextAccessor)
        {
            _context = context;
            _productContext = productContext;
            _domainContext = domainContext;
            _promotionContext = promotionContext;
            _userManager = userManager;
            _currencyContext = currencyContext;
            _mapper = mapper;
            _languageContext = languageContext;
        }
        public Result<string> FindCurrentUserShoppingCart(string userId, string domainId)
        {
            var result = new Result<string>();
            var entity = _context.Collection
                .Find(_ => _.CreatorUserId == userId && !_.IsDeleted && _.IsActive).FirstOrDefault();

            if (entity != null)
            {
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
                result.ReturnValue = entity.UserCartId;
            }
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
                result.ReturnValue = Guid.NewGuid().ToString();
            }
            return result;
        }
        public async Task<Result> AddOrChangeProductToUserCart(string productId, int orderCount)
        {
            var result = new Result();
            var productEntity = _productContext.ProductCollection
                .Find(_ => _.ProductId == productId).FirstOrDefault();
            var userId = base.GetUserId();
            var userCartEntity = _context.Collection
                .Find(_ => _.CreatorUserId == userId && !_.IsDeleted && _.IsActive).FirstOrDefault();
            var domainEntity = _domainContext.Collection.Find(_ => _.DomainName == GetCurrentDomainName()).FirstOrDefault();

            if (userCartEntity == null)
            {
                var res = await InsertUserShoppingCart(userId);
                if (res.Succeeded)
                {
                    userCartEntity = _context.Collection
                       .Find(_ => _.CreatorUserId == userId && !_.IsDeleted 
                             && _.AssociatedDomainId == domainEntity.DomainId && _.IsActive ).FirstOrDefault();
                }
            }
            
            if (productEntity != null)
            {
                if (userCartEntity.Details.Any(_ => _.Products.Any(a => a.ProductId == productId)))
                {
                    result = await ChangeProductCountInUserCart(userId, productId, orderCount);
                }
                else
                {
                    var currentPriceValue = GetCurrentPrice(productEntity);
                    var res = await GetCurrentDiscountPerUnit(productEntity, currentPriceValue);

                    if (productEntity.Inventory > 0)
                    {
                        int finalOrderCnt = productEntity.Inventory > orderCount ? orderCount : productEntity.Inventory;
                        var shopCartDetail = new ShoppingCartDetail()
                        {
                            ProductId = productEntity.ProductId,
                            ProductName = productEntity
                                .MultiLingualProperties.FirstOrDefault(a => a.LanguageId
                                == userCartEntity.ShoppingCartCulture.LanguageId).Name,
                            CreationDate = DateTime.Now,
                            CreatorUserId = userId,
                            AssociatedDomainId = domainEntity.DomainId,
                            CreatorUserName = base.GetUserName(),
                            DiscountPricePerUnit = res.DiscountPerUnit,
                            PricePerUnit = currentPriceValue,
                            IsActive = true,
                            OrderCount = finalOrderCnt,
                            SellerId = productEntity.SellerUserId,
                            TotalAmountToPay = (currentPriceValue - res.DiscountPerUnit) * finalOrderCnt
                        };
                        if (userCartEntity.Details.Any(_ => _.SellerId == productEntity.SellerUserId))
                        {

                            var row = userCartEntity.Details.FirstOrDefault(_ => _.SellerId == productEntity.SellerUserId);
                            row.Products.Add(shopCartDetail);
                        }
                        else
                        {
                            var purchase = new PurchasePerSeller()
                            {
                                SellerId = productEntity.SellerUserId
                            };
                            purchase.Products.Add(shopCartDetail);
                            userCartEntity.Details.Add(purchase);
                        }

                        var updateResult = await _context.Collection
                            .ReplaceOneAsync(_ => _.UserCartId == userCartEntity.UserCartId, userCartEntity);
                        if (updateResult.IsAcknowledged)
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
                        result.Message = ConstMessages.LackOfInventoryOfProduct;
                    }
                }
               
            }
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public async Task<Result> ChangeProductCountInUserCart(string userId, string productId, int newCount)
        {
            var result = new Result();
            var domainName = base.GetCurrentDomainName();
            if (newCount != 0)
            {
                var domainEntity = _domainContext.Collection.Find(_ => _.DomainName == domainName).FirstOrDefault();
                var userCartEntity = _context.Collection
                    .Find(_ => _.CreatorUserId == userId && !_.IsDeleted
                    && _.AssociatedDomainId == domainEntity.DomainId && _.IsActive).FirstOrDefault();

                var productEntity = _productContext.ProductCollection.Find(_ => _.ProductId == productId).FirstOrDefault();
                if (userCartEntity == null)
                {
                    var res = await InsertUserShoppingCart(userId);
                    if (res.Succeeded)
                    {
                        userCartEntity = _context.Collection
                        .Find(_ => _.CreatorUserId == userId && !_.IsDeleted
                            && _.AssociatedDomainId == domainEntity.DomainId && _.IsActive).FirstOrDefault();
                    }
                }
                if (userCartEntity != null)
                {
                    //surely this product added before to shoppingcart then it has this record
                    var sellerObj = userCartEntity.Details.Where(_ => _.SellerId == productEntity.SellerUserId).FirstOrDefault();
                    var productRow = sellerObj.Products.FirstOrDefault(_ => _.ProductId == productId);

                    var index = sellerObj.Products.IndexOf(productRow);
                    if (productEntity.Inventory > 0 && productEntity.Inventory >= newCount)
                    {
                        if (productRow != null)
                        {
                            productRow.OrderCount = newCount;
                            var price = GetCurrentPrice(productEntity);
                            var res = await GetCurrentDiscountPerUnit(productEntity, price);
                            productRow.DiscountPricePerUnit = res.DiscountPerUnit;
                            productRow.PricePerUnit = price;
                            sellerObj.Products[index] = productRow;

                            var updateResult = await _context.Collection
                                .ReplaceOneAsync(_ => _.UserCartId == userCartEntity.UserCartId, userCartEntity);
                            if (updateResult.IsAcknowledged)
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
                            result.Message = ConstMessages.ObjectNotFound;
                        }
                    }
                    else
                    {
                        result.Message = ConstMessages.LackOfInventoryOfProduct;
                    }
                }
                else
                {
                    result.Message = ConstMessages.ObjectNotFound;
                }
            }else //if new count == 0
            {
                result = await DeleteProductFromUserShoppingCart(userId, productId);
            }
            return result;
        }

        public async Task<Result> DeleteProductFromUserShoppingCart(string userId, string productId)
        {
            var result = new Result();
            var domainName = base.GetCurrentDomainName();
            var domainEntity = _domainContext.Collection.Find(_ => _.DomainName == domainName).FirstOrDefault();
            var userCartEntity = _context.Collection
                .Find(_ => _.CreatorUserId == userId && !_.IsDeleted
                && _.IsActive && _.AssociatedDomainId == domainEntity.DomainId).FirstOrDefault();

            var productEntity = _productContext.ProductCollection.Find(_ => _.ProductId == productId).FirstOrDefault();
            if (userCartEntity != null)
            {
                var sellerObj = userCartEntity.Details.FirstOrDefault(_ => _.SellerId == productEntity.SellerUserId);
                int sellerIndex = userCartEntity.Details.IndexOf(sellerObj);
                if (sellerObj != null)
                {
                    var productRow = sellerObj.Products.FirstOrDefault(_ => _.ProductId == productId);
                    sellerObj.Products.Remove(productRow);
                    if(sellerObj.Products.Count == 0)
                    {
                        userCartEntity.Details.Remove(sellerObj);
                        if(userCartEntity.Details.Count == 0)
                        {
                            userCartEntity.IsActive = false;
                            userCartEntity.IsDeleted = true;
                        }
                    }else
                    {
                        userCartEntity.Details[sellerIndex] = sellerObj;
                    }
                   
                    var updateResult = await _context.Collection
                                .ReplaceOneAsync(_ => _.UserCartId == userCartEntity.UserCartId, userCartEntity);
                    if (updateResult.IsAcknowledged)
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
                    result.Message = ConstMessages.ObjectNotFound;
                }
            }else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public async Task<Result> DeleteWholeUserShoppingCart(string userId)
        {
            var result = new Result();
            var userCartEntity = _context.Collection
                 .Find(_ => _.CreatorUserId == userId && !_.IsDeleted).FirstOrDefault();
            if(userCartEntity != null)
            {
                userCartEntity.IsDeleted = true;
                var updateResult = await _context.Collection
                                .ReplaceOneAsync(_ => _.UserCartId == userCartEntity.UserCartId, userCartEntity);
                if (updateResult.IsAcknowledged)
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
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public async Task<Result<ShoppingCartDTO>> FetchActiveUserShoppingCart(string userId, string domainId)
        {
            var result = new Result<ShoppingCartDTO>();
            var dto = new ShoppingCartDTO();
            var userCartEntity = _context.Collection
                .Find(_ => _.CreatorUserId == userId && !_.IsDeleted &&
                      _.IsActive && _.AssociatedDomainId == domainId).FirstOrDefault();
            if(userCartEntity != null)
            {
                dto.ShoppingCartCulture = userCartEntity.ShoppingCartCulture;
                dto.UserCartId = userCartEntity.UserCartId;
                dto.DomainId = domainId;
                dto.OwnerId = userCartEntity.CreatorUserId;
                var perSellerList = new List<PurchasePerSeller>();
                result.ReturnValue.Details = new List<PurchasePerSellerDTO>();
                //each time we fetch cartshopping data should be updated in it
                foreach (var sellerPurchase in userCartEntity.Details)
                {
                    var obj = new PurchasePerSellerDTO();
                    obj.SellerId = sellerPurchase.SellerId;
                    obj.TransferType = sellerPurchase.TransferType;
                    //??? update transeferExpense if seller change it
                    //TODO
                    foreach (var pro in sellerPurchase.Products)
                    {
                        var productId = pro.ProductId;
                        var productEntity = _productContext.ProductCollection
                       .Find(_ => _.ProductId == productId).FirstOrDefault();
                        ShoppingCartDetailDTO det = new ShoppingCartDetailDTO();
                        if (productEntity != null && productEntity.IsActive 
                            && !productEntity.IsDeleted && productEntity.Inventory > 0 )
                        {
                            var activePriceValue = GetCurrentPrice(productEntity);
                            var discountPerUnit = await GetCurrentDiscountPerUnit(productEntity, activePriceValue);
                            det = new ShoppingCartDetailDTO
                            {
                                CreationDate = DateTime.Now,
                                CreatorUserId = userCartEntity.CreatorUserId,
                                CreatorUserName = userCartEntity.CreatorUserName,
                                ProductId = productId,
                                ProductName = pro.ProductName,
                                OrderCount = pro.OrderCount,
                                PricePerUnit = activePriceValue,
                                DiscountPricePerUnit = discountPerUnit.DiscountPerUnit,
                                TotalAmountToPay = (activePriceValue - discountPerUnit.DiscountPerUnit) * pro.OrderCount
                            };
                            decimal previousAmountPerUnit = pro.PricePerUnit - pro.DiscountPricePerUnit;
                            decimal currentAmountPerUnit = det.PricePerUnit - det.DiscountPricePerUnit;
                            if (previousAmountPerUnit != currentAmountPerUnit)
                            {
                                det.PreviousFinalPricePerUnit = previousAmountPerUnit; 
                                var difference = currentAmountPerUnit - previousAmountPerUnit ;
                                if (previousAmountPerUnit < currentAmountPerUnit)
                                {
                                    det.Notifications.Add(Language.GetString("AlertAndMessage_ProductPriceIncrease")
                                        .Replace("[0]", $"{difference} {userCartEntity.ShoppingCartCulture.CurrencySymbol}"));
                                }
                                else
                                {
                                    difference = previousAmountPerUnit - currentAmountPerUnit;
                                    det.Notifications.Add(Language.GetString("AlertAndMessage_ProductPriceDecrease")
                                        .Replace("[0]", $"{difference} {userCartEntity.ShoppingCartCulture.CurrencySymbol}"));
                                }
                            }

                            //check inventory if it is less than orderCount then change our order Count to our inventory
                            if (pro.OrderCount > productEntity.Inventory)
                            {
                                det.OrderCount = productEntity.Inventory;
                                det.Notifications.Add(Language.GetString("AlertAndMessage_ProductDecreaseInventory")
                                    .Replace("[0]", productEntity.Inventory.ToString()));
                            }
                           
                        }
                        else 
                        if(productEntity.IsDeleted || productEntity.Inventory == 0)
                        {
                            det = new ShoppingCartDetailDTO
                            {
                                CreationDate = DateTime.Now,
                                CreatorUserId = userCartEntity.CreatorUserId,
                                CreatorUserName = userCartEntity.CreatorUserName,
                                ProductId = productId,
                                ProductName = pro.ProductName,
                                OrderCount = 0,
                                PricePerUnit = 0,
                                DiscountPricePerUnit = 0,
                                TotalAmountToPay = 0,
                                PreviousOrderCount = pro.OrderCount,
                                PreviousFinalPricePerUnit = pro.PricePerUnit - pro.DiscountPricePerUnit
                            };
                            det.Notifications.Add(Language.GetString("AlertAndMessage_ProductNullCount"));
                        }
                        obj.Products.Add(det);
                    }
                    dto.Details.Add(obj);
                }

                //then update current shopping cart in database
                userCartEntity.Details = _mapper.Map<List<PurchasePerSeller>>(dto.Details);

                var updateResult =await _context.Collection
                    .ReplaceOneAsync(_ => _.UserCartId == userCartEntity.UserCartId, userCartEntity);
                if (updateResult.IsAcknowledged)
                {
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                    result.ReturnValue = dto;
                }
                else
                {
                    result.Message = ConstMessages.GeneralError;
                }
            }
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public async Task<Result> InsertUserShoppingCart(string userId)
        {
            var result = new Result();
            var userEntity = await _userManager.FindByIdAsync(userId);
            Entities.General.Currency.Currency defCurrencyEntity;
            Entities.General.Language.Language defLanguageEntity;
            var domainEntity = _domainContext.Collection.Find(_ => _.DomainName == GetCurrentDomainName()).FirstOrDefault();
            defLanguageEntity = _languageContext.Collection.Find(_ => _.LanguageId == (!string.IsNullOrWhiteSpace(userEntity.Profile.DefaultLanguageId) ?
            userEntity.Profile.DefaultLanguageId : domainEntity.DefaultLanguageId)).FirstOrDefault();
            defCurrencyEntity = _currencyContext.Collection.Find(_ => _.CurrencyId == (!string.IsNullOrWhiteSpace(userEntity.Profile.DefaultCurrencyId) ?
            userEntity.Profile.DefaultCurrencyId : domainEntity.DefaultCurrencyId)).FirstOrDefault();
           
            var userCartModel = new Entities.Shop.ShoppingCart.ShoppingCart()
            {
                CreationDate = DateTime.UtcNow,
                CreatorUserId = userId,
                CreatorUserName = GetUserName(),
                UserCartId = Guid.NewGuid().ToString(),
                ShoppingCartCulture = new EntityCulture()
                {
                    CurrencyId = defCurrencyEntity != null ? defCurrencyEntity.CurrencyId :
                        domainEntity.DefaultCurrencyId,
                    CurrencyName = defCurrencyEntity.CurrencyName,
                    CurrencyPrefix = defCurrencyEntity.Prefix,
                    CurrencySymbol = defCurrencyEntity.Symbol,
                    LanguageId = defLanguageEntity.LanguageId,
                    LanguageName = defLanguageEntity.LanguageName,
                    LanguageSymbol = defLanguageEntity.Symbol
                }
            };
            try
            {
                _context.Collection.InsertOne(userCartModel);
                result.Message = ConstMessages.SuccessfullyDone;
                result.Succeeded = true;
            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.GeneralError;
            }
            return result;
        }

        private decimal GetCurrentPrice(Entities.Shop.Product.Product product)
        {
            var availablePrice = product.Prices.Any(_ => _.IsActive &&
                _.StartDate <= DateTime.Now && (_.EndDate == null || _.EndDate <= DateTime.Now)) ?
                product.Prices.FirstOrDefault(_ => _.IsActive &&
                _.StartDate <= DateTime.Now && (_.EndDate == null || _.EndDate <= DateTime.Now)) : null;
            if(availablePrice != null)
            {
                return Convert.ToDecimal(availablePrice.PriceValue);
            }else
            {
                return 0;
            }
        }
        private async Task<DiscountModel> GetCurrentDiscountPerUnit(Entities.Shop.Product.Product product,
            decimal priceValue)
        {
            var res = new DiscountModel();
            var activePromotion = await GetActivePromotionOfThisProduct(product);
            res.DiscountPerUnit = 0;
            if (activePromotion != null)
            {
                switch (activePromotion.DiscountType)
                {
                    case Entities.Shop.Promotion.DiscountType.Fixed:
                        res.DiscountPerUnit = activePromotion.Value.Value;
                        break;
                    case Entities.Shop.Promotion.DiscountType.Percentage:
                        res.DiscountPerUnit = (priceValue * activePromotion.Value.Value) / 100;
                        break;
                    case Entities.Shop.Promotion.DiscountType.Product:
                        res.DiscountPerUnit = 0;
                        res.PraisedProductId = activePromotion.PromotedProductId;
                        res.PraisedProductCount = activePromotion.PromotedCountofUnit.HasValue ?
                            activePromotion.PromotedCountofUnit.Value : 1;
                        break;
                    default:
                        break;
                }
            }
            return res;
        }

        private async Task<Entities.Shop.Promotion.Promotion> GetActivePromotionOfThisProduct(Entities.Shop.Product.Product product)
        {
            //maybe we have several promotions which is assign to product
            //promotion on all product promotion of productGroup
            //promotion of this product if it is so we should select latest promotion
            Entities.Shop.Promotion.Promotion promotionOnAll;
            Entities.Shop.Promotion.Promotion promotionOnThisProductGroup;
            Entities.Shop.Promotion.Promotion promotionOnThisProduct;
            List<Entities.Shop.Promotion.Promotion> promotionList = new List<Entities.Shop.Promotion.Promotion>();
            Entities.Shop.Promotion.Promotion finalPromotion = null;

            var currentDomainEntity = _domainContext.Collection.Find(_ => _.DomainName == GetCurrentDomainName()).FirstOrDefault();
            var sellerUserEntity = await _userManager.FindByIdAsync(product.SellerUserId);
            string searchDomainId = "";
            if (currentDomainEntity.IsDefault)//we should calculate the promotion of sellerDomain
            {
                if(sellerUserEntity.IsSystemAccount)
                {
                    searchDomainId = currentDomainEntity.DomainId;
                }
                else
                {
                    searchDomainId = product.AssociatedDomainId;
                }
            }
            else
            {
                //it isnt main domain then surely the seller is owner of domain and it isnt systemaccount
                searchDomainId = currentDomainEntity.DomainId; //product.AssociatedDomainId are the same
            }
            promotionOnAll = _promotionContext.Collection
                        .Find(_ => _.PromotionType == Entities.Shop.Promotion.PromotionType.All &&
                        _.AssociatedDomainId == searchDomainId && _.IsActive && !_.IsDeleted &&
                        _.StartDate <= DateTime.Now && (_.EndDate == null || _.EndDate >= DateTime.Now)).Any() ?
                        _promotionContext.Collection
                        .Find(_ => _.PromotionType == Entities.Shop.Promotion.PromotionType.All &&
                        _.AssociatedDomainId == searchDomainId && _.IsActive && !_.IsDeleted &&
                        _.StartDate <= DateTime.Now && (_.EndDate == null || _.EndDate >= DateTime.Now)).First() : null;

            promotionOnThisProductGroup = _promotionContext.Collection
                .Find(_ => _.PromotionType == Entities.Shop.Promotion.PromotionType.Group &&
                _.Infoes.Select(a => a.AffectedProductGroupId).Intersect(product.GroupIds).Any() &&
                _.AssociatedDomainId == searchDomainId && _.IsActive && !_.IsDeleted &&
                _.StartDate <= DateTime.Now && (_.EndDate == null || _.EndDate >= DateTime.Now)).Any() ?
                _promotionContext.Collection
                .Find(_ => _.PromotionType == Entities.Shop.Promotion.PromotionType.Group &&
                _.Infoes.Select(a => a.AffectedProductGroupId).Intersect(product.GroupIds).Any() &&
                _.AssociatedDomainId == searchDomainId && _.IsActive && !_.IsDeleted &&
                _.StartDate <= DateTime.Now && (_.EndDate == null || _.EndDate >= DateTime.Now)).First() : null;

            promotionOnThisProduct = product.Promotion != null && product.Promotion.IsActive && !product.Promotion.IsDeleted
                 && product.Promotion.StartDate >= DateTime.Now &&
                 (product.Promotion.EndDate == null || product.Promotion.EndDate >= DateTime.Now) ? product.Promotion : null;

            if (promotionOnAll != null)
                promotionList.Add(promotionOnAll);
            if (promotionOnThisProductGroup != null)
                promotionList.Add(promotionOnThisProductGroup);
            if (promotionOnThisProduct != null)
                promotionList.Add(promotionOnThisProduct);

            if (promotionList.Any())
            {
                finalPromotion = promotionList.OrderByDescending(_ => _.StartDate).First();
            }
            return finalPromotion;
        }

        public bool AddProductToCart(string productId)
        {
            throw new NotImplementedException();
        }
    }
}
