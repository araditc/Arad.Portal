
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.General.Role.Mongo
{
    public class RoleContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.Role.Role> Collection;
        public IMongoCollection<BsonDocument> BsonCollection;
        private readonly IConfiguration _configuration;

        public RoleContext(IConfiguration configuration)
        {
            _configuration = configuration;
             client = new MongoClient(_configuration["DatabaseConfig:ConnectionString"]);
            db = client.GetDatabase(_configuration["DatabaseConfig:DbName"]);
            Collection = db.GetCollection<Entities.General.Role.Role>("Role");
            BsonCollection = db.GetCollection<BsonDocument>("Role");
        }
    }

}
