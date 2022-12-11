using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Models.Currency;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AutoMapper;
using System.Collections.Specialized;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Hosting;

namespace Arad.Portal.DataLayer.Repositories.General.Currency.Mongo
{
    public class CurrencyRepository : BaseRepository, ICurrencyRepository
    {
        private readonly CurrencyContext _context;
        
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public CurrencyRepository(CurrencyContext context, 
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager) 
            : base(httpContextAccessor, env)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }
        public async Task<Result> SaveCurrency(CurrencyDTO dto)
        {
            Result result;

            //mapping the input model to equallent object of database
            var equallentModel = _mapper.Map<Entities.General.Currency.Currency>(dto);
           
            if (!string.IsNullOrWhiteSpace(dto.CurrencyId))//it is update case
            {
                result = await UpdateCurrencyAsync(equallentModel);
            }
            else //it is insert case
            {
                result = await InsertCurrencyAsync(equallentModel);
            }

            return result;
        }

        public bool HasAny()
        {
            return _context.Collection.AsQueryable().Any();
        }
        /// <summary>
        /// this method is used just in seedData
        /// </summary>
        /// <param name="entity"></param>
        public void InsertOne(Entities.General.Currency.Currency entity)
        {
            _context.Collection.InsertOne(entity);
        }
        private async Task<Result> UpdateCurrencyAsync(Entities.General.Currency.Currency equallentModel)
        {
            var result = new Result();

            var availableEntity = await _context.Collection
                    .Find(_ => _.CurrencyId.Equals(equallentModel.CurrencyId)).FirstOrDefaultAsync();

            if (availableEntity != null)
            {
               
                equallentModel.CreationDate = availableEntity.CreationDate;
                equallentModel.CreatorUserId = availableEntity.CreatorUserId;
                equallentModel.CreatorUserName = availableEntity.CreatorUserName;

                var updateResult = await _context.Collection
                   .ReplaceOneAsync(_ => _.CurrencyId == availableEntity.CurrencyId, equallentModel);

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

        private async Task<Result> InsertCurrencyAsync(
            Entities.General.Currency.Currency equallentModel)
        {
            var result = new Result();
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

                if (string.IsNullOrWhiteSpace(filter["page"]))
                {
                    filter.Set("page", "1");
                }

                if (string.IsNullOrWhiteSpace(filter["PageSize"]))
                {
                    filter.Set("PageSize", "20");
                }
               
                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);
                string keyToFilter = "";
                if (!string.IsNullOrWhiteSpace(filter["filter"]))
                {
                    keyToFilter = filter["filter"].ToString();
                }
                long totalCount = _context.Collection.AsQueryable().Count(c => keyToFilter == "" || c.CurrencyName.Contains(keyToFilter));
                var list = _context.Collection.AsQueryable().Where(c => keyToFilter == "" || c.CurrencyName.Contains(keyToFilter)).Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_=> new CurrencyDTO()
                   {
                       CurrencyId = _.CurrencyId,
                       IsDefault = _.IsDefault,
                       CurrencyName = _.CurrencyName,
                       Prefix = _.Prefix,
                       Symbol = _.Symbol,
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
                result.Items = new List<CurrencyDTO>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<Result> DeleteCurrency(string currencyId)
        {
            Result result = new Result();

            try
            {


                var currencyEntity = _context.Collection.Find(_ => _.CurrencyId == currencyId).FirstOrDefault();
                if (currencyEntity != null)
                {
                    currencyEntity.IsDeleted = true;
                    var upResult = await _context.Collection.ReplaceOneAsync(_ => _.CurrencyId == currencyId, currencyEntity);
                    if (upResult.IsAcknowledged)
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
            catch (Exception e)
            {
                result.Message = ConstMessages.ExceptionOccured;
            }
            return result;
        }

        public Result<CurrencyDTO> FetchCurrency(string currencyId)
        {
            Result<CurrencyDTO> result
                = new Result<CurrencyDTO>(); 
            try
            {
                var dbEntity = _context.Collection.Find(_=>_.CurrencyId == currencyId).FirstOrDefault();
                if (dbEntity == null)
                {
                    result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
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

        public Result<CurrencyDTO> GetDefaultCurrency(string userId)
        {
            Result<CurrencyDTO> result
                 = new Result<CurrencyDTO>();
            try
            {
                Entities.General.Currency.Currency dbEntity;
                var currentUser = _userManager.Users.FirstOrDefault(_=>_.Id == userId);

                if(currentUser != null && !string.IsNullOrWhiteSpace(currentUser.Profile.DefaultCurrencyId))
                {
                    dbEntity = _context.Collection.Find(_ => _.CurrencyId == currentUser.Profile.DefaultCurrencyId).FirstOrDefault();
                }
                else
                {
                    dbEntity = _context.Collection.Find(_ => _.IsDefault).FirstOrDefault();
                }
               
                if (dbEntity == null)
                {
                    result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
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

        public CurrencyDTO GetCurrencyByItsPrefix(string prefix)
        {
            var result = new CurrencyDTO();
            var currency = _context.Collection.Find(_ => _.Prefix == prefix).FirstOrDefault();
            if(currency != null)
            {
                result = _mapper.Map<CurrencyDTO>(currency);
            }
           
            return result;
        }

        public async Task<Result> RestoreCurrency(string currencyId)
        {
            Result result = new Result();

            try
            {
                var currencyEntity = _context.Collection.Find(_ => _.CurrencyId == currencyId).FirstOrDefault();
                if (currencyEntity != null)
                {
                    currencyEntity.IsDeleted = false;
                    var upResult = await _context.Collection.ReplaceOneAsync(_ => _.CurrencyId == currencyId, currencyEntity);
                    if (upResult.IsAcknowledged)
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
            catch (Exception e)
            {
                result.Message = ConstMessages.ExceptionOccured;
            }
            return result;
        }
    }
}
