using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;


namespace Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo
{
    public class ProductSharedRunTimeDataRepository : IProductSharedRunTimeDataRepository
    {
        private readonly ProductContext _context;
        public ProductSharedRunTimeDataRepository(ProductContext context)
        {
            _context = context;
        }
        public async Task<Result> UpdateProductInventory(string productId, bool isIncreament, int count, List<SpecValue> specValues)
        {
            var result = new Result();
            var entity = _context.ProductCollection.Find(_ => _.ProductId == productId).FirstOrDefault();
            var inventoryDetail = new InventoryDetail();
            List<InventoryDetail> lst = entity.Inventory;

            foreach (var item in specValues)
            {
                lst = lst.Where(_ => _.SpecValues.Any(a => a.SpecificationId == item.SpecificationId && a.SpecificationValue == item.SpecificationValue)).ToList();
            }
            if (lst.Count > 0)
            {
                inventoryDetail = lst[0];

                if (isIncreament)
                    entity.Inventory.FirstOrDefault(_ => _.SpecValuesId == inventoryDetail.SpecValuesId).Count += count;
                else
                    entity.Inventory.FirstOrDefault(_ => _.SpecValuesId == inventoryDetail.SpecValuesId).Count -= count;

                var updateResult = await _context.ProductCollection.ReplaceOneAsync(_ => _.ProductId == productId, entity);
                if (updateResult.IsAcknowledged)
                {
                    result.Succeeded = true;
                }
            }
            return result;
        }
    }
}
