using Arad.Portal.DataLayer.Contracts.Shop.ProductUnit;
using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo;
using Arad.Portal.DataLayer.Repositories.Shop.ProductGroup.Mongo;
using System.Web;
using System.Collections.Specialized;

namespace Arad.Portal.DataLayer.Repositories.Shop.ProductUnit.Mongo
{
    public class ProductUnitRepository : BaseRepository, IProductUnitRepository
    {
       
        ProductContext _productContext;
        IMapper _mapper;
        public ProductUnitRepository(
            ProductContext productContext,
            IMapper mapper, IHttpContextAccessor httpContextAccessor): base(httpContextAccessor)
        {
           
            _mapper = mapper;
            _productContext = productContext;
        }
        public async Task<RepositoryOperationResult> AddProductUnit(ProductUnitDTO dto)
        {
            RepositoryOperationResult result = new RepositoryOperationResult();
            var equallentModel = _mapper.Map<Entities.Shop.ProductUnit.ProductUnit>(dto);
            equallentModel.Modifications = new List<Modification>();

            equallentModel.CreationDate = DateTime.Now;
            equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            try
            {
                await _productContext.ProductUnitCollection.InsertOneAsync(equallentModel);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }

            return result;
        }

        public async Task<RepositoryOperationResult> Delete(string productUnitId)
        {
            RepositoryOperationResult result = new RepositoryOperationResult();

            try
            {
                #region check object dependency 
                var allowDeletion = true;
                if(_productContext.ProductCollection.CountDocuments(_=>_.Unit.ProductUnitId == productUnitId) > 0)
                {
                    allowDeletion = false;
                }
                #endregion
                if (allowDeletion)
                {
                    var delResult = await _productContext.ProductUnitCollection.DeleteOneAsync(_ => _.ProductUnitId == productUnitId);
                    if (delResult.IsAcknowledged)
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
                    result.Message = ConstMessages.DeletedNotAllowedForDependencies;
                }

            }
            catch (Exception e)
            {
                result.Message = ConstMessages.ExceptionOccured;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> EditProductUnit(ProductUnitDTO dto)
        {
            RepositoryOperationResult result = new RepositoryOperationResult();
            var equallentModel = _mapper.Map<Entities.Shop.ProductUnit.ProductUnit>(dto);

            var availableEntity = await _productContext.ProductUnitCollection
                    .Find(_ => _.ProductUnitId == dto.ProductUnitId).FirstOrDefaultAsync();

            if (availableEntity != null)
            {
                #region Add Modification
                var currentModification = availableEntity.Modifications;
                currentModification ??= new List<Modification>();
                var mod = GetCurrentModification(dto.ModificationReason);

                currentModification.Add(mod);
                #endregion

                equallentModel.Modifications = currentModification;

                equallentModel.CreationDate = availableEntity.CreationDate;
                equallentModel.CreatorUserId = availableEntity.CreatorUserId;
                equallentModel.CreatorUserName = availableEntity.CreatorUserName;

                var updateResult = await _productContext.ProductUnitCollection
                   .ReplaceOneAsync(_ => _.ProductUnitId == availableEntity.ProductUnitId, equallentModel);

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

        public Entities.Shop.ProductUnit.ProductUnit FetchByName(string unitName)
        {
            Entities.Shop.ProductUnit.ProductUnit result;
            result = _productContext.ProductUnitCollection.Find(_ => _.UnitName == unitName).FirstOrDefault();
            return result;
        }

        public ProductUnitDTO FetchUnit(string productUnitId)
        {
            var result = new ProductUnitDTO();
           
            try
            {
                var model = _productContext.ProductUnitCollection.Find(_ => _.ProductUnitId == productUnitId).FirstOrDefault();
                result = _mapper.Map<ProductUnitDTO>(model);
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }

        public async Task<PagedItems<ProductUnitDTO>> List(string queryString)
        {
            PagedItems<ProductUnitDTO> result = new PagedItems<ProductUnitDTO>();
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

                var page = Convert.ToInt32(filter["CurrentPage"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);

                long totalCount = await _productContext.ProductUnitCollection.Find(c => true).CountDocumentsAsync();
                var list = _productContext.ProductUnitCollection.AsQueryable().Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new ProductUnitDTO()
                   {
                      ProductUnitId = _.ProductUnitId,
                      ProductUnitName = _.UnitName
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
                result.Items = new List<ProductUnitDTO>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }
    }
}
