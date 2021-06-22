using Arad.Portal.DataLayer.Models.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.Shop.ProductSpecificationGroup
{
    interface IProductSpecGroupRepository 
    {
        Task<PagedItems<SpecificationGroupDTO>> List(string queryString);

        Task<RepositoryOperationResult> Add(SpecificationGroupDTO dto);

        Task<RepositoryOperationResult> Update(SpecificationGroupDTO dto);

        Task<RepositoryOperationResult> Delete(string productSpecificationGroupId,
            string modificationReason);

        Task<SpecificationGroupDTO> GroupSpecificationFetch(string productSpecificationGroupId);
        
    }
}
