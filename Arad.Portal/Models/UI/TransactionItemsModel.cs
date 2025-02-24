using Arad.Portal.DataLayer.Models.Shared.Product;

using System;
using System.Collections.Generic;

namespace Arad.Portal.Models.UI;

public class TransactionItems
{
    public TransactionItems()
    {
        Orders = new();
    }

    public DateTime CreatedDate { get; set; }
    public List<ProductOrder> Orders { get; set; }
}
public class ProductOrder
{
    public ProductOrder()
    {
        SpecValues = new();
    }
    public string ProductId { get; init; }

    public List<SpecValue> SpecValues { get; set; }

    public int OrderCount { get; init; }
}