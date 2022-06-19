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

namespace Arad.Portal.DataLayer.Repositories.General.ContentCategory.Mongo
{
    public class ContentCategoryRepository : BaseRepository, IContentCategoryRepository
    {
        private readonly IMapper _mapper;
        private readonly ContentCategoryContext _categoryContext;
        private readonly ContentContext _contentContext;
        private readonly LanguageContext _languageContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDomainRepository _domainRepository;

        public ContentCategoryRepository(IHttpContextAccessor httpContextAccessor,
            IMapper mapper, ContentCategoryContext categoryContext,
            IWebHostEnvironment env,
            IDomainRepository domainRepository,
            UserManager<ApplicationUser> userManager,
            LanguageContext langContext, ContentContext contentContext)
            : base(httpContextAccessor, env)
        {
            _mapper = mapper;
            _categoryContext = categoryContext;
            _languageContext = langContext;
            _contentContext = contentContext;
            _userManager = userManager;
            _domainRepository = domainRepository;
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
              
        public async Task<List<SelectListModel>> AllActiveContentCategory(string langId, string currentUserId)
        {
            var result = new List<SelectListModel>();
            var dbUser = await _userManager.FindByIdAsync(currentUserId);
            if(dbUser != null)
            {
                if (dbUser.IsSystemAccount)
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
                    result = _categoryContext.Collection.Aggregate().Match(_ => dbUser.Profile.Access.AccessibleContentCategoryIds.Contains(_.ContentCategoryId))
                    .Project(_ => new SelectListModel()
                    {
                        Text = _.CategoryNames.Where(a => a.LanguageId == langId).Count() != 0 ?
                             _.CategoryNames.First(a => a.LanguageId == langId).Name : "",
                        Value = _.ContentCategoryId
                    }).ToList();
                    // .Sort(Builders<SelectListModel>.Sort.Ascending(x => x.Text))
                }
            }
           
            result.Insert(0, new SelectListModel() { Text = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_Choose"), Value = "-1" });
            return result; ;
        }

        public async Task<List<string>> AllCategoryIdsWhichEndInContents(string domainName)
        {
            var currentDomain = _domainRepository.FetchByName(domainName, false);
            var defDomain = _domainRepository.GetDefaultDomain();

            FilterDefinitionBuilder<Entities.General.Content.Content> builder = new();
            FilterDefinition<Entities.General.Content.Content> filterDef;
           
            filterDef = builder.Eq(nameof(Entities.General.Content.Content.AssociatedDomainId), currentDomain.ReturnValue.DomainId);
           
            //TODO : doesnt work
            var cursor = await _contentContext.Collection.DistinctAsync<string>("ContentCategoryId", filterDef);
            var lst = await cursor.ToListAsync();

           
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

        public async Task<ContentCategoryDTO> ContentCategoryFetch(string contentCategoryId)
        {
            var result = new ContentCategoryDTO();
            try
            {
                var category = await _categoryContext.Collection
                    .Find(_ => _.ContentCategoryId == contentCategoryId).FirstOrDefaultAsync();
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

        public string FetchByCode(string slugOrCode)
        {
            string result = "";
            long codeNumber;
            var entity = new Entities.General.ContentCategory.ContentCategory();
            if (long.TryParse(slugOrCode, out codeNumber))
            {
                entity = _categoryContext.Collection
                 .Find(_ => _.CategoryCode == codeNumber).FirstOrDefault();
            }else
            {
                entity = _categoryContext.Collection
                       .Find(_ => _.CategoryNames.Any(a => a.UrlFriend == $"/category/{slugOrCode}")).FirstOrDefault();
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
                .Find(_ => _.CategoryNames.Any(a => a.UrlFriend == urlFriend)).FirstOrDefault();
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
               lst = _contentContext.Collection.Find(_ => _.ContentCategoryId == contentCategoryId && _.AssociatedDomainId == domainId).SortByDescending(_ => _.CreationDate).Skip(0).Limit(count).ToList();
            }else
            {
                lst = _contentContext.Collection.Find(_ => _.ContentCategoryId == contentCategoryId && _.AssociatedDomainId == domainId).SortByDescending(_ => _.CreationDate).ToList();
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
                lst = _categoryContext.Collection.Find(_ => _.ParentCategoryId == contentCategoryId).Skip(skip).Limit(count.Value).ToList();
            }
            else
            {
                lst = _categoryContext.Collection.Find(_ => _.ParentCategoryId == contentCategoryId).ToList();
            }
            var catList = 

            result = _mapper.Map<List<ContentCategoryDTO>>(lst);
            return result;

        }

        public async Task<PagedItems<ContentCategoryViewModel>> List(string queryString)
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
                long totalCount = await _categoryContext.Collection.Find(c => true).CountDocumentsAsync();
                var list = _categoryContext.Collection.AsQueryable().Skip((page - 1) * pageSize)
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
                #region Add Modification
                var currentModifications = availableEntity.Modifications;
                var mod = GetCurrentModification(dto.ModificationReason);
                currentModifications.Add(mod);
                #endregion
                availableEntity.Modifications = currentModifications;
                if (dto.IsDeleted)
                {
                    availableEntity.IsDeleted = true;
                }
                else
                {
                    availableEntity.CategoryNames = dto.CategoryNames;
                }
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
                result.Message = ConstMessages.ObjectNotFound;
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

                return !_categoryContext.Collection.Find(categoryBuilder.ElemMatch("CategoryNames", multiLingualFilterDefinition) & categoryBuilder.Ne("ContentCategoryId", contentCategoryId)).Any();
            }
        }
    }
}
