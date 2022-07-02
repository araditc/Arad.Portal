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
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;
using Microsoft.AspNetCore.Hosting;

namespace Arad.Portal.DataLayer.Repositories.Shop.ProductSpecification.Mongo
{
    public class ProductSpecificationRepository : BaseRepository, IProductSpecificationRepository
    {
        private readonly IMapper _mapper;
      
        private readonly ProductContext _productContext;
        private readonly LanguageContext _languageContext;
       
        public ProductSpecificationRepository(IHttpContextAccessor httpContextAccessor,
            IMapper mapper, 
            IWebHostEnvironment env,
            ProductContext productContext, LanguageContext languageContext): base(httpContextAccessor, env)
        {
            _mapper = mapper;
            _productContext = productContext;
            _languageContext = languageContext;     
        }
        public async Task<Result> Add(ProductSpecificationDTO dto)
        {
            Result result = new Result();
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
                equallentEntity.IsActive = true;
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

        public async Task<Result> Delete(string specificationId, string modificationReason)
        {
            Result result = new Result();

            try
            {
                #region check object dependency
                var allowDeletion = true;
                //??? this part should be checked in mongo
                var check = _productContext.ProductCollection
                    .AsQueryable()
                    .Any(baseProduct => baseProduct.Specifications.Any(_ => _.SpecificationId == specificationId));
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

        public async Task<ProductSpecificationDTO> SpecificationFetch(string specId)
        {
            ProductSpecificationDTO result = null;
            
            var entity = await _productContext.SpecificationCollection
                .Find(_ => _.ProductSpecificationId == specId).FirstOrDefaultAsync();
            if(entity != null)
            {
                result = _mapper.Map<ProductSpecificationDTO>(entity);
            }
            
            return result;
        }

        public Result<Entities.Shop.ProductSpecification.ProductSpecification> GetEntity(string specId)
        {
            Result<Entities.Shop.ProductSpecification.ProductSpecification> result =
               new Result<Entities.Shop.ProductSpecification.ProductSpecification>();
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

        
        public Result<List<MultiLingualProperty>> GetSpecificationValues(string productSpecificationId)
        {
            var result = new Result<List<MultiLingualProperty>>();
            var lst = _productContext.SpecificationCollection.AsQueryable()
                .Where(_ => _.ProductSpecificationId == productSpecificationId);
            if(lst != null && lst.Count() > 0)
            {
                var entity = lst;
              //  result.ReturnValue = entity.SpecificationValues;
                result.Message = ConstMessages.SuccessfullyDone;
                result.Succeeded = true;
            }
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public async Task<PagedItems<ProductSpecificationViewModel>> List(string queryString)
        {
            PagedItems<ProductSpecificationViewModel> result = new PagedItems<ProductSpecificationViewModel>();
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
                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);
                var langId = filter["LanguageId"].ToString();
                long totalCount = await _productContext.SpecificationCollection.Find(c => true).CountDocumentsAsync();
                var list = _productContext.SpecificationCollection.AsQueryable().Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new ProductSpecificationViewModel()
                   {
                      ProductSpecificationId = _.ProductSpecificationId,
                      SpecificationGroupId = _.SpecificationGroupId,
                      SpecificationNameValues = _.SpecificationNameValues.Any(a=>a.LanguageId == langId) ?
                      _.SpecificationNameValues.FirstOrDefault(a => a.LanguageId == langId) : 
                      _.SpecificationNameValues.FirstOrDefault()
                     
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
                result.Items = new List<ProductSpecificationViewModel>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<Result> Update(ProductSpecificationDTO dto)
        {
            var result = new Result();

            var availableEntity = await _productContext.SpecificationCollection
                    .Find(_ => _.ProductSpecificationId.Equals(dto.ProductSpecificationId)).FirstOrDefaultAsync();

            if (availableEntity != null)
            {
                #region Add Modification
                var currentModifications = availableEntity.Modifications;
                var mod = GetCurrentModification(dto.ModificationReason);
                currentModifications.Add(mod);
                #endregion

                availableEntity.Modifications = currentModifications;
                availableEntity.SpecificationGroupId = dto.SpecificationGroupId;
                availableEntity.SpecificationNameValues = dto.SpecificationNameValues;

              
                var updateResult = await _productContext.SpecificationCollection
                   .ReplaceOneAsync(_ => _.ProductSpecificationId == availableEntity.ProductSpecificationId, availableEntity);

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

        public async Task<Result> Restore(string specificationId)
        {
            var result = new Result();
            var entity = _productContext.SpecificationCollection
              .Find(_ => _.ProductSpecificationId == specificationId).FirstOrDefault();
            if(entity != null)
            {
                entity.IsDeleted = false;
                var updateResult = await _productContext.SpecificationCollection
                   .ReplaceOneAsync(_ => _.ProductSpecificationId == specificationId, entity);
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
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            
            return result;
        }

        public List<SelectListModel> GetSpecInGroupAndLanguage(string specificationGroupId, string languageId)
        {
            var result = new List<SelectListModel>();
            result = _productContext.SpecificationCollection.Find(_ => _.SpecificationGroupId == specificationGroupId)
                .Project(_ => new SelectListModel()
                {
                    Text = _.SpecificationNameValues.Where(a => a.LanguageId == languageId).Count() != 0 ?
                         _.SpecificationNameValues.FirstOrDefault(a => a.LanguageId == languageId).Name : "",
                    Value = _.ProductSpecificationId
                }).ToList();
            return result;
        }

        public List<SelectListModel> GetSpcificationValuesInLanguage(string specificationId, string languageId)
        {
            var result = new List<SelectListModel>();
            var res = _productContext.SpecificationCollection.Find(_ => _.ProductSpecificationId == specificationId).FirstOrDefault();

            result = res.SpecificationNameValues.Where(_ => _.LanguageId == languageId).FirstOrDefault().NameValues.Select(_ => new SelectListModel()
            {
                Text = _.Trim(),
                Value = ""
            }).ToList(); 
            return result;
        }
    }
}
