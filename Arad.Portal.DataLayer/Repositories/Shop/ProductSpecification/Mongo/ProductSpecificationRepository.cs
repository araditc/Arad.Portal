using Arad.Portal.DataLayer.Contracts.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Models.ProductSpecification;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Specialized;
using System.Web;

namespace Arad.Portal.DataLayer.Repositories.Shop.ProductSpecification.Mongo
{
    public class ProductSpecificationRepository : BaseRepository, IProductSpecificationRepository
    {
        private readonly IMapper _mapper;
      
        private readonly ProductContext _productContext;
        public ProductSpecificationRepository(IHttpContextAccessor httpContextAccessor,
            IMapper mapper, 
            ProductContext productContext): base(httpContextAccessor)
        {
            _mapper = mapper;
            _productContext = productContext;
        }
        public async Task<RepositoryOperationResult> Add(ProductSpecificationDTO dto)
        {
            RepositoryOperationResult result = new RepositoryOperationResult();
            try
            {

                var equallentEntity = _mapper.Map<Entities.Shop.ProductSpecification.ProductSpecification>(dto);
                equallentEntity.ProductSpecificationId = Guid.NewGuid().ToString();

                equallentEntity.CreationDate = DateTime.Now;
                equallentEntity.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                equallentEntity.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
                equallentEntity.ProductSpecificationId = Guid.NewGuid().ToString();

                await _productContext.SpecificationCollection.InsertOneAsync(equallentEntity);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;

            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.InternalServerErrorMessage;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> Delete(string specificationId, string modificationReason)
        {
            RepositoryOperationResult result = new RepositoryOperationResult();

            try
            {
                #region check object dependency
                var allowDeletion = true;
                //??? this part should be checked in mongo
                var check = _productContext.ProductCollection
                    .AsQueryable()
                    .Any(baseProduct => 
                        baseProduct.Specifications
                    .Any(_=> _.Specification.ProductSpecificationId == specificationId));


                if (check)
                {
                    allowDeletion = false;
                }


                #endregion
                if (allowDeletion)
                {
                    var entity = _productContext.SpecificationCollection
                        .Find(_ => _.ProductSpecificationId == specificationId).FirstOrDefault();
                    if (entity != null)
                    {
                        entity.IsDeleted = true;
                        #region add modification
                        var mod = GetCurrentModification(modificationReason);
                        entity.Modifications.Add(mod);
                        #endregion

                        var updateResult = await _productContext.SpecificationCollection
                            .ReplaceOneAsync(_ => _.ProductSpecificationId == specificationId, entity);
                        if (updateResult.IsAcknowledged)
                        {
                            result.Message = ConstMessages.SuccessfullyDone;
                            result.Succeeded = true;
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
                    result.Message = ConstMessages.DeletedNotAllowedForDependencies;
                }
            }
            catch (Exception e)
            {
                result.Message = ConstMessages.ExceptionOccured;
            }
            return result;
        }

        public async Task<RepositoryOperationResult<ProductSpecificationDTO>> GetModel(string specId)
        {
            RepositoryOperationResult<ProductSpecificationDTO> result =
                new RepositoryOperationResult<ProductSpecificationDTO>();
            var entity = await _productContext.SpecificationCollection
                .Find(_ => _.ProductSpecificationId == specId).FirstOrDefaultAsync();
            if(entity != null)
            {
                var dto = _mapper.Map<ProductSpecificationDTO>(entity);
                result.Message = ConstMessages.SuccessfullyDone;
                result.ReturnValue = dto;
                result.Succeeded = true;
            }
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public RepositoryOperationResult<Entities.Shop.ProductSpecification.ProductSpecification> GetEntity(string specId)
        {
            RepositoryOperationResult<Entities.Shop.ProductSpecification.ProductSpecification> result =
               new RepositoryOperationResult<Entities.Shop.ProductSpecification.ProductSpecification>();
            var entity = _productContext.SpecificationCollection
                .Find(_ => _.ProductSpecificationId == specId).FirstOrDefault();
            if (entity != null)
            {
                result.Message = ConstMessages.SuccessfullyDone;
                result.ReturnValue = entity;
                result.Succeeded = true;
            }
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public List<ProductSpecificationDTO> GetAllSpecificationsInGroup(string specificationGroupId)
        {
            var result = new List<ProductSpecificationDTO>();
            var lst = _productContext.SpecificationCollection.AsQueryable()
                .Where(_ => _.SpecificationGroupId == specificationGroupId).ToList();
            if(lst != null && lst.Count > 0)
            {
                result = _mapper.Map<List<ProductSpecificationDTO>>(lst);
            }
            return result;
        }

        
        public RepositoryOperationResult<List<string>> GetSpecificationValues(string productSpecificationId)
        {
            var result = new RepositoryOperationResult<List<string>>();
            var lst = _productContext.SpecificationCollection.AsQueryable()
                .Where(_ => _.ProductSpecificationId == productSpecificationId);
            if(lst != null && lst.Count() > 0)
            {
                var entity = lst.FirstOrDefault();
                result.ReturnValue = entity.SpecificationValues;
                result.Message = ConstMessages.SuccessfullyDone;
                result.Succeeded = true;
            }
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public async Task<PagedItems<ProductSpecificationDTO>> List(string queryString)
        {
            PagedItems<ProductSpecificationDTO> result = new PagedItems<ProductSpecificationDTO>();
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

                long totalCount = await _productContext.SpecificationCollection.Find(c => true).CountDocumentsAsync();
                var list = _productContext.SpecificationCollection.AsQueryable().Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new ProductSpecificationDTO()
                   {
                      ProductSpecificationId = _.ProductSpecificationId,
                      SpecificationGroupId = _.SpecificationGroupId,
                      SpecificationName = _.SpecificationName,
                      SpecificationValues = _.SpecificationValues
                   }).ToList();

                result.CurrentPage = page;
                result.Items = list;
                result.ItemsCount = totalCount;
                result.PageSize = pageSize;
                result.QueryString = queryString;

            }
            catch (Exception ex)
            {
                result.CurrentPage = 1;
                result.Items = new List<ProductSpecificationDTO>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> Update(ProductSpecificationDTO dto)
        {
            var result = new RepositoryOperationResult();

            var equallentModel = _mapper.Map<Entities.Shop.ProductSpecification.ProductSpecification>(dto);

            var availableEntity = await _productContext.SpecificationCollection
                    .Find(_ => _.ProductSpecificationId.Equals(dto.ProductSpecificationId)).FirstOrDefaultAsync();

            if (availableEntity != null)
            {
                #region Add Modification
                var currentModifications = availableEntity.Modifications;
                var mod = GetCurrentModification(dto.ModificationReason);

                currentModifications.Add(mod);
                #endregion

                equallentModel.Modifications = currentModifications;

                equallentModel.CreationDate = availableEntity.CreationDate;
                equallentModel.CreatorUserId = availableEntity.CreatorUserId;
                equallentModel.CreatorUserName = availableEntity.CreatorUserName;

                var updateResult = await _productContext.SpecificationCollection
                   .ReplaceOneAsync(_ => _.ProductSpecificationId == availableEntity.ProductSpecificationId, equallentModel);

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
            }
            return result;
        }

        
    }
}
