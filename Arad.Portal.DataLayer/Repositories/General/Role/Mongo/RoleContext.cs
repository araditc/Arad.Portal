using Arad.Portal.DataLayer.Entities.General.City;
using Arad.Portal.DataLayer.Entities.General.County;
using Arad.Portal.DataLayer.Entities.General.District;
using Arad.Portal.DataLayer.Entities.General.State;
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
             client = new MongoClient(_configuration["Database:ConnectionString"]);
            db = client.GetDatabase(_configuration["Database:DbName"]);
            Collection = db.GetCollection<Entities.General.Role.Role>("Role");
            BsonCollection = db.GetCollection<BsonDocument>("Role");
        }

        public IMongoCollection<State> States => db.GetCollection<State>("States");
        public IMongoCollection<BsonDocument> BsonStates => db.GetCollection<BsonDocument>("States");
        public IMongoCollection<City> Cities => db.GetCollection<City>("Cities");
        public IMongoCollection<BsonDocument> BsonCities => db.GetCollection<BsonDocument>("Cities");
        public IMongoCollection<District> Districts => db.GetCollection<District>("Districts");
        public IMongoCollection<BsonDocument> BsonDistricts => db.GetCollection<BsonDocument>("Districts");
        public IMongoCollection<County> Counties => db.GetCollection<County>("Counties");
        public IMongoCollection<BsonDocument> BsonCounties => db.GetCollection<BsonDocument>("Counties");
    }

}
