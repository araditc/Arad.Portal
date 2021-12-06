using Arad.Portal.DataLayer.Models.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.Shop.ProductSpecificationGroup
{
    public interface IProductSpecGroupRepository 
    {
        Task<PagedItems<SpecificationGroupViewModel>> List(string queryString);

        List<SelectListModel> AllActiveSpecificationGroup(string langId);

        Task<Result> Add(SpecificationGroupDTO dto);

        Task<Result> Update(SpecificationGroupDTO dto);

        Task<Result> Delete(string productSpecificationGroupId,
            string modificationReason);

        Task<Result> Restore(string productSpecificationGroupId);
            

        Task<SpecificationGroupDTO> GroupSpecificationFetch(string productSpecificationGroupId);

    }
}
