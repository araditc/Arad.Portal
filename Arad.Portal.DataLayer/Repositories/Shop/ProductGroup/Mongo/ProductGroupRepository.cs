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
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Repositories.General.Currency.Mongo;
using System.Globalization;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Models.Product;

namespace Arad.Portal.DataLayer.Repositories.Shop.ProductGroup.Mongo
{
    public class ProductGroupRepository : BaseRepository, IProductGroupRepository
    {
        private readonly ProductContext _productContext;
        private readonly LanguageContext _languageContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
      
        public ProductGroupRepository(ProductContext context,
            LanguageContext languageContext, 
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper, UserManager<ApplicationUser> userManager):
            base(httpContextAccessor)
        {
            
            _productContext = context;
            _languageContext = languageContext;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<RepositoryOperationResult> Add(ProductGroupDTO dto)
        {
            var result = new RepositoryOperationResult();
            var equallentModel = _mapper.Map<Entities.Shop.ProductGroup.ProductGroup>(dto);

            equallentModel.CreationDate = DateTime.Now;
            equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            try
            {
                equallentModel.ProductGroupId = Guid.NewGuid().ToString();
                equallentModel.IsActive = true;
                equallentModel.GroupImage = dto.GroupImage;
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

        public ProductGroupDTO ProductGroupFetch(string productGroupId)
        {
            var result = new ProductGroupDTO();
            var entity = _productContext.ProductGroupCollection
                .Find(_ => _.ProductGroupId == productGroupId).FirstOrDefault();
            if(entity != null)
            {
                result = _mapper.Map<ProductGroupDTO>(entity);
                //result.ProductGroupId = entity.ProductGroupId;
                //result.ParentId = entity.ParentId;
                //result.MultiLingualProperties = entity.MultiLingualProperties;
                //result.Promotion = entity.Promotion;
            }
            return result;
        }

        public List<ProductGroupDTO> GetsDirectChildren(string productGroupId)
        {
            var result = new List<ProductGroupDTO>();
            try
            {
                var lst = _productContext.ProductGroupCollection
                    .Find(_ => _.ParentId == productGroupId && !_.IsDeleted && _.IsActive).ToList();

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

        public async Task<PagedItems<ProductGroupViewModel>> List(string queryString)
        {
            PagedItems<ProductGroupViewModel> result = new PagedItems<ProductGroupViewModel>();
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
                if(string.IsNullOrWhiteSpace(filter["Name"]))
                {
                    filter.Set("Name", "");
                }
                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);
                var langId = filter["LanguageId"].ToString();
                var filterName = filter["Name"].ToString();
                long totalCount = await _productContext.ProductGroupCollection.Find(c => true).CountDocumentsAsync();
                var lst = _productContext.ProductGroupCollection.AsQueryable()
                    .Where(_=>_.MultiLingualProperties.Any(a => a.Name.Contains(filterName))).Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new ProductGroupViewModel()
                   {
                       ProductGroupId = _.ProductGroupId,
                       ParentId = _.ParentId,
                       IsDeleted = _.IsDeleted,
                       MultiLingualProperty = _.MultiLingualProperties.Where(a => a.LanguageId == langId).First()
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
                result.Items = new List<ProductGroupViewModel>();
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
            if(entity != null)
            {
                entity.ParentId = dto.ParentId;
               

                #region Add Modification
                var currentModifications = entity.Modifications;
                var mod = GetCurrentModification(dto.ModificationReason);
                currentModifications.Add(mod);
                #endregion
               
                entity.Modifications = currentModifications;
                entity.MultiLingualProperties = dto.MultiLingualProperties;
                entity.GroupImage = dto.GroupImage;
               
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
            }else
            {
                result.Succeeded = false;
                result.Message = ConstMessages.ObjectNotFound;
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
       
        public Entities.Shop.ProductGroup.ProductGroup FetchWholeProductGroup(string productGroupId)
        {
            Entities.Shop.ProductGroup.ProductGroup result;
            result = _productContext.ProductGroupCollection.Find(_ => _.ProductGroupId == productGroupId).FirstOrDefault();
            return result;
        }

        public async Task<List<SelectListModel>> GetAlActiveProductGroup(string langId, string currentUserId)
        {
            var result = new List<SelectListModel>();
            var dbUser = await _userManager.FindByIdAsync(currentUserId);
            if(dbUser.IsSystemAccount)
            {
                result = _productContext.ProductGroupCollection.Find(_ => _.IsActive && !_.IsDeleted)
               .Project(_ => new SelectListModel()
               {
                   Text = _.MultiLingualProperties.Where(a => a.LanguageId == langId).Count() != 0 ?
                        _.MultiLingualProperties.First(a => a.LanguageId == langId).Name : "",
                   Value = _.ProductGroupId
               }).ToList();
            }
            else
            {
                result = _productContext.ProductGroupCollection.Find(_ => dbUser.Profile.Access.AccessibleProductGroupIds.Contains(_.ProductGroupId))
               .Project(_ => new SelectListModel()
               {
                   Text = _.MultiLingualProperties.Where(a => a.LanguageId == langId).Count() != 0 ?
                        _.MultiLingualProperties.First(a => a.LanguageId == langId).Name : "",
                   Value = _.ProductGroupId
               }).ToList();
            }
           
            result.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "-1" });
            return result;
        }

        public ProductGroupDTO FetchBySlug(string slug, string domainName)
        {
            var result = new ProductGroupDTO();
            var urlFriend = $"{domainName}/category/{slug}";
            var groupEntity = _productContext.ProductGroupCollection
                .Find(_ => _.MultiLingualProperties.Any(a => a.UrlFriend == urlFriend)).First();

            result = _mapper.Map<ProductGroupDTO>(groupEntity);
            return result;
        }

        public CommonViewModel FetchByCode(long groupCode)
        {
            var result = new CommonViewModel();

            var productEntity = _productContext.ProductGroupCollection
                .Find(_ => _.GroupCode == groupCode).First();

            result.Groups = GetsDirectChildren(productEntity.ProductGroupId).Skip(0).Take(10).ToList();

            result.ProductList = GetLatestProductInThisGroup(10, productEntity.ProductGroupId);
            return result;
        }
        
        //public ProductGroupDTO FetchByCode(long groupCode)
        //{
        //    var result = new ProductGroupDTO();
        //    var groupEntity = _productContext.ProductGroupCollection
        //        .Find(_ => _.GroupCode == groupCode).First();
        //    result = _mapper.Map<ProductGroupDTO>(groupEntity);
        //    return result;
        //}

        public List<ProductOutputDTO> GetLatestProductInThisGroup(int count, string productGroupId)
        {
            var result = new List<ProductOutputDTO>();
            var list = _productContext.ProductCollection
                 .Find(_ => _.GroupIds.Contains(productGroupId)).SortByDescending(_ => _.CreationDate).Skip(0).Limit(count).ToList();

            result = _mapper.Map<List<ProductOutputDTO>>(list);
            return result;
        }
    }
}
