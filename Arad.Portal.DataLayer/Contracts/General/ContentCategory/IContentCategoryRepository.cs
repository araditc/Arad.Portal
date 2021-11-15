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
        Task<PagedItems<ContentCategoryViewModel>> List(string queryString);

        Task<List<SelectListModel>> AllActiveContentCategory(string langId, string currentUserId);

        Task<RepositoryOperationResult> Add(ContentCategoryDTO dto);

        Task<RepositoryOperationResult> Update(ContentCategoryDTO dto);

        Task<RepositoryOperationResult> Delete(string contentCategoryId,
            string modificationReason);

        Task<RepositoryOperationResult> Restore(string contentCategoryId);

        Task<ContentCategoryDTO> ContentCategoryFetch(string contentCategoryId);

        List<ContentCategoryDTO> GetDirectChildrens(string contentCategoryId, int? count, int skip = 0);

        List<ContentViewModel> GetContentsInCategory(string contentCategoryId, int? count, int skip = 0);

        ContentCategoryDTO FetchBySlug(string slug, string domainName);

        CommonViewModel FetchByCode(long categoryCode);

        List<SelectListModel> GetAllContentCategoryType();

    }
}
