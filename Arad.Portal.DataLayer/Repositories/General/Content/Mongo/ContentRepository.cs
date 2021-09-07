﻿using Arad.Portal.DataLayer.Contracts.General.Content;
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


namespace Arad.Portal.DataLayer.Repositories.General.Content.Mongo
{
    public class ContentRepository : BaseRepository, IContentRepository
    {
        private readonly IMapper _mapper;
        private readonly ContentContext _contentContext;
        private readonly ContentCategoryContext _categoryContext;
        private readonly LanguageContext _languageContext;
        public ContentRepository(IHttpContextAccessor httpContextAccessor,
           IMapper mapper, ContentCategoryContext categoryContext, ContentContext contentContext, LanguageContext langContext)
            : base(httpContextAccessor)
        {
            _mapper = mapper;
            _categoryContext = categoryContext;
            _contentContext = contentContext;
            _languageContext = langContext;
        }

        public async Task<RepositoryOperationResult> Add(ContentDTO dto)
        {
            RepositoryOperationResult result = new RepositoryOperationResult();
            try
            {
                var equallentModel = _mapper.Map<Entities.General.Content.Content>(dto);

                equallentModel.CreationDate = DateTime.Now;
                equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

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
            var entity = await _contentContext.Collection
                .Find(_ => _.ContentId == contentId).FirstOrDefaultAsync();

            if (entity != null)
            {
                result = _mapper.Map<ContentDTO>(entity);
                result.PersianStartShowDate = DateHelper.ToPersianDdate(result.StartShowDate.Value);
                result.PersianEndShowDate = DateHelper.ToPersianDdate(result.EndShowDate.Value);
            }
            return result;
        }

        public async Task<RepositoryOperationResult> Delete(string contentId, string modificationReason)
        {
            var result = new RepositoryOperationResult();
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

        public List<ContentViewModel> GetAllContentsInCategory(string contentCategoryId)
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

                long totalCount = await _contentContext.Collection.Find(c => true).CountDocumentsAsync();
                var totalList = _contentContext.Collection.AsQueryable();
                if (!string.IsNullOrWhiteSpace(filter["catId"]))
                {
                    totalList = totalList.Where(_ => _.ContentCategoryId == filter["catId"]);
                }
                if (!string.IsNullOrWhiteSpace(filter["from"]))
                {
                    //???
                    totalList = totalList
                        .Where(_ => _.CreationDate >= filter["from"].ToString().ToEnglishDate().ToUniversalTime());
                }
                if (!string.IsNullOrWhiteSpace(filter["to"]))
                {
                    //???
                    totalList = totalList
                        .Where(_ => _.CreationDate <= filter["to"].ToString().ToEnglishDate().ToUniversalTime());
                }
                if(!string.IsNullOrWhiteSpace(filter["keyword"]))
                {
                    totalList = totalList.Where(_ => _.TagKeywords.Contains(filter["keyword"]));
                }
                var list = totalList.Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new ContentViewModel()
                   {
                       ContentId = _.ContentId,
                       SeoDescription = _.SeoDescription,
                       PopularityRate = _.PopularityRate,
                       LanguageName = _.LanguageName,
                       LanguageId = _.LanguageId,
                       Images = _.Images,
                       FileLogo = _.FileLogo,
                       EndShowDate = _.EndShowDate,
                       ContentCategoryId = _.ContentCategoryId,
                       ContentCategoryName = _.ContentCategoryName,
                       ContentProviderName = _.ContentProviderName,
                       Description = _.Description,
                       SeoTitle = _.SeoTitle,
                       PersianEndShowDate = DateHelper.ToPersianDdate(_.EndShowDate),
                       PersianStartShowDate = DateHelper.ToPersianDdate(_.StartShowDate),
                       SourceType = _.SourceType,
                       StartShowDate = _.StartShowDate,
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

        public async Task<RepositoryOperationResult> Restore(string contentId)
        {
            var result = new RepositoryOperationResult();
            var entity = _contentContext.Collection
              .Find(_ => _.ContentId == contentId).FirstOrDefault();
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
            return result;
        }

        public async Task<RepositoryOperationResult> Update(ContentDTO dto)
        {
            var result = new RepositoryOperationResult();

            var equallentModel = _mapper.Map<Entities.General.Content.Content>(dto);

            #region add modification
            var mod = GetCurrentModification(dto.ModificationReason);
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
    }
}
