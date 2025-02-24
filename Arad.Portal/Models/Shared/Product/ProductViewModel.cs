using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Entities.Shop.ProductUnit;
using Arad.Portal.DataLayer.Models.Shared;

using System;
using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Product;

public class ProductViewModel
{
    public ProductViewModel()
    {
        MultiLingualProperties = new();
        Prices = new();
        Inventory = new();
    }
    public string ProductId { get; init; }
    public List<string> GroupNames { get; init; }
    public List<string> GroupIds { get; set; }
    public string UniqueCode { get; init; }
    public long ProductCode { get; init; }
    public List<InventoryDetail> Inventory { get; init; }
    public List<MultiLingualProperty> MultiLingualProperties { get; init; }
    public List<Image> Images { get; init; }
    public List<Price> Prices { get; init; }
    public ProductUnit Unit { get; init; }
    public DateTime CreationDate { get; init; }
    public bool IsDeleted { get; init; }
    public List<string> ImageUrls { get; set; }
}