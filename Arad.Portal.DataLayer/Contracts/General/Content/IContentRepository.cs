using Arad.Portal.DataLayer.Entities.General.DesignStructure;
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
        Task<Result> Add(ContentDTO dto);
        Task<PagedItems<ContentViewModel>> List(string queryString);
        List<ContentGlance> GetSpecialContent(int count, ProductOrContentType contentType, bool isDevelopment);
        Task<ContentDTO> ContentFetch(string contentId);
        Task<Result> Update(ContentDTO dto);
        Task<Result> Delete(string contentId, string modificationReason);
        Task<Result<EntityRate>> RateContent(string contentId, int score, bool isNew, int prevScore);
        Task<Result> Restore(string contentId);
        List<SelectListModel> GetContentsList(string domainId, string currentUserId, string categoryId);
        List<SelectListModel> GetAllSourceType();
        List<SelectListModel> GetAllImageRatio();
        ContentDTO FetchBySlug(string slug, string domainName);
        ContentDTO FetchByCode(long contentCode);
    }
}
