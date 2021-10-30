using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.General.BasicData.Mongo
{
    public class BasicDataContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.BasicData.BasicData> Collection;
        private readonly IConfiguration _configuration;

        public BasicDataContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["Database:ConnectionString"]);
            db = client.GetDatabase(_configuration["Database:DbName"]);
            Collection = db.GetCollection<Entities.General.BasicData.BasicData>("BasicData");
        }
    }
}
