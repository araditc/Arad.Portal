using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.Shop.Promotion.Mongo
{
    public class PromotionContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.Shop.Promotion.Promotion> Collection;
        private readonly IConfiguration _configuration;

        public PromotionContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["DB:ConnectionString"]);
            db = client.GetDatabase(_configuration["DB:DbName"]);
            Collection = db.GetCollection<Entities.Shop.Promotion.Promotion>("Promotion");
        }
    }
}
