using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.General.User.Mongo
{
    public class UserContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.User.ApplicationUser> Collection;
        public IMongoCollection<Entities.General.User.UserFavorites> UserFavoritesCollection;
       
        public IMongoCollection<BsonDocument> BsonCollection;
        public IMongoCollection<BsonDocument> UserFavoritesBsonCollection;
        
        private readonly IConfiguration _configuration;

        public UserContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["DatabaseConfig:ConnectionString"]);
            db = client.GetDatabase(_configuration["DatabaseConfig:DbName"]);
            Collection = db.GetCollection<Entities.General.User.ApplicationUser>("Users");
            BsonCollection = db.GetCollection<BsonDocument>("Users");
            UserFavoritesCollection = db.GetCollection<Entities.General.User.UserFavorites>("UserFavorites");
            UserFavoritesBsonCollection = db.GetCollection<BsonDocument>("UserFavorites");
           
        }
    }
}
