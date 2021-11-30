using Arad.Portal.DataLayer.Entities.General.Comment;
using Arad.Portal.DataLayer.Models.Promotion;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class ProductOutputDTO
    {
        public ProductOutputDTO()
        {
            Images = new();
            Comments = new();
            Specifications = new();
            Prices = new List<Price>();
            MultiLingualProperties = new();
            //SpecificationVms = new();
        }
        public string ProductId { get; set; }

        public List<string> GroupIds { get; set; }

        public List<string> GroupNames { get; set; }

        public List<MultiLingualProperty> MultiLingualProperties { get; set; }

        public string UniqueCode { get; set; }

        public long ProductCode { get; set; }

        public List<ProductSpecificationValue> Specifications { get; set; }

        //public List<ProductSpecViewModel> SpecificationVms { get; set; }

        public List<Image> Images { get; set; }

        public int Inventory { get; set; }

        public int MinimumCount { get; set; }

        public bool ShowInLackOfInventory { get; set; }

        public string SellerUserId { get; set; }

        public string SellerUserName { get; set; }

        public Entities.Shop.ProductUnit.ProductUnit Unit { get; set; }

        public List<Price> Prices { get; set; }

        public decimal PriceValWithPromotion { get; set; }

        public ProductOutputDTO GiftProduct { get; set; }

        public Entities.Shop.Promotion.Promotion Promotion { get; set; }

        public int PopularityRate { get; set; }

        public int SaleCount { get; set; }

        public int VisitCount { get; set; }

        public List<CommentVM> Comments { get; set; }

        public bool IsPublishedOnMainDomain { get; set; }

        public string ModificationReason { get; set; }
    }
}
