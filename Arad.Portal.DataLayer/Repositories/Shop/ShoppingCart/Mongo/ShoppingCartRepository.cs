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

namespace Arad.Portal.DataLayer.Repositories.Shop.ShoppingCart.Mongo
{
    public class ShoppingCartRepository : BaseRepository, IShoppingCartRepository
    {
        private readonly ShoppingCartContext _context;
        private readonly ProductContext _productContext;
        public ShoppingCartRepository(IHttpContextAccessor httpContextAccessor,
            ShoppingCartContext context,
            ProductContext productContext)
            : base(httpContextAccessor)
        {
            _context = context;
            _productContext = productContext;
        }
        public RepositoryOperationResult<string> FindCurrentUserShoppingCart(string userId)
        {
            var result = new RepositoryOperationResult<string>();
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
        public async Task<RepositoryOperationResult> AddProductToUserCart(ShoppingCartProductDTO productDto)
        {
            var result = new RepositoryOperationResult();
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
                        ShoppingCartDetailId = Guid.NewGuid().ToString(),
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

        public async Task<RepositoryOperationResult> ChangeProductCountInUserCart(string userId, string productId, int newCount)
        {
            var result = new RepositoryOperationResult();
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

        public async Task<RepositoryOperationResult> DeleteProductFromUserShoppingCart(string userId, string productId)
        {
            var result = new RepositoryOperationResult();
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

        public async Task<RepositoryOperationResult> DeleteWholeUserShoppingCart(string userId)
        {
            var result = new RepositoryOperationResult();
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

        public async Task<RepositoryOperationResult> RefreshUserShoppingCart(string userId)
        {
            var result = new RepositoryOperationResult();
            var userCartEntity = _context.Collection
                .Find(_ => _.CreatorUserId == userId && !_.IsDeleted).FirstOrDefault();
            if(userCartEntity != null)
            {
                var details = new List<ShoppingCartDetail>();
                foreach (var detail in userCartEntity.Details)
                {
                    var productId = detail.ProductId;
                    var productEntity = _productContext.ProductCollection
                        .Find(_ => _.ProductId == productId).FirstOrDefault();
                    if(productEntity != null)
                    {
                        var activePriceValue = GetCurrentPrice(productEntity);
                        var discountPerUnit = GetCurrentDiscountPerUnit(productEntity, activePriceValue);
                        var det = new ShoppingCartDetail
                        {
                            ShoppingCartDetailId = Guid.NewGuid().ToString(),
                            CreationDate = DateTime.Now,
                            CreatorUserId = userCartEntity.CreatorUserId,
                            CreatorUserName = userCartEntity.CreatorUserName,
                            ProductId = productId,
                            ProductName = detail.ProductName,
                            OrderCount = detail.OrderCount,
                            PricePerUnit = activePriceValue,
                            DiscountPricePerUnit = discountPerUnit,
                            TotalAmountToPay = (activePriceValue - discountPerUnit) * detail.OrderCount
                        };
                       
                        //check inventory if it is less than orderCount then change our order Count to our inventory
                        if (detail.OrderCount > productEntity.Inventory)
                        {
                            det.OrderCount = productEntity.Inventory;
                        }

                        details.Add(det);
                    }
                }
                userCartEntity.Details = details;
                var updateResult =await _context.Collection
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
            }else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public RepositoryOperationResult InsertUserShoppingCart(string userId)
        {
            var result = new RepositoryOperationResult();
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
            var availablePrice = product.Prices
                .FirstOrDefault(_ => _.IsActive && 
                _.StartDate <= DateTime.UtcNow && _.EndDate == null);
            if(availablePrice != null)
            {
                return availablePrice.PriceValue;
            }else
            {
                return 0;
            }
        }
        private decimal GetCurrentDiscountPerUnit(Entities.Shop.Product.Product product,decimal priceValue)
        {
            decimal discountPerUnit = 0;
            var promotion = product.Promotion;
            var isValid = promotion.StartDate >= DateTime.UtcNow && promotion.EndDate == null;
            if (isValid)
            {
                switch (promotion.DiscountType)
                {
                    case Entities.Shop.Promotion.DiscountType.Fixed:
                        discountPerUnit = promotion.Value;
                        break;
                    case Entities.Shop.Promotion.DiscountType.Percentage:
                        discountPerUnit = (priceValue * promotion.Value) / 100;
                        break;
                    default:
                        break;
                }
            }
            return discountPerUnit;
        }
    }
}
