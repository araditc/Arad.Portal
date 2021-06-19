using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Models.Domain;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace Arad.Portal.DataLayer.Repositories.General.Domain.Mongo
{
    public class DomainRepository : BaseRepository, IDomainRepository
    {
        private readonly DomainContext _context;
        private readonly User.UserContext _userContext;
        private readonly IMapper _mapper;
        public DomainRepository(DomainContext context,
                                User.UserContext user,
                                IHttpContextAccessor httpContextAccessor,
                                IMapper mapper): base(httpContextAccessor)
        {
            _context = context;
            _userContext = user;
            _mapper = mapper;
        }
        public async Task<RepositoryOperationResult> AddDomain(DomainDTO dto)
        {
            RepositoryOperationResult result = new RepositoryOperationResult();
            try
            {
                if (_context.Collection.Find(_ => _.DomainName == dto.DomainName).Any())
                {
                    result.Message = ConstMessages.DuplicateField;
                }
                else
                {

                    var equallentEntity = _mapper.Map<Entities.General.Domain.Domain>(dto);
                    if (!string.IsNullOrWhiteSpace(dto.OwnerUserId))
                    {
                        var ownerUser = _userContext.Collection.Find(_ => _.Id == dto.OwnerUserId).FirstOrDefault();
                        if (ownerUser != null)
                            equallentEntity.Owner = ownerUser;
                    }

                    if (dto.DomainPrice != null)
                    {
                        dto.DomainPrice.PriceId = Guid.NewGuid().ToString();
                    }
                    equallentEntity.Prices = new List<Models.Price.Price>();
                    equallentEntity.Prices.Add(dto.DomainPrice);


                    equallentEntity.Modifications = new List<Modification>();
                    equallentEntity.CreationDate = DateTime.Now;
                    equallentEntity.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                        .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                    equallentEntity.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                        .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;


                    await _context.Collection.InsertOneAsync(equallentEntity);
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                }
            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.InternalServerErrorMessage;
            }
            return result;
        }
       
        public async Task<PagedItems<DomainDTO>> AllDomainList(string queryString)
        {

            PagedItems<DomainDTO> result = new PagedItems<DomainDTO>();
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
                   .Take(pageSize).Select(_ => new DomainDTO()
                   {
                       DomainId = _.DomainId,
                       DomainName = _.DomainName,
                       DomainPrice  = _.Prices.FirstOrDefault(_=>_.IsActive),
                       OwnerUserId = _.Owner.Id,
                       OwnerUserName = _.Owner.UserName,
                       Prices = _.Prices
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
                result.Items = new List<DomainDTO>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> DeleteDomain(string domainId)
        {
            RepositoryOperationResult result = new RepositoryOperationResult();

            try
            {
                #region check object dependency 
                //??
                //checke all dependencies in other entities if any dependecy exist delete not allowed
                var allowDeletion = true;
                #endregion
                if(allowDeletion)
                {
                    var delResult = await _context.Collection.DeleteOneAsync(_ => _.DomainId == domainId);
                    if (delResult.IsAcknowledged)
                    {
                        result.Message = ConstMessages.SuccessfullyDone;
                        result.Succeeded = true;
                    }
                    else
                    {
                        result.Message = ConstMessages.GeneralError;
                    }
                }else
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

        public async Task<RepositoryOperationResult> DomainChangePrice(DomainPrice dto)
        {
            var result = new RepositoryOperationResult();


            var dbEntity = _context.Collection.Find(_ => _.DomainId == dto.DomainId).FirstOrDefault();
            dbEntity.Prices.FirstOrDefault(_ => _.IsActive).EndDate = DateTime.Now;
            dbEntity.Prices.FirstOrDefault(_ => _.IsActive).IsActive = false;
            dto.Price.IsActive = true;
            dto.Price.PriceId = Guid.NewGuid().ToString();
            dbEntity.Prices.Add(dto.Price);

            var mod = GetCurrentModification(dto.ModificationReason);
            dbEntity.Modifications.Add(mod);

            var updateResult = await _context.Collection
                   .ReplaceOneAsync(_ => _.DomainId == dto.DomainId, dbEntity);
            if(updateResult.IsAcknowledged)
            {
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            else
            {
                result.Message = ConstMessages.ErrorInSaving;

            }
            return result;
        }

        public RepositoryOperationResult<DomainDTO> FetchDomain(string domainId)
        {
            RepositoryOperationResult<DomainDTO> result
                = new RepositoryOperationResult<DomainDTO>();
            try
            {
                var dbEntity = _context.Collection.AsQueryable().FirstOrDefault(_ => _.DomainId == domainId);
                if (dbEntity == null)
                {
                    result.Message = ConstMessages.ObjectNotFound;
                }

                var dto = _mapper.Map<DomainDTO>(dbEntity);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
                result.ReturnValue = dto;

            }
            catch (Exception)
            {
                result.Message = ConstMessages.ExceptionOccured;
            }

            return result;
        }
    }
}
