using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Content;
using Arad.Portal.DataLayer.Models.ContentCategory;
using Arad.Portal.DataLayer.Models.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.ContentCategory
{
    public interface IContentCategoryRepository 
    {
        Task<PagedItems<ContentCategoryViewModel>> List(string queryString, ApplicationUser user);

        Task<List<SelectListModel>> AllActiveContentCategory(string langId, string currentUserId, string domainId = "");

        Task<Result> Add(ContentCategoryDTO dto);

        Task<Result> Update(ContentCategoryDTO dto);

        Task<Result> Delete(string contentCategoryId,
            string modificationReason);

        Task<Result> Restore(string contentCategoryId);

        Task<ContentCategoryDTO> ContentCategoryFetch(string contentCategoryId, bool isDeleted = false);

        List<ContentCategoryDTO> GetDirectChildrens(string contentCategoryId, int? count, int skip = 0);

        List<ContentViewModel> GetContentsInCategory(string domainId, string contentCategoryId, int? count, int skip = 0);

        ContentCategoryDTO FetchBySlug(string slug, string domainName);

        string FetchByCode(string slugOrCode);

        List<SelectListModel> GetAllContentCategoryType();

        Task<List<string>> AllCategoryIdsWhichEndInContents(string domainName);

        bool IsUniqueUrlFriend(string urlFriend, string contentCategoryId = "");

        void InsertMany(List<Entities.General.ContentCategory.ContentCategory> categories);

    }
}
