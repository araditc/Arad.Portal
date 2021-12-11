﻿using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.General.Email.Mongo
{
    public class SMTPContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.Email.SMTP> Collection;
        public IMongoCollection<BsonDocument> BsonCollection;
        private readonly IConfiguration _configuration;

        public SMTPContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["DatabaseConfig:ConnectionString"]);
            db = client.GetDatabase(_configuration["DatabaseConfig:DbName"]);
            Collection = db.GetCollection<Entities.General.Email.SMTP>("SMTP");
            BsonCollection = db.GetCollection<BsonDocument>("SMTP");
        }
    }
}
