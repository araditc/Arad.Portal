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

namespace Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo
{
    public class ProductRepository : BaseRepository, IProductRepository
    {
        private readonly ProductContext _context;
        private readonly OrderContext _orderContext;
        private readonly TransactionContext _transactionContext;
        private readonly PromotionContext _promotionContext;
        private readonly IMapper _mapper;
        public ProductRepository(IHttpContextAccessor httpContextAccessor,
            ProductContext context, IMapper mapper,
            PromotionContext promotionContext,
            OrderContext orderContext,
            TransactionContext transactionContext)
            : base(httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _promotionContext = promotionContext;
            _orderContext = orderContext;
            _transactionContext = transactionContext;
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

        public async Task<RepositoryOperationResult> AddPictureToProduct(string productId, Picture picture)
        {
            var result = new RepositoryOperationResult();
            var entity = await _context.ProductCollection.Find(_ => _.ProductId == productId)
                .FirstOrDefaultAsync();
            if (entity != null)
            {
                if (string.IsNullOrWhiteSpace(picture.PictureId))
                {
                    picture.PictureId = Guid.NewGuid().ToString();
                }
                entity.Pictures.Add(picture);
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

        public async Task<RepositoryOperationResult> AddProductSpecifications(string productId, SpecificationValueDTO specValueDto)
        {
            var result = new RepositoryOperationResult();
            var entity = await _context.ProductCollection.Find(_ => _.ProductId == productId)
                .FirstOrDefaultAsync();

            if (entity != null)
            {
                if (!string.IsNullOrWhiteSpace(specValueDto.ProductSpecificationId))
                {
                    var specEntity = _context.SpecificationCollection
                        .Find(_ => _.ProductSpecificationId == specValueDto.ProductSpecificationId).FirstOrDefault();
                    if (specEntity != null)
                    {
                        entity.Specifications.Add(new ProductSpecificationValue()
                        {
                            Specification = specEntity,
                            Value = specValueDto.specificationValue
                        });

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

        public async Task<RepositoryOperationResult> ChangeProductUnitOfProduct(string productId, string unitId, string modificationReason)
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

        public async Task<ProductOutputDTO> GetByUrlFriend(string urlFriend)
        {
            var result = new ProductOutputDTO();
            var entity = await _context.ProductCollection.AsQueryable()
                .Where(_ =>
                _.MultiLingualProperties.Any(_ => _.UrlFriend.Equals(urlFriend))).FirstOrDefaultAsync();
            if(entity != null)
            {
                result = _mapper.Map<ProductOutputDTO>(entity);
            }
            return result;
        }

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
                        .SpecificationCollection.Find(_ => _.ProductSpecificationId == item.ProductSpecificationId).FirstOrDefault();
                    var obj = new ProductSpecificationValue()
                    {
                        Specification = specEntity,
                        Value = item.specificationValue
                    };
                    equallentModel.Specifications.Add(obj);
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
            equallentModel.Prices = new List<Models.Price.Price>();
            if (dto.Price != null)
            {
                equallentModel.Prices.Add(dto.Price);
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

        public List<Picture> GetPictures(string productId)
        {
            var result = new List<Picture>();
            var entity = _context.ProductCollection
                .Find(_ => _.ProductId == productId).FirstOrDefault();
            if(entity != null)
            {
                result = entity.Pictures;
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

        public Task<PagedItems<ProductsListGrid>> List(string queryString)
        {
            throw new NotImplementedException();
        }

        public async Task<RepositoryOperationResult> UpdateProduct(ProductInputDTO dto, string modificationReason)
        {
            var result = new RepositoryOperationResult();
           
                var equallentModel = MappingProduct(dto);

                #region add modification
                var mod = GetCurrentModification(modificationReason);
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

    }
}
