using Arad.Portal.DataLayer.Entities.Shop.ProductUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Promotion;
using Arad.Portal.DataLayer.Models.ProductSpecification;
using static Arad.Portal.DataLayer.Models.Shared.Enums;
using Arad.Portal.DataLayer.Entities.Shop.Product;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class ProductInputDTO
    {
        public ProductInputDTO()
        {
            MultiLingualProperties = new();
            Prices = new();
            Pictures = new ();
            //Comments = new ();
            Specifications = new ();
            Inventory = new();
        }
        public string ProductId { get; set; }

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

        public string ProductFileContent { get; set; }

        public string ProductFileName { get; set; }

        public string ProductFileUrl { get; set; }

        public bool ShowInLackOfInventory { get; set; }
       
        public string SellerUserId { get; set; }

        public string SellerUserName { get; set; }

        public bool  IsActive { get; set; }

        public string UnitId { get; set; }

        public List<PriceDTO> Prices { get; set; }

        public string PromotionId { get; set; }

        //public int PopularityRate { get; set; }

        //public int SaleCount { get; set; }

        //public int VisitCount { get; set; }

        public bool IsPublishedOnMainDomain { get; set; }

        public string AssociatedDomainId { get; set; }

        //public string ModificationReason { get; set; }

        //public List<Comment.Comment> Comments { get; set; }
    }
}
