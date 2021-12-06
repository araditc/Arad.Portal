using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.General.SystemSetting.Mongo
{
    public class SystemSettingContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.SystemSetting.SystemSetting> Collection;
        public IMongoCollection<BsonDocument> BsonCollection;
        private readonly IConfiguration _configuration;

        public SystemSettingContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["Database:ConnectionString"]);
            db = client.GetDatabase(_configuration["Database:DbName"]);
            Collection = db.GetCollection<Entities.General.SystemSetting.SystemSetting>("SystemSetting");
            BsonCollection = db.GetCollection<BsonDocument>("SystemSetting");
        }
    }
}
