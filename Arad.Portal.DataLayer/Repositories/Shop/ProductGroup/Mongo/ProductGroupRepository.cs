using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.ProductGroup;
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
using MongoDB.Driver.Linq;
using Arad.Portal.DataLayer.Models.Promotion;
using Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo;
using System.Web;
using System.Collections.Specialized;
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Currency.Mongo;
using System.Globalization;

namespace Arad.Portal.DataLayer.Repositories.Shop.ProductGroup.Mongo
{
    public class ProductGroupRepository : BaseRepository, IProductGroupRepository
    {
        private readonly ProductContext _productContext;
        private readonly LanguageContext _languageContext;
        private readonly CurrencyContext _currencyContext;
        private readonly IMapper _mapper;
      
        public ProductGroupRepository(ProductContext context,
            LanguageContext languageContext, CurrencyContext currencyContext,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper):
            base(httpContextAccessor)
        {
            
            _productContext = context;
            _languageContext = languageContext;
            _currencyContext = currencyContext;
            _mapper = mapper;
        }

        public async Task<RepositoryOperationResult> Add(ProductGroupDTO dto)
        {
            var result = new RepositoryOperationResult();
            var equallentModel = new Entities.Shop.ProductGroup.ProductGroup();

            equallentModel.CreationDate = DateTime.Now;
            equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

           
            equallentModel.MultiLingualProperties.Add(dto.MultiLingualProperty);
            equallentModel.ParentId = dto.ParentId;
            try
            {
                equallentModel.ProductGroupId = Guid.NewGuid().ToString();
                await _productContext.ProductGroupCollection.InsertOneAsync(equallentModel);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }

            return result;
        }

        public async Task<List<ProductGroupDTO>> AllProductGroups()
        {
            var result = new List<ProductGroupDTO>();
            var lst = await _productContext.ProductGroupCollection.AsQueryable().Where(_ => !_.IsDeleted).ToListAsync();
            result = _mapper.Map<List<ProductGroupDTO>>(lst);
            return result;
        }

        public async Task<RepositoryOperationResult> Delete(string productGroupId, string modificationReason)
        {
            var result = new RepositoryOperationResult();
            try
            {
                var allowDeletion = false;
                var check = _productContext.ProductCollection.AsQueryable().Any(_ => _.GroupIds.Contains(productGroupId));
                if(!check)
                {
                    allowDeletion = true;
                }
                if(allowDeletion)
                {
                    var groupEntity = _productContext.ProductGroupCollection.Find(_ => _.ProductGroupId == productGroupId).First();
                    if (groupEntity != null)
                    {
                        groupEntity.IsDeleted = true;

                        #region Add Modification record
                        var currentModifications = groupEntity.Modifications;
                        var mod = GetCurrentModification(modificationReason);
                        currentModifications.Add(mod);
                        groupEntity.Modifications = currentModifications;
                        #endregion

                        var updateResult = await _productContext.ProductGroupCollection.ReplaceOneAsync(_ => _.ProductGroupId == productGroupId, groupEntity);
                        if (updateResult.IsAcknowledged)
                        {
                            result.Succeeded = true;
                            result.Message = ConstMessages.SuccessfullyDone;
                        }
                        else
                        {
                            result.Message = ConstMessages.ErrorInSaving;
                        }
                    }
                    else
                    {
                        result.Message = ConstMessages.ObjectNotFound;
                    }
                }else
                {
                    result.Message = ConstMessages.DeletedNotAllowedForDependencies;
                }
                
            }
            catch (Exception e)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }
            return result;
        }

        public ProductGroupDTO ProductGroupFetch(string productGroupId, string langId)
        {
            var result = new ProductGroupDTO();
            var entity = _productContext.ProductGroupCollection.Find(_ => _.ProductGroupId == productGroupId).FirstOrDefault();
            if(entity != null)
            {
                result.ProductGroupId = entity.ProductGroupId;
                result.ParentId = entity.ParentId;
                result.MultiLingualProperty = entity.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == langId) != null ?
                    entity.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == langId) : new MultiLingualProperty();
            }
            return result;
        }

        public List<ProductGroupDTO> GetsDirectChildren(string productGroupId)
        {
            var result = new List<ProductGroupDTO>();
            try
            {
                var lst = _productContext.ProductGroupCollection.AsQueryable()
                    .Where(_ => _.ParentId == productGroupId && !_.IsDeleted).ToList();

                result = _mapper.Map<List<ProductGroupDTO>>(lst);
            }
            catch (Exception e)
            {
            }
            return result;
        }

        public List<ProductGroupDTO> GetsParents()
        {
            var result = new List<ProductGroupDTO>();
            var lst= _productContext.ProductGroupCollection.AsQueryable()
               .Where(_ => !_.IsDeleted && _.ParentId == null)
               .ToList();
            result = _mapper.Map<List<ProductGroupDTO>>(lst);
            return result;
        }

        public bool GroupExistance(string productGroupId)
        {
            bool result = false;
            try
            {
                var exist = _productContext.ProductGroupCollection.AsQueryable()
                    .Any(_=>_.ProductGroupId == productGroupId);
                if (exist)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
               
            }
            return result;
        }

        public async Task<RepositoryOperationResult> AddPromotionToGroup(string productGroupId,
            PromotionDTO promotionDto, string modificationReason)
        {
            var result = new RepositoryOperationResult();
            var groupEntity = await _productContext.ProductGroupCollection
                .Find(_ => _.ProductGroupId == productGroupId).FirstOrDefaultAsync();
            if(groupEntity != null)
            {
                var promotion = _mapper.Map<Entities.Shop.Promotion.Promotion>(promotionDto);
                groupEntity.Promotion = promotion;

                #region add modification
                var modification = GetCurrentModification(modificationReason);
                groupEntity.Modifications.Add(modification);
                #endregion

                try
                {
                    var updateResult = await _productContext.ProductGroupCollection
                    .ReplaceOneAsync(_ => _.ProductGroupId == productGroupId, groupEntity);

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
                catch (Exception)
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

        public async Task<PagedItems<ProductGroupDTO>> List(string queryString)
        {
            PagedItems<ProductGroupDTO> result = new PagedItems<ProductGroupDTO>();
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
                    var symbol = CultureInfo.CurrentCulture.Name;
                    var lan = _languageContext.Collection.Find(_ => _.Symbol == symbol).FirstOrDefault();
                    filter.Set("LanguageId", lan.LanguageId);
                }
                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);
                var langId = filter["LanguageId"].ToString();
                long totalCount = await _productContext.ProductGroupCollection.Find(c => true).CountDocumentsAsync();
                var lst = _productContext.ProductGroupCollection.AsQueryable().Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new ProductGroupDTO()
                   {
                       ProductGroupId = _.ProductGroupId,
                       ParentId = _.ParentId,
                       IsDeleted = _.IsDeleted,
                       MultiLingualProperty = _.MultiLingualProperties.ToList().First(a=>a.LanguageId == langId)
                   }).ToList();
                result.CurrentPage = page;
                result.Items = lst;
                result.ItemsCount = totalCount;
                result.PageSize = pageSize;
                result.QueryString = queryString;
            }
            catch (Exception ex)
            {
                result.CurrentPage = 1;
                result.Items = new List<ProductGroupDTO>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> Update(ProductGroupDTO dto)
        {
            var result = new RepositoryOperationResult();
            var entity = _productContext.ProductGroupCollection
                .Find(_ => _.ProductGroupId == dto.ProductGroupId).FirstOrDefault();

            entity.ParentId = dto.ParentId;

            var innerModel = entity.MultiLingualProperties
                .FirstOrDefault(_ => _.MultiLingualPropertyId == dto.MultiLingualProperty.MultiLingualPropertyId);
            entity.MultiLingualProperties.Remove(innerModel);
            entity.MultiLingualProperties.Add(dto.MultiLingualProperty);
           
            #region Add Modification
            var currentModifications = entity.Modifications;
            var mod = GetCurrentModification(dto.ModificationReason);
            currentModifications.Add(mod);
            #endregion

            var updateResult = await _productContext.ProductGroupCollection
                .ReplaceOneAsync(_ => _.ProductGroupId == dto.ProductGroupId, entity);
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

        public async Task<RepositoryOperationResult> Restore(string id)
        {
            var result = new RepositoryOperationResult();
            var entity = _productContext.ProductGroupCollection
              .Find(_ => _.ProductGroupId == id).FirstOrDefault();
            entity.IsDeleted = false;
            var updateResult = await _productContext.ProductGroupCollection
               .ReplaceOneAsync(_ => _.ProductGroupId == id, entity);
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

        public List<SelectListModel> GetAlActiveProductGroup(string langId)
        {
            var result = new List<SelectListModel>();
            result = _productContext.ProductGroupCollection.AsQueryable()
                .Where(_ => _.IsActive).Select(_ => new SelectListModel()
            {
                Text = _.MultiLingualProperties.First(a=>a.LanguageId == langId) != null ?
                _.MultiLingualProperties.First(a => a.LanguageId == langId).Name : "",
                Value = _.ProductGroupId
            }).OrderBy(_=>_.Text).ToList();
            return result;
        }

        public Entities.Shop.ProductGroup.ProductGroup FetchWholeProductGroup(string productGroupId)
        {
            Entities.Shop.ProductGroup.ProductGroup result;
            result = _productContext.ProductGroupCollection.Find(_ => _.ProductGroupId == productGroupId).FirstOrDefault();
            return result;
        }
    }
}
