using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Entities.General.ContentCategory;
using Arad.Portal.DataLayer.Models.ContentCategory;
using Arad.Portal.DataLayer.Models.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.General.Content.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Models.Content;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;

namespace Arad.Portal.DataLayer.Repositories.General.ContentCategory.Mongo
{
    public class ContentCategoryRepository : BaseRepository, IContentCategoryRepository
    {
        private readonly IMapper _mapper;
        private readonly ContentCategoryContext _categoryContext;
        private readonly ContentContext _contentContext;
        private readonly LanguageContext _languageContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DomainContext _domainContext;
        public ContentCategoryRepository(IHttpContextAccessor httpContextAccessor,
            IMapper mapper, ContentCategoryContext categoryContext,
            IWebHostEnvironment env,
            DomainContext domainContext,
            UserManager<ApplicationUser> userManager,
            LanguageContext langContext, ContentContext contentContext)
            : base(httpContextAccessor, env)
        {
            _mapper = mapper;
            _categoryContext = categoryContext;
            _languageContext = langContext;
            _contentContext = contentContext;
            _userManager = userManager;
            _domainContext = domainContext;
        }
        public async Task<Result> Add(ContentCategoryDTO dto)
        {
            Result result = new Result();
            try
            {
                var equallentEntity = _mapper.Map<Entities.General.ContentCategory.ContentCategory>(dto);
                equallentEntity.ContentCategoryId = Guid.NewGuid().ToString();

                equallentEntity.CreationDate = DateTime.Now;
                equallentEntity.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                equallentEntity.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

                equallentEntity.IsActive = true;
                await _categoryContext.Collection.InsertOneAsync(equallentEntity);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            catch (Exception e)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }
            return result;
        }
              
        public async Task<List<SelectListModel>> AllActiveContentCategory(string langId, string currentUserId, string domainId = "")
        {
            var result = new List<SelectListModel>();
            var dbUser = await _userManager.FindByIdAsync(currentUserId);
            if(dbUser != null)
            {
                try
                {
                    if (dbUser.IsSystemAccount && string.IsNullOrWhiteSpace(domainId))
                    {
                        result = _categoryContext.Collection.Find(_ => _.IsActive && !_.IsDeleted)
                        .Project(_ => new SelectListModel()
                        {
                            Text = _.CategoryNames.Where(a => a.LanguageId == langId).Count() != 0 ?
                                 _.CategoryNames.First(a => a.LanguageId == langId).Name : "",
                            Value = _.ContentCategoryId
                        }).ToList();
                    }
                    else
                    {
                        var lastDomainId = !string.IsNullOrWhiteSpace(domainId) ? domainId : dbUser.Domains.FirstOrDefault(a => a.IsOwner).DomainId;
                        var lst = _categoryContext.Collection.AsQueryable().Where(_ => _.IsActive && !_.IsDeleted &&
                                   //dbUser.Profile.Access.AccessibleContentCategoryIds.Contains(_.ContentCategoryId) &&
                                    _.AssociatedDomainId == lastDomainId).ToList();
                        result = lst
                                .Select(_ => new SelectListModel()
                                {
                                    Text = _.CategoryNames.Any(a => a.LanguageId == langId) ?
                                         _.CategoryNames.First(a => a.LanguageId == langId).Name : "",
                                    Value = _.ContentCategoryId
                                }).ToList();
                    }
                }
                catch (Exception ex)
                {

                   
                }
            }
            if(result.Count > 0)
            result.Insert(0, new SelectListModel() { Text = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_Choose"), Value = "-1" });

            return result; ;
        }

        public async Task<List<string>> AllCategoryIdsWhichEndInContents(string domainName)
        {
            
            var dbEntity = _domainContext.Collection.Find(_ => _.DomainName == domainName).FirstOrDefault();
            var defDomain = _domainContext.Collection.Find(_ => _.IsDefault).FirstOrDefault();

            //FilterDefinitionBuilder<Entities.General.Content.Content> builder = new();
            //FilterDefinition<Entities.General.Content.Content> filterDef;
            //filterDef = builder.Eq(nameof(Entities.General.Content.Content.AssociatedDomainId), dbEntity.DomainId);
            //filterDef &= builder.Eq(nameof(Entities.General.Content.Content.IsDeleted), false);
            //var cursor = await _contentContext.Collection.<string>("ContentCategoryId", filterDef);
            //var lst = await cursor.ToListAsync();

            var lst = _contentContext.Collection.AsQueryable().Where(_ => !_.IsDeleted && _.AssociatedDomainId == dbEntity.DomainId).GroupBy(_ => _.ContentCategoryId).Select(a => a.Key);
            var finalList = new List<string>();
            foreach (var catId in lst)
            {
                finalList.Add(catId);
                var entity = _categoryContext.Collection
                             .Find(_ => _.ContentCategoryId == catId).FirstOrDefault();
                if (entity != null)
                {
                    var tmp = entity;
                    while (tmp.ParentCategoryId != null)
                    {
                        finalList.Add(tmp.ParentCategoryId);
                        tmp = _categoryContext.Collection
                             .Find(_ => _.ContentCategoryId == tmp.ParentCategoryId).Any() ?
                             _categoryContext.Collection
                             .Find(_ => _.ContentCategoryId == tmp.ParentCategoryId).FirstOrDefault() : null;
                    }
                }
            }
            return finalList;
        }
        /// <summary>
        /// if isDeleted = true it also shows the category even if it is deleted
        /// </summary>
        /// <param name="contentCategoryId"></param>
        /// <param name="isDeleted"></param>
        /// <returns></returns>
        public async Task<ContentCategoryDTO> ContentCategoryFetch(string contentCategoryId, bool isDeleted = false)
        {
            var result = new ContentCategoryDTO();
            Entities.General.ContentCategory.ContentCategory category = null; 
            try
            {
                if(isDeleted)
                {
                   category = await _categoryContext.Collection
                   .Find(_ => _.ContentCategoryId == contentCategoryId).FirstOrDefaultAsync();
                }else
                {
                    category = await _categoryContext.Collection
                   .Find(_ => _.ContentCategoryId == contentCategoryId && !_.IsDeleted).FirstOrDefaultAsync();
                }
               
                if(category != null)
                {
                    result = _mapper.Map<ContentCategoryDTO>(category);
                }
            }
            catch (Exception ex)
            {
                result = null;
            }
            return result;
        }

        public async Task<Result> Delete(string contentCategoryId, string modificationReason)
        {
            Result result = new Result();

            try
            {
                #region check object dependency
                var allowDeletion = true;
                if (_contentContext.Collection
                    .AsQueryable().Any(_ => _.ContentCategoryId == contentCategoryId && !_.IsDeleted))
                {
                    allowDeletion = false;
                }

                #endregion
                if (allowDeletion)
                {
                    var entity = _categoryContext.Collection
                        .Find(_ => _.ContentCategoryId == contentCategoryId).FirstOrDefault();
                    if (entity != null)
                    {
                        entity.IsDeleted = true;
                        #region add modification
                        var mod = GetCurrentModification(modificationReason);
                        entity.Modifications.Add(mod);
                        #endregion

                        var updateResult = await _categoryContext.Collection.ReplaceOneAsync(_ => _.ContentCategoryId == contentCategoryId, entity);
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
                    result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_DeletedNotAllowedForDependencies");
                }
            }
            catch (Exception e)
            {
                result.Message = ConstMessages.ExceptionOccured;
            }
            return result;
        }

        public string FetchByCode(string slugOrCode)
        {
            string result = "";
            long codeNumber;
            var entity = new Entities.General.ContentCategory.ContentCategory();
            if (long.TryParse(slugOrCode, out codeNumber))
            {
                entity = _categoryContext.Collection
                 .Find(_ => _.CategoryCode == codeNumber && !_.IsDeleted).FirstOrDefault();
            }else
            {
                entity = _categoryContext.Collection
                       .Find(_ => _.CategoryNames.Any(a => a.UrlFriend == $"/category/{slugOrCode}") && !_.IsDeleted).FirstOrDefault();
            }

            if (entity != null)
            {
                result = entity.ContentCategoryId;
            }
            return result;
        }
        
        public ContentCategoryDTO FetchBySlug(string slug, string domainName)
        {
            var result = new ContentCategoryDTO();
            var urlFriend = $"{domainName}/category/{slug}";
            var categoryEntity = _categoryContext.Collection
                .Find(_ => _.CategoryNames.Any(a => a.UrlFriend == urlFriend) && !_.IsDeleted).FirstOrDefault();
            if(categoryEntity != null)
            {
                result = _mapper.Map<ContentCategoryDTO>(categoryEntity);
            }
            result = _mapper.Map<ContentCategoryDTO>(categoryEntity);
            return result;
        }

        public List<SelectListModel> GetAllContentCategoryType()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(ContentCategoryType)))
            {
                string name = Enum.GetName(typeof(ContentCategoryType), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel() { Text = GeneralLibrary.Utilities.Language.GetString("Choose"), Value = "-1" });
            return result;
        }

        public List<ContentViewModel> GetContentsInCategory(string domainId, string contentCategoryId,int? count, int skip = 0)
        {
            var result = new List<ContentViewModel>();
            List<Entities.General.Content.Content> lst = new List<Entities.General.Content.Content>();
            if(count != null)
            {
               lst = _contentContext.Collection
                    .Find(_ => _.ContentCategoryId == contentCategoryId && _.AssociatedDomainId == domainId && !_.IsDeleted)
                    .SortByDescending(_ => _.CreationDate).Skip(0).Limit(count).ToList();
            }else
            {
                lst = _contentContext.Collection
                    .Find(_ => _.ContentCategoryId == contentCategoryId && _.AssociatedDomainId == domainId && !_.IsDeleted)
                    .SortByDescending(_ => _.CreationDate).ToList();
            }
            result = _mapper.Map<List<ContentViewModel>>(lst);

            foreach (var content in result)
            {
                var r = Helpers.Utilities.ConvertPopularityRate(content.TotalScore ?? 0, content.ScoredCount ?? 0);
                content.LikeRate = r.LikeRate;
                content.DisikeRate = r.DisikeRate;
                content.HalfLikeRate = r.HalfLikeRate;
            }
            return result;
        }

        public List<ContentCategoryDTO> GetDirectChildrens(string contentCategoryId, int? count, int skip = 0)
        {
            var result = new List<ContentCategoryDTO>();
            List<DataLayer.Entities.General.ContentCategory.ContentCategory> lst;
            if(count != null)
            {
                lst = _categoryContext.Collection.Find(_ => _.ParentCategoryId == contentCategoryId && !_.IsDeleted).Skip(skip).Limit(count.Value).ToList();
            }
            else
            {
                lst = _categoryContext.Collection.Find(_ => _.ParentCategoryId == contentCategoryId && !_.IsDeleted).ToList();
            }
            var catList = 

            result = _mapper.Map<List<ContentCategoryDTO>>(lst);
            return result;

        }

        public async Task<PagedItems<ContentCategoryViewModel>> List(string queryString, ApplicationUser user)
        {
            PagedItems<ContentCategoryViewModel> result = new PagedItems<ContentCategoryViewModel>();
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

                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);
                var langId = filter["LanguageId"].ToString();
                var filterKey = "";
                if (!string.IsNullOrWhiteSpace(filter["filter"]))
                {
                    filterKey = filter["filter"].ToString();
                }
                long totalCount = 0;
                string domainId = "";
                if (user.IsSystemAccount)
                {
                    totalCount = await _categoryContext.Collection.Find(c => true).CountDocumentsAsync();
                }else
                {
                    domainId = user.Domains.FirstOrDefault(_ => _.IsOwner).DomainId;
                    totalCount = await _categoryContext.Collection.Find(_=>_.AssociatedDomainId == domainId).CountDocumentsAsync();
                }
                   
                var list = _categoryContext.Collection.AsQueryable()
                    .Where( _=> (domainId == "" || _.AssociatedDomainId == domainId) && (filterKey == "" || _.CategoryNames.Any(a => a.Name.Contains(filterKey))))
                    .OrderByDescending(_=>_.CreationDate).Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new ContentCategoryViewModel()
                   {
                       ContentCategoryId = _.ContentCategoryId,
                       ParentCategoryId = _.ParentCategoryId,
                       CategoryType = _.CategoryType,
                       IsDeleted = _.IsDeleted,
                       CategoryName = _.CategoryNames.Any(a=>a.LanguageId == langId) ?
                       _.CategoryNames.First(a => a.LanguageId == langId) : _.CategoryNames.First()
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
                result.Items = new List<ContentCategoryViewModel>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<Result> Restore(string contentCategoryId)
        {
            var result = new Result();
            var entity = _categoryContext.Collection
              .Find(_ => _.ContentCategoryId == contentCategoryId).FirstOrDefault();
            if(entity != null)
            {
                entity.IsDeleted = false;
                var updateResult = await _categoryContext.Collection
                   .ReplaceOneAsync(_ => _.ContentCategoryId == contentCategoryId, entity);
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

        public async Task<Result> Update(ContentCategoryDTO dto)
        {
            Result result = new Result();
            //var equallentModel = _mapper.Map<ProductSpecGroup>(dto);

            var availableEntity = await _categoryContext.Collection
                    .Find(_ => _.ContentCategoryId == dto.ContentCategoryId).FirstOrDefaultAsync();

            if (availableEntity != null)
            {
                if (dto.IsDeleted)
                {
                    availableEntity.IsDeleted = true;
                }
                else
                {
                    availableEntity.CategoryNames = dto.CategoryNames;
                }
                availableEntity.AssociatedDomainId = dto.AssociatedDomainId;
                var updateResult = await _categoryContext.Collection
                   .ReplaceOneAsync(_ => _.ContentCategoryId == availableEntity.ContentCategoryId, availableEntity);

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

        public bool IsUniqueUrlFriend(string urlFriend, string contentCategoryId = "")
        {
            if (string.IsNullOrWhiteSpace(contentCategoryId)) //insert
            {
                return !_categoryContext.Collection.Find(_ => _.CategoryNames.Any(a => a.UrlFriend == urlFriend)).Any();
            }
            else
            { //update
                FilterDefinitionBuilder<Entities.General.ContentCategory.ContentCategory> categoryBuilder = new();
                FilterDefinitionBuilder<MultiLingualProperty> multiLingualBuilder = new();
                FilterDefinition<MultiLingualProperty> multiLingualFilterDefinition = multiLingualBuilder.Eq("UrlFriend", urlFriend) ;

                return !_categoryContext.Collection.Find(categoryBuilder.ElemMatch("CategoryNames", multiLingualFilterDefinition) &
                           categoryBuilder.Ne("ContentCategoryId", contentCategoryId)).Any();
            }
        }

        public void InsertMany(List<Entities.General.ContentCategory.ContentCategory> categories)
        {
            _categoryContext.Collection.InsertMany(categories);
        }
    }
}
