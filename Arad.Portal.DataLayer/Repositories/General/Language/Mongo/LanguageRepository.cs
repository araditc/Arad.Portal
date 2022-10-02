using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Models.Language;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Arad.Portal.DataLayer.Entities;
using System.Security.Claims;
using System.Web;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Hosting;

namespace Arad.Portal.DataLayer.Repositories.General.Language.Mongo
{
    public class LanguageRepository : BaseRepository, ILanguageRepository
    {
        private readonly LanguageContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        public IMongoCollection<Entities.General.Language.Language> Languages { get; set; }
        public LanguageRepository(LanguageContext context, IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor): base(httpContextAccessor, env)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            Languages = context.Collection;
        }
        public async Task<Result> AddNewLanguage(LanguageDTO dto)
        {
            Result result = new Result();
            var equallentModel = _mapper.Map<Entities.General.Language.Language>(dto);

            equallentModel.CreationDate = DateTime.Now;
            equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            try
            {
                equallentModel.LanguageId = Guid.NewGuid().ToString();
                await _context.Collection.InsertOneAsync(equallentModel);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }

            return result;
        }
               
        public async Task<Result> EditLanguage(LanguageDTO dto)
        {
            Result result = new Result();
            var equallentModel = _mapper.Map<Entities.General.Language.Language>(dto);

            var availableEntity = await _context.Collection
                    .Find(_ => _.LanguageId == dto.LanguageId).FirstOrDefaultAsync();

            if (availableEntity != null)
            {
                #region Add Modification
                var currentModification = availableEntity.Modifications;
                var mod = GetCurrentModification(dto.ModificationReason);
                currentModification.Add(mod);
                #endregion

                equallentModel.Modifications = currentModification;
                equallentModel.CreationDate = availableEntity.CreationDate;
                equallentModel.CreatorUserId = availableEntity.CreatorUserId;
                equallentModel.CreatorUserName = availableEntity.CreatorUserName;

                var updateResult = await _context.Collection
                   .ReplaceOneAsync(_ => _.LanguageId == availableEntity.LanguageId, equallentModel);

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

        public bool HasAny()
        {
            return _context.Collection.AsQueryable().Any();
        }
        public async Task<Result> Delete(string languageId, string modificationReason)
        {
            Result result = new Result();

            try
            {
                #region check object dependency 
                //??
                //checke all dependencies in other entities if any dependecy exist delete not allowed
                var allowDeletion = true;
                #endregion
                if (allowDeletion)
                {
                    var languageEntity =await  _context.Collection.Find(_ => _.LanguageId == languageId).FirstOrDefaultAsync();
                    if(languageEntity != null)
                    {
                        languageEntity.IsDeleted = true;

                        #region Add Modification
                        var currentModification = languageEntity.Modifications;
                        var mod = GetCurrentModification(modificationReason);
                        currentModification.Add(mod);
                        languageEntity.Modifications = currentModification;
                        #endregion

                        var updateResult = await _context.Collection
                            .ReplaceOneAsync(_ => _.LanguageId == languageId, languageEntity);
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

        public async Task<PagedItems<LanguageDTO>> List(string queryString)
        {
            PagedItems<LanguageDTO> result = new PagedItems<LanguageDTO>();
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

                long totalCount = await _context.Collection.Find(c => true).CountDocumentsAsync();
                var list = _context.Collection.AsQueryable().Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new LanguageDTO()
                   {
                       LanguageId = _.LanguageId,
                       LanguageName = _.LanguageName,
                       Symbol = _.Symbol,
                       Direction = _.Direction
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
                result.Items = new List<LanguageDTO>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public List<SelectListModel> GetAllActiveLanguage()
        {
            var result = new List<SelectListModel>();
            result = _context.Collection.AsQueryable().Where(_ => _.IsActive).Select(_ => new SelectListModel()
            {
                Text = _.LanguageName,
                Value = _.LanguageId
            }).ToList();
            //result.Insert(0, new SelectListModel() { Text = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_Choose"), Value = "-1" });
            return result;
        }

        public LanguageDTO FetchLanguage(string languageId)
        {
            LanguageDTO result;
            var entity = _context.Collection.Find(_ => _.LanguageId == languageId).FirstOrDefault();
            if(entity != null)
            {
                result = _mapper.Map<LanguageDTO>(entity);
            }else
            {
                result = null;
            }
            return result;
        }

        public LanguageDTO GetDefaultLanguage(string currentUserId)
        {
            LanguageDTO result = null;
            Entities.General.Language.Language lan ;
            var currentUser = _userManager.Users.AsQueryable().FirstOrDefault(_ => _.Id == currentUserId);
            if(currentUser != null && !string.IsNullOrWhiteSpace(currentUser.Profile.DefaultLanguageId))
            {
                lan = _context.Collection.Find(_ => _.LanguageId == currentUser.Profile.DefaultLanguageId).FirstOrDefault();
            }else
            {
                lan = _context.Collection.Find(_ => _.IsDefault).FirstOrDefault();
            }
            if(lan != null)
            {
                result = _mapper.Map<LanguageDTO>(lan);
            }
            return result;
        }

        public string FetchBySymbol(string symbol)
        {
            var entity = _context.Collection
                .Find(_ => _.Symbol.ToLower() == symbol.ToLower()).FirstOrDefault();
            if(entity != null)
            {
                return entity.LanguageId;
            }else
            {
                return String.Empty;

            }
            
        }
    }
}
