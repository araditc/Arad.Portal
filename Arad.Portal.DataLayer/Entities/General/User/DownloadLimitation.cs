using Arad.Portal.DataLayer.Entities.Shop.Product;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.User
{
    public class DownloadLimitation : BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string DownloadLimitId { get; set; }
        public string ShoppingCartDetailId { get; set; }

        public string ProductId { get; set; }

        public int? DownloadedCount { get; set; }

        public DateTime? StartDate { get; set; }
    }
}
