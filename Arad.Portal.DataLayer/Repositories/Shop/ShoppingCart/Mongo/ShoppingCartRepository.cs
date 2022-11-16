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
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Repositories.Shop.Setting.Mongo;
using Arad.Portal.DataLayer.Helpers;
using static Arad.Portal.DataLayer.Models.Shared.Enums;
using System.Globalization;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Entities.Shop.Product;

namespace Arad.Portal.DataLayer.Repositories.Shop.ShoppingCart.Mongo
{
    public class ShoppingCartRepository : BaseRepository, IShoppingCartRepository
    {
        private readonly ShoppingCartContext _context;
        private readonly DomainContext _domainContext;
        private readonly ShippingSettingContext _shippingContext;
        private readonly LanguageContext _languageContext;
        private readonly CurrencyContext _currencyContext;
        private readonly ProductContext _productContext;
        private readonly PromotionContext _promotionContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CreateNotification _createNotification;
        public ShoppingCartRepository(IHttpContextAccessor httpContextAccessor,
            ShoppingCartContext context,
            ProductContext productContext,
            PromotionContext promotionContext,
            DomainContext domainContext,
            LanguageContext languageContext,
            CurrencyContext currencyContext,
            UserManager<ApplicationUser> userManager,
            ShippingSettingContext shippingSettingContext,
            IWebHostEnvironment env,
            CreateNotification createNotification,
            IMapper mapper)
            : base(httpContextAccessor, env)
        {
            _context = context;
            _productContext = productContext;
            _domainContext = domainContext;
            _promotionContext = promotionContext;
            _userManager = userManager;
            _currencyContext = currencyContext;
            _mapper = mapper;
            _languageContext = languageContext;
            _shippingContext = shippingSettingContext;
            _createNotification = createNotification;
        }
        public Result<string> FindCurrentUserShoppingCart(string userId, string domainId)
        {
            var result = new Result<string>();
            var entity = _context.Collection
                .Find(_ => _.CreatorUserId == userId && !_.IsDeleted && _.IsActive && _.AssociatedDomainId == domainId).FirstOrDefault();

            if (entity != null)
            {
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
                result.ReturnValue = entity.ShoppingCartId;
            }
            else
            {
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
                result.ReturnValue = Guid.NewGuid().ToString();
            }
            return result;
        }
        private InventoryDetail FindProductSpecValuesRecord(string productId, List<SpecValue> specValues)
        {
            var entity = _productContext.ProductCollection.Find(_ => _.ProductId == productId).FirstOrDefault();
            InventoryDetail inventoryDetail = null;
            List<InventoryDetail> lst = entity.Inventory;
            foreach (var item in specValues)
            {
                lst = lst.Where(_ => _.SpecValues.Any(a => a.SpecificationId == item.SpecificationId &&
                                      a.SpecificationValue == item.SpecificationValue)).ToList();
            }
            if (lst.Count > 0) //surely lst have just one
            {
                inventoryDetail = lst[0];
            }
            return inventoryDetail;
        }
        public async Task<Result<CartItemsCount>> AddOrChangeProductToUserCart(string productId, int orderCount, List<SpecValue> specValues)
        {
            var result = new Result<CartItemsCount>();
            result.ReturnValue = new CartItemsCount();
            var productEntity = _productContext.ProductCollection
                .Find(_ => _.ProductId == productId && !_.IsDeleted).FirstOrDefault();
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
                    var res = await ChangeProductCountInUserCart(userId, productId, orderCount, specValues);
                    result.Message = res.Message;
                    result.Succeeded = res.Succeeded;
                }
                else
                {
                    var currentPriceValue = GetCurrentPrice(productEntity);
                    var res = await GetCurrentDiscountPerUnit(productEntity, currentPriceValue);

                    if (productEntity.Inventory.Sum(_=>_.Count) > 0)
                    {
                        var inventoryDetail = FindProductSpecValuesRecord(productId, specValues);
                        int finalOrderCnt = inventoryDetail.Count > orderCount ? orderCount : inventoryDetail.Count;
                        var shopCartDetail = new ShoppingCartDetail()
                        {
                            ShoppingCartDetailId = Guid.NewGuid().ToString(),
                            ProductId = productEntity.ProductId,
                            ProductName = productEntity
                                .MultiLingualProperties.FirstOrDefault(a => a.LanguageId == userCartEntity.ShoppingCartCulture.LanguageId).Name,
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
                            var sellerEntity = await _userManager.FindByIdAsync(productEntity.SellerUserId);
                            var sellerDomainEntity = _domainContext.Collection.Find(_ => _.DomainId == sellerEntity.DomainId).FirstOrDefault();
                           
                            var relatedDomain = "";
                            if(domainEntity.IsDefault)
                            {
                                relatedDomain = domainEntity.DomainId;
                            }else
                            {
                                relatedDomain = sellerDomainEntity.DomainId;
                            }
                            Entities.Shop.Setting.ShippingSetting settingEntity = null;
                            FilterDefinitionBuilder<Entities.Shop.Setting.ShippingSetting> shippingBuilder = new();
                            FilterDefinitionBuilder<Entities.Shop.Setting.ShippingTypeDetail> detailBuilder = new();
                            FilterDefinition<Entities.Shop.Setting.ShippingSetting> shippingFilterDef = null;
                            FilterDefinition<Entities.Shop.Setting.ShippingTypeDetail> detailFilterDef = null;
                            detailFilterDef = detailBuilder.Eq(nameof(Entities.Shop.Setting.ShippingTypeDetail.HasFixedExpense), true);
                            shippingFilterDef = shippingBuilder.Eq(nameof(Entities.Shop.Setting.ShippingSetting.AssociatedDomainId), relatedDomain);
                            settingEntity = _shippingContext.Collection.Find(shippingFilterDef).FirstOrDefault();


                            var finalShippingTypeId = domainEntity.IsDefault ? domainEntity.DefaultShippingTypeId : sellerDomainEntity.DefaultShippingTypeId;
                            var purchase = new PurchasePerSeller()
                            {
                                SellerId = productEntity.SellerUserId,
                                SellerUserName = sellerEntity.Profile.FullName,
                                Products = new(),
                                ShippingTypeId = finalShippingTypeId,
                                ShippingExpense = settingEntity != null ? settingEntity.AllowedShippingTypes.FirstOrDefault(a => a.ShippingTypeId == finalShippingTypeId && a.HasFixedExpense).FixedExpenseValue : 0
                            };
                      
                            purchase.Products.Add(shopCartDetail);
                            userCartEntity.Details.Add(purchase);
                        }

                        var updateResult = await _context.Collection
                            .ReplaceOneAsync(_ => _.ShoppingCartId == userCartEntity.ShoppingCartId, userCartEntity);
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
                result.ReturnValue.ItemsCount = GetItemCountsInCart(userCartEntity);
            }
            else
            {
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            return result;
        }
        public int GetItemCountsInCart(Entities.Shop.ShoppingCart.ShoppingCart entity)
        {
            var totalCount = 0;
            foreach (var item in entity.Details)
            {
                var productCount = item.Products.Where(_ => !_.IsDeleted).Count();
                totalCount += productCount;
            }
            return totalCount;
        }
        public async Task<Result> ChangeProductCountInUserCart(string userId, string productId, int newCount, List<SpecValue> specValues)
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
                var inventoryDetail = FindProductSpecValuesRecord(productId, specValues);
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
                
                    //surely this product added before to shoppingcart then it has this record
                    var sellerObj = userCartEntity.Details.Where(_ => _.SellerId == productEntity.SellerUserId).FirstOrDefault();
                    var productRow = sellerObj.Products.FirstOrDefault(_ => _.ProductId == productId);

                    var index = sellerObj.Products.IndexOf(productRow);
                    if (inventoryDetail.Count > 0 && inventoryDetail.Count >= newCount)
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
                                .ReplaceOneAsync(_ => _.ShoppingCartId == userCartEntity.ShoppingCartId, userCartEntity);
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
                            result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
                        }
                    }
                    else
                    {
                        result.Message = ConstMessages.LackOfInventoryOfProduct;
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
                                .ReplaceOneAsync(_ => _.ShoppingCartId == userCartEntity.ShoppingCartId, userCartEntity);
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
                    result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
                }
            }else
            {
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            return result;
        }

        public async Task<Result> DeleteShoppingCartItem(string shoppingCartId, string shoppingCartDetailId)
        {
            var result = new Result();
            var userCart = _context.Collection.Find(_ => _.ShoppingCartId == shoppingCartId).FirstOrDefault();

            var item = userCart.Details.SelectMany(_ => _.Products).ToList().FirstOrDefault(_ => _.ShoppingCartDetailId == shoppingCartDetailId);
            item.IsDeleted = true;
            //var res = userCart.Details.SelectMany(_ => _.Products).ToList().Remove(item);

            var updateResult = await _context.Collection.ReplaceOneAsync(_ => _.ShoppingCartId == shoppingCartId, userCart);
            if (updateResult.IsAcknowledged)
            {
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            else
            {
                result.Succeeded = false;
                result.Message = ConstMessages.ErrorInSaving;
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
                userCartEntity.IsActive = false;
                var updateResult = await _context.Collection
                                .ReplaceOneAsync(_ => _.ShoppingCartId == userCartEntity.ShoppingCartId, userCartEntity);
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
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            return result;
        }

        public async Task<Result<ShoppingCartDTO>> FetchActiveUserShoppingCart(string userId, string domainId)
        {
            var result = new Result<ShoppingCartDTO>();
            result.ReturnValue = new ShoppingCartDTO();
            var dto = new ShoppingCartDTO();
            Entities.Shop.ShoppingCart.ShoppingCart userCartEntity = null;
             if(!_context.Collection
                .Find(_ => _.CreatorUserId == userId && !_.IsDeleted &&
                      _.IsActive && _.AssociatedDomainId == domainId).Any())
            {
                var res = await InsertUserShoppingCart(userId);
            }

            userCartEntity = _context.Collection
                .Find(_ => _.CreatorUserId == userId && !_.IsDeleted &&
                      _.IsActive && _.AssociatedDomainId == domainId).FirstOrDefault();

            //surely we have an active instance of ShoppingCart
                dto.ShoppingCartCulture = userCartEntity.ShoppingCartCulture;
                dto.ShoppingCartId = userCartEntity.ShoppingCartId;
                dto.DomainId = domainId;
                dto.OwnerId = userCartEntity.CreatorUserId;
                var perSellerList = new List<PurchasePerSeller>();
                result.ReturnValue.Details = new List<PurchasePerSellerDTO>();
                int rowNumber = 1;
                decimal finalPaymentPrice = 0;

                //each time we fetch shoppingCart data should be updated in it
                foreach (var sellerPurchase in userCartEntity.Details)
                {
                    var obj = new PurchasePerSellerDTO();
                    obj.SellerId = sellerPurchase.SellerId;
                    decimal sellerFactor = 0;
                    obj.SellerUserName = sellerPurchase.SellerUserName;
                    obj.ShippingTypeId = sellerPurchase.ShippingTypeId;
                   obj.ShippingExpense = sellerPurchase.ShippingExpense;
                    //??? update shippingExpense if seller change it
                    //TODO
                    finalPaymentPrice += sellerPurchase.ShippingExpense;
                    sellerFactor += sellerPurchase.ShippingExpense;
                    foreach (var pro in sellerPurchase.Products.Where(_=>!_.IsDeleted))
                    {
                        var productId = pro.ProductId;
                        var productEntity = _productContext.ProductCollection
                       .Find(_ => _.ProductId == productId).FirstOrDefault();
                       var inventoryDetail = FindProductSpecValuesRecord(productId, pro.ProductSpecValues);
                        ShoppingCartDetailDTO det = new ShoppingCartDetailDTO();
                        if (productEntity != null && productEntity.IsActive 
                            && !productEntity.IsDeleted && inventoryDetail != null && inventoryDetail.Count > 0 )
                        {
                            var activePriceValue = GetCurrentPrice(productEntity);
                            var discountPerUnit = await GetCurrentDiscountPerUnit(productEntity, activePriceValue);
                            det = new ShoppingCartDetailDTO
                            {
                                RowNumber = rowNumber,
                                ProductCode = productEntity.ProductCode,
                                CreationDate = DateTime.Now,
                                CreatorUserId = userCartEntity.CreatorUserId,
                                CreatorUserName = userCartEntity.CreatorUserName,
                                ProductId = productId,
                                ProductName = pro.ProductName,
                                ShoppingCartDetailId = pro.ShoppingCartDetailId,
                                OrderCount =  pro.OrderCount,
                                PricePerUnit = activePriceValue,
                                DiscountPricePerUnit = discountPerUnit.DiscountPerUnit,
                                TotalAmountToPay = (activePriceValue - discountPerUnit.DiscountPerUnit) * pro.OrderCount,
                                ProductImage = productEntity.Images.Any(_=>_.IsMain) ? 
                                               productEntity.Images.FirstOrDefault(_ => _.IsMain) : productEntity.Images.FirstOrDefault()
                            };
                            finalPaymentPrice += det.TotalAmountToPay;
                            sellerFactor += det.TotalAmountToPay;

                            #region changing in final amount per unit
                            decimal previousAmountPerUnit = pro.PricePerUnit - pro.DiscountPricePerUnit;
                            decimal currentAmountPerUnit = det.PricePerUnit - det.DiscountPricePerUnit;

                            if (previousAmountPerUnit != currentAmountPerUnit)
                            {
                                det.PreviousFinalPricePerUnit = previousAmountPerUnit; 
                                var difference = (currentAmountPerUnit >= previousAmountPerUnit ) ? (currentAmountPerUnit - previousAmountPerUnit) :
                                                (previousAmountPerUnit - currentAmountPerUnit);
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
                            #endregion changing in final amount pre unit


                            //check inventory if it is less than orderCount then change our order Count to our inventory
                            if (pro.OrderCount > inventoryDetail.Count)
                            {
                                det.OrderCount = inventoryDetail.Count;

                                det.Notifications.Add(Language.GetString("AlertAndMessage_ProductDecreaseInventory")
                                    .Replace("[0]", productEntity.Inventory.ToString()));
                            }
                           
                        }
                        else 
                        if(productEntity.IsDeleted || inventoryDetail.Count == 0)
                        {
                            det = new ShoppingCartDetailDTO
                            {
                                RowNumber = rowNumber,
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
                        rowNumber += 1;
                        obj.Products.Add(det);
                    }
                    obj.TotalDetailsAmountWithShipping = sellerFactor;
                    dto.Details.Add(obj);
                }

            //then update current shopping cart in database
            userCartEntity.Details = new List<PurchasePerSeller>();
            foreach (var obj in dto.Details)
            {
                userCartEntity.Details.Add(new PurchasePerSeller()
                {
                    Products = _mapper.Map<List<ShoppingCartDetail>>(obj.Products),
                    SellerId = obj.SellerId,
                    SellerUserName = obj.SellerUserName,
                    ShippingExpense = Convert.ToInt64(obj.ShippingExpense),
                    ShippingTypeId = obj.ShippingTypeId,
                    TotalDetailsAmountWithShipping = Convert.ToInt64(obj.TotalDetailsAmountWithShipping)
                });
            }
                //userCartEntity.Details = _mapper.Map<List<PurchasePerSeller>>(dto.Details);
                dto.FinalPriceForPay = finalPaymentPrice;
                var updateResult =await _context.Collection
                    .ReplaceOneAsync(_ => _.ShoppingCartId == userCartEntity.ShoppingCartId, userCartEntity);
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
                ShoppingCartId = Guid.NewGuid().ToString(),
                IsActive = true,
                IsDeleted = false,
                AssociatedDomainId = domainEntity.DomainId,
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

        private long GetCurrentPrice(Entities.Shop.Product.Product product)
        {
            var availablePrice = product.Prices.Any(_ => _.IsActive &&
                _.StartDate <= DateTime.Now && (_.EndDate == null || _.EndDate >= DateTime.Now)) ?
                product.Prices.FirstOrDefault(_ => _.IsActive &&
                _.StartDate <= DateTime.Now && (_.EndDate == null || _.EndDate >= DateTime.Now)) : null;
            if(availablePrice != null)
            {
                return Convert.ToInt64(availablePrice.PriceValue);
            }else
            {
                return 0;
            }
        }
        private async Task<DiscountModel> GetCurrentDiscountPerUnit(Entities.Shop.Product.Product product,
            long priceValue)
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
            Entities.Shop.Promotion.Promotion promotionOnAll = null;
            Entities.Shop.Promotion.Promotion promotionOnThisProductGroup = null;
            Entities.Shop.Promotion.Promotion promotionOnThisProduct = null;
            List<Entities.Shop.Promotion.Promotion> promotionList = new List<Entities.Shop.Promotion.Promotion>();
            Entities.Shop.Promotion.Promotion finalPromotion = null;

            var currentDomainEntity = _domainContext.Collection.Find(_ => _.DomainName == GetCurrentDomainName()).FirstOrDefault();
            var sellerUserEntity = await _userManager.FindByIdAsync(product.SellerUserId);
            string searchDomainId = "";
            if (currentDomainEntity.IsDefault)//we should calculate the promotion of sellerDomain
            {
                if(sellerUserEntity != null)
                {
                    if (sellerUserEntity.IsSystemAccount)
                    {
                        searchDomainId = currentDomainEntity.DomainId;
                    }
                    else
                    {
                        searchDomainId = product.AssociatedDomainId;
                    }
                }else
                {
                    return null;
                }
                
            }
            else
            {
                //it isnt main domain then surely the seller is owner of domain and it isnt systemaccount
                searchDomainId = currentDomainEntity.DomainId; //product.AssociatedDomainId are the same
            }
            try
            {
                promotionOnAll = _promotionContext.Collection
                        .Find(_ => _.PromotionType == Entities.Shop.Promotion.PromotionType.All &&
                        _.AssociatedDomainId == searchDomainId && _.IsActive && !_.IsDeleted &&
                        _.SDate <= DateTime.Now && (_.EDate == null || _.EDate >= DateTime.Now)).Any() ?
                        _promotionContext.Collection
                        .Find(_ => _.PromotionType == Entities.Shop.Promotion.PromotionType.All &&
                        _.AssociatedDomainId == searchDomainId && _.IsActive && !_.IsDeleted &&
                        _.SDate <= DateTime.Now && (_.EDate == null || _.EDate >= DateTime.Now)).FirstOrDefault() : null;

                var groupIds = product.GroupIds;

                //var filter = Builders<Entities.Shop.Promotion.Promotion>.Filter.Empty;
                //filter &= Builders<Entities.Shop.Promotion.Promotion>.Filter.Eq(x => x.PromotionType, Entities.Shop.Promotion.PromotionType.Group);
                //filter &= Builders<Entities.Shop.Promotion.Promotion>.Filter.Eq(x => x.IsActive,true);
                //filter &= Builders<Entities.Shop.Promotion.Promotion>.Filter.Eq(x => x.AssociatedDomainId,searchDomainId);
                //filter &= Builders<Entities.Shop.Promotion.Promotion>.Filter.Ne(x => x.IsDeleted,true);
                //filter &= Builders<Entities.Shop.Promotion.Promotion>.Filter.Lte(x => x.SDate, DateTime.Now);
                
                //filter &= Builders<Entities.Shop.Promotion.Promotion>.Filter.Eq(x => x.PromotionType, Entities.Shop.Promotion.PromotionType.Group);
                //filter &= Builders<Entities.Shop.Promotion.Promotion>.Filter.AnyIn(x => x.Infoes.Select(a => a.AffectedProductGroupId), groupIds);
                //filter &= Builders<Entities.Shop.Promotion.Promotion>.Filter.Lte(x => x.SDate, DateTime.Now);
                //var timeFilter = Builders<Entities.Shop.Promotion.Promotion>.Filter.Or(Builders<Entities.Shop.Promotion.Promotion>.Filter.Gte(x => x.EDate, DateTime.Now)
                //    , Builders<Entities.Shop.Promotion.Promotion>.Filter.Eq(x => x.EDate, null));

                //filter &= timeFilter;

            //    promotionOnThisProductGroup = await _promotionContext.Collection.Find(filter).FirstOrDefaultAsync();


                promotionOnThisProductGroup = _promotionContext.Collection.AsQueryable()
                    .FirstOrDefault(_ => _.PromotionType == Entities.Shop.Promotion.PromotionType.Group &&
                    _.Infoes.Any(a => groupIds.Contains(a.AffectedProductGroupId))&&
                    _.AssociatedDomainId == searchDomainId && _.IsActive && !_.IsDeleted &&
                    _.SDate <= DateTime.Now && (_.EDate == null || _.EDate >= DateTime.Now));


                promotionOnThisProduct = product.Promotion != null && product.Promotion.IsActive && !product.Promotion.IsDeleted
                     && product.Promotion.SDate <= DateTime.Now &&
                     (product.Promotion.EDate == null || product.Promotion.EDate >= DateTime.Now) ? product.Promotion : null;
            }
            catch (Exception ex)
            {

            }


            if (promotionOnAll != null)
                promotionList.Add(promotionOnAll);
            if (promotionOnThisProductGroup != null)
                promotionList.Add(promotionOnThisProductGroup);
            if (promotionOnThisProduct != null)
                promotionList.Add(promotionOnThisProduct);

            if (promotionList.Any())
            {
                finalPromotion = promotionList.OrderByDescending(_ => _.SDate).FirstOrDefault();
            }
            return finalPromotion;
        }

        public bool AddProductToCart(string productId)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<Entities.Shop.ShoppingCart.ShoppingCart>> FetchUserShoppingCart(string userCartId)
        {
            var result = new Result<Entities.Shop.ShoppingCart.ShoppingCart>();
            result.ReturnValue = new Entities.Shop.ShoppingCart.ShoppingCart();
            var model = new Entities.Shop.ShoppingCart.ShoppingCart();
            var userCartEntity = _context.Collection
                .Find(_ => _.ShoppingCartId == userCartId).FirstOrDefault();
            if (userCartEntity != null)
            {
                model.ShoppingCartCulture = userCartEntity.ShoppingCartCulture;
                model.ShoppingCartId = userCartEntity.ShoppingCartId;
                model.AssociatedDomainId = userCartEntity.AssociatedDomainId;
                model.CreatorUserId = userCartEntity.CreatorUserId;
                var perSellerList = new List<PurchasePerSeller>();
                result.ReturnValue.Details = new List<PurchasePerSeller>();
                int rowNumber = 1;
                decimal finalPaymentPrice = 0;

                //each time we fetch cartshopping data should be updated in it
                foreach (var sellerPurchase in userCartEntity.Details)
                {
                    var purchasePerSeller = new PurchasePerSeller();
                    decimal sellerFactor = 0;
                    purchasePerSeller.SellerId = sellerPurchase.SellerId;
                    purchasePerSeller.SellerUserName = sellerPurchase.SellerUserName;
                    purchasePerSeller.ShippingTypeId = sellerPurchase.ShippingTypeId;

                    // update shippingExpense if seller change it
                    //TODO

                    finalPaymentPrice += sellerPurchase.ShippingExpense;
                    sellerFactor += sellerPurchase.ShippingExpense;
                    foreach (var pro in sellerPurchase.Products)
                    {
                        var productId = pro.ProductId;
                        var productEntity = _productContext.ProductCollection
                                            .Find(_ => _.ProductId == productId).FirstOrDefault();
                        var inventoryDetail = FindProductSpecValuesRecord(pro.ProductId, pro.ProductSpecValues);
                      
                        if (inventoryDetail != null && productEntity.IsActive
                            && !productEntity.IsDeleted && inventoryDetail.Count > 0)
                        {
                            var activePriceValue = GetCurrentPrice(productEntity);
                            var discountPerUnit = await GetCurrentDiscountPerUnit(productEntity, activePriceValue);

                            pro.PricePerUnit = activePriceValue;
                            pro.DiscountPricePerUnit = discountPerUnit.DiscountPerUnit;
                            pro.TotalAmountToPay = (activePriceValue - discountPerUnit.DiscountPerUnit) * pro.OrderCount;
                            finalPaymentPrice += pro.TotalAmountToPay;
                            sellerFactor += pro.TotalAmountToPay;

                            //check inventory if it is less than orderCount then change our order Count to our inventory
                            if (pro.OrderCount > inventoryDetail.Count)
                            {
                                pro.OrderCount = inventoryDetail.Count;
                            }
                        }
                        else
                        if (productEntity.IsDeleted || inventoryDetail.Count == 0)
                        {
                            pro.OrderCount = 0;
                            pro.DiscountPricePerUnit = 0;
                            pro.PricePerUnit = 0;
                            pro.TotalAmountToPay = 0;
                        }
                        
                        purchasePerSeller.Products.Add(pro);
                        rowNumber += 1;
                    }
                    purchasePerSeller.TotalDetailsAmountWithShipping = sellerFactor;
                    model.FinalPriceToPay += sellerFactor;
                    model.Details.Add(purchasePerSeller);
                }
                model.CouponCode = userCartEntity.CouponCode;
                model.FinalPriceAfterCouponCode = userCartEntity.FinalPriceAfterCouponCode;
                //model.FinalPriceForPay = finalPaymentPrice;
                var updateResult = await _context.Collection
                    .ReplaceOneAsync(_ => _.ShoppingCartId == userCartEntity.ShoppingCartId, model);
                if (updateResult.IsAcknowledged)
                {
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                    result.ReturnValue = model;
                }
                else
                {
                    result.Message = ConstMessages.GeneralError;
                }
            }
            else
            {
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            return result;
        }

        public bool UserCartShoppingValidation(string userCartId)
        {
            bool result = true;
            var entity = _context.Collection.Find(_ => _.ShoppingCartId == userCartId).FirstOrDefault();
            foreach (var seller in entity.Details)
            {
                foreach (var product in seller.Products)
                {
                   //check the quantity and price if no quantity and
                   //price change shoppingCart is ready to go to paymentGateway
                    var proEntity = _productContext.ProductCollection
                        .Find(_ => _.ProductId == product.ProductId).FirstOrDefault();
                    var inventoryDetail = FindProductSpecValuesRecord(product.ProductId, product.ProductSpecValues);
                    if(proEntity.IsDeleted || inventoryDetail == null ||
                        inventoryDetail.Count < product.OrderCount || 
                        GetCurrentPrice(proEntity) != product.PricePerUnit )
                    {
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }

        public async Task<Result> SubtractUserCartOrderCntFromInventory(Entities.Shop.ShoppingCart.ShoppingCart shoppingCart)
        {
            var result = new Result();
            var adminUser = _userManager.Users.FirstOrDefault(_ => _.IsDomainAdmin && _.DomainId == shoppingCart.AssociatedDomainId);
            var lanId = _languageContext.Collection
                .Find(_ => _.Symbol.ToLower() == CultureInfo.CurrentCulture.Name.ToLower()).FirstOrDefault().LanguageId;
            try
            {
                foreach (var seller in shoppingCart.Details)
                {
                    foreach (var product in seller.Products)
                    {
                        var productEntity = _productContext.ProductCollection
                            .Find(_ => _.ProductId == product.ProductId).FirstOrDefault();
                        var inventoryDetail = FindProductSpecValuesRecord(product.ProductId, product.ProductSpecValues);
                        if (inventoryDetail != null)
                        {
                            productEntity.Inventory.FirstOrDefault(_ => _.SpecValuesId == inventoryDetail.SpecValuesId).Count -= product.OrderCount;
                            var upDateResult = await _productContext.ProductCollection
                                .ReplaceOneAsync(_ => _.ProductId == product.ProductId, productEntity);

                            if(productEntity.Inventory.Sum(_=>_.Count) <= productEntity.MinimumCount)
                            {
                                var productName = productEntity.MultiLingualProperties.Any(_ => _.LanguageId == lanId) ?
                                    productEntity.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).Name :
                                    productEntity.MultiLingualProperties.FirstOrDefault().Name;
                                await _createNotification.NotifySiteAdminForLackOfInventory("NotifyProductMinimumCount", productName, adminUser, NotificationType.Sms);
                            }
                        }
                    }
                }
                result.Succeeded = true;
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                result.Message = Language.GetString("AlertAndMessage_ExceptionOccured");
            }
            return result;
        }

        public async Task<int> LoadUserCartShopping(string userId)
        {
            var domainEntity = _domainContext.Collection.Find(_ => _.DomainName == GetCurrentDomainName()).FirstOrDefault();
            if(domainEntity != null)
            {
                var res = await FetchActiveUserShoppingCart(userId, domainEntity.DomainId);
                var cartEntiry = _context.Collection.Find(_ => _.ShoppingCartId == res.ReturnValue.ShoppingCartId).FirstOrDefault();
                return GetItemCountsInCart(cartEntiry);
            }else
            {
                return 0;
            }
            
        }

        public async Task<Result> ChangePriceWithCouponCode(string shoppingCartId, string code, long oldPrice, long newPrice)
        {
            var res = new Result();
            var entity = await _context.Collection.Find(_ => _.ShoppingCartId == shoppingCartId).FirstOrDefaultAsync();
            if(entity != null)
            {
                entity.CouponCode = code;
                entity.FinalPriceAfterCouponCode = newPrice;
                entity.FinalPriceToPay = oldPrice;
                var updateResult = await _context.Collection.ReplaceOneAsync(_ => _.ShoppingCartId == shoppingCartId, entity);
                if(updateResult.IsAcknowledged)
                {
                    res.Succeeded = true;
                }
            }
            return res;
        }

        public Entities.Shop.ShoppingCart.ShoppingCart FetchShoppingCart(string shoppingCartId)
        {
            var entity = _context.Collection.Find(_ => _.ShoppingCartId == shoppingCartId).FirstOrDefault();
            return entity;
        }

        public async Task<Result> Reorder(string shoppingCartId)
        {
            var res = new Result();
            var entity = _context.Collection.Find(_ => _.ShoppingCartId == shoppingCartId).FirstOrDefault();
            decimal finalPrice = 0;
            Entities.Shop.ShoppingCart.ShoppingCart shoppingCart = new Entities.Shop.ShoppingCart.ShoppingCart();
            shoppingCart.ShoppingCartId = Guid.NewGuid().ToString();
            shoppingCart.ShoppingCartCulture = entity.ShoppingCartCulture;
            shoppingCart.CreatorUserId = entity.CreatorUserId;
            shoppingCart.AssociatedDomainId = entity.AssociatedDomainId;
            shoppingCart.CreationDate = DateTime.UtcNow;
            foreach (var detail in entity.Details)
            {
                decimal purchaseamnt = 0; 
                var purchaseperseller = new PurchasePerSeller();
                purchaseperseller.SellerId = detail.SellerId;
                purchaseperseller.SellerUserName = detail.SellerUserName;
                purchaseperseller.ShippingExpense = detail.ShippingExpense;
                purchaseperseller.ShippingTypeId = detail.ShippingTypeId;
                purchaseamnt += detail.ShippingExpense;
                foreach (var pro in detail.Products)
                {
                    var shoppingDetail = new ShoppingCartDetail();
                    shoppingDetail.ShoppingCartDetailId = Guid.NewGuid().ToString();
                    var productEntity = _productContext.ProductCollection
                          .Find(_ => _.ProductId == pro.ProductId).FirstOrDefault();

                    var inventoryDetail = FindProductSpecValuesRecord(productEntity.ProductId, pro.ProductSpecValues);
                    if (inventoryDetail.Count >= 0)
                    {
                        shoppingDetail.ProductId = pro.ProductId;
                        shoppingDetail.ProductName = pro.ProductName;
                        shoppingDetail.SellerId = pro.SellerId;
                        shoppingDetail.OrderCount = inventoryDetail.Count >= pro.OrderCount ? pro.OrderCount : inventoryDetail.Count;

                        var activePriceValue = GetCurrentPrice(productEntity);
                        var discountPerUnit = await GetCurrentDiscountPerUnit(productEntity, activePriceValue);

                        shoppingDetail.PricePerUnit = activePriceValue;
                        shoppingDetail.DiscountPricePerUnit = discountPerUnit.DiscountPerUnit;
                        shoppingDetail.TotalAmountToPay = (activePriceValue - discountPerUnit.DiscountPerUnit) * shoppingDetail.OrderCount;

                        purchaseperseller.Products.Add(shoppingDetail);
                        purchaseamnt += shoppingDetail.TotalAmountToPay;
                    }
                    else //this product doesnt have stock then it cant be added to shoppingCart
                        continue;
                }
                purchaseperseller.TotalDetailsAmountWithShipping = purchaseamnt;
                finalPrice += purchaseamnt;
                shoppingCart.Details.Add(purchaseperseller);
            }
            shoppingCart.FinalPriceToPay = finalPrice;

            try
            {
                await _context.Collection.InsertOneAsync(shoppingCart);
                res.Succeeded = true;
                res.Message = Language.GetString("AlertAndMessage_OperationDoneSuccessfully");
            }
            catch (Exception)
            {
                res.Message = ConstMessages.InternalServerErrorMessage;
            }
            return res;
        }
    }
}
