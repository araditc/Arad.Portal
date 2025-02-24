using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.MongoDbContext;

public class MongoDbContext : IMongoDbContext
{
    public IMongoClient Client { get; }

    public IMongoDatabase Database { get; }

    public string GetCollectionName<TEntity>()
    {
        return CollectionNameFinder.GetCollectionName<TEntity>();
    }

    #region Ctor
    //static MongoDbContext()
    //{
    //    ConventionPack pack = new() { new IgnoreEmptyArraysConvention() };
    //    ConventionRegistry.Register("Do not serialize empty lists", pack, _ => true);
    //}

    public MongoDbContext(IMongoDatabase mongoDatabase)
    {
        Database = mongoDatabase;
        Client = mongoDatabase.Client;
    }

    public MongoDbContext(IMongoClient client, string databaseName)
    {
        Client = client;
        Database = client.GetDatabase(databaseName);
    }

    public MongoDbContext(string connectionString, string databaseName)
    {
        Client = new MongoClient(connectionString);
        Database = Client.GetDatabase(databaseName);
    }

    public MongoDbContext(string connectionString)
        : this(connectionString, new MongoUrl(connectionString).DatabaseName)
    {
    }
    #endregion

    #region Collection
    public void DropCollection<TEntity>()
    {
        Database.DropCollection(GetCollectionName<TEntity>());
    }

    public void DropCollection<TEntity>(string collectionName)
    {
        Database.DropCollection(collectionName);
    }

    public Task DropCollectionAsync<TEntity>(CancellationToken cancellationToken = default)
    {
        return Database.DropCollectionAsync(GetCollectionName<TEntity>(), cancellationToken);
    }

    public Task DropCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        return Database.DropCollectionAsync(collectionName, cancellationToken);
    }

    public IMongoCollection<TEntity> GetCollection<TEntity>(MongoCollectionSettings settings = null)
    {
        return Database.GetCollection<TEntity>(GetCollectionName<TEntity>(), settings);
    }

    public IMongoCollection<TEntity> GetCollection<TEntity>(string collectionName, MongoCollectionSettings settings = null)
    {
        return Database.GetCollection<TEntity>(collectionName, settings);
    }

    public void RenameCollection(string oldName, string newName, RenameCollectionOptions options = null)
    {
        Database.RenameCollection(oldName, newName, options);
    }

    public Task RenameCollectionAsync(string oldName, string newName, RenameCollectionOptions options = null, CancellationToken cancellationToken = default)
    {
        return Database.RenameCollectionAsync(oldName, newName, options, cancellationToken);
    }
    #endregion


    public List<string> GetOutboxArchiveCollectionNames(string dbName, string connectionString)
    {
        MongoClientSettings settings = MongoClientSettings.FromConnectionString(connectionString);
        IMongoClient archiveClient = new MongoClient(settings);
        IMongoDatabase archiveDatabase = archiveClient.GetDatabase(dbName);

        return archiveDatabase != null ? archiveDatabase.ListCollectionNames().ToList().Where(a => a.ToUpper().StartsWith("OutboxArchive".ToUpper())).OrderByDescending(a => a).ToList() : [];
    }

    //public IMongoCollection<AradMessage> GetOutboxArchive(string archiveCollectionName, string dbName, string connectionString)
    //{
    //    MongoClientSettings settings = MongoClientSettings.FromConnectionString(connectionString);
    //    IMongoClient client = new MongoClient(settings);
    //    IMongoDatabase archiveDatabase = client.GetDatabase(dbName);

    //    if (archiveDatabase != null)
    //    {
    //        return string.IsNullOrWhiteSpace(archiveCollectionName) ? null : archiveDatabase.GetCollection<AradMessage>(archiveCollectionName);
    //    }

    //    return null;
    //}
}