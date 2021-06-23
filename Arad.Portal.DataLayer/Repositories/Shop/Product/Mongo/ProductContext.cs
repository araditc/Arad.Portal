using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public ProductContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["DB:ConnectionString"]);
            db = client.GetDatabase(_configuration["DB:DbName"]);

            ProductCollection = db.GetCollection<Entities.Shop.Product.Product>("Product");
            ProductGroupCollection = 
                db.GetCollection<Entities.Shop.ProductGroup.ProductGroup>("ProductGroup");
            SpecificationCollection = 
                db.GetCollection<Entities.Shop.ProductSpecification.ProductSpecification>("ProductSpecification");
            SpecGroupCollection =
                db.GetCollection<Entities.Shop.ProductSpecificationGroup.ProductSpecGroup>("ProductSpecGroup");
            ProductUnitCollection =
                db.GetCollection<Entities.Shop.ProductUnit.ProductUnit>("ProductUnit");
        }
    }
}
