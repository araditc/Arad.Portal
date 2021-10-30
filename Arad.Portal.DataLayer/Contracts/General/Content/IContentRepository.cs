using Arad.Portal.DataLayer.Models.Content;
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
        Task<RepositoryOperationResult> Add(ContentDTO dto);
        Task<PagedItems<ContentViewModel>> List(string queryString);
        Task<ContentDTO> ContentFetch(string contentId);
        Task<RepositoryOperationResult> Update(ContentDTO dto);
        Task<RepositoryOperationResult> Delete(string contentId, string modificationReason);
        Task<RepositoryOperationResult> Restore(string contentId);
        List<ContentViewModel> GetAllContentsInCategory(string contentCategoryId);
        List<SelectListModel> GetContentsList(string domainId, string currentUserId, string categoryId);
        List<SelectListModel> GetAllSourceType();
        ContentDTO FetchBySlug(string slug, string domainName);
    }
}
