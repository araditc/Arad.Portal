using Arad.Portal.DataLayer.Contracts.Shop.Promotion;
using Arad.Portal.DataLayer.Models.Promotion;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using AutoMapper;
using System.Collections.Specialized;
using System.Web;

namespace Arad.Portal.DataLayer.Repositories.Shop.Promotion.Mongo
{
    public class PromotionRepository : BaseRepository, IPromotionRepository
    {
        private readonly PromotionContext _context;
        private readonly ProductContext _productContext;
        private readonly IMapper _mapper;
        public PromotionRepository(IHttpContextAccessor httpContextAccessor,
                                   PromotionContext promotionContext,
                                   IMapper mapper,
                                   ProductContext productContext) : base(httpContextAccessor)
        {
            _context = promotionContext;
            _productContext = productContext;
            _mapper = mapper;
        }

        public async Task<RepositoryOperationResult> AssignPromotionToProduct(string promotionId, string productId)
        {
            var result = new RepositoryOperationResult();
            var promotionEntity = _context.Collection
                .Find(_ => _.PromotionId == promotionId &&
                _.StartDate <= DateTime.UtcNow && _.EndDate == null)
                .FirstOrDefault();
            if(promotionEntity != null)
            {
                var productEntity = _productContext.ProductCollection
                    .Find(_ => _.ProductId == productId).FirstOrDefault();
                if(productEntity != null)
                {
                    productEntity.Promotion = promotionEntity;
                    var updateResult = await _productContext.ProductCollection
                        .ReplaceOneAsync(_=>_.ProductId == productId, productEntity);
                    if(updateResult.IsAcknowledged)
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
            }
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> AssingPromotionToProductGroup(string promotionId, string ProductGroupId)
        {
            var result = new RepositoryOperationResult();
            var promotionEntity = _context.Collection
                .Find(_ => _.PromotionId == promotionId &&
                _.StartDate <= DateTime.UtcNow && _.EndDate == null)
                .FirstOrDefault();
            if (promotionEntity != null)
            {
                var productGroupEntity = _productContext.ProductGroupCollection
                    .Find(_ => _.ProductGroupId == ProductGroupId).FirstOrDefault();
                if (productGroupEntity != null)
                {
                    productGroupEntity.Promotion = promotionEntity;
                    var updateResult = await _productContext.ProductGroupCollection
                        .ReplaceOneAsync(_ => _.ProductGroupId == ProductGroupId, productGroupEntity);
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
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> DeletePromotion(string promotionId, string modificationReason)
        {
            var result = new RepositoryOperationResult();
            var promotionEntity = _context.Collection
                .Find(_ => _.PromotionId == promotionId).FirstOrDefault();
            bool isValid = (promotionEntity.EndDate != null && promotionEntity.EndDate.Value >= DateTime.UtcNow)
                 || promotionEntity.EndDate == null;
            if (isValid)
            {
                bool allowDeletion;

                var check = _productContext.ProductCollection
                    .AsQueryable().Any(_ => _.Promotion.PromotionId == promotionId);
                check &= _productContext.ProductGroupCollection
                    .AsQueryable().Any(_=>_.Promotion.PromotionId == promotionId);
                if (check)
                    allowDeletion = false;
                else
                    allowDeletion = true;

                if(allowDeletion)
                {
                    promotionEntity.IsDeleted = true;
                    #region add modification
                    var mod = GetCurrentModification(modificationReason);
                    promotionEntity.Modifications.Add(mod);
                    #endregion
                    var updateResult = await _context.Collection
                        .ReplaceOneAsync(_ => _.PromotionId == promotionId, promotionEntity);
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
                    result.Message = ConstMessages.DeletedNotAllowedForDependencies;
                }
            }else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> InsertPromotion(PromotionDTO dto)
        {
            var result = new RepositoryOperationResult();
            var equallentModel = _mapper.Map<Entities.Shop.Promotion.Promotion>(dto);
            equallentModel.PromotionId = Guid.NewGuid().ToString();
            equallentModel.CreationDate = DateTime.Now;
            equallentModel.CreatorUserId = GetUserId();
            equallentModel.CreatorUserName = GetUserName();
            try
            {
                await _context.Collection.InsertOneAsync(equallentModel);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;

            }
            catch (Exception)
            {
                result.Message = ConstMessages.GeneralError;
            }
            return result;
        }
        public async Task<RepositoryOperationResult> UpdatePromotion(PromotionDTO dto)
        {
            var result = new RepositoryOperationResult();
            var oldEntity = _context.Collection
                .Find(_ => _.PromotionId == dto.PromotionId).FirstOrDefault();
            if(oldEntity != null)
            {
                var equallentModel = _mapper.Map<Entities.Shop.Promotion.Promotion>(dto);
                var modifications = oldEntity.Modifications;
                var newModification = GetCurrentModification(dto.ModificationReason);
                modifications.Add(newModification);
                equallentModel.Modifications = modifications;
                equallentModel.CreationDate = oldEntity.CreationDate;
                equallentModel.CreatorUserId = oldEntity.CreatorUserId;
                equallentModel.CreatorUserName = oldEntity.CreatorUserName;

                var updateResult = await _context.Collection
                    .ReplaceOneAsync(_ => _.PromotionId == dto.PromotionId, equallentModel);
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

        public async Task<PagedItems<PromotionDTO>> ListPromotions(string queryString)
        {
            PagedItems<PromotionDTO> result = new PagedItems<PromotionDTO>()
            {
             CurrentPage = 1,
             ItemsCount = 0,
             PageSize = 10,
             QueryString = queryString
            };
            try
            {
                NameValueCollection filter = HttpUtility.ParseQueryString(queryString);

                if (string.IsNullOrWhiteSpace(filter["CurrentPage"]))
                {
                    filter.Set("CurrentPage", "1");
                }

                if (string.IsNullOrWhiteSpace(filter["PageSize"]))
                {
                    filter.Set("PageSize", "20");
                }

                var page = Convert.ToInt32(filter["CurrentPage"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);

                long totalCount = await _context.Collection.Find(c => true).CountDocumentsAsync();
                var list = _productContext.ProductUnitCollection.AsQueryable().Skip((page - 1) * pageSize)
                   .Take(pageSize).ToList();
                result.Items = _mapper.Map<List<PromotionDTO>>(list);
               
            }
            catch (Exception ex)
            {
                result.CurrentPage = 1;
                result.ItemsCount = 0;
                result.PageSize = 10;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> SetPromotionExpirationDate(string promotionId, DateTime? dateTime)
        {
            var result = new RepositoryOperationResult();
            var promotionEntity = _context.Collection
                .Find(_ => _.PromotionId == promotionId).FirstOrDefault();
            if(promotionEntity != null)
            {
                promotionEntity.EndDate = dateTime == null ? DateTime.Now : dateTime.Value;
                var updateResult = await _context.Collection
                        .ReplaceOneAsync(_ => _.PromotionId == promotionId, promotionEntity);
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
    }
}
