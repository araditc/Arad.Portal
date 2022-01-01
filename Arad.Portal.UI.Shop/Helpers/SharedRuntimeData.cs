using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Contracts.Shop.Transaction;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Helpers
{
    public class SharedRuntimeData
    {
        private readonly ConcurrentDictionary<string, TransactionItems> _PayingOrders
                        = new ConcurrentDictionary<string, TransactionItems>();
        private readonly IProductRepository _productRepository;
        private readonly ITransactionRepository _transactionRepository;
        public SharedRuntimeData(IProductRepository productRepository, ITransactionRepository transactionRepository)
        {
            _productRepository = productRepository;
            _transactionRepository = transactionRepository;
        }
      
        public ConcurrentDictionary<string, TransactionItems> PayingOrders => _PayingOrders;

        public void AddToPayingOrders(string key , TransactionItems value)
        {
            PayingOrders[key] = value;
        }

        /// <summary>
        /// after successfull respone payment from psp
        /// </summary>
        /// <param name="transactionId"></param>
        public void DeleteDataWithoutRollBack(string transactionId)
        {
            PayingOrders.TryRemove($"ar_{transactionId}", out _);
        }

        /// <summary>
        /// after unsuccessful response payment from psp
        /// </summary>
        public async Task<bool> DeleteDataWithRollBack(string transactionId)
        {
            bool result = true;
            var canGet = PayingOrders.TryGetValue($"ar_{transactionId}", out var transactionItemsObj);
            if(canGet)
            {
                foreach (var order in transactionItemsObj.Orders)
                {
                    result &= (await _productRepository.UpdateProductInventory(order.ProductId, true, order.OrderCount)).Succeeded;
                }
            }
            else
            {
                result = false; 
            }

            if(result)
            {
                PayingOrders.TryRemove($"ar_{transactionId}", out transactionItemsObj);
            }
            return result;
        }

        /// <summary>
        /// this is the method which is called in schedule task
        /// </summary>
        public  async void DeleteAllUnusedData()
        {
            foreach (var order in PayingOrders)
            {
                if(order.Value.CreatedDate.AddMinutes(20) <= DateTime.Now)
                {
                    await DeleteDataWithRollBack(order.Key.Replace("ar_", ""));
                }
            }
        }
    }
}
