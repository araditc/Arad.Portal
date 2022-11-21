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
using Arad.Portal.DataLayer.Entities.General.Role;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;

namespace Arad.Portal.DataLayer.Repositories.Shop.ProductUnit.Mongo
{
    public class ProductUnitRepository : BaseRepository, IProductUnitRepository
    {
       
        private readonly ProductContext _productContext;
        private readonly LanguageContext _languageContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public ProductUnitRepository(
            ProductContext productContext,
            LanguageContext languageContext,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager,
            IMapper mapper, IHttpContextAccessor httpContextAccessor): base(httpContextAccessor, env)
        {
            _mapper = mapper;
            _languageContext = languageContext;
            _productContext = productContext;
            _userManager = userManager;
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
                    var unitEntity = _productContext.ProductUnitCollection.Find(_ => _.ProductUnitId == productUnitId).FirstOrDefault();
                    if(unitEntity != null)
                    {
                        unitEntity.IsDeleted = true;
                        var updateRes = await _productContext.ProductUnitCollection.ReplaceOneAsync(_ => _.ProductUnitId == productUnitId, unitEntity);

                        if (updateRes.IsAcknowledged)
                        {
                            result.Message = ConstMessages.SuccessfullyDone;
                            result.Succeeded = true;
                        }
                        else
                        {
                            result.Message = ConstMessages.GeneralError;
                        }
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

        public async Task<List<SelectListModel>> GetAllActiveProductUnit(string langId, string currentUserId, string domainId = "")
        {
            var result = new List<SelectListModel>();
            var userEntity = await _userManager.FindByIdAsync(currentUserId);

            if(userEntity.IsSystemAccount && string.IsNullOrWhiteSpace(domainId))
            {
                result = _productContext.ProductUnitCollection.Find(_ => _.IsActive && !_.IsDeleted)
               .Project(_ => new SelectListModel()
               {
                   Text = _.UnitNames.Where(a => a.LanguageId == langId).Count() != 0 ?
                        _.UnitNames.FirstOrDefault(a => a.LanguageId == langId).Name : "",
                   Value = _.ProductUnitId
               }).ToList();
            }else
            {
                result = _productContext.ProductUnitCollection.AsQueryable().Where(_=> _.IsActive && !_.IsDeleted &&
                _.AssociatedDomainId == (!string.IsNullOrWhiteSpace(domainId) ? domainId : userEntity.Domains.FirstOrDefault(a=>a.IsOwner).DomainId))
               .Select(_ => new SelectListModel()
               {
                   Text = _.UnitNames.Where(a => a.LanguageId == langId).Count() != 0 ?
                        _.UnitNames.FirstOrDefault(a => a.LanguageId == langId).Name : "",
                   Value = _.ProductUnitId
               }).ToList();
            }
           
            return result;
        }

        public async Task<PagedItems<ProductUnitViewModel>> List(string queryString, ApplicationUser user)
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
                if (string.IsNullOrWhiteSpace(filter["Name"]))
                {
                    filter.Set("Name", "");
                }
                var langId = filter["LanguageId"].ToString();
                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);
                var filterName = filter["Name"].ToString();
                long totalCount = 0;
                var domainId = ""
;                if(user.IsSystemAccount)
                {
                    totalCount = await _productContext.ProductUnitCollection.Find(c => true).CountDocumentsAsync();
                }else
                {
                    domainId = user.Domains.FirstOrDefault(_ => _.IsOwner).DomainId;
                    totalCount = await _productContext.ProductUnitCollection.Find(_=>_.AssociatedDomainId == domainId).CountDocumentsAsync();
                }
                
                var list = _productContext.ProductUnitCollection.AsQueryable()
                    .Where(_=>_.UnitNames.Any( a => a.Name.Contains(filterName)) && (domainId == "" || _.AssociatedDomainId == domainId)).Skip((page - 1) * pageSize)
                   .OrderByDescending(_=>_.CreationDate)
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
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            
            return result;
        }
    }
}
