using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.Shop.ShoppingCart.Mongo
{
    public class ShoppingCartContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.Shop.ShoppingCart.ShoppingCart> Collection;
        public IMongoCollection<BsonDocument> BsonCollection;
        private readonly IConfiguration _configuration;

        public ShoppingCartContext(IConfiguration configuration)
        {
            _configuration = configuration;
             client = new MongoClient(_configuration["Database:ConnectionString"]);
           db = client.GetDatabase(_configuration["Database:DbName"]);
            Collection = db.GetCollection<Entities.Shop.ShoppingCart.ShoppingCart>("ShoppingCart");
            BsonCollection = db.GetCollection<BsonDocument>("ShoppingCart");
        }
    }
}
