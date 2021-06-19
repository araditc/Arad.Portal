using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.General.Language
{
    public class LanguageContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.Language.Language> Collection;
        private readonly IConfiguration _configuration;

        public LanguageContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["DB:ConnectionString"]);
            db = client.GetDatabase(_configuration["DB:DbName"]);
            Collection = db.GetCollection<Entities.General.Language.Language>("Language");
        }
    }
}
