using Arad.Portal.DataLayer.Entities.General.City;
using Arad.Portal.DataLayer.Entities.General.County;
using Arad.Portal.DataLayer.Entities.General.District;
using Arad.Portal.DataLayer.Entities.General.State;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.General.Role.Mongo
{
    public class RoleContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.Role.Role> Collection;
        private readonly IConfiguration _configuration;

        public RoleContext(IConfiguration configuration)
        {
            _configuration = configuration;
             client = new MongoClient(_configuration["Database:ConnectionString"]);
            db = client.GetDatabase(_configuration["Database:DbName"]);
            Collection = db.GetCollection<Entities.General.Role.Role>("Role");
        }

        public IMongoCollection<State> States => db.GetCollection<State>("States");
        public IMongoCollection<City> Cities => db.GetCollection<City>("Cities");
        public IMongoCollection<District> Districts => db.GetCollection<District>("Districts");
        public IMongoCollection<County> Counties => db.GetCollection<County>("Counties");
    }

}
