﻿using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.ProductGroup
{
    public class ProductGroupDTO
    {
        public ProductGroupDTO()
        {
            MultiLingualProperties = new List<MultiLingualProperty>();
        }
        public string Id { get; set; }

        public List<MultiLingualProperty> MultiLingualProperties { get; set; }

        public string ParentId { get; set; }

        public Entities.Shop.Promotion.Promotion Promotion { get; set; }

    }
}
