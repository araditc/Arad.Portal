using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.General.Domain.Mongo
{
    public class DomainContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.Domain.Domain> Collection;
        public IMongoCollection<BsonDocument> BsonCollection;
        private readonly IConfiguration _configuration;

        public DomainContext(IConfiguration configuration)
        {
            _configuration = configuration;
             client = new MongoClient(_configuration["Database:ConnectionString"]);
            db = client.GetDatabase(_configuration["Database:DbName"]);
            Collection = db.GetCollection<Entities.General.Domain.Domain>("Domain");
            BsonCollection  = db.GetCollection<BsonDocument>("Domain");
        }
    }
}
