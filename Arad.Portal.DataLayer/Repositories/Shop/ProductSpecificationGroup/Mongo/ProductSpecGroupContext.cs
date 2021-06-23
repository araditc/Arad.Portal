//using Microsoft.Extensions.Configuration;
//using MongoDB.Driver;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Arad.Portal.DataLayer.Repositories.Shop.ProductSpecificationGroup.Mongo
//{
//    public class ProductSpecGroupContext
//    {
//        private readonly MongoClient client;
//        private readonly IMongoDatabase db;
//        public IMongoCollection<Entities.Shop.ProductSpecificationGroup.ProductSpecGroup> Collection;
//        private readonly IConfiguration _configuration;

//        public ProductSpecGroupContext(IConfiguration configuration)
//        {
//            _configuration = configuration;
//            client = new MongoClient(_configuration["DB:ConnectionString"]);
//            db = client.GetDatabase(_configuration["DB:DbName"]);
//            Collection = db.GetCollection<Entities.Shop.ProductSpecificationGroup.ProductSpecGroup>("ProductSpeGroup");
//        }
//    }
//}
