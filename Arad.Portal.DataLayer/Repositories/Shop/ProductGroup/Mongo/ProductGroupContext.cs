//using Microsoft.Extensions.Configuration;
//using MongoDB.Driver;

//namespace Arad.Portal.DataLayer.Repositories.Shop.ProductGroup.Mongo
//{
//    public class ProductGroupContext
//    {
//        private readonly MongoClient client;
//        private readonly IMongoDatabase db;
//        public IMongoCollection<Entities.Shop.ProductGroup.ProductGroup> Collection;
//        private readonly IConfiguration _configuration;

//        public ProductGroupContext(IConfiguration configuration)
//        {
//            _configuration = configuration;
//             client = new MongoClient(_configuration["DatabaseConfig:ConnectionString"]);
//           db = client.GetDatabase(_configuration["DatabaseConfig:DbName"]);
//            Collection = db.GetCollection<Entities.Shop.ProductGroup.ProductGroup>("ProductGroup");
//        }
//    }
//}
