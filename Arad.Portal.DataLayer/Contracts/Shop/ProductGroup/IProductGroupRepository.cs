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
        List<ProductGroupDTO> List(string parentId);
        Task<List<ProductGroupDTO>> AllProductGroups();
        Task<RepositoryOperationResult> Add(ProductGroupDTO productGroup);
        ProductGroupDTO GetById(string productGroupId);
        bool GroupExistance(string productGroupId);
        Task<RepositoryOperationResult> Update(ProductGroupDTO dto);
        Task<RepositoryOperationResult> Delete(string id, string modificationReason);
        //???
        //ProductGroup GetByMenuId(string menuId);
        List<ProductGroupDTO> GetsDirectChildren(string productGroupId);
        List<ProductGroupDTO> GetsParents();
    }
}
