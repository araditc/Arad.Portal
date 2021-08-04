using Arad.Portal.DataLayer.Models.ProductSpecification;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.Shop.ProductSpecification
{
    public interface IProductSpecificationRepository
    {
        Task<RepositoryOperationResult> Add(ProductSpecificationDTO dto);
        Task<PagedItems<ProductSpecificationViewModel>> List(string queryString);
        Task<ProductSpecificationDTO> SpecificationFetch(string specId);
        RepositoryOperationResult<Entities.Shop.ProductSpecification.ProductSpecification> GetEntity(string specId);
        Task<RepositoryOperationResult> Update(ProductSpecificationDTO spec);
        Task<RepositoryOperationResult> Delete(string specificationId, string modificationReason);
        Task<RepositoryOperationResult> Restore(string specificationId);
        RepositoryOperationResult<List<MultiLingualProperty>> GetSpecificationValues(string productSpecificationId);
        List<ProductSpecificationDTO> GetAllSpecificationsInGroup(string specificationGroupId);
    }
}
