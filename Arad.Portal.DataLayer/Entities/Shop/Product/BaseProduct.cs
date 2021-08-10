using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Comment;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Product;
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
            Specifications = new ();
            Pictures = new ();
            Prices = new ();
            Comments = new ();
            MultiLingualProperties = new ();
        }


        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string ProductId { get; set; }

        public List<string> GroupIds { get; set; }

        public List<string> GroupNames { get; set; }
        /// <summary>
        /// Name, language, currency, seo
        /// </summary>
        public List<MultiLingualProperty> MultiLingualProperties { get; set; }

        public string UniqueCode { get; set; }

        public List<ProductSpecificationValue> Specifications { get; set; }
      
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

        public string SellerUserName { get; set; }

        public ProductUnit.ProductUnit Unit { get; set; }

        public List<Price> Prices { get; set; }

        public Promotion.Promotion Promotion { get; set; }

        public int PopularityRate { get; set; }

        public int SaleCount { get; set; }

        public int VisitCount { get; set; }

        public bool IsPublishedOnMainDomain { get; set; }

        public List<Comment> Comments { get; set; }

        
    }
}
