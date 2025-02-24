using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.MongoDbContext;

public interface IMongoDbContext
{
    IMongoClient Client { get; }

    IMongoDatabase Database { get; }

    string GetCollectionName<TEntity>();

    void DropCollection<TEntity>();

    void DropCollection<TEntity>(string collectionName);

    Task DropCollectionAsync<TEntity>(CancellationToken cancellationToken = default);

    Task DropCollectionAsync(string collectionName, CancellationToken cancellationToken = default);

    IMongoCollection<TEntity> GetCollection<TEntity>(MongoCollectionSettings settings = null);

    IMongoCollection<TEntity> GetCollection<TEntity>(string collectionName, MongoCollectionSettings settings = null);

    void RenameCollection(string oldName, string newName, RenameCollectionOptions options = null);

    Task RenameCollectionAsync(string oldName, string newName, RenameCollectionOptions options = null, CancellationToken cancellationToken = default);

    List<string> GetOutboxArchiveCollectionNames(string dbName, string connectionString);

    //IMongoCollection<AradMessage> GetOutboxArchive(string archiveCollectionName, string dbName, string connectionString);
}