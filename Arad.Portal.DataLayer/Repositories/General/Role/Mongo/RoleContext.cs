using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.General.Role
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
            client = new MongoClient(_configuration["DB:ConnectionString"]);
            db = client.GetDatabase(_configuration["DB:DbName"]);
            Collection = db.GetCollection<Entities.General.Role.Role>("Role");
        }
    }
}
