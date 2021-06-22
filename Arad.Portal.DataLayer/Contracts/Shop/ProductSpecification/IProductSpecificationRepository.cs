using Arad.Portal.DataLayer.Models.ProductSpecification;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.Shop.ProductSpecification
{
    interface IProductSpecificationRepository
    {
        Task<RepositoryOperationResult> Add(ProductSpecificationDTO dto);
        Task<PagedItems<ProductSpecificationDTO>> List(string queryString);
        Task<RepositoryOperationResult<ProductSpecificationDTO>> Fetch(string specId);
        Task<bool> Update(ProductSpecificationDTO spec);
        Task<RepositoryOperationResult> Delete(string specificationId, string modificationReason);
        RepositoryOperationResult<List<string>> GetSpecificationValues(string productSpecificationId);
        List<ProductSpecificationDTO> GetAllSpecificationsInGroup(string specificationGroupId);
    }
}
