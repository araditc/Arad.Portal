using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.General.Currency.Mongo
{
    public class CurrencyContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.Currency.Currency> Collection;
        private readonly IConfiguration _configuration;

        public CurrencyContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["DB:ConnectionString"]);
            db = client.GetDatabase(_configuration["DB:DbName"]);
            Collection = db.GetCollection<Entities.General.Currency.Currency>("Currency");
        }
    }
}
