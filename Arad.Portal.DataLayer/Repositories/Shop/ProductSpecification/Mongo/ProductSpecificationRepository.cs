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
using Arad.Portal.DataLayer.Entities.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;

namespace Arad.Portal.DataLayer.Repositories.Shop.ProductSpecification.Mongo
{
    public class ProductSpecificationRepository : BaseRepository, IProductSpecificationRepository
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ProductContext _productContext;
        private readonly LanguageContext _languageContext;
       
        public ProductSpecificationRepository(IHttpContextAccessor httpContextAccessor,
            IMapper mapper, UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            ProductContext productContext, LanguageContext languageContext): base(httpContextAccessor, env)
        {
            _mapper = mapper;
            _userManager = userManager;
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
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
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
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            return result;
        }

        public async Task<PagedItems<ProductSpecificationViewModel>> List(string queryString, ApplicationUser user)
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
                    totalCount = await _productContext.SpecificationCollection.Find(c => true).CountDocumentsAsync();
                }else
                {
                    domainId = user.Domains.FirstOrDefault(_ => _.IsOwner).DomainId;
                    totalCount = await _productContext.SpecificationCollection.Find(_=>_.AssociatedDomainId == domainId).CountDocumentsAsync();
                }
                
                var list = _productContext.SpecificationCollection.AsQueryable()
                    .Where(_ => 
                        _.SpecificationNameValues.Any(a => a.Name.Contains(filterName)) 
                     && _.SpecificationNameValues.Any(a => a.LanguageId == langId) 
                     && (domainId == "" || _.AssociatedDomainId == domainId)).Skip((page - 1) * pageSize)
                    .OrderByDescending(_=>_.CreationDate)
                   .Take(pageSize).Select(_ => new ProductSpecificationViewModel()
                   {
                      ProductSpecificationId = _.ProductSpecificationId,
                      SpecificationGroupId = _.SpecificationGroupId,
                      SpecificationNameValues = _.SpecificationNameValues.Any(a=> a.LanguageId == langId) ?
                      _.SpecificationNameValues.First(a => a.LanguageId == langId) : 
                      _.SpecificationNameValues.First(),
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
                availableEntity.ControlType = dto.ControlType;

              
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
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            
            return result;
        }

        public async Task<List<SelectListModel>> GetSpecInGroupAndLanguage(string specificationGroupId, string languageId, string currentUserId, string domainId="")
        {
            var result = new List<SelectListModel>();
            var userDb = await _userManager.FindByIdAsync(currentUserId);

            if(userDb.IsSystemAccount &&  string.IsNullOrWhiteSpace(domainId))
            {
                result = _productContext.SpecificationCollection.Find(_ => _.SpecificationGroupId == specificationGroupId && _.IsActive && !_.IsDeleted)
               .Project(_ => new SelectListModel()
               {
                   Text = _.SpecificationNameValues.Where(a => a.LanguageId == languageId).Count() != 0 ?
                        _.SpecificationNameValues.FirstOrDefault(a => a.LanguageId == languageId).Name : "",
                   Value = _.ProductSpecificationId
               }).ToList();
            }else
            {
                result = _productContext.SpecificationCollection.AsQueryable().Where(_ => _.SpecificationGroupId == specificationGroupId && _.IsActive && !_.IsDeleted &&
                 _.AssociatedDomainId == (!string.IsNullOrWhiteSpace(domainId) ? domainId : userDb.Domains.FirstOrDefault(a=> a.IsOwner).DomainId))
               .Select(_ => new SelectListModel()
               {
                   Text = _.SpecificationNameValues.Where(a => a.LanguageId == languageId).Count() != 0 ?
                        _.SpecificationNameValues.FirstOrDefault(a => a.LanguageId == languageId).Name : "",
                   Value = _.ProductSpecificationId
               }).ToList();
            }
           
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

        public List<SelectListModel> GetAllControlTypes()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(ControlType)))
            {
                string name = Enum.GetName(typeof(ControlType), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel() { Text = Arad.Portal.GeneralLibrary.Utilities.Language.GetString("Choose"), Value = "-1" });
            return result;
        }
    }
}
