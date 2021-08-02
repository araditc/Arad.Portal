using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Models.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.DataLayer.Contracts.Shop.ProductGroup
{
    public interface IProductGroupRepository
    {
        Task<List<ProductGroupDTO>> AllProductGroups();
        Task<RepositoryOperationResult> Add(ProductGroupDTO dto);

        Task<RepositoryOperationResult> Update(ProductGroupDTO dto);
        ProductGroupDTO ProductGroupFetch(string productGroupId, string langId);

        Entities.Shop.ProductGroup.ProductGroup FetchWholeProductGroup(string productGroupId);
        bool GroupExistance(string productGroupId);
        //update based on the UI
        //Task<RepositoryOperationResult> Update(ProductGroupDTO dto);

        Task<PagedItems<ProductGroupDTO>> List(string queryString);
        Task<RepositoryOperationResult> Delete(string productGroupId, string modificationReason);
        Task<RepositoryOperationResult> Restore(string id);
        List<ProductGroupDTO> GetsDirectChildren(string productGroupId);
        List<ProductGroupDTO> GetsParents();

        List<SelectListModel> GetAlActiveProductGroup(string langId);
        Task<RepositoryOperationResult> AddPromotionToGroup(string productGroupId,
            Models.Promotion.PromotionDTO promotionDto, string modificationReason);
    }
}
