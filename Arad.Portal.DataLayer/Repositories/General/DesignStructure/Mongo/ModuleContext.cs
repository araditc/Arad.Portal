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
        public IMongoCollection<Entities.General.DesignStructure.Module> ModuleCollection;
        public IMongoCollection<BsonDocument> BsonModuleCollection;
        //public IMongoCollection<Entities.General.DesignStructure.Template> TemplateCollection;
        //public IMongoCollection<BsonDocument> BsonTemplateCollection;
        private readonly IConfiguration _configuration;

        public ModuleContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["DatabaseConfig:ConnectionString"]);
            db = client.GetDatabase(_configuration["DatabaseConfig:DbName"]);
            ModuleCollection = db.GetCollection<Entities.General.DesignStructure.Module>("Module");
            BsonModuleCollection = db.GetCollection<BsonDocument>("Module");
            //TemplateCollection = db.GetCollection<Entities.General.DesignStructure.Template>("Template");
            //BsonTemplateCollection = db.GetCollection<BsonDocument>("Template");
        }
    }
}
