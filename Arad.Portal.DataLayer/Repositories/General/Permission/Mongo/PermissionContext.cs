using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.General.Permission
{
    public class PermissionContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.Permission.Permission> Collection;
        private readonly IConfiguration _configuration;

        public PermissionContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["DB:ConnectionString"]);
            db = client.GetDatabase(_configuration["DB:DbName"]);
            Collection = db.GetCollection<Entities.General.Permission.Permission>("Permission");
        }
    }
}
