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
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;
using Microsoft.AspNetCore.Hosting;

namespace Arad.Portal.DataLayer.Repositories.Shop.ProductUnit.Mongo
{
    public class ProductUnitRepository : BaseRepository, IProductUnitRepository
    {
       
        private readonly ProductContext _productContext;
        private readonly LanguageContext _languageContext;
        private readonly IMapper _mapper;
        public ProductUnitRepository(
            ProductContext productContext,
            LanguageContext languageContext,
            IWebHostEnvironment env,
            IMapper mapper, IHttpContextAccessor httpContextAccessor): base(httpContextAccessor, env)
        {
            _mapper = mapper;
            _languageContext = languageContext;
            _productContext = productContext;
        }
        public async Task<Result> AddProductUnit(ProductUnitDTO dto)
        {
            Result result = new Result();
            var equallentModel = _mapper.Map<Entities.Shop.ProductUnit.ProductUnit>(dto);

            equallentModel.Modifications = new List<Modification>();

            equallentModel.CreationDate = DateTime.Now;
            equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

            equallentModel.ProductUnitId = Guid.NewGuid().ToString();
            equallentModel.IsActive = true;
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

        public async Task<Result> Delete(string productUnitId)
        {
            Result result = new Result();

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

        public async Task<Result> EditProductUnit(ProductUnitDTO dto)
        {
            Result result = new Result();
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


        public ProductUnitDTO FetchUnit(string productUnitId)
        {
            var result = new ProductUnitDTO();
           
            try
            {
                var model = _productContext.ProductUnitCollection.Find(_ => _.ProductUnitId == productUnitId).FirstOrDefault();
                if(model != null)
                result = _mapper.Map<ProductUnitDTO>(model);
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }

        public List<SelectListModel> GetAllActiveProductUnit(string langId)
        {
            var result = new List<SelectListModel>();
            result = _productContext.ProductUnitCollection.Find(_ => _.IsActive)
                .Project(_ => new SelectListModel()
                {
                    Text = _.UnitNames.Where(a => a.LanguageId == langId).Count() != 0 ?
                         _.UnitNames.FirstOrDefault(a => a.LanguageId == langId).Name : "",
                    Value = _.ProductUnitId
                }).ToList();
            return result;
        }

        public async Task<PagedItems<ProductUnitViewModel>> List(string queryString)
        {
            PagedItems<ProductUnitViewModel> result = new PagedItems<ProductUnitViewModel>();
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
                if (string.IsNullOrWhiteSpace(filter["LanguageId"]))
                {
                    var lan = _languageContext.Collection.Find(_ => _.IsDefault).FirstOrDefault();
                    filter.Set("LanguageId", lan.LanguageId);
                }
                var langId = filter["LanguageId"].ToString();
                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);

                long totalCount = await _productContext.ProductUnitCollection.Find(c => true).CountDocumentsAsync();
                var list = _productContext.ProductUnitCollection.AsQueryable().Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new ProductUnitViewModel()
                   {
                      ProductUnitId = _.ProductUnitId,
                      UnitName = _.UnitNames.Any(a => a.LanguageId == langId) ?
                      _.UnitNames.Where(a => a.LanguageId == langId).First() : _.UnitNames.First(),
                      IsDeleted = _.IsDeleted
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
                result.Items = new List<ProductUnitViewModel>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<Result> Restore(string productUnitId)
        {
            var result = new Result();
            var entity = _productContext.ProductUnitCollection
              .Find(_ => _.ProductUnitId == productUnitId).FirstOrDefault();
            if(entity != null)
            {
                entity.IsDeleted = false;
                var updateResult = await _productContext.ProductUnitCollection
                   .ReplaceOneAsync(_ => _.ProductUnitId == productUnitId, entity);
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
            }else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            
            return result;
        }
    }
}
