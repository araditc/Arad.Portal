using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.User
{
    public class UserFavorites : BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]

        public string UserFavoritesId { get; set; }

        public FavoriteType FavoriteType { get; set; }

        public string EntityId { get; set; }

        public string Url { get; set; }
    }

    public enum FavoriteType
    {
        Product = 1,
        Content = 2
    }
}
