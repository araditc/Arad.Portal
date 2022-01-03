using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.ProductSpecification;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Arad.Portal.DataLayer.Repositories.Shop.Promotion.Mongo;
using System.Security.Claims;
using Arad.Portal.DataLayer.Repositories.Shop.Order.Mongo;
using Arad.Portal.DataLayer.Repositories.Shop.Transaction.Mongo;
using System.Collections.Specialized;
using System.Web;
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;
using Arad.Portal.GeneralLibrary.Utilities;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.General.Comment;
using Microsoft.Extensions.Configuration;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Arad.Portal.DataLayer.Models.Domain;
using Arad.Portal.DataLayer.Repositories.Shop.ShoppingCart.Mongo;

namespace Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo
{
    public class ProductRepository : BaseRepository, IProductRepository
    {
        private readonly ProductContext _context;
        //private readonly OrderContext _orderContext;
        private readonly DomainContext _domainContext;
        private readonly TransactionContext _transactionContext;
        private readonly PromotionContext _promotionContext;
        private readonly LanguageContext _languageContext;
        private readonly ShoppingCartContext _shoppingCartContext;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _accessor;
        //private readonly UserManager<ApplicationUser> _userManager;
        
        public ProductRepository(IHttpContextAccessor httpContextAccessor,
            ProductContext context, IMapper mapper,
            PromotionContext promotionContext,
            //OrderContext orderContext,
            //UserManager<ApplicationUser> userManager,
            ShoppingCartContext shoppingCartContext,
            IConfiguration configuration,
            LanguageContext languageContext,
            DomainContext domainContext,
            IHttpContextAccessor accessor,
            TransactionContext transactionContext)
            : base(httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            //_userManager = userManager;
            _promotionContext = promotionContext;
            _transactionContext = transactionContext;
            _languageContext = languageContext;
            _configuration = configuration;
            _domainContext = domainContext;
            _shoppingCartContext = shoppingCartContext;
            _accessor = accessor;
        }

        public async Task<Result> Add(ProductInputDTO dto)
        {
            Result result = new Result();
            try
            {
                var equallentModel = MappingProduct(dto);

                equallentModel.CreationDate = DateTime.Now;
                equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

                //var claims = ClaimsPrincipal.Current.Identities.First().Claims.ToList();
                var domainId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("RelatedDomain"))?.Value;
                equallentModel.AssociatedDomainId = domainId;

                await _context.ProductCollection.InsertOneAsync(equallentModel);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;

            }
            catch (Exception ex)
            {
                foreach (var item in dto.Pictures)
                {
                    if(System.IO.File.Exists(item.Url))
                    {
                        System.IO.File.Delete(item.Url);
                    }
                }
                result.Message = ConstMessages.InternalServerErrorMessage;
            }
            return result;
        }

        public async Task<Result> AddCommentToProduct(string productId, Comment comment)
        {
            var result = new Result();
            var entity = await _context.ProductCollection.Find(_ => _.ProductId == productId)
                .FirstOrDefaultAsync();
            if (entity != null)
            {
                if (string.IsNullOrWhiteSpace(comment.CommentId))
                {
                    comment.CommentId = Guid.NewGuid().ToString();
                }

                entity.Comments.Add(comment);
                var updateResult = await _context.ProductCollection.ReplaceOneAsync(_ => _.ProductId == productId,
                    entity);
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

        public async Task<Result> AddMultilingualProperty(string productId,
            MultiLingualProperty multiLingualProperty)
        {
            var result = new Result();
            var entity = await _context.ProductCollection.Find(_ => _.ProductId == productId)
                .FirstOrDefaultAsync();
            if (entity != null)
            {
                if (string.IsNullOrWhiteSpace(multiLingualProperty.MultiLingualPropertyId))
                {
                    multiLingualProperty.MultiLingualPropertyId = Guid.NewGuid().ToString();
                }
                entity.MultiLingualProperties.Add(multiLingualProperty);
                var updateResult = await _context.ProductCollection.ReplaceOneAsync(_ => _.ProductId == productId,
                    entity);
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

        public async Task<Result> AddPictureToProduct(string productId, Image picture)
        {
            var result = new Result();
            var entity = await _context.ProductCollection.Find(_ => _.ProductId == productId)
                .FirstOrDefaultAsync();
            if (entity != null)
            {
                if (string.IsNullOrWhiteSpace(picture.ImageId))
                {
                    picture.ImageId = Guid.NewGuid().ToString();
                }
                entity.Images.Add(picture);
                var updateResult = await _context.ProductCollection.ReplaceOneAsync(_ => _.ProductId == productId,
                    entity);
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

        public async Task<Result> AddProductSpecifications(string productId, ProductSpecificationValue specValues)
        {
            var result = new Result();
            var entity = await _context.ProductCollection.Find(_ => _.ProductId == productId)
                .FirstOrDefaultAsync();
            if (entity != null)
            {
                if (!string.IsNullOrWhiteSpace(specValues.SpecificationId))
                {
                    var specEntity = _context.SpecificationCollection
                        .Find(_ => _.ProductSpecificationId == specValues.SpecificationId).FirstOrDefault();
                    if (specEntity != null)
                    {
                        entity.Specifications.Add(specValues);

                        var updateResult = await _context.ProductCollection
                            .ReplaceOneAsync(_ => _.ProductId == productId, entity);

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
            }
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public async Task<Result> ChangeActivation(string productId, string modificationReason)
        {
            var result = new Result();
            var entity = _context.ProductCollection.Find(_ => _.ProductId == productId).FirstOrDefault();

            if (entity != null)
            {
                entity.IsActive = !entity.IsActive;
                #region add modification
                var modification = GetCurrentModification(modificationReason);
                entity.Modifications.Add(modification);
                #endregion
                var updateResult = await _context.ProductCollection
                    .ReplaceOneAsync(_ => _.ProductId == productId, entity);

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

        public async Task<Result> ChangeUnitOfProduct(string productId, string unitId, string modificationReason)
        {
            var result = new Result();
            var entity = _context.ProductCollection.Find(_ => _.ProductId == productId).FirstOrDefault();
            if (entity != null)
            {
                if (!string.IsNullOrWhiteSpace(unitId))
                {
                    var newUnit = _context.ProductUnitCollection
                    .Find(_ => _.ProductUnitId == unitId).FirstOrDefault();

                    if (newUnit != null)
                    {
                        entity.Unit = newUnit;
                        #region add modification
                        var modification = GetCurrentModification(modificationReason);
                        entity.Modifications.Add(modification);
                        #endregion
                        var updateResult = await _context.ProductCollection
                           .ReplaceOneAsync(_ => _.ProductId == productId, entity);

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
            }
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public async Task<Result> DeleteProduct(string productId, string modificationReason)
        {
            var result = new Result();
            var entity = await _context.ProductCollection.Find(_ => _.ProductId == productId).FirstOrDefaultAsync();
            if(entity != null)
            {
                bool allowDeletion;
                bool check;
                check = _shoppingCartContext.Collection.Find(_ => _.Details.Any(a => a.Products.Any(b => b.ProductId == productId)) 
                && !_.IsDeleted && _.IsActive).Any();
                check &= _transactionContext.Collection
                    .Find(_ => _.SubInvoices.Any(a => a.ParchasePerSeller.Products.Any(b => b.ProductId == productId))).Any();

                allowDeletion = !check;

                if (allowDeletion)
                {
                    entity.IsDeleted = true;
                    #region Add modification
                    var mod = GetCurrentModification(modificationReason);
                    entity.Modifications.Add(mod);
                    #endregion
                    var updateResult = await _context.ProductCollection
                        .ReplaceOneAsync(_ => _.ProductId == productId, entity);
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
                    result.Message = ConstMessages.DeletedNotAllowedForDependencies;
                }
            }else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }

            return result;
        }

        //public async Task<ProductOutputDTO> GetByUrlFriend(string urlFriend)
        //{
        //    var result = new ProductOutputDTO();
        //    var entity = await _context.ProductCollection.AsQueryable()
        //        .Where(_ =>
        //        _.MultiLingualProperties.Any(_ => _.UrlFriend.Equals(urlFriend))).FirstOrDefaultAsync();
        //    if(entity != null)
        //    {
        //        result = _mapper.Map<ProductOutputDTO>(entity);
        //        result.MultiLingualProperty = entity.MultiLingualProperties.First(_=>_.)
        //    }
        //    return result;
        //}

        private Entities.Shop.Product.Product MappingProduct(ProductInputDTO dto)
        {
            var equallentModel = _mapper.Map<Entities.Shop.Product.Product>(dto);
            if(string.IsNullOrWhiteSpace(dto.ProductId)) //insert Case
            {
                equallentModel.ProductId = Guid.NewGuid().ToString();
            }
            #region MultiLingualProperties
            if (dto.MultiLingualProperties.Count > 0)
            {
                foreach (var item in equallentModel.MultiLingualProperties)
                {
                    if(string.IsNullOrWhiteSpace(item.MultiLingualPropertyId))
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                }
            }
            #endregion

            #region ProductUnit
            if (!string.IsNullOrWhiteSpace(dto.UnitId))
            {
                var unitEntity = _context
                    .ProductUnitCollection.Find(_ => _.ProductUnitId == dto.UnitId).FirstOrDefault();
                equallentModel.Unit = unitEntity;
            }
            #endregion

            #region Prices
            equallentModel.Prices = new List<Price>();
            foreach (var price in dto.Prices.OrderBy(_=>_.StartDate))
            {
                if(price.IsActive && string.IsNullOrWhiteSpace(price.EndDate))//price is valid from client
                {
                    if(equallentModel.Prices.Any(_ => _.CurrencyId == price.CurrencyId && _.EndDate != null && _.IsActive))
                    {
                        var exist = equallentModel.Prices.First(_ => _.CurrencyId == price.CurrencyId && _.EndDate != null && _.IsActive);
                        equallentModel.Prices.Remove(exist);
                        exist.IsActive = false;
                        exist.EndDate = DateTime.Now;
                        equallentModel.Prices.Add(exist);

                    }
                }
                
                var p = new Price()
                {
                    PriceId = !string.IsNullOrWhiteSpace(price.PriceId) ? price.PriceId : Guid.NewGuid().ToString(),
                    CurrencyId = price.CurrencyId,
                    CurrencyName = price.CurrencyName,
                    IsActive = !string.IsNullOrWhiteSpace(price.PriceId) ? price.IsActive : true,
                    Prefix = price.Prefix,
                    PriceValue = price.PriceValue,
                    StartDate = DateHelper.ToEnglishDate(price.StartDate.Split(" ")[0]),
                    EndDate = !string.IsNullOrWhiteSpace(price.EndDate) ? 
                    GeneralLibrary.Utilities.DateHelper.ToEnglishDate(price.EndDate.Split(" ")[0]) : null
                };
                equallentModel.Prices.Add(p);
            }
            #endregion

            #region Promotion
            if (!string.IsNullOrWhiteSpace(dto.PromotionId))
            {
                var promotionEntity =
                    _promotionContext.Collection.Find(_ => _.PromotionId == dto.PromotionId).FirstOrDefault();
                equallentModel.Promotion = promotionEntity;
            }
            #endregion Promotion

            #region images
            equallentModel.Images = dto.Pictures;
            #endregion
            return equallentModel;
        }

        public int GetInventory(string productId)
        {
            var result = -1;
            var entity =  _context.ProductCollection
                .Find(_ => _.ProductId == productId).FirstOrDefault();
            if(entity != null)
            {
                result = entity.Inventory;
            }
            return result;
        }

        public List<Image> GetPictures(string productId)
        {
            var result = new List<Image>();
            var entity = _context.ProductCollection
                .Find(_ => _.ProductId == productId).FirstOrDefault();
            if(entity != null)
            {
                result = entity.Images;
            }
            return result;
        }

        public async Task<ProductOutputDTO> ProductFetch(string productId)
        {
            var result = new ProductOutputDTO();
            var entity = await _context.ProductCollection
                .Find(_=>_.ProductId == productId).FirstOrDefaultAsync();
            if (entity != null)
            {
                result = _mapper.Map<ProductOutputDTO>(entity);
                //TODO : 
                var staticFileStorageURL = _configuration["StaticFilesPlace:APIURL"];
            }
            return result;
        }

        public List<string> GetProductGroups(string productId)
        {
            var result = new List<string>();
            var entity =  _context.ProductCollection
                .Find(_ => _.ProductId == productId).FirstOrDefault();
            if(entity != null)
            {
                result = entity.GroupIds;
            }
            return result;
        }

        public List<ProductSpecificationValue> GetProductSpecifications(string productId)
        {
            var result = new List<ProductSpecificationValue>();
            var entity = _context.ProductCollection
               .Find(_ => _.ProductId == productId).FirstOrDefault();
            if(entity != null)
            {
                result = entity.Specifications;
            }
            return result;
        }

        public async Task<PagedItems<ProductViewModel>> List(string queryString)
        {
            PagedItems<ProductViewModel> result = new PagedItems<ProductViewModel>();
            try
            {
                NameValueCollection filter = HttpUtility.ParseQueryString(queryString);

                if (string.IsNullOrWhiteSpace(filter["page"]))
                {
                    filter.Set("page", "1");
                }

                if (string.IsNullOrWhiteSpace(filter["PageSize"]))
                {
                    filter.Set("PageSize", "20");
                }
                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);

                long totalCount = await _context.ProductCollection.Find(c => true).CountDocumentsAsync();
                var totalList = _context.ProductCollection.AsQueryable();
                if(!string.IsNullOrWhiteSpace(filter["groupIds"]))
                {
                    var arr = filter["groupIds"].ToString().Split("|");
                    totalList = totalList.Where(_ => _.GroupIds.Intersect(arr.ToList()).Any());
                }
                
                if(!string.IsNullOrWhiteSpace(filter["name"]))
                {
                    totalList = totalList
                        .Where(_=>_.MultiLingualProperties.Any(a=>a.Name.Contains(filter["name"])));
                }
                if (!string.IsNullOrWhiteSpace(filter["code"]))
                {
                    totalList = totalList
                        .Where(_ => _.UniqueCode.Equals(filter["code"].ToString()));
                }
                if (!string.IsNullOrWhiteSpace(filter["desc"]))
                {
                    totalList = totalList
                        .Where(_ => _.MultiLingualProperties.Any(a => a.Description.Contains(filter["desc"])));
                }
                if(!string.IsNullOrWhiteSpace(filter["from"]))
                {
                    //???
                    totalList = totalList
                        .Where(_ => _.CreationDate >= filter["from"].ToString().ToEnglishDate().ToUniversalTime());
                }
                if (!string.IsNullOrWhiteSpace(filter["to"]))
                {
                    //???
                    totalList = totalList
                        .Where(_ => _.CreationDate <= filter["to"].ToString().ToEnglishDate().ToUniversalTime());
                }
                if (!string.IsNullOrWhiteSpace(filter["inventory"]))
                {
                    totalList = totalList
                        .Where(_ => _.Inventory <= int.Parse(filter["inventory"].ToString()));
                }
                if (!string.IsNullOrWhiteSpace(filter["promotion"]) && Convert.ToBoolean(filter["promotion"].ToString()))
                {
                    totalList = totalList
                        .Where(_ => _.Promotion != null &&
                                    _.Promotion.StartDate <= DateTime.UtcNow &&
                        (_.Promotion.EndDate == null || _.Promotion.EndDate <= DateTime.UtcNow));
                }
                if (!string.IsNullOrWhiteSpace(filter["exist"]) && Convert.ToBoolean(filter["exist"].ToString()))
                {
                    totalList = totalList
                        .Where(_ => _.Inventory > 0 );
                }
                if (string.IsNullOrWhiteSpace(filter["LanguageId"]))
                {
                    var lan = _languageContext.Collection.Find(_ => _.IsDefault).FirstOrDefault();
                    filter.Set("LanguageId", lan.LanguageId);
                }
                var langId = filter["LanguageId"].ToString();
                var list = totalList.Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new ProductViewModel()
                   {
                       ProductId =_.ProductId,
                       GroupNames = _.GroupNames,
                       GroupIds = _.GroupIds,
                       Inventory = _.Inventory,
                       UniqueCode = _.UniqueCode,
                       IsDeleted = _.IsDeleted,
                       MultiLingualProperties = _.MultiLingualProperties,
                       Images = _.Images,
                       Prices = _.Prices,
                       Unit =  _.Unit,
                       CreationDate = _.CreationDate
                   }).ToList();

                result.Items = list;
                result.CurrentPage = page;
                result.ItemsCount = totalCount;
                result.PageSize = pageSize;
                result.QueryString = queryString;

            }
            catch (Exception ex)
            {
                result.CurrentPage = 1;
                result.Items = new List<ProductViewModel>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<Result> UpdateProduct(ProductInputDTO dto)
        {
            var result = new Result();
            var product = _context.ProductCollection.Find(_ => _.ProductId == dto.ProductId).First();
           
                var equallentModel = MappingProduct(dto);
                equallentModel.CreationDate = product.CreationDate;

                var updateResult = await _context.ProductCollection
                    .ReplaceOneAsync(_ => _.ProductId == dto.ProductId, equallentModel);

                if (updateResult.IsAcknowledged)
                {
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    result.Message = ConstMessages.GeneralError;

                }
            return result;
        }

        public async Task<Result> Restore(string productId)
        {
            var result = new Result();
            var entity = _context.ProductCollection
              .Find(_ => _.ProductId == productId).FirstOrDefault();
            entity.IsDeleted = false;
            var updateResult = await _context.ProductCollection
               .ReplaceOneAsync(_ => _.ProductId == productId, entity);
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

        public bool HasActiveProductPromotion(string productId)
        {
            var result = false;
            var productEntity = _context.ProductCollection.Find(_ => _.ProductId == productId).First();
            if(productEntity != null)
            {
                if(productEntity.Promotion != null)
                {
                    if(productEntity.Promotion.PromotionType == Entities.Shop.Promotion.PromotionType.Product && 
                        productEntity.Promotion.IsActive && productEntity.Promotion.StartDate <= DateTime.Now &&
                        (productEntity.Promotion.EndDate >= DateTime.Now || productEntity.Promotion == null))
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
       
        public  List<SelectListModel> GetAllActiveProductList(string langId, string currentUserId,
            string productGroupId, string vendorId)
        {
            var result = new List<SelectListModel>();
          
            if (currentUserId == Guid.Empty.ToString())//systemAccount
            {
                result = _context.ProductCollection.Find(_ => _.GroupIds.Contains(productGroupId) && (vendorId =="-1" || _.SellerUserId == vendorId) && _.IsActive)
                  .Project(_ => new SelectListModel() {
                      Text = _.MultiLingualProperties.Where(a => a.LanguageId == langId).Count() != 0 ?
                         _.MultiLingualProperties.First(a => a.LanguageId == langId).Name : "",
                      Value = _.ProductId
                  }).ToList();
            }
            else
            {
                result = _context.ProductCollection.Find(_ => _.GroupIds.Contains(productGroupId) && (vendorId == "-1" || _.SellerUserId == vendorId)
                && _.IsActive && _.CreatorUserId == currentUserId)
                  .Project(_ => new SelectListModel()
                  {
                      Text = _.MultiLingualProperties.Where(a => a.LanguageId == langId).Count() != 0 ?
                         _.MultiLingualProperties.First(a => a.LanguageId == langId).Name : "",
                      Value = _.ProductId
                  }).ToList();
            }
            return result;
        }

        public List<SelectListModel> GetAlActiveProductGroup(string langId)
        {
            var result = new List<SelectListModel>();
            result = _context.ProductGroupCollection.Find(_ => _.IsActive)
                .Project(_ => new SelectListModel()
                {
                    Text = _.MultiLingualProperties.Where(a => a.LanguageId == langId).Count() != 0 ?
                         _.MultiLingualProperties.First(a => a.LanguageId == langId).Name : "",
                    Value = _.ProductGroupId
                }).ToList();
            result.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "-1" });
            return result;
        }

        public List<SelectListModel> GetProductsOfThisVendor(string langId, string currentUserId)
        {
            var result = new List<SelectListModel>();
            result = _context.ProductCollection.Find(_ =>  _.SellerUserId == currentUserId && _.IsActive)
                  .Project(_ => new SelectListModel()
                  {
                      Text = _.MultiLingualProperties.Where(a => a.LanguageId == langId).Count() != 0 ?
                         _.MultiLingualProperties.First(a => a.LanguageId == langId).Name : "",
                      Value = _.ProductId
                  }).ToList();
            return result;
        }

        public List<SelectListModel> GetGroupsOfThisVendor(string vendorId, string langId)
        {
            var result = new List<SelectListModel>();
            if (vendorId != "-1")
            {
                var lst = _context.ProductCollection.Find(_ => _.SellerUserId == vendorId && _.IsActive)
                .Project(_ => _.GroupIds).ToList();
                var finalList = new List<string>();
                foreach (var item in lst)
                {
                    foreach (var element in item)
                    {
                        finalList.Add(element);
                    }
                }
                finalList = finalList.Distinct().ToList();
                foreach (var item in finalList)
                {
                    result.Add(new SelectListModel()
                    {
                        Text = _context.ProductGroupCollection.Find(_ => _.ProductGroupId == item)
                        .First().MultiLingualProperties.Any(_ => _.LanguageId == langId) ? _context.ProductGroupCollection.Find(_ => _.ProductGroupId == item)
                        .First().MultiLingualProperties.First(_ => _.LanguageId == langId).Name : _context.ProductGroupCollection.Find(_ => _.ProductGroupId == item)
                        .First().MultiLingualProperties.First().Name,
                        Value = item
                    });
                }
            }
            else
            {
                result = _context.ProductGroupCollection.Find(_ => _.IsActive)
                .Project(_ => new SelectListModel()
                {
                    Text = _.MultiLingualProperties.Any(_ => _.LanguageId == langId) ? _.MultiLingualProperties.First(_ => _.LanguageId == langId).Name :
                    _.MultiLingualProperties.First().Name,
                    Value = _.ProductGroupId
                }).ToList();

            }

            return result;
        }

        public List<SelectListModel> GetAllProductList(ApplicationUser user, string productGroupId, string domainId)
        {
            var result = new List<SelectListModel>();
            var domainEntity = _domainContext.Collection.Find(_ => _.DomainId == domainId).CountDocuments() > 0 
                ? _domainContext.Collection.Find(_ => _.DomainId == domainId).First() : new Entities.General.Domain.Domain();
            if (user.IsSystemAccount)//systemAccount
            {
                result = _context.ProductCollection.Find(_ => _.GroupIds.Contains(productGroupId) && _.IsActive)
                  .Project(_ => new SelectListModel()
                  {
                      Text = _.MultiLingualProperties.Where(a => a.LanguageId == user.Profile.DefaultLanguageId).Count() != 0 ?
                         _.MultiLingualProperties.First(a => a.LanguageId == user.Profile.DefaultLanguageId).Name : _.MultiLingualProperties.First().Name,
                      Value = _.ProductId
                  }).ToList();
            }
            else
            {
                result = _context.ProductCollection.Find(_ => _.GroupIds.Contains(productGroupId) && _.IsActive && _.CreatorUserId == user.Id)
                  .Project(_ => new SelectListModel()
                  {
                      Text = _.MultiLingualProperties.Where(a => a.LanguageId == domainEntity.DefaultLanguageId).Count() != 0 ?
                         _.MultiLingualProperties.First(a => a.LanguageId == domainEntity.DefaultLanguageId).Name : _.MultiLingualProperties.First().Name,
                      Value = _.ProductId
                  }).ToList();
            }
            return result;
        }

        public ProductOutputDTO FetchProductWithSlug(string slug, string domainName)
        {
            var result = new ProductOutputDTO();
            var urlFriend = $"{domainName}/product/{slug}";
            var productEntity = _context.ProductCollection
                .Find(_ => _.MultiLingualProperties.Any(a => a.UrlFriend == urlFriend)).First();

            result = _mapper.Map<ProductOutputDTO>(productEntity);
            result.MultiLingualProperties = productEntity.MultiLingualProperties;

            return result;
        }

        public ProductOutputDTO FetchBySlug(string slug, string domainName)
        {
            var result = new ProductOutputDTO();
            var urlFriend = $"{domainName}/product/{slug}";
            var productEntity = _context.ProductCollection
                .Find(_ => _.MultiLingualProperties.Any(a => a.UrlFriend == urlFriend)).First();

            result = _mapper.Map<ProductOutputDTO>(productEntity);
            result.MultiLingualProperties = productEntity.MultiLingualProperties;

            return result;
        }

        public ProductOutputDTO FetchByCode(long productCode, DomainDTO dto, string userId)
        {
            var result = new ProductOutputDTO();
            
            var productEntity = _context.ProductCollection
                .Find(_ => _.ProductCode == productCode).FirstOrDefault();

            result = _mapper.Map<ProductOutputDTO>(productEntity);
            result.Comments = CreateNestedTreeComment(productEntity.Comments, userId);
            result.MultiLingualProperties = productEntity.MultiLingualProperties;

            #region evaluate finalPrice
            var activePrice = productEntity.Prices.Where(_ => _.IsActive && _.CurrencyId == dto.DefaultCurrencyId 
            && _.StartDate <= DateTime.Now && (_.EndDate == null || _.EndDate.Value >= DateTime.Now)).FirstOrDefault();
            //check whether this product has any valid promotion
            Entities.Shop.Promotion.Promotion promotionOnAll = null;
            Entities.Shop.Promotion.Promotion promotionOnProductGroup = null;
            Entities.Shop.Promotion.Promotion promotionOnThisProduct = null;
            Entities.Shop.Promotion.Promotion newestPromotion = null;

            var promotionList = new List<Entities.Shop.Promotion.Promotion>();
            if(_promotionContext.Collection.Find(_ => _.PromotionType ==
            Entities.Shop.Promotion.PromotionType.All && (_.EndDate == null || _.EndDate.Value >= DateTime.Now) && _.StartDate <= DateTime.Now).Any())
            {
                promotionOnAll = _promotionContext.Collection
                    .Find(_ => _.PromotionType == Entities.Shop.Promotion.PromotionType.All && (_.EndDate == null || _.EndDate.Value >= DateTime.Now) && _.StartDate <= DateTime.Now).First();
            }
           if(_promotionContext.Collection.Find(_ => _.PromotionType == Entities.Shop.Promotion.PromotionType.Group && 
           _.StartDate <= DateTime.Now && (_.EndDate == null || _.EndDate.Value >= DateTime.Now) && _.Infoes.Any(a => productEntity.GroupIds.Contains(a.AffectedProductGroupId))).Any())
            {
                promotionOnProductGroup =
                     _promotionContext.Collection.Find(_ => _.PromotionType == Entities.Shop.Promotion.PromotionType.Group && _.StartDate <= DateTime.Now &&
                          ( _.EndDate == null ||_.EndDate.Value >= DateTime.Now) && _.Infoes.Any(a => productEntity.GroupIds.Contains(a.AffectedProductGroupId))).First();
            }
             
        
            if (_promotionContext.Collection.Find(_ => _.PromotionType ==
                 Entities.Shop.Promotion.PromotionType.Product && _.StartDate <= DateTime.Now && (_.EndDate == null || _.EndDate.Value >= DateTime.Now)
                   && _.Infoes.Any(a => a.AffectedProductId == productEntity.ProductId)).Any())
            {
                promotionOnThisProduct = _promotionContext.Collection.Find(_ => _.PromotionType ==
                 Entities.Shop.Promotion.PromotionType.Product && _.StartDate <= DateTime.Now && (_.EndDate == null || _.EndDate.Value >= DateTime.Now)
                   && _.Infoes.Any(a => a.AffectedProductId == productEntity.ProductId)).First();
                   
            }
            promotionList.Add(promotionOnAll);
            promotionList.Add(promotionOnProductGroup);
            promotionList.Add(promotionOnThisProduct);
            decimal finalPrice = 0;
            for (int i = 0; i < promotionList.Count; i++)
            {
                if(newestPromotion == null)
                {
                    if(promotionList[i] != null)
                    {
                        newestPromotion = promotionList[i];
                    }
                }
                else
                {
                    if(promotionList[i].StartDate > newestPromotion.StartDate)
                    {
                        //this promotion is newer than current promotion
                        newestPromotion = promotionList[i];
                    }
                }
            }
            if(newestPromotion != null)
            {
                switch (newestPromotion.DiscountType)
                {
                    case Entities.Shop.Promotion.DiscountType.Fixed:
                        finalPrice = activePrice.PriceValue - newestPromotion.Value.Value; 
                        break;
                    case Entities.Shop.Promotion.DiscountType.Percentage:
                        var percentage = (activePrice.PriceValue * newestPromotion.Value.Value) / 100;
                        finalPrice = activePrice.PriceValue - percentage;
                        break;
                    case Entities.Shop.Promotion.DiscountType.Product:
                        finalPrice = activePrice.PriceValue;
                        var giftProduct = _context.ProductCollection.Find(_ => _.ProductId == newestPromotion.PromotedProductId).First();
                        result.GiftProduct = _mapper.Map<ProductOutputDTO>(giftProduct);
                        break;
                }

                result.Promotion = newestPromotion;
            }
            if(finalPrice == 0)
            {
                result.PriceValWithPromotion = activePrice.PriceValue;
            }else
            {
                result.PriceValWithPromotion = finalPrice;
            }
            #endregion

            foreach (var item in result.Specifications)
            {
                var lst = item.Values.Split("|").ToList();
                var i = 1;
                foreach (var str in lst)
                {
                    var obj = new SelectListModel()
                    {
                        Text = str,
                        Value = i.ToString()
                    };
                    i += 1;
                    item.ValueList.Add(obj);
                }
            }
            var r = Helpers.Utilities.ConvertPopularityRate(productEntity.TotalScore, productEntity.ScoredCount);
            result.LikeRate = r.LikeRate;
            result.DisikeRate = r.DisikeRate;
            result.halfLikeRate = r.halfLikeRate;
            return result;
        }


      

        private List<CommentVM> CreateNestedTreeComment(List<Comment> comments, string currentUserId)
        {
            var result = new List<CommentVM>();
            result = comments.Where(_ => _.ParentId == null).Select(_ => new CommentVM()
            {
                CommentId = _.CommentId,
                Content = _.Content,
                CreationDate = _.CreationDate,
                CreatorUserId = _.CreatorUserId,
                CreatorUserName = _.CreatorUserName,
                DislikeCount = _.DislikeCount,
                ReferenceId = _.ReferenceId,
                LikeCount = _.LikeCount,
                userStatus = !string.IsNullOrWhiteSpace(currentUserId) ? ( _httpContextAccessor.HttpContext.Request.Cookies[$"{currentUserId}_cmt{_.CommentId}"] != null ?
                ( _httpContextAccessor.HttpContext.Request.Cookies[$"{currentUserId}_cmt{_.CommentId}"] == "true" ? userStatus.Like : userStatus.Dislike ) :
                     userStatus.NoAction ) : userStatus.UnAuthorized,
                Childrens = GetChildren(comments, _.CommentId, currentUserId)

            }).ToList();
            return result;
        }

        private List<CommentVM>  GetChildren(List<Comment> list , string currentCommentId, string currentUserId)
        {
            List<CommentVM> result;
            if(!list.Any(_=>_.ParentId == currentCommentId))
            {
                result = new List<CommentVM>();
            }
            else
            {
                result = list.Where(_ => _.ParentId == currentCommentId).Select(_ => new CommentVM()
                {
                    CommentId = _.CommentId,
                    Content = _.Content,
                    CreationDate = _.CreationDate,
                    CreatorUserId = _.CreatorUserId,
                    CreatorUserName = _.CreatorUserName,
                    DislikeCount = _.DislikeCount,
                    ReferenceId = _.ReferenceId,
                    LikeCount = _.LikeCount,
                    userStatus = !string.IsNullOrWhiteSpace(currentUserId) ? (_httpContextAccessor.HttpContext.Request.Cookies[$"{currentUserId}_cmt{_.CommentId}"] != null ?
                      (_httpContextAccessor.HttpContext.Request.Cookies[$"{currentUserId}_cmt{_.CommentId}"] == "true" ? userStatus.Like : userStatus.Dislike) :
                     userStatus.NoAction) : userStatus.UnAuthorized,
                    Childrens = GetChildren(list, currentCommentId, currentUserId)
                }).ToList();
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="score"></param>
        /// <param name="isNew">it means this is the first time which this user rate this product otherweise user rated product again</param>
        /// <param name="prevScore"></param>
        /// <returns></returns>
        public async Task<Result<EntityRate>> RateProduct(string productId, int score, bool isNew, int prevScore)
        {
            var result = new Result<EntityRate>();
            var entity = _context.ProductCollection.Find(_ => _.ProductId == productId).First();
            if(isNew)
            {
                entity.TotalScore += score;
                entity.ScoredCount += 1;
            }
            else if(score != prevScore) //if user change the score of product
            {
                entity.TotalScore -= prevScore;
                entity.TotalScore += score;
                //scoredCount doesnt change cause this user rated before just change its score
            }
           
            if (score != prevScore)
            {
                var updateResult = await _context.ProductCollection.ReplaceOneAsync(_ => _.ProductId == productId, entity);
                var res = Helpers.Utilities.ConvertPopularityRate(entity.TotalScore, entity.ScoredCount);
                if (updateResult.IsAcknowledged)
                {
                    result.Succeeded = true;
                    result.ReturnValue = res;
                }
                else
                {
                    result.Succeeded = false;
                }
            }
            return result;
        }

        public async Task<Result> UpdateProductInventory(string productId, bool isIncreament, int count)
        {
            var result = new Result();
            var entity = _context.ProductCollection.Find(_ => _.ProductId == productId).FirstOrDefault();
            if (isIncreament)
                entity.Inventory += count;
            else
                entity.Inventory -= count;

            var updateResult = await _context.ProductCollection.ReplaceOneAsync(_ => _.ProductId == productId, entity);
            if(updateResult.IsAcknowledged)
            {
                result.Succeeded = true;
            }
            return result;
        }
    }
}
