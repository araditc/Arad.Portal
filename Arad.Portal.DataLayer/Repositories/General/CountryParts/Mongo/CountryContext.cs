using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.General.CountryParts.Mongo
{
    public class CountryContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.Country.Country> CountryCollection;
        public IMongoCollection<BsonDocument> BsonCountryCollection;

        public IMongoCollection<Entities.General.State.State> StateCollection;
        public IMongoCollection<BsonDocument> BsonStateCollection;

        public IMongoCollection<Entities.General.City.City> CityCollection;
        public IMongoCollection<BsonDocument> BsonCityCollection;


        private readonly IConfiguration _configuration;

        public CountryContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["DatabaseConfig:ConnectionString"]);
            db = client.GetDatabase(_configuration["DatabaseConfig:DbName"]);

            CountryCollection = db.GetCollection<Entities.General.Country.Country>("Country");
            BsonCountryCollection = db.GetCollection<BsonDocument>("Country");

            StateCollection = db.GetCollection<Entities.General.State.State>("State");
            BsonStateCollection = db.GetCollection<BsonDocument>("State");

            CityCollection = db.GetCollection<Entities.General.City.City>("City");
            BsonCityCollection = db.GetCollection<BsonDocument>("Country");
        }
    }
}
