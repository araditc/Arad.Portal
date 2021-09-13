using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.General.Comment.Mongo
{
    public class CommentContext
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        public IMongoCollection<Entities.General.Comment.Comment> Collection;
        private readonly IConfiguration _configuration;

        public CommentContext(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new MongoClient(_configuration["Database:ConnectionString"]);
            db = client.GetDatabase(_configuration["Database:DbName"]);
            Collection = db.GetCollection<Entities.General.Comment.Comment>("Comment");
        }
    }
}
