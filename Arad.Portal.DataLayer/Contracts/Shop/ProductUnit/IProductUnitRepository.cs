using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.Shop.ProductUnit
{
    public interface IProductUnitRepository
    {
        Task<PagedItems<ProductUnitViewModel>> List(string queryString, ApplicationUser user);
        Task<Result> AddProductUnit(ProductUnitDTO dto);
        Task<Result> EditProductUnit(ProductUnitDTO dto);
        Task<Result> Delete(string productUnitId);
        Task<Result> Restore(string productUnitId);
        ProductUnitDTO FetchUnit(string productUnitId);
        Task<List<SelectListModel>> GetAllActiveProductUnit(string langId, string currentUserId, string domainId = "");
    }
}
