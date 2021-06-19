using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.General.User
{
    public class UserContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.User.ApplicationUser> Collection;
        private readonly IConfiguration _configuration;

        public UserContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["DB:ConnectionString"]);
            db = client.GetDatabase(_configuration["DB:DbName"]);
            Collection = db.GetCollection<Entities.General.User.ApplicationUser>("User");
        }
    }
}
