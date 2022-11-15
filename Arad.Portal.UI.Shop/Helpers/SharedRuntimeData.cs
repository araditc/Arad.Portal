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
        /// <summary>
        /// first parameter which is string is  ar_TransactionId
        /// </summary>
        private readonly ConcurrentDictionary<string, TransactionItems> _PayingOrders
                        = new ConcurrentDictionary<string, TransactionItems>();


        private readonly IProductRepository _productRepository;
       
        public SharedRuntimeData(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
      
        public ConcurrentDictionary<string, TransactionItems> PayingOrders => _PayingOrders;

        public void AddToPayingOrders(string key , TransactionItems value)
        {
            PayingOrders[key] = value;
        }

        /// <summary>
        /// after successfull response which comes from 
        /// successfull payment
        /// </summary>
        /// <param name="transactionId"></param>
        public void DeleteDataWithoutRollBack(string transactionId)
        {
            PayingOrders.TryRemove($"ar_{transactionId}", out _);
        }

        /// <summary>
        /// this method called when transaction is being cancelled
        /// or the time of creation passed more than  20 minutes
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteDataWithRollBack(string transactionId)
        {
            bool result = true;
            var canGet = PayingOrders.TryGetValue($"ar_{transactionId}", out var transactionItemsObj);
            if(canGet)
            {
                foreach (var order in transactionItemsObj.Orders)
                {
                    result &= (await _productRepository.UpdateProductInventory(order.ProductId, true, order.OrderCount, order.SpecValues)).Succeeded;
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
        /// this is the method which is called in schedule 
        /// task delete all orders that creation time passed more than 20 minutes
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
        /// <summary>
        /// this method called when application forced to stopped
        /// /count of deleted product should be returns to inventory
        /// </summary>
        public async void DeleteAllDataWithRoleBack()
        {
            foreach (var order in PayingOrders)
            {
                await DeleteDataWithRollBack(order.Key.Replace("ar_", ""));
            }
        }
    }
}
