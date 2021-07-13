
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.General.MessageTemplate.Mongo
{
    public class MessageTemplateContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.MessageTemplate.MessageTemplate> Collection;
        private readonly IConfiguration _configuration;

        public MessageTemplateContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["Database:ConnectionString"]);
            db = client.GetDatabase(_configuration["Database:DbName"]);
            Collection = db.GetCollection<Entities.General.MessageTemplate.MessageTemplate>("MessageTemplate");
        }
    }
}
