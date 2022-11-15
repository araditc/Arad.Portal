﻿using Arad.Portal.DataLayer.Entities.General.Comment;
using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using Arad.Portal.DataLayer.Models.Promotion;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

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
            Inventory = new();
        }
        public string ProductId { get; set; }

        public List<string> GroupIds { get; set; }

        public List<string> GroupNames { get; set; }


        public bool IsNew { get; set; }
        public List<MultiLingualProperty> MultiLingualProperties { get; set; }

        public string UniqueCode { get; set; }

        public long ProductCode { get; set; }

        public ProductType ProductType { get; set; }
        public DownloadLimitationType DownloadLimitationType { get; set; }
        public int? AllowedDownloadDurationDay { get; set; }
        public int? AllowedDownloadCount { get; set; }
        public string ProductFileContent { get; set; }
        public string ProductFileName { get; set; }

        public string ProductFileUrl { get; set; }

        public List<ProductSpecificationValue> Specifications { get; set; }

        public List<Image> Images { get; set; }

        public List<InventoryDetail> Inventory { get; set; }

        public int TotalInventory { get; set; }

        public int MinimumCount { get; set; }

        public string MainImageUrl { get; set; }

        public string MainAlt { get; set; }

        public bool ShowInLackOfInventory { get; set; }

        public string SellerUserId { get; set; }

        public string SellerUserName { get; set; }

        public Entities.Shop.ProductUnit.ProductUnit Unit { get; set; }
        public DiscountType? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal OldPrice { get; set; }
        public decimal PriceValWithPromotion { get; set; }
        public List<Price> Prices { get; set; }
        public ProductOutputDTO GiftProduct { get; set; }

        public Entities.Shop.Promotion.Promotion Promotion { get; set; }
        public long? TotalScore { get; set; }
        public int? ScoredCount { get; set; }
        public int LikeRate { get; set; }

        public bool HalfLikeRate { get; set; }

        public int DisikeRate { get; set; }

        public int SaleCount { get; set; }

        public int VisitCount { get; set; }

        public List<CommentVM> Comments { get; set; }

        public bool IsPublishedOnMainDomain { get; set; }

        public string ModificationReason { get; set; }
    }
}
