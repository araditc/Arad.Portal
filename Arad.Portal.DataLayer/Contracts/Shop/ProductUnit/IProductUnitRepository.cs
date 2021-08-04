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
        Task<PagedItems<ProductUnitDTO>> List(string queryString);
        Task<RepositoryOperationResult> AddProductUnit(ProductUnitDTO dto);
        Task<RepositoryOperationResult> EditProductUnit(ProductUnitDTO dto);
        Task<RepositoryOperationResult> Delete(string productUnitId);
        Task<RepositoryOperationResult> Restore(string productUnitId);

        ProductUnitDTO FetchUnit(string productUnitId);
        Entities.Shop.ProductUnit.ProductUnit FetchByName(string unitName);
    }
}
