using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.Models.UI;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Models.Shared.Product;

namespace Arad.Portal.Helpers.UI;

public class SharedRuntimeData
{
    /// <summary>
    /// first parameter which is string is  ar_TransactionId
    /// </summary>
    private readonly ConcurrentDictionary<string, TransactionItems> _payingOrders = new();


    private readonly IProductRepository _productRepository;

    public SharedRuntimeData(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public ConcurrentDictionary<string, TransactionItems> PayingOrders => _payingOrders;

    public void AddToPayingOrders(string key, TransactionItems value)
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
        bool canGet = PayingOrders.TryGetValue($"ar_{transactionId}", out TransactionItems transactionItemsObj);
        if (canGet)
        {
            foreach (ProductOrder order in transactionItemsObj.Orders)
            {

                Result results = new Result();
                Product entity = _productRepository.FirstOrDefault(p => p.Id == order.ProductId);
                InventoryDetail inventoryDetail = new InventoryDetail();
                List<InventoryDetail> lst = entity.Inventory;

                foreach (SpecValue item in order.SpecValues)
                {
                    lst = lst.Where(d => d.SpecValues.Any(a => a.SpecificationId == item.SpecificationId && a.SpecificationValue == item.SpecificationValue)).ToList();
                }
                if (lst.Count > 0)
                {
                    inventoryDetail = lst[0];
                    entity.Inventory.FirstOrDefault(d => d.SpecValuesId == inventoryDetail.SpecValuesId).Count += order.OrderCount;
                    // var updateResult = await _productRepository.UpdateAsync(c => c.Id == order.ProductId, entity);
                    // if (updateResult.IsAcknowledged)
                    // {
                    //result.Succeeded = true;
                    // }
                }

                //result &= (await _productRepository.UpdateProductInventory(order.ProductId, true, order.OrderCount, order.SpecValues)).Succeeded;
            }
        }
        else
        {
            result = false;
        }

        if (result)
        {
            PayingOrders.TryRemove($"ar_{transactionId}", out transactionItemsObj);
        }
        return result;
    }

    /// <summary>
    /// this is the method which is called in schedule 
    /// task delete all orders that creation time passed more than 20 minutes
    /// </summary>
    public async void DeleteAllUnusedData()
    {
        foreach (KeyValuePair<string, TransactionItems> order in PayingOrders)
        {
            if (order.Value.CreatedDate.AddMinutes(20) <= DateTime.Now)
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
        foreach (KeyValuePair<string, TransactionItems> order in PayingOrders)
        {
            await DeleteDataWithRollBack(order.Key.Replace("ar_", ""));
        }
    }
}