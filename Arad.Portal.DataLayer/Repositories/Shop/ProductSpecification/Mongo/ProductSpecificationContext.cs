//using Microsoft.Extensions.Configuration;
//using MongoDB.Driver;

//namespace Arad.Portal.DataLayer.Repositories.Shop.ProductSpecification.Mongo
//{
//    public class ProductSpecificationContext
//    {
//        private readonly MongoClient client;
//        private readonly IMongoDatabase db;
//        public IMongoCollection<Entities.Shop.ProductSpecification.ProductSpecification> Collection;
//        private readonly IConfiguration _configuration;

//        public ProductSpecificationContext(IConfiguration configuration)
//        {
//            _configuration = configuration;
//            client = new MongoClient(_configuration["DB:ConnectionString"]);
//            db = client.GetDatabase(_configuration["DB:DbName"]);
//            Collection = db.GetCollection<Entities.Shop.ProductSpecification.ProductSpecification>("ProductSpecification");
//        }
//    }
//}
