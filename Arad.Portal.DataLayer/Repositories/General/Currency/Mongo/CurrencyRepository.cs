using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Models.Currency;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AutoMapper;
using System.Collections.Specialized;
using System.Web;


namespace Arad.Portal.DataLayer.Repositories.General.Currency.Mongo
{
    public class CurrencyRepository : BaseRepository, ICurrencyRepository
    {
        private readonly CurrencyContext _context;
        
        private readonly IMapper _mapper;
        public CurrencyRepository(CurrencyContext context, 
            IMapper mapper,IHttpContextAccessor httpContextAccessor) 
            : base(httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<RepositoryOperationResult> SaveCurrency(CurrencyDTO dto)
        {
            RepositoryOperationResult result;

            //mapping the input model to equallent object of database
            var equallentModel = _mapper.Map<Entities.General.Currency.Currency>(dto);
           
            if (!string.IsNullOrWhiteSpace(dto.CurrencyId))//it is update case
            {
                result = await UpdateCurrencyAsync(equallentModel, dto.ModificationReason);
            }
            else //it is insert case
            {
                result = await InsertCurrencyAsync(equallentModel);
            }

            return result;
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
        /// <summary>
        /// this method is used just in seedData
        /// </summary>
        /// <param name="entity"></param>
        public void InsertOne(Entities.General.Currency.Currency entity)
        {
            _context.Collection.InsertOne(entity);
        }
        private async Task<RepositoryOperationResult> UpdateCurrencyAsync(Entities.General.Currency.Currency equallentModel, string modificationReason)
        {
            var result = new RepositoryOperationResult();

            var availableEntity = await _context.Collection
                    .Find(_ => _.CurrencyId.Equals(equallentModel.CurrencyId)).FirstOrDefaultAsync();

            if (availableEntity != null)
            {
                #region Add Modification
                var currentModification = availableEntity.Modifications;
                //currentModification ??= new List<Modification>();
                var mod = GetCurrentModification(modificationReason);

                currentModification.Add(mod);
                #endregion

                equallentModel.Modifications = currentModification;

                equallentModel.CreationDate = availableEntity.CreationDate;
                equallentModel.CreatorUserId = availableEntity.CreatorUserId;
                equallentModel.CreatorUserName = availableEntity.CreatorUserName;

                var updateResult = await _context.Collection
                   .ReplaceOneAsync(_ => _.CurrencyId == availableEntity.CurrencyId, equallentModel);

                if (updateResult.IsAcknowledged)
                {
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                    //??? we have to update this entity wherever it is used in other entities
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = ConstMessages.ErrorInSaving;
                }
            }
            return result;
        }

        private async Task<RepositoryOperationResult> InsertCurrencyAsync(
            Entities.General.Currency.Currency equallentModel)
        {
            var result = new RepositoryOperationResult();
            equallentModel.Modifications = new List<Modification>();

            equallentModel.CreationDate = DateTime.Now;
            equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            try
            {
                equallentModel.CurrencyId = Guid.NewGuid().ToString();
                await _context.Collection.InsertOneAsync(equallentModel);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            catch (Exception e)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }

            return result;
        }
        public async Task<PagedItems<CurrencyDTO>> AllCurrencyList(string queryString)
        {
            PagedItems<CurrencyDTO> result 
                = new PagedItems<CurrencyDTO>();
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
                   .Take(pageSize).Select(_=> new CurrencyDTO()
                   {
                       CurrencyId = _.CurrencyId,
                       IsDefault = _.IsDefault,
                       CurrencyName = _.CurrencyName,
                       Prefix = _.Prefix,
                       Symbol = _.Symbol
                       
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
                result.Items = new List<CurrencyDTO>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> DeleteCurrency(string currencyId)
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
                    var delResult = await _context.Collection.DeleteOneAsync(_ => _.CurrencyId == currencyId);
                    if (delResult.IsAcknowledged)
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
                    result.Message = ConstMessages.DeletedNotAllowedForDependencies;
                }
               
            }
            catch (Exception e)
            {
                result.Message = ConstMessages.ExceptionOccured;
            }
            return result;
        }

        public RepositoryOperationResult<CurrencyDTO> FetchCurrency(string currencyId)
        {
            RepositoryOperationResult<CurrencyDTO> result
                = new RepositoryOperationResult<CurrencyDTO>(); 
            try
            {
                var dbEntity = _context.Collection.AsQueryable().FirstOrDefault(_=>_.CurrencyId == currencyId);
                if (dbEntity == null)
                {
                    result.Message = ConstMessages.ObjectNotFound;
                }

                var dto = _mapper.Map<CurrencyDTO>(dbEntity);
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

        public RepositoryOperationResult<CurrencyDTO> GetDefaultCurrency()
        {
            RepositoryOperationResult<CurrencyDTO> result
                 = new RepositoryOperationResult<CurrencyDTO>();
            try
            {
                var dbEntity = _context.Collection.AsQueryable().FirstOrDefault(_ =>_.IsDefault);
                if (dbEntity == null)
                {
                    result.Message = ConstMessages.ObjectNotFound;
                }

                var dto = _mapper.Map<CurrencyDTO>(dbEntity);
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

        public List<SelectListModel> GetAllActiveCurrency()
        {
            var result = new List<SelectListModel>();
            result = _context.Collection.AsQueryable().Where(_ => _.IsActive).Select(_ => new SelectListModel()
            {
                Text = _.CurrencyName,
                Value = _.CurrencyId
            }).ToList();
            result.Insert(0, new SelectListModel() { Value = "-1", Text = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_Choose") });
            return result;
        }
    }
}
