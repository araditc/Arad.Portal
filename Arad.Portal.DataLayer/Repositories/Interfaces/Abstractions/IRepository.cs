using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.Interfaces.Abstractions;

public interface IRepository<TEntity>
    where TEntity : class, IEntity
{
    MongoDbContext.IMongoDbContext DbContext { get; }

    Paging<TEntity> GetPage(Expression<Func<TEntity, bool>> predicate, string queryString);

    Paging<TEntity> GetPage(BsonDocument predicate, string queryString);

    Paging<TEntity> GetPageAscending(Expression<Func<TEntity, bool>> predicate, string queryString, Expression<Func<TEntity, object>> sort);

    Paging<TEntity> GetPageAscending(BsonDocument predicate, string queryString, Expression<Func<TEntity, object>> sort);

    Paging<TEntity> GetPageDescending(Expression<Func<TEntity, bool>> predicate, string queryString, Expression<Func<TEntity, object>> sort);

    Paging<TEntity> GetPageDescending(BsonDocument predicate, string queryString, Expression<Func<TEntity, object>> sort);

    List<TEntity> GetAll();

    ValueTask<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    ValueTask<IAsyncCursor<TEntity>> GetCursorAsync(Expression<Func<TEntity, bool>> predicate, FindOptions<TEntity, TEntity> findOptions);

    TEntity GetById(string id);

    ValueTask<TEntity> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    List<TEntity> GetList(Expression<Func<TEntity, bool>> predicate);

    List<TEntity> GetList(Expression<Func<TEntity, bool>> predicate, int take);

    List<TEntity> GetListAscending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort);

    List<TEntity> GetListAscending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, int take);

    List<TEntity> GetListDescending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort);

    List<TEntity> GetListDescending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, int take);

    ValueTask<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    ValueTask<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, int take, CancellationToken cancellationToken = default);

    ValueTask<List<TEntity>> GetListAscendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default);

    ValueTask<List<TEntity>> GetListAscendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, int take, CancellationToken cancellationToken = default);

    ValueTask<List<TEntity>> GetListDescendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default);

    ValueTask<List<TEntity>> GetListDescendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, int take, CancellationToken cancellationToken = default);



    bool Any();

    bool Any(Expression<Func<TEntity, bool>> predicate);

    ValueTask<bool> AnyAsync(CancellationToken cancellationToken = default);

    ValueTask<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);


    long GetCount();

    long GetCount(Expression<Func<TEntity, bool>> predicate);

    ValueTask<long> GetCountAsync(CancellationToken cancellationToken = default);

    ValueTask<long> GetCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);


    TEntity First();

    TEntity FirstByAscending(Expression<Func<TEntity, object>> sort);

    TEntity FirstByDescending(Expression<Func<TEntity, object>> sort);

    TEntity First(Expression<Func<TEntity, bool>> predicate);

    TEntity FirstByAscending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort);

    TEntity FirstByDescending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort);

    ValueTask<TEntity> FirstAsync(CancellationToken cancellationToken = default);

    ValueTask<TEntity> FirstByAscendingAsync(Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default);

    ValueTask<TEntity> FirstByDescendingAsync(Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default);

    ValueTask<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    ValueTask<TEntity> FirstByAscendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default);

    ValueTask<TEntity> FirstByDescendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default);



    TEntity FirstOrDefault();

    TEntity FirstOrDefaultByAscending(Expression<Func<TEntity, object>> sort);

    TEntity FirstOrDefaultByDescending(Expression<Func<TEntity, object>> sort);

    TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

    TEntity FirstOrDefaultByAscending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort);

    TEntity FirstOrDefaultByDescending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort);

    ValueTask<TEntity> FirstOrDefaultAsync(CancellationToken cancellationToken = default);

    ValueTask<TEntity> FirstOrDefaultByAscendingAsync(Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default);

    ValueTask<TEntity> FirstOrDefaultByDescendingAsync(Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default);

    ValueTask<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    ValueTask<TEntity> FirstOrDefaultByAscendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default);

    ValueTask<TEntity> FirstOrDefaultByDescendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default);


    ValueTask<Result<TEntity>> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    ValueTask<Result<TEntity>> InsertAsync(List<TEntity> entities, CancellationToken cancellationToken = default);


    ValueTask<Result<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    ValueTask<Result<TEntity>> UpdateAsync(List<TEntity> entities, CancellationToken cancellationToken = default);

    ValueTask<Result<TEntity>> UpdateAsync(TEntity entity, UpdateDefinition<TEntity> update, CancellationToken cancellationToken = default);

    ValueTask<Result<TEntity>> UpdateAsync(string id, UpdateDefinition<TEntity> update, CancellationToken cancellationToken = default);

    ValueTask<Result<TEntity>> UpdateAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> field, TProperty value, CancellationToken cancellationToken = default);

    ValueTask<Result<TEntity>> UpdateAsync(Expression<Func<TEntity, bool>> predicate, UpdateDefinition<TEntity> update, CancellationToken cancellationToken = default);

    ValueTask<Result<TEntity>> UpdateAsync<TProperty>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TProperty>> field, TProperty value, CancellationToken cancellationToken = default);


    ValueTask<Result<TEntity>> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    ValueTask<Result<TEntity>> DeleteAsync(List<TEntity> entities, CancellationToken cancellationToken = default);

    ValueTask<Result<TEntity>> DeleteAsync(string id, CancellationToken cancellationToken = default);

    ValueTask<Result<TEntity>> DeleteAsync(List<string> keys, CancellationToken cancellationToken = default);

    ValueTask<Result<TEntity>> DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);


    ValueTask<BulkWriteResult<TEntity>> DoBulkOperation(List<WriteModel<TEntity>> writeModels);

    ValueTask<BulkWriteResult<TEntity>> DoBulkOperation(List<WriteModel<TEntity>> writeModels, BulkWriteOptions options);

    ValueTask<BulkWriteResult<TEntity>> DoBulkOperation(List<WriteModel<TEntity>> writeModels, CancellationToken cancellationToken);

    ValueTask<BulkWriteResult<TEntity>> DoBulkOperation(List<WriteModel<TEntity>> writeModels, BulkWriteOptions options, CancellationToken cancellationToken);

    ValueTask WithBulkOperation(Func<Task> func);

    ValueTask WithBulkOperation(Func<Task> func, BulkWriteOptions options);

    ValueTask WithBulkOperation(Func<Task> func, CancellationToken cancellationToken);

    ValueTask WithBulkOperation(Func<Task> func, BulkWriteOptions options, CancellationToken cancellationToken);



    ValueTask<string> CreateIndexAsync(Expression<Func<TEntity, object>> field, bool descending, bool unique, bool sparse = false, string name = null, CancellationToken cancellationToken = default);

    ValueTask<List<string>> GetIndexesNamesAsync(CancellationToken cancellationToken = default);

    ValueTask<bool> IndexExistsAsync(string name, CancellationToken cancellationToken = default);

    ValueTask<string> CreateTextIndexAsync(Expression<Func<TEntity, object>> field, CreateIndexOptions options = null, CancellationToken cancellationToken = default);

    ValueTask<string> CreateAscendingIndexAsync(Expression<Func<TEntity, object>> field, CreateIndexOptions options = null, CancellationToken cancellationToken = default);

    ValueTask<string> CreateDescendingIndexAsync(Expression<Func<TEntity, object>> field, CreateIndexOptions options = null, CancellationToken cancellationToken = default);

    ValueTask<string> CreateHashedIndexAsync(Expression<Func<TEntity, object>> field, CreateIndexOptions options = null, CancellationToken cancellationToken = default);

    ValueTask<string> CreateCombinedTextIndexAsync(IEnumerable<Expression<Func<TEntity, object>>> fields, CreateIndexOptions options = null, CancellationToken cancellationToken = default);

    ValueTask<string> CreateCombinedAscendingIndexAsync(IEnumerable<Expression<Func<TEntity, object>>> fields, CreateIndexOptions options = null, CancellationToken cancellationToken = default);

    ValueTask<string> CreateCombinedDescendingIndexAsync(IEnumerable<Expression<Func<TEntity, object>>> fields, CreateIndexOptions options = null, CancellationToken cancellationToken = default);

    ValueTask<string> CreateCombinedHashedIndexAsync(IEnumerable<Expression<Func<TEntity, object>>> fields, CreateIndexOptions options = null, CancellationToken cancellationToken = default);

    Task DropIndexAsync(string indexName, CancellationToken cancellationToken = default);
  
}