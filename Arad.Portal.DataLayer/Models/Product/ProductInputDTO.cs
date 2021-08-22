using Arad.Portal.DataLayer.Entities.Shop.ProductUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Promotion;
using Arad.Portal.DataLayer.Models.ProductSpecification;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class ProductInputDTO
    {
        public ProductInputDTO()
        {
            MultiLingualProperties = new();
            Prices = new();
            Pictures = new ();
            Comments = new ();
            Specifications = new ();
        }
        public string ProductId { get; set; }

        public List<string> GroupIds { get; set; }

        public List<string> GroupNames { get; set; }

        public List<MultiLingualProperty> MultiLingualProperties { get; set; }

        public string UniqueCode { get; set; }

        public List<ProductSpecificationValue> Specifications { get; set; }

        public List<Image> Pictures { get; set; }

        public int Inventory { get; set; }

        public int MinimumCount { get; set; }
        
        public bool ShowInLackOfInventory { get; set; }
       
        public string SellerUserId { get; set; }

        public string SellerUserName { get; set; }

        public string UnitId { get; set; }

        public List<Price> Prices { get; set; }

        public string PromotionId { get; set; }

        public int PopularityRate { get; set; }

        public int SaleCount { get; set; }

        public int VisitCount { get; set; }

        public bool IsPublishedOnMainDomain { get; set; }

        public string ModificationReason { get; set; }

        public List<Comment.Comment> Comments { get; set; }
    }
}
