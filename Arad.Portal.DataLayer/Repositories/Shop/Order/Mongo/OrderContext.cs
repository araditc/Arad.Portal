using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.Shop.Order.Mongo
{
    //public class OrderContext
    //{
    //    private readonly MongoClient client;
    //    private readonly IMongoDatabase db;
    //    public IMongoCollection<Entities.Shop.Order.Order> Collection;
    //    public IMongoCollection<BsonDocument> BsonCollection;
    //    private readonly IConfiguration _configuration;

    //    public OrderContext(IConfiguration configuration)
    //    {
    //        _configuration = configuration;
    //         client = new MongoClient(_configuration["DatabaseConfig:ConnectionString"]);
    //       db = client.GetDatabase(_configuration["DatabaseConfig:DbName"]);
    //        Collection = db.GetCollection<Entities.Shop.Order.Order>("Order");
    //        BsonCollection = db.GetCollection<BsonDocument>("Order");
    //    }
    //}
}
