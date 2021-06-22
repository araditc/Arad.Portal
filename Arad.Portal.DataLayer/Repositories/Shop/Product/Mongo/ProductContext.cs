using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo
{
    public class ProductContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.Shop.Product.BaseProduct> Collection;
        private readonly IConfiguration _configuration;

        public ProductContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["DB:ConnectionString"]);
            db = client.GetDatabase(_configuration["DB:DbName"]);
            Collection = db.GetCollection<Entities.Shop.Product.BaseProduct>("Product");
        }
    }
}
