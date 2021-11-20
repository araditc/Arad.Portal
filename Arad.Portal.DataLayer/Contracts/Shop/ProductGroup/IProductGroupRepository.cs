using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
namespace Arad.Portal.DataLayer.Contracts.Shop.ProductGroup
{
    public interface IProductGroupRepository
    {
        Task<RepositoryOperationResult> Add(ProductGroupDTO dto);
        Task<RepositoryOperationResult> Update(ProductGroupDTO dto);
        ProductGroupDTO ProductGroupFetch(string productGroupId);
        ProductGroupDTO FetchBySlug(string slug, string domainName);
        //ProductGroupDTO FetchByCode(long groupCode);
        //CommonViewModel FetchByCode(long groupCode);
        string FetchByCode(long groupCode);
        bool GroupExistance(string productGroupId);
        Task<PagedItems<ProductGroupViewModel>> List(string queryString);
        Task<RepositoryOperationResult> Delete(string productGroupId, string modificationReason);
        Task<RepositoryOperationResult> Restore(string id);
        List<ProductGroupDTO> GetsDirectChildren(List<string> groupsWithProduct, 
            string domainName, string productGroupId, int? count, int skip = 0);
        Task<long> GetDircetChildrenCount(string domainName, string productGroupId, List<string> groupsWithProduct);
        long GetProductsInGroupCounts(string domainName, string productGroupId);
        List<ProductGroupDTO> GetsParents();
        List<ProductOutputDTO> GetLatestProductInThisGroup(string domainName, string productGroupId, int? count, int skip = 0);
        Task<List<SelectListModel>> GetAlActiveProductGroup(string langId, string currentUserId);
        Task<RepositoryOperationResult> AddPromotionToGroup(string productGroupId,
            Models.Promotion.PromotionDTO promotionDto, string modificationReason);

        Task<List<string>> AllGroupIdsWhichEndInProducts(string domainName);
    }
}
