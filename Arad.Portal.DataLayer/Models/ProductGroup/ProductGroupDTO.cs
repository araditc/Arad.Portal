using Arad.Portal.DataLayer.Entities.Shop.Promotion;
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
            GroupImage = new();
        }
        public string ProductGroupId { get; set; }

        public List<MultiLingualProperty> MultiLingualProperties { get; set; }

        public string ParentId { get; set; }

        public bool IsDeleted { get; set; }

        public long GroupCode { get; set; }

        public Image GroupImage { get; set; }

        public string FileName { get; set; }

        public string FileContent { get; set; }

        public string ModificationReason { get; set; }

        public Entities.Shop.Promotion.Promotion Promotion { get; set; }

    }
}
