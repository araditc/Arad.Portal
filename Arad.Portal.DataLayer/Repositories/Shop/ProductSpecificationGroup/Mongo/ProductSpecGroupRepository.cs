using Arad.Portal.DataLayer.Contracts.Shop.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Models.ProductSpecificationGroup;
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
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Repositories.Shop.ProductSpecification.Mongo;
using System.Collections.Specialized;
using System.Web;
using Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo;
using Arad.Portal.DataLayer.Entities.Shop.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Currency.Mongo;

namespace Arad.Portal.DataLayer.Repositories.Shop.ProductSpecificationGroup.Mongo
{
    public class ProductSpecGroupRepository : BaseRepository, IProductSpecGroupRepository
    {
        private readonly IMapper _mapper;
        private readonly ProductContext _productContext;
        private readonly LanguageContext _languageContext;
        
        public ProductSpecGroupRepository(IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env,
            IMapper mapper, ProductContext productContext, LanguageContext langContext)
            : base(httpContextAccessor, env)
        {
            _mapper = mapper;
            _productContext = productContext;
            _languageContext = langContext;
        }

        public async Task<Result> Add(SpecificationGroupDTO dto)
        {
            Result result = new Result();
            try
            {
                var equallentEntity = _mapper.Map<ProductSpecGroup>(dto);

                equallentEntity.CreationDate = DateTime.Now;
                equallentEntity.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                equallentEntity.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
                equallentEntity.SpecificationGroupId = Guid.NewGuid().ToString();
                equallentEntity.IsActive = true;
                await _productContext.SpecGroupCollection.InsertOneAsync(equallentEntity);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            catch (Exception e)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }
            return result;
        }

        public List<SelectListModel> AllActiveSpecificationGroup(string langId)
        {
            var result = new List<SelectListModel>();
            result = _productContext.SpecGroupCollection.Find(_=>_.IsActive)
                .Project(_=> new SelectListModel()
                {
                    Text = _.GroupNames.Where(a => a.LanguageId == langId).Count() != 0 ?
                         _.GroupNames.FirstOrDefault(a => a.LanguageId == langId).Name : "",
                    Value = _.SpecificationGroupId
                }).ToList();
            return result;
        }

        public async Task<Result> Delete(string productSpecificationGroupId,
            string modificationReason)
        {
            Result result = new Result();

            try
            {
                #region check object dependency
                var allowDeletion = true;
                if (_productContext.SpecificationCollection
                    .AsQueryable().Any(_ => _.SpecificationGroupId == productSpecificationGroupId))
                {
                    allowDeletion = false;
                }

                #endregion
                if (allowDeletion)
                {
                    var entity = _productContext.SpecGroupCollection
                        .Find(_ => _.SpecificationGroupId == productSpecificationGroupId).FirstOrDefault();
                    if (entity != null)
                    {
                        entity.IsDeleted = true;
                        #region add modification
                        var mod = GetCurrentModification(modificationReason);
                        entity.Modifications.Add(mod);
                        #endregion

                        var updateResult = await _productContext.SpecGroupCollection.ReplaceOneAsync(_ => _.SpecificationGroupId == productSpecificationGroupId, entity);
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

        public async Task<SpecificationGroupDTO> GroupSpecificationFetch(string productSpecificationGroupId)
        {
            var result = new SpecificationGroupDTO();
            try
            {
                var group = await _productContext.SpecGroupCollection
                    .Find(_ => _.SpecificationGroupId == productSpecificationGroupId).FirstOrDefaultAsync();
                if(group != null)
                {
                    result = _mapper.Map<SpecificationGroupDTO>(group);
                }
                
            }
            catch (Exception e)
            {
                result = null;
            }
            return result;
        }

        public async Task<PagedItems<SpecificationGroupViewModel>> List(string queryString)
        {
            PagedItems<SpecificationGroupViewModel> result = new PagedItems<SpecificationGroupViewModel>();
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
                long totalCount = await _productContext.SpecGroupCollection.Find(c => true).CountDocumentsAsync();
                var list = _productContext.SpecGroupCollection.AsQueryable().Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new SpecificationGroupViewModel()
                   {
                       SpecificationGroupId = _.SpecificationGroupId,
                       IsDeleted = _.IsDeleted,
                       GroupName = _.GroupNames.Any(a => a.LanguageId == langId) ? _.GroupNames.First(a => a.LanguageId == langId) : _.GroupNames.First()
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
                result.Items = new List<SpecificationGroupViewModel>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<Result> Restore(string productSpecificationGroupId)
        {
            var result = new Result();
            var entity = _productContext.SpecGroupCollection
              .Find(_ => _.SpecificationGroupId == productSpecificationGroupId).FirstOrDefault();
            if(entity != null)
            {
                entity.IsDeleted = false;
                var updateResult = await _productContext.SpecGroupCollection
                   .ReplaceOneAsync(_ => _.SpecificationGroupId == productSpecificationGroupId, entity);
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

        public async Task<Result> Update(SpecificationGroupDTO dto)
        {
            Result result = new Result();
            //var equallentModel = _mapper.Map<ProductSpecGroup>(dto);

            var availableEntity = await _productContext.SpecGroupCollection
                    .Find(_ => _.SpecificationGroupId == dto.SpecificationGroupId).FirstOrDefaultAsync();

            if (availableEntity != null)
            {
                #region Add Modification
                var currentModifications = availableEntity.Modifications;
                var mod = GetCurrentModification(dto.ModificationReason);
                currentModifications.Add(mod);
                #endregion
                availableEntity.Modifications = currentModifications;
                if (dto.IsDeleted)
                {
                    availableEntity.IsDeleted = true;
                }
                else
                {
                    availableEntity.GroupNames = dto.GroupNames;
                }
                var updateResult = await _productContext.SpecGroupCollection
                   .ReplaceOneAsync(_ => _.SpecificationGroupId == availableEntity.SpecificationGroupId, availableEntity);

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
                result.Succeeded = false;
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }
    }
}
