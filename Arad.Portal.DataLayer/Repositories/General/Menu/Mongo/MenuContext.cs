using Microsoft.Extensions.Configuration;
using MongoDB.Driver;


namespace Arad.Portal.DataLayer.Repositories.General.Menu.Mongo
{
    public class MenuContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.Menu.Menu> Collection;
        private readonly IConfiguration _configuration;

        public MenuContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["Database:ConnectionString"]);
            db = client.GetDatabase(_configuration["Database:DbName"]);
            Collection = db.GetCollection<Entities.General.Menu.Menu>("Menu");
        }
    }
}
