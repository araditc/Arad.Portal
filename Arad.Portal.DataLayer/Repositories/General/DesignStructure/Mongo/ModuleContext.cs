using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.General.DesignStructure.Mongo
{
    public class ModuleContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.DesignStructure.Module> Collection;
        public IMongoCollection<BsonDocument> BsonCollection;
        public IMongoCollection<Entities.General.DesignStructure.Template> TemplateCollection;
        public IMongoCollection<BsonDocument> BsonTemplateCollectionCollection;
        private readonly IConfiguration _configuration;

        public ModuleContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["DatabaseConfig:ConnectionString"]);
            db = client.GetDatabase(_configuration["DatabaseConfig:DbName"]);
            Collection = db.GetCollection<Entities.General.DesignStructure.Module>("Module");
            BsonCollection = db.GetCollection<BsonDocument>("Module");
            TemplateCollection = db.GetCollection<Entities.General.DesignStructure.Template>("Template");
            BsonTemplateCollectionCollection = db.GetCollection<BsonDocument>("Template");
        }
    }
}
