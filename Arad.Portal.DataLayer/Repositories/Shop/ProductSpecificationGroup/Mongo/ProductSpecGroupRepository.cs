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
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;

namespace Arad.Portal.DataLayer.Repositories.Shop.ProductSpecificationGroup.Mongo
{
    public class ProductSpecGroupRepository : BaseRepository, IProductSpecGroupRepository
    {
        private readonly IMapper _mapper;
        private readonly ProductContext _productContext;
        private readonly LanguageContext _languageContext;
        private readonly UserManager<ApplicationUser> _userManager;
        
        public ProductSpecGroupRepository(IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env,UserManager<ApplicationUser> userManager,
            IMapper mapper, ProductContext productContext, LanguageContext langContext)
            : base(httpContextAccessor, env)
        {
            _mapper = mapper;
            _productContext = productContext;
            _languageContext = langContext;
            _userManager = userManager;
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

        public async Task<List<SelectListModel>> AllActiveSpecificationGroup(string langId, string currentUserId, string domainId = "")
        {
            var result = new List<SelectListModel>();
            
            try
            {
                var userDb = await _userManager.FindByIdAsync(currentUserId);
                if (userDb.IsSystemAccount && string.IsNullOrWhiteSpace(domainId))
                {
                    result = _productContext.SpecGroupCollection.Find(_ => _.IsActive && !_.IsDeleted)
                    .Project(_ => new SelectListModel()
                    {
                        Text = _.GroupNames.Where(a => a.LanguageId == langId).Count() != 0 ?
                             _.GroupNames.FirstOrDefault(a => a.LanguageId == langId).Name : "",
                        Value = _.SpecificationGroupId
                    }).ToList();
                }
                else
                {
                    var finalDomainId = !string.IsNullOrWhiteSpace(domainId) ? domainId : userDb.Domains.FirstOrDefault(a => a.IsOwner).DomainId;
                    var lst = _productContext.SpecGroupCollection.AsQueryable()
                    .Where(_ => _.IsActive && !_.IsDeleted && _.AssociatedDomainId == finalDomainId).ToList();

                    result = lst.Select(b => new SelectListModel()
                    {
                        Text = b.GroupNames.Any(a => a.LanguageId == langId) ?
                             b.GroupNames.FirstOrDefault(a => a.LanguageId == langId).Name : "",
                        Value = b.SpecificationGroupId
                    }).ToList();
                }

            }
            catch (Exception ex)
            {
            }
            
            
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
                        result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
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

        public async Task<PagedItems<SpecificationGroupViewModel>> List(string queryString, ApplicationUser user)
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
                if (string.IsNullOrWhiteSpace(filter["Name"]))
                {
                    filter.Set("Name", "");
                }
                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);
                var langId = filter["LanguageId"].ToString();
                var filterName = filter["Name"].ToString();
                long totalCount = 0;
                string domainId = "";
                if(user.IsSystemAccount)
                {
                    totalCount = await _productContext.SpecGroupCollection.Find(c => true).CountDocumentsAsync();
                }else
                {
                    domainId = user.Domains.FirstOrDefault(_ => _.IsOwner).DomainId;
                    totalCount = await _productContext.SpecGroupCollection.Find(_=>_.AssociatedDomainId == domainId).CountDocumentsAsync();
                }
                 
                var list = _productContext.SpecGroupCollection.AsQueryable()
                       .Where(a => a.GroupNames.Any(b => b.Name.Contains(filterName)) 
                                && a.GroupNames.Any(a => a.LanguageId == langId) 
                                && (domainId == "" || a.AssociatedDomainId == domainId)).Skip((page - 1) * pageSize)
                       .OrderByDescending(_=>_.CreationDate)
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
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
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
                availableEntity.AssociatedDomainId = dto.AssociatedDomainId;
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
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            return result;
        }
    }
}
