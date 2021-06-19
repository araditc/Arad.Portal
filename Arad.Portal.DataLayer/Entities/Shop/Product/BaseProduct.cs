using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Comment;
using Arad.Portal.DataLayer.Models.Price;
using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.Product
{
    public class BaseProduct : BaseEntity
    {
        public BaseProduct()
        {
            GroupNames = new ();
            Attributes = new ();
            Pictures = new ();
            Prices = new ();
            Comments = new ();
            MultiLingualProperties = new ();
        }

        public string Id { get; set; }

        public List<string> GroupIds { get; set; }

        public List<string> GroupNames { get; set; }

        public List<MultiLingualProperty> MultiLingualProperties { get; set; }

        public string UniqueCode { get; set; }

        public Dictionary<ProductSpecification.ProductSpecification, string> Attributes { get; set; }

        public List<Picture> Pictures { get; set; }

        public int Inventory { get; set; }

        public int MinimumCount { get; set; }
        /// <summary>
        /// نمایش در صورت عدم موجودی
        /// </summary>
        public bool ShowInLackOfInventory { get; set; }
        /// <summary>
        /// فروشنده این محصول
        /// </summary>
        public string SellerUserId { get; set; }

        public ProductUnit.ProductUnit Unit { get; set; }

        public string SellerUserName { get; set; }

        public List<Price> Prices { get; set; }

        public Promotion.Promotion Promotion { get; set; }

        public int PopularityRate { get; set; }

        public int SaleCount { get; set; }

        public int VisitCount { get; set; }

        public List<Comment> Comments { get; set; }

        
    }
}
