using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.Shop.Transaction.Mongo
{
    public class TransactionContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.Shop.Transaction.Transaction> Collection;
        public IMongoCollection<BsonDocument> BsonCollection;
        private readonly IConfiguration _configuration;

        public TransactionContext(IConfiguration configuration)
        {
            _configuration = configuration;
             client = new MongoClient(_configuration["DatabaseConfig:ConnectionString"]);
           db = client.GetDatabase(_configuration["DatabaseConfig:DbName"]);
            Collection = db.GetCollection<Entities.Shop.Transaction.Transaction>("Transaction");
            BsonCollection = db.GetCollection<BsonDocument>("Transaction");
        }
    }
}
