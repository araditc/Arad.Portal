using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;


namespace Arad.Portal.DataLayer.Repositories.General.Menu.Mongo
{
    public class MenuContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.Menu.Menu> Collection;
        private readonly IConfiguration _configuration;
        public IMongoCollection<BsonDocument> BsonCollection;
        public MenuContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["DatabaseConfig:ConnectionString"]);
            db = client.GetDatabase(_configuration["DatabaseConfig:DbName"]);
            BsonCollection = db.GetCollection<BsonDocument>("Menu");
            Collection = db.GetCollection<Entities.General.Menu.Menu>("Menu");
        }
    }
}
