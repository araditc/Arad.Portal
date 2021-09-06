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

        List<SelectListModel> GetAllContentCategoryType();

    }
}
