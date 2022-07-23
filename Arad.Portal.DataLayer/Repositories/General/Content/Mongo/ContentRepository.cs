using Arad.Portal.DataLayer.Contracts.General.Content;
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Repositories.General.ContentCategory.Mongo;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Content;
using System.Security.Claims;
using MongoDB.Driver;
using System.Collections.Specialized;
using System.Web;
using MongoDB.Driver.Linq;
using Arad.Portal.GeneralLibrary.Utilities;
using System.Linq;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Models.DesignStructure;

namespace Arad.Portal.DataLayer.Repositories.General.Content.Mongo
{
    public class ContentRepository : BaseRepository, IContentRepository
    {
        private readonly IMapper _mapper;
        private readonly ContentContext _contentContext;
        private readonly DomainContext _domainContext;
        private readonly LanguageContext _languageContext;
        public ContentRepository(IHttpContextAccessor httpContextAccessor,DomainContext domainContext,
           IMapper mapper,
           ContentContext contentContext,
           IWebHostEnvironment env,
           LanguageContext langContext)
            : base(httpContextAccessor, env)
        {
            _mapper = mapper;
            _contentContext = contentContext;
            _languageContext = langContext;
            _domainContext = domainContext;
        }

        public async Task<Result> Add(ContentDTO dto)
        {
            Result result = new Result();
            try
            {
                var equallentModel = _mapper.Map<Entities.General.Content.Content>(dto);
                if(!string.IsNullOrEmpty(dto.PersianStartShowDate))
                {
                    equallentModel.StartShowDate = dto.PersianStartShowDate.ToEnglishDate().ToUniversalTime();
                }else if(dto.StartShowDate != null)
                {
                    equallentModel.StartShowDate = dto.StartShowDate.Value;
                }
                

                if(!string.IsNullOrEmpty(dto.PersianEndShowDate))
                {
                    equallentModel.EndShowDate = dto.PersianEndShowDate.ToEnglishDate().ToUniversalTime();
                }else if(dto.EndShowDate != null)
                {
                    equallentModel.EndShowDate = dto.EndShowDate.Value;
                }
               
                equallentModel.CreationDate = DateTime.Now.ToUniversalTime();
                equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
                equallentModel.IsActive = true;
                equallentModel.ContentId = Guid.NewGuid().ToString();


              
                //Filter specific claim    
                var domainId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("RelatedDomain"))?.Value;
                if(domainId == null)
                {
                    domainId = _domainContext.Collection.Find(_ => _.IsDefault == true).FirstOrDefault().DomainId;
                }
                equallentModel.AssociatedDomainId = domainId;

                await _contentContext.Collection.InsertOneAsync(equallentModel);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;

            }
            catch (Exception ex)
            {
                foreach (var item in dto.Images)
                {
                    if (!string.IsNullOrWhiteSpace(item.Url) && System.IO.File.Exists(item.Url))
                    {
                        System.IO.File.Delete(item.Url);
                    }
                }
                result.Message = ConstMessages.InternalServerErrorMessage;
            }
            return result;
        }

        public async Task<ContentDTO> ContentFetch(string contentId)
        {
            var result = new ContentDTO();
            //try
            //{
                var entity = await _contentContext.Collection
                .Find(_ => _.ContentId == contentId).FirstOrDefaultAsync();
                if(entity != null)
                {
                    result = _mapper.Map<ContentDTO>(entity);
                    result.PersianStartShowDate = DateHelper.ToPersianDdate(result.StartShowDate.Value);
                    result.PersianEndShowDate = DateHelper.ToPersianDdate(result.EndShowDate.Value);
                }
                
            //}
            //catch (Exception ex)
            //{

            //    throw;
            //}
            return result;
        }

        public async Task<Result> Delete(string contentId, string modificationReason)
        {
            var result = new Result();
            var entity = await _contentContext.Collection.Find(_ => _.ContentId == contentId).FirstOrDefaultAsync();
            if (entity != null)
            {
                entity.IsDeleted = true;
                #region Add modification
                var mod = GetCurrentModification(modificationReason);
                entity.Modifications.Add(mod);
                #endregion
                var updateResult = await _contentContext.Collection
                    .ReplaceOneAsync(_ => _.ContentId == contentId, entity);
                if (updateResult.IsAcknowledged)
                {
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
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
            return result;
        }

        public ContentDTO FetchByCode(string slugOrCode)
        {
            var result = new ContentDTO();
            var contentEntity = new Entities.General.Content.Content();
            long codeNumber;
            if (long.TryParse(slugOrCode, out codeNumber))
            {
                contentEntity = _contentContext.Collection
                   .Find(_ => _.ContentCode == codeNumber).FirstOrDefault();
            }
            else
            {
                contentEntity = _contentContext.Collection
                     .Find(_=>_.UrlFriend == $"/blog/{slugOrCode}").FirstOrDefault();
            }

            if(contentEntity != null)
            {
                result = _mapper.Map<ContentDTO>(contentEntity);
            }
           
            var r = Helpers.Utilities.ConvertPopularityRate(contentEntity.TotalScore, contentEntity.ScoredCount);
            result.LikeRate = r.LikeRate;
            result.DisikeRate = r.DisikeRate;
            result.HalfLikeRate = r.HalfLikeRate;
            return result;
        }

        public async Task<Result<EntityRate>> RateContent(string contentId, int score, bool isNew, int prevScore)
        {
            var result = new Result<EntityRate>();
            var entity = _contentContext.Collection.Find(_ => _.ContentId == contentId).FirstOrDefault();
            if(entity != null)
            {
                if (isNew)
                {
                    entity.TotalScore += score;
                    entity.ScoredCount += 1;
                }
                else if (score != prevScore) //if user change the score of product
                {
                    entity.TotalScore -= prevScore;
                    entity.TotalScore += score;
                    //scoredCount doesnt change cause this user rated before just change its score
                }

                if (score != prevScore)
                {
                    var updateResult = await _contentContext.Collection.ReplaceOneAsync(_ => _.ContentId == contentId, entity);
                    var res = Helpers.Utilities.ConvertPopularityRate(entity.TotalScore, entity.ScoredCount);
                    if (updateResult.IsAcknowledged)
                    {
                        result.Succeeded = true;
                        result.ReturnValue = res;
                    }
                    else
                    {
                        result.Succeeded = false;
                    }
                }
            }
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            
            return result;
        }

        public ContentDTO FetchBySlug(string slug, string domainName)
        {
            var result = new ContentDTO();
            var urlFriend = $"{domainName}/blog/{slug}";
            var contentEntity = _contentContext.Collection
                .Find(_ => _.UrlFriend == urlFriend).FirstOrDefault();
            if(contentEntity != null)
            {
                result = _mapper.Map<ContentDTO>(contentEntity);
            }
            return result;
        }

        public List<ContentViewModel> GetAllContentsInCategory(string contentCategoryId, int? count)
        {
            throw new NotImplementedException();
        }

        public List<SelectListModel> GetAllSourceType()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(Entities.General.Content.SourceType)))
            {
                string name = Enum.GetName(typeof(Entities.General.Content.SourceType), i);
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

        /// <summary>
        /// if categoryId is empty or null it returns all categories
        /// </summary>
        /// <param name="domainId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public List<SelectListModel> GetContentsList(string domainId, string currentUserId, string categoryId)
        {
            var result = new List<SelectListModel>();
            var domainEntity = _domainContext.Collection.Find(_ => _.DomainId == domainId).FirstOrDefault();
            if(domainEntity == null)
            {
                domainEntity = _domainContext.Collection.Find(_ => _.IsDefault).FirstOrDefault();
            }
            if(currentUserId != Guid.Empty.ToString())
            {
                if(!string.IsNullOrWhiteSpace(categoryId))
                {
                    result = _contentContext.Collection.Find(_ => _.ContentCategoryId == categoryId && _.IsActive && _.LanguageId == domainEntity.DefaultLanguageId)
                  .Project(_ => new SelectListModel()
                  {
                      Text = _.Title,
                      Value = _.ContentId
                  }).ToList();
                }
                else
                {
                    result = _contentContext.Collection.Find(_ => _.IsActive && _.LanguageId == domainEntity.DefaultLanguageId)
                  .Project(_ => new SelectListModel()
                  {
                      Text = _.Title,
                      Value = _.ContentId
                  }).ToList();

                }
            }

            return result;
        }

        public async Task<PagedItems<ContentViewModel>> List(string queryString)
        {
            PagedItems<ContentViewModel> result = new PagedItems<ContentViewModel>();
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

                var langId = filter["LanguageId"].ToString();
                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);

                long totalCount = await _contentContext.Collection.Find(_=>!_.IsDeleted).CountDocumentsAsync();
                var totalList = _contentContext.Collection.AsQueryable().Where(_=>!_.IsDeleted);
                if (!string.IsNullOrWhiteSpace(filter["catId"]))
                {
                    totalList = totalList.Where(_ => _.ContentCategoryId == filter["catId"]);
                }
                //if (!string.IsNullOrWhiteSpace(filter["from"]))
                //{
                   
                //    totalList = totalList
                //        .Where(_ => _.StartShowDate >= filter["from"].ToString().ToEnglishDate().ToUniversalTime());
                //}
                //if (!string.IsNullOrWhiteSpace(filter["to"]))
                //{
                    
                //    totalList = totalList
                //        .Where(_ => _.EndShowDate <= filter["to"].ToString().ToEnglishDate().ToUniversalTime());
                //}
                if(!string.IsNullOrWhiteSpace(filter["filter"]))
                {
                    totalList = totalList.Where(_ => _.TagKeywords.Contains(filter["filter"]) || _.Contents.Contains(filter["filter"]));
                }
                var list = totalList.Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new ContentViewModel()
                   {
                       ContentId = _.ContentId,
                       SeoDescription = _.SeoDescription,
                       //PopularityRate = _.PopularityRate,
                       LanguageName = _.LanguageName,
                       LanguageId = _.LanguageId,
                       Images = _.Images,
                       //FileLogo = _.FileLogo,
                       EndShowDate = _.EndShowDate,
                       ContentCategoryId = _.ContentCategoryId,
                       ContentCategoryName = _.ContentCategoryName,
                       ContentProviderName = _.ContentProviderName,
                       Description = _.Description,
                       SeoTitle = _.SeoTitle,
                       StartShowDate = _.StartShowDate,
                       //PersianEndShowDate = _.EndShowDate,
                       //PersianStartShowDate = DateHelper.ToPersianDdate(_.StartShowDate),
                       SourceType = _.SourceType,
                       SubTitle = _.SubTitle,
                       TagKeywords = _.TagKeywords,
                       Title = _.Title,
                       UrlFriend = _.UrlFriend,
                       VisitCount = _.VisitCount
                   }).ToList();

                result.Items = list;
                result.CurrentPage = page;
                result.ItemsCount = totalCount;
                result.PageSize = pageSize;
                result.QueryString = queryString;

            }
            catch (Exception ex)
            {
                result.CurrentPage = 1;
                result.Items = new List<ContentViewModel>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<Result> Restore(string contentId)
        {
            var result = new Result();
            var entity = _contentContext.Collection
              .Find(_ => _.ContentId == contentId).FirstOrDefault();
            if(entity != null)
            {
                entity.IsDeleted = false;
                var updateResult = await _contentContext.Collection
                   .ReplaceOneAsync(_ => _.ContentId == contentId, entity);
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

        public async Task<Result> Update(ContentDTO dto)
        {
            var result = new Result();

            var equallentModel = _mapper.Map<Entities.General.Content.Content>(dto);
            if(!string.IsNullOrWhiteSpace(dto.PersianStartShowDate))
            {
                equallentModel.StartShowDate = DateHelper.ToEnglishDate(dto.PersianStartShowDate);

            }else if(dto.StartShowDate != null)
            {
                equallentModel.StartShowDate = dto.StartShowDate.Value;
            }
           

            if(!string.IsNullOrWhiteSpace(dto.PersianEndShowDate))
            {
                equallentModel.EndShowDate = DateHelper.ToEnglishDate(dto.PersianEndShowDate);
            }
            else if(dto.EndShowDate != null)
            {
                equallentModel.EndShowDate = dto.EndShowDate.Value;
            }
          
            var userName = _httpContextAccessor.HttpContext.User.Claims
                   .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            var userId = _httpContextAccessor.HttpContext.User.Claims
                   .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            equallentModel.CreatorUserName = userName;

            #region add modification
            var mod = GetCurrentModification($"update this content by userId:'{userId}' and userName:'{userName}' in date:'{DateTime.Now.ToPersianLetDateTime()}'");
            equallentModel.Modifications.Add(mod);
            #endregion

            var updateResult = await _contentContext.Collection
                .ReplaceOneAsync(_ => _.ContentId == dto.ContentId, equallentModel);

            if (updateResult.IsAcknowledged)
            {
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            else
            {
                result.Message = ConstMessages.GeneralError;

            }
            return result;
        }

        public List<SelectListModel> GetAllImageRatio()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(ImageRatio)))
            {
                string name = Enum.GetName(typeof(ImageRatio), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            return result;
        }

        public List<ContentGlance> GetSpecialContent(int count, ProductOrContentType contentType, SelectionType selectionType, 
            string categoryId, List<string> selectedIds = null, bool isDevelopment = false)
        {
            Entities.General.Domain.Domain domainEntity = new Entities.General.Domain.Domain();
            List<ContentGlance> lst = new List<ContentGlance>();
            if (!isDevelopment)
            {
               
                //var domainName = "arad-itc.com";
                var domainName = this.GetCurrentDomainName();
                domainEntity = _domainContext.Collection.Find(_ => _.DomainName == domainName).FirstOrDefault();
            }
            
            FilterDefinitionBuilder<Entities.General.Content.Content> builder = new();
            FilterDefinition<Entities.General.Content.Content> filterDef = builder.Empty;
            

            if(selectionType == SelectionType.CustomizedSelection && selectedIds != null && selectedIds.Count > 0)
            {
                filterDef =  builder.In(nameof(Entities.General.Content.Content.ContentId), selectedIds);
            }
            else
            {
                filterDef = builder.Lte(nameof(Entities.General.Content.Content.StartShowDate), DateTime.Now);
                filterDef = builder.And(filterDef, builder.Gte(nameof(Entities.General.Content.Content.EndShowDate), DateTime.Now));

                if(selectionType == SelectionType.LatestFromProductOrContentTypeSelectedCategory)
                {
                    filterDef = builder.And(filterDef, builder.Eq(nameof(Entities.General.Content.Content.ContentCategoryId), categoryId));
                }
            }
            if (domainEntity != null)
            {
               //??? should be uncommented in development mode
                filterDef &= builder.Eq(nameof(Entities.General.Content.Content.AssociatedDomainId), domainEntity.DomainId);
                switch (contentType)
                {
                    case ProductOrContentType.Newest:
                        lst = _contentContext.Collection
                           .Find(filterDef)
                           .Project(_ =>
                               new ContentGlance()
                               {
                                   TotalScore = _.TotalScore,
                                   ScoredCount = _.ScoredCount,
                                   VisitCount = _.VisitCount,
                                   CategoryName = _.ContentCategoryName,
                                   ContentCategoryId = _.ContentCategoryId,
                                   ContentId = _.ContentId,
                                   ContentProviderName = _.ContentProviderName,
                                   Images = _.Images,
                                   SubTitle = _.SubTitle,
                                   TagKeywords = _.TagKeywords,
                                   Title = _.Title,
                                   UrlFriend = _.UrlFriend,
                                   ContentCode = _.ContentCode
                               }).Sort(Builders<Entities.General.Content.Content>.Sort.Descending(_ => _.StartShowDate)).Limit(count).ToList();
                        break;
                    case ProductOrContentType.MostPopular:
                        lst = _contentContext.Collection
                       .Find(filterDef)
                       .Project(_ =>
                           new ContentGlance()
                           {
                               TotalScore = _.TotalScore,
                               ScoredCount = _.ScoredCount,
                               VisitCount = _.VisitCount,
                               CategoryName = _.ContentCategoryName,
                               ContentCategoryId = _.ContentCategoryId,
                               ContentId = _.ContentId,
                               ContentProviderName = _.ContentProviderName,
                               Images = _.Images,
                               SubTitle = _.SubTitle,
                               TagKeywords = _.TagKeywords,
                               Title = _.Title,
                               UrlFriend = _.UrlFriend,
                               ContentCode = _.ContentCode
                           }).Sort(Builders<Entities.General.Content.Content>.Sort.Descending(_ => (float)_.TotalScore / _.ScoredCount)).Limit(count).ToList();
                        break;
                    case ProductOrContentType.MostVisited:
                        lst = _contentContext.Collection
                       .Find(filterDef)
                       .Project(_ =>
                           new ContentGlance()
                           {
                               TotalScore = _.TotalScore,
                               ScoredCount = _.ScoredCount,
                               VisitCount = _.VisitCount,
                               CategoryName = _.ContentCategoryName,
                               ContentCategoryId = _.ContentCategoryId,
                               ContentId = _.ContentId,
                               ContentProviderName = _.ContentProviderName,
                               Images = _.Images,
                               SubTitle = _.SubTitle,
                               TagKeywords = _.TagKeywords,
                               Title = _.Title,
                               UrlFriend = _.UrlFriend,
                               ContentCode = _.ContentCode
                           }).Sort(Builders<Entities.General.Content.Content>.Sort.Descending(_ => _.VisitCount)).Limit(count).ToList();
                        break;
                    default:
                        break;
                }
                foreach (var con in lst)
                {
                    var r = Helpers.Utilities.ConvertPopularityRate(con.TotalScore, con.ScoredCount);
                    con.LikeRate = r.LikeRate;
                    con.HalfLikeRate = r.HalfLikeRate;
                    con.DisikeRate = r.DisikeRate;
                }
            }
            return lst;
        }

        public bool IsUniqueUrlFriend(string urlFriend, string domainId, string contentId = "")
        {
            if (string.IsNullOrWhiteSpace(contentId)) //insert
            {
                return !_contentContext.Collection.Find(_ => _.UrlFriend == urlFriend && _.AssociatedDomainId == domainId).Any();
            }
            else
            { //update
                FilterDefinitionBuilder<Entities.General.Content.Content> contentBuilder = new();
                FilterDefinition<Entities.General.Content.Content> filterDef = null;

                filterDef = contentBuilder.Ne("ContentId", contentId);
                filterDef = contentBuilder.And(filterDef, contentBuilder.Eq(nameof(Entities.General.Content.Content.UrlFriend), urlFriend));
                filterDef = contentBuilder.And(filterDef, contentBuilder.Eq(nameof(Entities.General.Content.Content.AssociatedDomainId), domainId));


                return !_contentContext.Collection.Find(filterDef).Any();
            }
        }

        public List<ContentGlance> GetContentInCategory(int count, ProductOrContentType contentType, string contentCategoryId, bool isDevelopment = false)
        {
            Entities.General.Domain.Domain domainEntity = new Entities.General.Domain.Domain();
            List<ContentGlance> lst = new List<ContentGlance>();
            if (!isDevelopment)
            {
                var domainName = this.GetCurrentDomainName();
                domainEntity = _domainContext.Collection.Find(_ => _.DomainName == domainName).FirstOrDefault();
            }

            FilterDefinitionBuilder<Entities.General.Content.Content> builder = new();
            FilterDefinition<Entities.General.Content.Content> filterDef = builder.Empty;
            filterDef &= builder.Eq(nameof(Entities.General.Content.Content.ContentCategoryId), contentCategoryId);
            filterDef = builder.Gte(nameof(Entities.General.Content.Content.EndShowDate), DateTime.Now);
            filterDef &= builder.Lte(nameof(Entities.General.Content.Content.StartShowDate), DateTime.Now);
            if (domainEntity != null)
            {
                //??? should be uncommented in development mode
                filterDef &= builder.Eq(nameof(Entities.General.Content.Content.AssociatedDomainId), domainEntity.DomainId);
                switch (contentType)
                {
                    case ProductOrContentType.Newest:
                        lst = _contentContext.Collection
                           .Find(filterDef)
                           .Project(_ =>
                               new ContentGlance()
                               {
                                   TotalScore = _.TotalScore,
                                   ScoredCount = _.ScoredCount,
                                   VisitCount = _.VisitCount,
                                   CategoryName = _.ContentCategoryName,
                                   ContentCategoryId = _.ContentCategoryId,
                                   ContentId = _.ContentId,
                                   ContentProviderName = _.ContentProviderName,
                                   Images = _.Images,
                                   SubTitle = _.SubTitle,
                                   TagKeywords = _.TagKeywords,
                                   Title = _.Title,
                                   UrlFriend = _.UrlFriend,
                                   ContentCode = _.ContentCode
                               }).Sort(Builders<Entities.General.Content.Content>.Sort.Descending(_ => _.StartShowDate)).Limit(count).ToList();
                        break;
                    case ProductOrContentType.MostPopular:
                        lst = _contentContext.Collection
                       .Find(filterDef)
                       .Project(_ =>
                           new ContentGlance()
                           {
                               TotalScore = _.TotalScore,
                               ScoredCount = _.ScoredCount,
                               VisitCount = _.VisitCount,
                               CategoryName = _.ContentCategoryName,
                               ContentCategoryId = _.ContentCategoryId,
                               ContentId = _.ContentId,
                               ContentProviderName = _.ContentProviderName,
                               Images = _.Images,
                               SubTitle = _.SubTitle,
                               TagKeywords = _.TagKeywords,
                               Title = _.Title,
                               UrlFriend = _.UrlFriend,
                               ContentCode = _.ContentCode
                           }).Sort(Builders<Entities.General.Content.Content>.Sort.Descending(_ => (float)_.TotalScore / _.ScoredCount)).Limit(count).ToList();
                        break;
                    case ProductOrContentType.MostVisited:
                        lst = _contentContext.Collection
                       .Find(filterDef)
                       .Project(_ =>
                           new ContentGlance()
                           {
                               TotalScore = _.TotalScore,
                               ScoredCount = _.ScoredCount,
                               VisitCount = _.VisitCount,
                               CategoryName = _.ContentCategoryName,
                               ContentCategoryId = _.ContentCategoryId,
                               ContentId = _.ContentId,
                               ContentProviderName = _.ContentProviderName,
                               Images = _.Images,
                               SubTitle = _.SubTitle,
                               TagKeywords = _.TagKeywords,
                               Title = _.Title,
                               UrlFriend = _.UrlFriend,
                               ContentCode = _.ContentCode
                           }).Sort(Builders<Entities.General.Content.Content>.Sort.Descending(_ => _.VisitCount)).Limit(count).ToList();
                        break;
                    default:
                        break;
                }
                foreach (var con in lst)
                {
                    var r = Helpers.Utilities.ConvertPopularityRate(con.TotalScore, con.ScoredCount);
                    con.LikeRate = r.LikeRate;
                    con.HalfLikeRate = r.HalfLikeRate;
                    con.DisikeRate = r.DisikeRate;
                }
            }
            return lst;
        }

        public PagedItems<ContentGlance> GetAllBlogList(string queryString, string domainId, string languageId)
        {
            var result = new PagedItems<ContentGlance>();
            NameValueCollection filter = HttpUtility.ParseQueryString(queryString);

            if (string.IsNullOrWhiteSpace(filter["page"]))
            {
                filter.Set("page", "1");
            }
            if (string.IsNullOrWhiteSpace(filter["pageSize"]))
            {
                filter.Set("pageSize", "5");
            }
            var page = Convert.ToInt32(filter["page"]);
            var pageSize = Convert.ToInt32(filter["pageSize"]);
            try
            {
                FilterDefinitionBuilder<Entities.General.Content.Content> builder = new();
                FilterDefinition<Entities.General.Content.Content> filterDef = builder.Empty;

                //??? should be uncommented in development mode
                filterDef = builder.Eq(nameof(Entities.General.Content.Content.AssociatedDomainId), domainId);
                filterDef = builder.And(filterDef, builder.Eq(nameof(Entities.General.Content.Content.LanguageId), languageId));
                filterDef = builder.And(filterDef, builder.Eq(nameof(Entities.General.Content.Content.IsDeleted), false));

                long totalCount = _contentContext.Collection.Find(filterDef).CountDocuments();
                var lst = _contentContext.Collection
                           .Find(filterDef)
                           .Project(_ =>
                               new ContentGlance()
                               {
                                   TotalScore = _.TotalScore,
                                   ScoredCount = _.ScoredCount,
                                   VisitCount = _.VisitCount,
                                   CategoryName = _.ContentCategoryName,
                                   ContentCategoryId = _.ContentCategoryId,
                                   ContentId = _.ContentId,
                                   ContentProviderName = _.ContentProviderName,
                                   Images = _.Images,
                                   SubTitle = _.SubTitle,
                                   TagKeywords = _.TagKeywords,
                                   Title = _.Title,
                                   UrlFriend = _.UrlFriend,
                                   ContentCode = _.ContentCode
                               }).Sort(Builders<Entities.General.Content.Content>.Sort.Descending(_ => _.StartShowDate)).Skip((page - 1) * pageSize).Limit(pageSize).ToList();

                result.Items = lst;
                result.CurrentPage = page;
                result.ItemsCount = totalCount;
                result.PageSize = pageSize;
                result.QueryString = queryString;

            }
            catch (Exception ex)
            {
               
            }
            return result;
        }

        public async Task<Result> UpdateVisitCount(string contentId)
        {
            var result = new Result();
            var entity = _contentContext.Collection.Find(_ => _.ContentId == contentId).FirstOrDefault();
            if(entity != null)
            {
                entity.VisitCount += 1;
                var updateResult = await _contentContext.Collection.ReplaceOneAsync(_ => _.ContentId == contentId, entity);
                if (updateResult.IsAcknowledged)
                {
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
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
            return result;
        }
    }
}
