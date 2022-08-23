using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.Shop.Promotion.Mongo
{
    public class PromotionContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.Shop.Promotion.Promotion> Collection;
        public IMongoCollection<Entities.Shop.Promotion.UserCoupon> UserCouponCollection;
        public IMongoCollection<BsonDocument> BsonCollection;
        public IMongoCollection<BsonDocument> BsonUserCouponCollection;
        private readonly IConfiguration _configuration;

        public PromotionContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["DatabaseConfig:ConnectionString"]);
            db = client.GetDatabase(_configuration["DatabaseConfig:DbName"]);
            Collection = db.GetCollection<Entities.Shop.Promotion.Promotion>("Promotion");
            UserCouponCollection = db.GetCollection<Entities.Shop.Promotion.UserCoupon>("UserCoupon");
            BsonCollection = db.GetCollection<BsonDocument>("Promotion");
            BsonUserCouponCollection = db.GetCollection<BsonDocument>("UserCoupon");
        }
    }
}
