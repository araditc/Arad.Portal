using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Models.Comment;
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

namespace Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo
{
    public class ProductRepository : BaseRepository, IProductRepository
    {
        private readonly ProductContext _context;
        private readonly OrderContext _orderContext;
        private readonly TransactionContext _transactionContext;
        private readonly PromotionContext _promotionContext;
        private readonly LanguageContext _languageContext;
        private readonly IMapper _mapper;
        public ProductRepository(IHttpContextAccessor httpContextAccessor,
            ProductContext context, IMapper mapper,
            PromotionContext promotionContext,
            OrderContext orderContext,
            LanguageContext languageContext,
            TransactionContext transactionContext)
            : base(httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _promotionContext = promotionContext;
            _orderContext = orderContext;
            _transactionContext = transactionContext;
            _languageContext = languageContext;
        }

        public async Task<RepositoryOperationResult> Add(ProductInputDTO dto)
        {
            RepositoryOperationResult result = new RepositoryOperationResult();
            try
            {
                var equallentModel = MappingProduct(dto);

                equallentModel.CreationDate = DateTime.Now;
                equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
                

                await _context.ProductCollection.InsertOneAsync(equallentModel);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;

            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.InternalServerErrorMessage;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> AddCommentToProduct(string productId, Comment comment)
        {
            var result = new RepositoryOperationResult();
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

        public async Task<RepositoryOperationResult> AddMultilingualProperty(string productId,
            MultiLingualProperty multiLingualProperty)
        {
            var result = new RepositoryOperationResult();
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

        public async Task<RepositoryOperationResult> AddPictureToProduct(string productId, Image picture)
        {
            var result = new RepositoryOperationResult();
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

        public async Task<RepositoryOperationResult> AddProductSpecifications(string productId, ProductSpecificationValue specValues)
        {
            var result = new RepositoryOperationResult();
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

        public async Task<RepositoryOperationResult> ChangeActivation(string productId, string modificationReason)
        {
            var result = new RepositoryOperationResult();
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

        public async Task<RepositoryOperationResult> ChangeUnitOfProduct(string productId, string unitId, string modificationReason)
        {
            var result = new RepositoryOperationResult();
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

        public async Task<RepositoryOperationResult> DeleteProduct(string productId, string modificationReason)
        {
            var result = new RepositoryOperationResult();
            var entity = await _context.ProductCollection.Find(_ => _.ProductId == productId).FirstOrDefaultAsync();
            if(entity != null)
            {
                bool allowDeletion;
                bool check;
                check = _orderContext.Collection.AsQueryable().Any(_ => _.Details.Any(_ => _.ProductId == productId));
                check &= _transactionContext.Collection.AsQueryable().Any(_ => _.TransactionItems.Any(_ => _.ProductId == productId));
                if (check)
                {
                    allowDeletion = false;
                }
                else
                {
                    allowDeletion = true;
                }
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
            if(!string.IsNullOrWhiteSpace(dto.ProductId)) //insert Case
            {
                equallentModel.ProductId = Guid.NewGuid().ToString();
            }
            

            #region MultiLingualProperties
            if (dto.MultiLingualProperties.Count > 0)
            {
                foreach (var item in equallentModel.MultiLingualProperties)
                {
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                }
            }
            #endregion


            #region Specifications
            equallentModel.Specifications = new List<ProductSpecificationValue>();
            if (dto.Specifications.Count > 0)
            {
                foreach (var item in dto.Specifications)
                {
                    var specEntity = _context
                        .SpecificationCollection.Find(_ => _.ProductSpecificationId == item.SpecificationId).FirstOrDefault();
                    if(specEntity != null)
                    {
                        equallentModel.Specifications.Add(item);
                    }
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
            if (dto.Prices.Count() == 1)
            {
                equallentModel.Prices.Add(dto.Prices[0]);
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
            return equallentModel;
        }


        //private ProductInputDTO UpdateOr
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
                       MultiLingualProperty =_.MultiLingualProperties.Where(_=>_.LanguageId == langId).First(),
                       MainImage = _.Images.Where(_=>_.IsMain).First().Url,
                       Price = _.Prices.Where(_=>_.IsActive && DateTime.Now >= _.StartDate && (_.EndDate == null || DateTime.Now < _.EndDate)).First(),
                       Unit =  new ProductUnitViewModel()
                       {
                           ProductUnitId = _.Unit.ProductUnitId,
                           UnitName = _.Unit.UnitNames.Where(a=>a.LanguageId == langId).First()
                       },
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

        public async Task<RepositoryOperationResult> UpdateProduct(ProductInputDTO dto)
        {
            var result = new RepositoryOperationResult();
           
                var equallentModel = MappingProduct(dto);

                #region add modification
                var mod = GetCurrentModification(dto.ModificationReason);
                equallentModel.Modifications.Add(mod);
                #endregion

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

        public async Task<RepositoryOperationResult> Restore(string productId)
        {
            var result = new RepositoryOperationResult();
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
    }
}
