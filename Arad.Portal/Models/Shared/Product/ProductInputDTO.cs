using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Shared.Product;
using Arad.Portal.DataLayer.Entities.Shop.Product;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Product;

public class ProductInputDto
{
    public ProductInputDto()
    {
        MultiLingualProperties = [];
        Prices = [];
        Pictures = [];
        //Comments = new ();
        Specifications = [];
        Inventory = [];
    }
    public string Id { get; set; }

    public List<string> GroupIds { get; set; }

    public List<string> GroupNames { get; set; }

    public List<MultiLingualProperty> MultiLingualProperties { get; set; }

    public string UniqueCode { get; set; }

    public List<ProductSpecificationValue> Specifications { get; set; }

    public List<Image> Pictures { get; set; }

    public List<InventoryDetail> Inventory { get; set; }

    public int MinimumCount { get; set; }

    public long ProductCode { get; set; }

    public ProductType ProductType { get; set; }

    public DownloadLimitationType DownloadLimitationType { get; set; }

    public int? AllowedDownloadDurationDay { get; set; }

    public int? AllowedDownloadCount { get; set; }

    public string? ProductFileContent { get; set; }

    public string? ProductFileName { get; set; }

    public string? ProductFileUrl { get; set; }

    public bool ShowInLackOfInventory { get; set; }

    public string? SellerUserId { get; set; }

    public string? SellerUserName { get; set; }

    public bool IsActive { get; set; }

    public string UnitId { get; set; }

    public List<PriceDto> Prices { get; set; }

    public string? PromotionId { get; set; }
    public bool IsPublishedOnMainDomain { get; set; }

    public string AssociatedDomainId { get; set; }
}