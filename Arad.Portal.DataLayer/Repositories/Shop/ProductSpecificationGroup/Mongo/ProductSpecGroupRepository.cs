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
using Arad.Portal.DataLayer.Repositories.Shop.ProductSpecification.Mongo;
using System.Collections.Specialized;
using System.Web;

namespace Arad.Portal.DataLayer.Repositories.Shop.ProductSpecificationGroup.Mongo
{
    public class ProductSpecGroupRepository : BaseRepository , IProductSpecGroupRepository
    {
        private readonly IMapper _mapper;
        private readonly ProductSpecGroupContext _context;
        private readonly ProductSpecificationContext _productSpecificationContext;
        public ProductSpecGroupRepository(IHttpContextAccessor httpContextAccessor,
            IMapper mapper, ProductSpecGroupContext context,
            ProductSpecificationContext productSpecificationContext)
            : base(httpContextAccessor)
        {
            _mapper = mapper;
            _context = context;
            _productSpecificationContext = productSpecificationContext;
        }

        public async Task<RepositoryOperationResult> Add(SpecificationGroupDTO dto)
        {
            RepositoryOperationResult result = new RepositoryOperationResult();
            try
            {

                var equallentEntity = _mapper.Map<Entities.Shop.ProductSpecificationGroup
                    .ProductSpecGroup>(dto);
                equallentEntity.SpecificationGroupId = Guid.NewGuid().ToString();
             
                equallentEntity.CreationDate = DateTime.Now;
                equallentEntity.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                equallentEntity.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;


                await _context.Collection.InsertOneAsync(equallentEntity);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
               
            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.InternalServerErrorMessage;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> Delete(string productSpecificationGroupId,
            string modificationReason)
        {
            RepositoryOperationResult result = new RepositoryOperationResult();

            try
            {
                #region check object dependency
                var allowDeletion = true;
                if (_productSpecificationContext.Collection
                    .AsQueryable().Any(_=>_.SpecificationGroupId == productSpecificationGroupId))
                {
                    allowDeletion = false;
                }
                
                #endregion
                if (allowDeletion)
                {
                    var entity = _context.Collection
                        .Find(_ => _.SpecificationGroupId == productSpecificationGroupId).FirstOrDefault();
                    if (entity != null)
                    {
                        entity.IsDeleted = true;
                        #region add modification
                        var mod = GetCurrentModification(modificationReason);
                        entity.Modifications.Add(mod);
                        #endregion

                        var updateResult = await _context.Collection.ReplaceOneAsync(_ => _.SpecificationGroupId == productSpecificationGroupId, entity);
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
                var group = await _context.Collection
                    .Find(_=>_.SpecificationGroupId == productSpecificationGroupId).FirstOrDefaultAsync();
                result = _mapper.Map<SpecificationGroupDTO>(group);
            }
            catch (Exception e)
            {
                result = null;
            }
            return result;
        }

        public async Task<PagedItems<SpecificationGroupDTO>> List(string queryString)
        {
            PagedItems<SpecificationGroupDTO> result = new PagedItems<SpecificationGroupDTO>();
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

                long totalCount = await _context.Collection.Find(c => true).CountDocumentsAsync();
                var list = _context.Collection.AsQueryable().Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new SpecificationGroupDTO()
                   {
                       GroupName = _.GroupName,
                       LanguageId = _.LanguageId,
                       LanguageName = _.LanguageName,
                       SpecificationGroupId = _.SpecificationGroupId
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
                result.Items = new List<SpecificationGroupDTO>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> Update(SpecificationGroupDTO dto)
        {
            RepositoryOperationResult result = new RepositoryOperationResult();
            var equallentModel = _mapper.Map<Entities.Shop.ProductSpecificationGroup.ProductSpecGroup>(dto);

            var availableEntity = await _context.Collection
                    .Find(_ => _.SpecificationGroupId == dto.SpecificationGroupId).FirstOrDefaultAsync();

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

                var updateResult = await _context.Collection
                   .ReplaceOneAsync(_ => _.SpecificationGroupId == availableEntity.SpecificationGroupId, equallentModel);

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
