//using Microsoft.Extensions.Configuration;
//using MongoDB.Driver;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Arad.Portal.DataLayer.Repositories.Shop.ProductUnit.Mongo
//{
//    public class ProductUnitContext
//    {
//        private readonly MongoClient client;
//        private readonly IMongoDatabase db;
//        public IMongoCollection<Entities.Shop.ProductUnit.ProductUnit> Collection;
//        private readonly IConfiguration _configuration;

//        public ProductUnitContext(IConfiguration configuration)
//        {
//            _configuration = configuration;
//            client = new MongoClient(_configuration["DB:ConnectionString"]);
//            db = client.GetDatabase(_configuration["DB:DbName"]);
//            Collection = db.GetCollection<Entities.Shop.ProductUnit.ProductUnit>("ProductUnit");
//        }
//    }
//}
