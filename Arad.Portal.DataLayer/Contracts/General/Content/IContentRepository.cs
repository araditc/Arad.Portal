﻿using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Content;
using Arad.Portal.DataLayer.Models.DesignStructure;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.Content
{
    public interface IContentRepository
    {
        Task<Result<string>> Add(ContentDTO dto);
        Task<PagedItems<ContentViewModel>> List(string queryString, ApplicationUser user);
        List<ContentGlance> GetSpecialContent(int count, ProductOrContentType contentType, SelectionType selectionType, string categoryId, List<string> selectedIds = null, bool isDevelopment = false);
        List<ContentGlance> GetContentInCategory(int count, ProductOrContentType contentType, string contentCategoryId, bool isDevelopment = false);
        Task<ContentDTO> ContentFetch(string contentId);
        Task<Result> Update(ContentDTO dto);
        List<Image> GetPictures(string contentId);
        Task<Result> UpdateVisitCount(string contentId);
        Task<Result> Delete(string contentId, string modificationReason);
        Task<Result<EntityRate>> RateContent(string contentId, int score, bool isNew, int prevScore);
        Task<Result<List<GeneralSearchResult>>> GeneralSearch(string filter, string lanId, string CurrencyId, string domainId);
        Task<Result> Restore(string contentId);
        List<SelectListModel> GetContentsList(string domainId, string currentUserId, string categoryId);
        List<SelectListModel> GetAllSourceType();
        List<SelectListModel> GetAllImageRatio();
        /// <param name="queryString">guerystring contains pageIndex and pageCount</param>
        /// <param name="domainId"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        PagedItems<ContentGlance> GetAllBlogList(string queryString, string domainId, string languageId);
        ContentDTO FetchBySlug(string slug, string domainName);
        ContentDTO FetchByCode(string slugOrCode);

        List<Entities.General.Content.Content> AllContents(string domainId);
        bool IsUniqueUrlFriend(string urlFriend, string domainId, string contentId = "");
        Task<Entities.General.Content.Content> ContentSelect(string contentId);
    }
}
