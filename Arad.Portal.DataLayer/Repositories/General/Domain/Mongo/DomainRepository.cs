using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Domain;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.General.User.Mongo;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Arad.Portal.GeneralLibrary.Utilities;

namespace Arad.Portal.DataLayer.Repositories.General.Domain.Mongo
{
    public class DomainRepository : BaseRepository, IDomainRepository
    {
        private readonly DomainContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        public DomainRepository(DomainContext context,
                                UserContext user,
                                IHttpContextAccessor httpContextAccessor,
                                UserManager<ApplicationUser> userManager,
                                IMapper mapper): base(httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
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
                    equallentEntity.DomainId = Guid.NewGuid().ToString();
                    foreach (var price in dto.Prices)
                    {
                        price.PriceId = Guid.NewGuid().ToString();
                    }
                    #region Prices
                    equallentEntity.Prices = new List<Price>();
                    foreach (var price in dto.Prices.OrderBy(_ => _.StartDate))
                    {
                        if (price.IsActive && string.IsNullOrWhiteSpace(price.EndDate))//price is valid from client
                        {
                            if (equallentEntity.Prices.Any(_ => _.CurrencyId == price.CurrencyId && _.EndDate != null && _.IsActive))
                            {
                                var exist = equallentEntity.Prices.First(_ => _.CurrencyId == price.CurrencyId && _.EndDate != null && _.IsActive);
                                equallentEntity.Prices.Remove(exist);
                                exist.IsActive = false;
                                exist.EndDate = DateTime.UtcNow;
                                equallentEntity.Prices.Add(exist);

                            }
                        }

                        var p = new Price()
                        {
                            PriceId = !string.IsNullOrWhiteSpace(price.PriceId) ? price.PriceId : Guid.NewGuid().ToString(),
                            CurrencyId = price.CurrencyId,
                            CurrencyName = price.CurrencyName,
                            IsActive = !string.IsNullOrWhiteSpace(price.PriceId) ? price.IsActive : true,
                            Prefix = price.Prefix,
                            PriceValue = price.PriceValue,
                            StartDate = DateHelper.ToEnglishDate(price.StartDate).ToUniversalTime(),
                            EndDate = !string.IsNullOrWhiteSpace(price.EndDate) ?
                            GeneralLibrary.Utilities.DateHelper.ToEnglishDate(price.EndDate).ToUniversalTime() : null
                        };
                        equallentEntity.Prices.Add(p);
                    }
                    #endregion
                    equallentEntity.Modifications = new List<Modification>();
                    equallentEntity.CreationDate = DateTime.Now;
                    equallentEntity.CreatorUserId = this.GetUserId();
                    equallentEntity.CreatorUserName = this.GetUserName();


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
       
        public async Task<PagedItems<DomainViewModel>> AllDomainList(string queryString)
        {

            PagedItems<DomainViewModel> result = new PagedItems<DomainViewModel>();
            try
            {
                NameValueCollection filter = HttpUtility.ParseQueryString(queryString);

                if (string.IsNullOrWhiteSpace(filter["page"]))
                {
                    filter.Set("CurrentPage", "1");
                }

                if (string.IsNullOrWhiteSpace(filter["PageSize"]))
                {
                    filter.Set("PageSize", "20");
                }

                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);

                long totalCount = await _context.Collection.Find(c => true).CountDocumentsAsync();
                var list = _context.Collection.AsQueryable().Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new DomainViewModel()
                   {
                       DomainId = _.DomainId,
                       DomainName = _.DomainName,
                       Prices = _.Prices, 
                       //DomainPrice  = _.Prices.FirstOrDefault(_=>_.IsActive),
                       OwnerUserId = _.OwnerUserId.ToString(),
                       OwnerUserName = _.OwnerUserName,
                       DefaultLanguageId = _.DefaultLanguageId,
                       DefaultLanguageName =_.DefaultLanguageName,
                       IsDefault = _.IsDefault
                      
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
                result.Items = new List<DomainViewModel>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> DeleteDomain(string domainId, string modificationReason)
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
                    var entity = _context.Collection.Find(_ => _.DomainId == domainId).FirstOrDefault();
                    if(entity != null)
                    {
                        entity.IsDeleted = true;
                        #region add modification
                        var mod = GetCurrentModification(modificationReason);
                        entity.Modifications.Add(mod);
                        #endregion

                        var updateResult = await _context.Collection.ReplaceOneAsync(_ => _.DomainId == domainId, entity);
                        if (updateResult.IsAcknowledged)
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
                        result.Message = ConstMessages.ObjectNotFound;
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

        public RepositoryOperationResult<DomainDTO> FetchByName(string domainName)
        {
            RepositoryOperationResult<DomainDTO> result
                 = new RepositoryOperationResult<DomainDTO>();
            try
            {
                var dbEntity = _context.Collection.Find(_ => _.DomainName == domainName).First();
                if (dbEntity == null)
                {
                    dbEntity = _context.Collection.Find(_ => _.IsDefault).First();
                }

                var dto = _mapper.Map<DomainDTO>(dbEntity);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
                result.ReturnValue = dto;
            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.ExceptionOccured;
            }

            return result;
        }

        public RepositoryOperationResult<DomainDTO> FetchDomain(string domainId)
        {
            RepositoryOperationResult<DomainDTO> result
                = new RepositoryOperationResult<DomainDTO>();
            try
            {
                var dbEntity = _context.Collection.Find(_ => _.DomainId == domainId).First();
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

        public async Task<RepositoryOperationResult> Restore(string id)
        {
            var result = new RepositoryOperationResult();
            var entity = _context.Collection
              .Find(_ => _.DomainId == id).FirstOrDefault();
            entity.IsDeleted = false;
            var updateResult = await _context.Collection
               .ReplaceOneAsync(_ => _.DomainId == id, entity);
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
        public async Task InsertMany(List<Entities.General.Domain.Domain> domains)
        {
           await _context.Collection.InsertManyAsync(domains);
        }
        public bool HasAny()
        {
            var result = false;
            if (_context.Collection.AsQueryable().Any())
            {
                result = true;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> EditDomain(DomainDTO dto)
        {
            var result = new RepositoryOperationResult();
            var entity = _context.Collection
                .Find(_ => _.DomainId == dto.DomainId).FirstOrDefault();
            if (entity != null)
            {
                
                #region Add Modification
                var currentModifications = entity.Modifications;
                var mod = GetCurrentModification($"Change domain by userId={this.GetUserId()} and UserName={this.GetUserName()} at datetime={DateTime.Now.ToPersianDdate()}");
                currentModifications.Add(mod);
                #endregion

                entity.Modifications = currentModifications;
                entity.DomainName = dto.DomainName;
                entity.OwnerUserId = dto.OwnerUserId;
                entity.OwnerUserName = dto.OwnerUserName;
                foreach (var price in dto.Prices)
                {
                    price.PriceId = Guid.NewGuid().ToString();
                }
                #region Prices
                entity.Prices = new List<Price>();
                foreach (var price in dto.Prices.OrderBy(_ => _.StartDate))
                {
                    if (price.IsActive && string.IsNullOrWhiteSpace(price.EndDate))//price is valid from client
                    {
                        if (entity.Prices.Any(_ => _.CurrencyId == price.CurrencyId && _.EndDate != null && _.IsActive))
                        {
                            var exist = entity.Prices.First(_ => _.CurrencyId == price.CurrencyId && _.EndDate != null && _.IsActive);
                            entity.Prices.Remove(exist);
                            exist.IsActive = false;
                            exist.EndDate = DateTime.UtcNow;
                            entity.Prices.Add(exist);

                        }
                    }

                    var p = new Price()
                    {
                        PriceId = !string.IsNullOrWhiteSpace(price.PriceId) ? price.PriceId : Guid.NewGuid().ToString(),
                        CurrencyId = price.CurrencyId,
                        CurrencyName = price.CurrencyName,
                        IsActive = !string.IsNullOrWhiteSpace(price.PriceId) ? price.IsActive : true,
                        Prefix = price.Prefix,
                        PriceValue = price.PriceValue,
                        StartDate = DateHelper.ToEnglishDate(price.StartDate).ToUniversalTime(),
                        EndDate = !string.IsNullOrWhiteSpace(price.EndDate) ?
                        GeneralLibrary.Utilities.DateHelper.ToEnglishDate(price.EndDate).ToUniversalTime() : null
                    };
                    entity.Prices.Add(p);
                }
                #endregion
                var updateResult = await _context.Collection
               .ReplaceOneAsync(_ => _.DomainId == dto.DomainId, entity);
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

        public List<SelectListModel> GetAllActiveDomains()
        {
            var result = new List<SelectListModel>();
            result = _context.Collection.Find(_ => _.IsActive && !_.IsDeleted)
              .Project(_ => new SelectListModel()
              {
                  Text = _.DomainName,
                  Value = _.DomainId
              }).ToList();
            return result;
        }

        public RepositoryOperationResult<DomainDTO> GetDefaultDomain()
        {
            var result = new RepositoryOperationResult<DomainDTO>();
            var entity = _context.Collection.Find(_ => _.IsDefault).First();
            if(entity != null)
            {
                result.Succeeded = true;
                result.ReturnValue = _mapper.Map<DomainDTO>(entity);
                result.Message = ConstMessages.SuccessfullyDone;
            }else
            {
                result.Succeeded = false;
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }
    }
}
