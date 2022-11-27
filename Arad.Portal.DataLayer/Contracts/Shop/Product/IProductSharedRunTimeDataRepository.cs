using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.Shop.Product
{
    public interface IProductSharedRunTimeDataRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="isIncreament">if equals to false then it is decreament</param>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<Result> UpdateProductInventory(string productId, bool isIncreament, int count, List<SpecValue> specValues);
    }
}
