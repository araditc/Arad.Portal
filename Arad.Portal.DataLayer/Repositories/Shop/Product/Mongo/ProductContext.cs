using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo
{
    public class ProductContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.Shop.Product.Product> ProductCollection;
        public IMongoCollection<Entities.Shop.ProductGroup.ProductGroup> ProductGroupCollection;
        public IMongoCollection<Entities.Shop.ProductSpecification.ProductSpecification> SpecificationCollection;
        public IMongoCollection<Entities.Shop.ProductSpecificationGroup.ProductSpecGroup> SpecGroupCollection;
        public IMongoCollection<Entities.Shop.ProductUnit.ProductUnit> ProductUnitCollection;
        public IMongoCollection<Entities.General.User.DownloadLimitation> DownloadLimitationCollection;


        public IMongoCollection<BsonDocument> BsonProductUnitCollection;
        public IMongoCollection<BsonDocument> BsonProductCollection;
        public IMongoCollection<BsonDocument> BsonProductGroupCollection;
        public IMongoCollection<BsonDocument> BsonSpecificationCollection;
        public IMongoCollection<BsonDocument> BsonSpecGroupCollection;
        public IMongoCollection<BsonDocument> BsonDownloadLimitationCollection;
        private readonly IConfiguration _configuration;

        public ProductContext(IConfiguration configuration)
        {
            _configuration = configuration;
             client = new MongoClient(_configuration["DatabaseConfig:ConnectionString"]);
           db = client.GetDatabase(_configuration["DatabaseConfig:DbName"]);

            ProductCollection = db.GetCollection<Entities.Shop.Product.Product>("Product");
            ProductGroupCollection = 
                db.GetCollection<Entities.Shop.ProductGroup.ProductGroup>("ProductGroup");
            SpecificationCollection = 
                db.GetCollection<Entities.Shop.ProductSpecification.ProductSpecification>("ProductSpecification");
            SpecGroupCollection =
                db.GetCollection<Entities.Shop.ProductSpecificationGroup.ProductSpecGroup>("ProductSpecGroup");
            ProductUnitCollection =
                db.GetCollection<Entities.Shop.ProductUnit.ProductUnit>("ProductUnit");

            BsonProductCollection = db.GetCollection<BsonDocument>("Product");
            BsonProductGroupCollection =
                db.GetCollection<BsonDocument>("ProductGroup");
            BsonSpecificationCollection =
                db.GetCollection<BsonDocument>("ProductSpecification");
            BsonSpecGroupCollection =
                db.GetCollection<BsonDocument>("ProductSpecGroup");
            BsonProductUnitCollection =
                db.GetCollection<BsonDocument>("ProductUnit");
            DownloadLimitationCollection = db.GetCollection<Entities.General.User.DownloadLimitation>("DownloadLimitations");
            BsonDownloadLimitationCollection = db.GetCollection<BsonDocument>("DownloadLimitations");
        }
    }
}
