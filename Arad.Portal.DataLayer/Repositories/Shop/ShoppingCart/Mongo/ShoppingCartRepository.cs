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

namespace Arad.Portal.DataLayer.Repositories.Shop.ShoppingCart.Mongo
{
    public class ShoppingCartRepository : BaseRepository, IShoppingCartRepository
    {
        private readonly ShoppingCartContext _context;
        private readonly DomainContext _domainContext;
        private readonly ProductContext _productContext;
        private readonly PromotionContext _promotionContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public ShoppingCartRepository(IHttpContextAccessor httpContextAccessor,
            ShoppingCartContext context,
            ProductContext productContext,
            PromotionContext promotionContext,
            DomainContext domainContext,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
            : base(httpContextAccessor)
        {
            _context = context;
            _productContext = productContext;
            _domainContext = domainContext;
            _promotionContext = promotionContext;
            _userManager = userManager;
            _mapper = mapper;
        }
        public Result<string> FindCurrentUserShoppingCart(string userId)
        {
            var result = new Result<string>();
            var entity = _context.Collection
                .Find(_ => _.CreatorUserId == userId && !_.IsDeleted).FirstOrDefault();

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
        public async Task<Result> AddProductToUserCart(ShoppingCartProductDTO productDto)
        {
            var result = new Result();
            var productEntity = _productContext.ProductCollection
                .Find(_ => _.ProductId == productDto.ProductId).FirstOrDefault();

            var userCartEntity = _context.Collection
                .Find(_ => _.CreatorUserId == productDto.UserId && !_.IsDeleted).FirstOrDefault();

            if (userCartEntity == null)
            {
                var res = InsertUserShoppingCart(productDto.UserId);
                if (res.Succeeded)
                {
                    userCartEntity = _context.Collection
                    .Find(_ => _.CreatorUserId == productDto.UserId && !_.IsDeleted).FirstOrDefault();
                }
            }

            if (productEntity != null)
            {
                decimal discountPerUnit = 0;
                var CurrentPriceValue = GetCurrentPrice(productEntity);
              

                var productPromotion = productEntity.Promotion;

                discountPerUnit = GetCurrentDiscountPerUnit(productEntity, CurrentPriceValue);

                if (productEntity.Inventory > 0 &&
                    productEntity.Inventory >= productDto.OrderCount)
                {

                    userCartEntity.Details.Add(new ShoppingCartDetail
                    {
                       
                        CreationDate = DateTime.Now,
                        CreatorUserId = productDto.UserId,
                        CreatorUserName = productDto.UserName,
                        ProductId = productDto.ProductId,
                        ProductName = productDto.ProductName,
                        OrderCount = productDto.OrderCount,
                        PricePerUnit = CurrentPriceValue,
                        DiscountPricePerUnit = discountPerUnit,
                        TotalAmountToPay = (CurrentPriceValue - discountPerUnit) * productDto.OrderCount
                    });

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
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public async Task<Result> ChangeProductCountInUserCart(string userId, string productId, int newCount)
        {
            var result = new Result();
            var userCartEntity = _context.Collection
                .Find(_ => _.CreatorUserId == userId && !_.IsDeleted).FirstOrDefault();
            if (userCartEntity != null)
            {
                var detailList = userCartEntity.Details;
                var currentDetail = detailList.FirstOrDefault(_ => _.ProductId == productId);
                var productEntity = _productContext
                    .ProductCollection.Find(_ => _.ProductId == productId).FirstOrDefault();
                if (productEntity != null)
                {
                    if (productEntity.Inventory > 0 && productEntity.Inventory >= newCount)
                    {
                        if (currentDetail != null)
                        {
                            detailList.FirstOrDefault(_ => _.ProductId == productId).OrderCount = newCount;
                            userCartEntity.Details = detailList;
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
            }
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public async Task<Result> DeleteProductFromUserShoppingCart(string userId, string productId)
        {
            var result = new Result();
            var userCartEntity = _context.Collection
                .Find(_ => _.CreatorUserId == userId && !_.IsDeleted).FirstOrDefault();
            if (userCartEntity != null)
            {
                var detailObj = userCartEntity.Details.FirstOrDefault(_ => _.ProductId == productId);
                if (detailObj != null)
                {
                    userCartEntity.Details.Remove(detailObj);
                   
                    if(userCartEntity.Details.Count == 0) //there is nothing in userCart then delete the userCart
                    {
                        userCartEntity.IsDeleted = true;
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

                            if(pro.DiscountPricePerUnit != det.DiscountPricePerUnit)
                            {

                            }

                            //check inventory if it is less than orderCount then change our order Count to our inventory
                            if (pro.OrderCount > productEntity.Inventory)
                            {
                                det.OrderCount = productEntity.Inventory;
                                det.Notifications.Add(Language.GetString("AlertAndMessage_ProductDecreaseInventory").Replace("[0]", productEntity.Inventory.ToString()));
                                
                            }
                           
                        }else 
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
                                Notification = Language.GetString("AlertAndMessage_ProductNullCount"),
                                PreviousDiscount = pro.DiscountPricePerUnit,
                                PreviousOrderCount = pro.OrderCount,
                                PreviousPrice = pro.PricePerUnit
                            };

                        }
                        obj.Products.Add(det);
                    }
                    dto.Details.Add(obj);
                }

                //then update current shopping cart in database
                userCartEntity.Details = dto.Details;
                var updateResult =await _context.Collection
                    .ReplaceOneAsync(_ => _.UserCartId == userCartEntity.UserCartId, userCartEntity);
                if (updateResult.IsAcknowledged)
                {
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                    result.ReturnValue = _mapper.Map<ShoppingCartDTO>(userCartEntity);
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

        public Result InsertUserShoppingCart(string userId)
        {
            var result = new Result();
            var userCartModel = new Entities.Shop.ShoppingCart.ShoppingCart()
            {
                CreationDate = DateTime.UtcNow,
                CreatorUserId = userId,
                CreatorUserName = GetUserName(),
                UserCartId = Guid.NewGuid().ToString()
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
    }
}
