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
        Task<Result> Add(ProductSpecificationDTO dto);
        Task<PagedItems<ProductSpecificationViewModel>> List(string queryString);
        Task<ProductSpecificationDTO> SpecificationFetch(string specId);
        Result<Entities.Shop.ProductSpecification.ProductSpecification> GetEntity(string specId);
        Task<Result> Update(ProductSpecificationDTO spec);
        Task<Result> Delete(string specificationId, string modificationReason);
        Task<Result> Restore(string specificationId);
        Result<List<MultiLingualProperty>> GetSpecificationValues(string productSpecificationId);
        List<SelectListModel> GetSpcificationValuesInLanguage(string specificationId, string languageId);

        List<SelectListModel> GetAllControlTypes();
        List<ProductSpecificationDTO> GetAllSpecificationsInGroup(string specificationGroupId);
        List<SelectListModel> GetSpecInGroupAndLanguage(string specificationGroupId, string languageId);
    }
}
