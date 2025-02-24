using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Arad.Portal.GeneralLibrary.Utilities;

namespace Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;

public class Repository<TEntity>(IMongoDbContext dbContext) : IRepository<TEntity>
    where TEntity : class, IEntity
{
    public IMongoDbContext DbContext { get; } = dbContext;

    public IMongoCollection<TEntity> Collection { get; } = dbContext.GetCollection<TEntity>();

    public virtual Paging<TEntity> GetPage(Expression<Func<TEntity, bool>> predicate, string queryString)
    {
        try
        {
            int pageSize = 10;
            int page = 1;

            NameValueCollection queryParams = HttpUtility.ParseQueryString(queryString);

            if (!string.IsNullOrWhiteSpace(queryParams["page"]))
            {
                page = Convert.ToInt32(queryParams["page"]);
            }

            if (!string.IsNullOrWhiteSpace(queryParams["pageSize"]))
            {
                pageSize = Convert.ToInt32(queryParams["pageSize"]);
            }

            List<TEntity> list = Collection.Find(predicate).Skip((page - 1) * pageSize).Limit(pageSize).ToList();

            return new() { CurrentPage = page, Items = list, PageSize = pageSize, QueryString = queryString };
        }
        catch (Exception ex)
        {
            Log.Error($"GetPage error : {ex.Message}");
            return new() { QueryString = queryString, CurrentPage = 1, Items = [], PageSize = 10 };
        }
    }

    public Paging<TEntity> GetPage(BsonDocument predicate, string queryString)
    {
        try
        {
            int pageSize = 10;
            int page = 1;

            NameValueCollection queryParams = HttpUtility.ParseQueryString(queryString);

            if (!string.IsNullOrWhiteSpace(queryParams["page"]))
            {
                page = Convert.ToInt32(queryParams["page"]);
            }

            if (!string.IsNullOrWhiteSpace(queryParams["pageSize"]))
            {
                pageSize = Convert.ToInt32(queryParams["pageSize"]);
            }

            List<TEntity> list = Collection.Find(predicate).Skip((page - 1) * pageSize).Limit(pageSize).ToList();

            return new() { CurrentPage = page, Items = list, PageSize = pageSize, QueryString = queryString };
        }
        catch (Exception ex)
        {
            Log.Error($"GetPage error : {ex.Message}");
            return new() { QueryString = queryString, CurrentPage = 1, Items = [], PageSize = 10 };
        }
    }

    public Paging<TEntity> GetPageAscending(Expression<Func<TEntity, bool>> predicate, string queryString, Expression<Func<TEntity, object>> sort)
    {
        try
        {
            int pageSize = 10;
            int page = 1;

            NameValueCollection queryParams = HttpUtility.ParseQueryString(queryString);

            if (!string.IsNullOrWhiteSpace(queryParams["page"]))
            {
                page = Convert.ToInt32(queryParams["page"]);
            }

            if (!string.IsNullOrWhiteSpace(queryParams["pageSize"]))
            {
                pageSize = Convert.ToInt32(queryParams["pageSize"]);
            }

            List<TEntity> list = Collection.Find(predicate).SortBy(sort).Skip((page - 1) * pageSize).Limit(pageSize).ToList();

            return new() { CurrentPage = page, Items = list, PageSize = pageSize, QueryString = queryString };
        }
        catch (Exception ex)
        {
            Log.Error($"GetPage error : {ex.Message}");
            return new() { QueryString = queryString, CurrentPage = 1, Items = [], PageSize = 10 };
        }
    }

    public Paging<TEntity> GetPageAscending(BsonDocument predicate, string queryString, Expression<Func<TEntity, object>> sort)
    {
        try
        {
            int pageSize = 10;
            int page = 1;

            NameValueCollection queryParams = HttpUtility.ParseQueryString(queryString);

            if (!string.IsNullOrWhiteSpace(queryParams["page"]))
            {
                page = Convert.ToInt32(queryParams["page"]);
            }

            if (!string.IsNullOrWhiteSpace(queryParams["pageSize"]))
            {
                pageSize = Convert.ToInt32(queryParams["pageSize"]);
            }

            List<TEntity> list = Collection.Find(predicate).SortBy(sort).Skip((page - 1) * pageSize).Limit(pageSize).ToList();

            return new() { CurrentPage = page, Items = list, PageSize = pageSize, QueryString = queryString };
        }
        catch (Exception ex)
        {
            Log.Error($"GetPage error : {ex.Message}");
            return new() { QueryString = queryString, CurrentPage = 1, Items = [], PageSize = 10 };
        }
    }

    public Paging<TEntity> GetPageDescending(Expression<Func<TEntity, bool>> predicate, string queryString, Expression<Func<TEntity, object>> sort)
    {
        try
        {
            int pageSize = 10;
            int page = 1;

            NameValueCollection queryParams = HttpUtility.ParseQueryString(queryString);

            if (!string.IsNullOrWhiteSpace(queryParams["page"]))
            {
                page = Convert.ToInt32(queryParams["page"]);
            }

            if (!string.IsNullOrWhiteSpace(queryParams["pageSize"]))
            {
                pageSize = Convert.ToInt32(queryParams["pageSize"]);
            }

            List<TEntity> list = Collection.Find(predicate).SortByDescending(sort).Skip((page - 1) * pageSize).Limit(pageSize).ToList();

            return new() { CurrentPage = page, Items = list, PageSize = pageSize, QueryString = queryString };
        }
        catch (Exception ex)
        {
            Log.Error($"GetPage error : {ex.Message}");
            return new() { QueryString = queryString, CurrentPage = 1, Items = [], PageSize = 10 };
        }
    }

    public Paging<TEntity> GetPageDescending(BsonDocument predicate, string queryString, Expression<Func<TEntity, object>> sort)
    {
        try
        {
            int pageSize = 10;
            int page = 1;

            NameValueCollection queryParams = HttpUtility.ParseQueryString(queryString);

            if (!string.IsNullOrWhiteSpace(queryParams["page"]))
            {
                page = Convert.ToInt32(queryParams["page"]);
            }

            if (!string.IsNullOrWhiteSpace(queryParams["pageSize"]))
            {
                pageSize = Convert.ToInt32(queryParams["pageSize"]);
            }

            List<TEntity> list = Collection.Find(predicate).SortByDescending(sort).Skip((page - 1) * pageSize).Limit(pageSize).ToList();

            return new() { CurrentPage = page, Items = list, PageSize = pageSize, QueryString = queryString };
        }
        catch (Exception ex)
        {
            Log.Error($"GetPage error : {ex.Message}");
            return new() { QueryString = queryString, CurrentPage = 1, Items = [], PageSize = 10 };
        }
    }

    public virtual List<TEntity> GetAll()
    {
        return Collection.Find(_ => true).ToList();
    }

    public virtual async ValueTask<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public virtual async ValueTask<IAsyncCursor<TEntity>> GetCursorAsync(Expression<Func<TEntity, bool>> predicate, FindOptions<TEntity, TEntity> findOptions)
    {
        return await Collection.FindAsync(predicate, findOptions);
    }

    //public virtual async ValueTask<IAsyncCursor<TEntity>> RandomListAsync(Expression<Func<TEntity, bool>> predicate,int count)
    //{
    //    return await Collection.AggregateAsync(predicate);
    //}



    public virtual TEntity GetById(string id)
    {
        return FirstOrDefault(f => f.Id == id);
    }

    public virtual async ValueTask<TEntity> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public virtual List<TEntity> GetList(Expression<Func<TEntity, bool>> predicate)
    {
        return Collection.Find(predicate).ToList();
    }

    public virtual List<TEntity> GetList(Expression<Func<TEntity, bool>> predicate, int take)
    {
        return Collection.Find(predicate).Limit(take).ToList();
    }

    public virtual List<TEntity> GetListAscending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort)
    {
        return Collection.Find(predicate).SortBy(sort).ToList();
    }

    public virtual List<TEntity> GetListAscending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, int take)
    {
        return Collection.Find(predicate).SortBy(sort).Limit(take).ToList();
    }

    public virtual List<TEntity> GetListDescending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort)
    {
        return Collection.Find(predicate).SortByDescending(sort).ToList();
    }

    public virtual List<TEntity> GetListDescending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, int take)
    {
        return Collection.Find(predicate).SortByDescending(sort).Limit(take).ToList();
    }

    public virtual async ValueTask<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await Collection.Find(predicate).ToListAsync(cancellationToken);
    }

    public virtual async ValueTask<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, int take, CancellationToken cancellationToken)
    {
        return await Collection.Find(predicate).Limit(take).ToListAsync(cancellationToken);
    }

    public virtual async ValueTask<List<TEntity>> GetListAscendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken)
    {
        return await Collection.Find(predicate).SortBy(sort).ToListAsync(cancellationToken);
    }

    public virtual async ValueTask<List<TEntity>> GetListAscendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, int take, CancellationToken cancellationToken)
    {
        return await Collection.Find(predicate).SortBy(sort).Limit(take).ToListAsync(cancellationToken);
    }

    public virtual async ValueTask<List<TEntity>> GetListDescendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken)
    {
        return await Collection.Find(predicate).SortByDescending(sort).ToListAsync(cancellationToken);
    }

    public virtual async ValueTask<List<TEntity>> GetListDescendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, int take, CancellationToken cancellationToken)
    {
        return await Collection.Find(predicate).SortByDescending(sort).Limit(take).ToListAsync(cancellationToken);
    }


    public virtual bool Any()
    {
        return Collection.Find(entity => true).Any();
    }

    public virtual bool Any(Expression<Func<TEntity, bool>> predicate)
    {
        return Collection.Find(predicate).Any();
    }

    public virtual async ValueTask<bool> AnyAsync(CancellationToken cancellationToken)
    {
        return await Collection.Find(entity => true).AnyAsync(cancellationToken);
    }

    public virtual async ValueTask<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await Collection.Find(predicate).AnyAsync(cancellationToken);
    }


    public virtual long GetCount()
    {
        return Collection.Find(entity => true).CountDocuments();
    }

    public virtual long GetCount(Expression<Func<TEntity, bool>> predicate)
    {
        return Collection.Find(predicate).CountDocuments();
    }

    public virtual async ValueTask<long> GetCountAsync(CancellationToken cancellationToken)
    {
        return await Collection.Find(entity => true).CountDocumentsAsync(cancellationToken);
    }

    public virtual async ValueTask<long> GetCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await Collection.Find(predicate).CountDocumentsAsync(cancellationToken);
    }


    public virtual TEntity First()
    {
        return First(_ => true);
    }

    public virtual TEntity FirstByAscending(Expression<Func<TEntity, object>> sort)
    {
        return FirstByAscending(_ => true, sort);
    }

    public virtual TEntity FirstByDescending(Expression<Func<TEntity, object>> sort)
    {
        return FirstByDescending(_ => true, sort);
    }

    public virtual TEntity First(Expression<Func<TEntity, bool>> predicate)
    {
        return Collection.Find(predicate).First();
    }

    public virtual TEntity FirstByAscending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort)
    {
        return Collection.Find(predicate).SortBy(sort).First();
    }

    public virtual TEntity FirstByDescending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort)
    {
        return Collection.Find(predicate).SortByDescending(sort).First();
    }

    public virtual async ValueTask<TEntity> FirstAsync(CancellationToken cancellationToken = default)
    {
        return await FirstAsync(_ => true, cancellationToken);
    }

    public virtual async ValueTask<TEntity> FirstByAscendingAsync(Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default)
    {
        return await FirstByAscendingAsync(_ => true, sort, cancellationToken);
    }

    public virtual async ValueTask<TEntity> FirstByDescendingAsync(Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default)
    {
        return await FirstByDescendingAsync(_ => true, sort, cancellationToken);
    }

    public virtual async ValueTask<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(predicate).FirstAsync(cancellationToken);
    }

    public virtual async ValueTask<TEntity> FirstByAscendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(predicate).SortBy(sort).FirstAsync(cancellationToken);
    }

    public virtual async ValueTask<TEntity> FirstByDescendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(predicate).SortByDescending(sort).FirstAsync(cancellationToken);
    }


    public virtual TEntity FirstOrDefault()
    {
        return FirstOrDefault(_ => true);
    }

    public virtual TEntity FirstOrDefaultByAscending(Expression<Func<TEntity, object>> sort)
    {
        return FirstOrDefaultByAscending(_ => true, sort);
    }

    public virtual TEntity FirstOrDefaultByDescending(Expression<Func<TEntity, object>> sort)
    {
        return FirstOrDefaultByDescending(_ => true, sort);
    }

    public virtual TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
    {
        return Collection.Find(predicate).FirstOrDefault();
    }

    public virtual TEntity FirstOrDefaultByAscending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort)
    {
        return Collection.Find(predicate).SortBy(sort).FirstOrDefault();
    }

    public virtual TEntity FirstOrDefaultByDescending(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort)
    {
        return Collection.Find(predicate).SortByDescending(sort).FirstOrDefault();
    }

    public virtual async ValueTask<TEntity> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await FirstOrDefaultAsync(_ => true, cancellationToken);
    }

    public virtual async ValueTask<TEntity> FirstOrDefaultByAscendingAsync(Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default)
    {
        return await FirstOrDefaultByAscendingAsync(_ => true, sort, cancellationToken);
    }

    public virtual async ValueTask<TEntity> FirstOrDefaultByDescendingAsync(Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default)
    {
        return await FirstOrDefaultByDescendingAsync(_ => true, sort, cancellationToken);
    }

    public virtual async ValueTask<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(predicate).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async ValueTask<TEntity> FirstOrDefaultByAscendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(predicate).SortBy(sort).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async ValueTask<TEntity> FirstOrDefaultByDescendingAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> sort, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(predicate).SortByDescending(sort).FirstOrDefaultAsync(cancellationToken);
    }


    public virtual async ValueTask<Result<TEntity>> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            SetId(entity);
            await Collection.InsertOneAsync(entity, new(), cancellationToken);
            return new() { Succeeded = true, Message = ConstMessages.SuccessfullyDone };
        }
        catch (Exception)
        {
                
            return new() { Succeeded = false, Message = ConstMessages.GeneralError };
        }
    }

    public virtual async ValueTask<Result<TEntity>> InsertAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            SetId(entities);
            await Collection.InsertManyAsync(entities, new(), cancellationToken);
            return new() { Succeeded = true, Message = ConstMessages.SuccessfullyDone };
        }
        catch
        {
            return new() { Succeeded = false, Message = ConstMessages.GeneralError };
        }
    }


    public virtual async ValueTask<Result<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ReplaceOneResult result = await Collection.ReplaceOneAsync(p => p.Id.Equals(entity.Id), entity, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (result.MatchedCount == result.ModifiedCount)
        {
            return new() { Message = "AlertAndMessage_SuccessfullyDone", Succeeded = true };
        }

        return new() { Message = ConstMessages.NotRecordChange, Succeeded = false };
    }

    public virtual async ValueTask<Result<TEntity>> UpdateAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
    {
        IEnumerable<ReplaceOneModel<TEntity>> updateModel = CreateReplaceOneModels(entities);

        //BulkWriteOptions
        BulkWriteResult<TEntity> result = await Collection.BulkWriteAsync(updateModel, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (result.MatchedCount == result.ModifiedCount)
        {
            return new() { Message = ConstMessages.SuccessfullyDone, Succeeded = true };
        }

        return new() { Message = ConstMessages.NotRecordChange, Succeeded = false };
    }

    public virtual async ValueTask<Result<TEntity>> UpdateAsync(TEntity entity, UpdateDefinition<TEntity> update, CancellationToken cancellationToken = default)
    {
        UpdateResult result = await Collection.UpdateOneAsync(p => p.Id.Equals(entity.Id), update, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (result.MatchedCount == result.ModifiedCount)
        {
            return new() { Message = ConstMessages.SuccessfullyDone, Succeeded = true };
        }

        return new() { Message = ConstMessages.NotRecordChange, Succeeded = false };
    }

    public virtual async ValueTask<Result<TEntity>> UpdateAsync(string id, UpdateDefinition<TEntity> update, CancellationToken cancellationToken = default)
    {
        UpdateResult result = await Collection.UpdateOneAsync(p => p.Id.Equals(id), update, cancellationToken: cancellationToken).ConfigureAwait(false); //UpdateManyAsync

        if (result.MatchedCount == result.ModifiedCount)
        {
            return new() { Message = ConstMessages.SuccessfullyDone, Succeeded = true };
        }

        return new() { Message = ConstMessages.NotRecordChange, Succeeded = false };
    }

    public virtual async ValueTask<Result<TEntity>> UpdateAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> field, TProperty value, CancellationToken cancellationToken = default)
    {
        UpdateResult result = await Collection.UpdateOneAsync(p => p.Id.Equals(entity.Id), Builders<TEntity>.Update.Set(field, value), cancellationToken: cancellationToken).ConfigureAwait(false);

        if (result.MatchedCount == result.ModifiedCount)
        {
            return new() { Message = ConstMessages.SuccessfullyDone, Succeeded = true };
        }

        return new() { Message = ConstMessages.NotRecordChange, Succeeded = false };
    }

    public virtual async ValueTask<Result<TEntity>> UpdateAsync(Expression<Func<TEntity, bool>> predicate, UpdateDefinition<TEntity> update, CancellationToken cancellationToken = default)
    {
        UpdateResult result = await Collection.UpdateManyAsync(predicate, update, cancellationToken: cancellationToken).ConfigureAwait(false); //UpdateOneAsync

        if (result.MatchedCount == result.ModifiedCount)
        {
            return new() { Message = ConstMessages.SuccessfullyDone, Succeeded = true };
        }

        return new() { Message = ConstMessages.NotRecordChange, Succeeded = false };
    }

    public virtual async ValueTask<Result<TEntity>> UpdateAsync<TProperty>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TProperty>> field, TProperty value, CancellationToken cancellationToken = default)
    {
        UpdateResult result = await Collection.UpdateManyAsync(predicate, Builders<TEntity>.Update.Set(field, value), cancellationToken: cancellationToken).ConfigureAwait(false); //UpdateOneAsync

        if (result.MatchedCount == result.ModifiedCount)
        {
            return new() { Message = ConstMessages.SuccessfullyDone, Succeeded = true };
        }

        return new() { Message = ConstMessages.NotRecordChange, Succeeded = false };
    }


    public virtual async ValueTask<Result<TEntity>> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        //DeleteOptions
        DeleteResult result = await Collection.DeleteOneAsync(p => p.Id.Equals(entity.Id), cancellationToken).ConfigureAwait(false);

        if (result.IsAcknowledged)
        {
            return new() { Message = ConstMessages.SuccessfullyDone, Succeeded = true };
        }

        return new() { Message = ConstMessages.GeneralError, Succeeded = false };
    }

    public virtual async ValueTask<Result<TEntity>> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        //DeleteOptions
        DeleteResult result = await Collection.DeleteOneAsync(p => p.Id.Equals(id), cancellationToken);

        if (result.IsAcknowledged)
        {
            return new() { Message = ConstMessages.SuccessfullyDone, Succeeded = true };
        }

        return new() { Message = ConstMessages.GeneralError, Succeeded = false };
    }

    public virtual async ValueTask<Result<TEntity>> DeleteAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (!entities.Any())
        {
            return new() { Message = ConstMessages.GeneralError, Succeeded = false };
        }

        //DeleteOptions
        String[] ids = entities.Select(p => p.Id).ToArray();
        DeleteResult result = await Collection.DeleteManyAsync(p => ids.Contains(p.Id), cancellationToken).ConfigureAwait(false);

        if (result.IsAcknowledged)
        {
            return new() { Message = ConstMessages.SuccessfullyDone, Succeeded = true };
        }

        return new() { Message = ConstMessages.GeneralError, Succeeded = false };
    }

    public virtual async ValueTask<Result<TEntity>> DeleteAsync(List<string> ids, CancellationToken cancellationToken = default)
    {
        if (!ids.Any())
        {
            return new() { Message = ConstMessages.GeneralError, Succeeded = false };
        }

        //DeleteOptions
        DeleteResult result = await Collection.DeleteManyAsync(p => ids.Contains(p.Id), cancellationToken).ConfigureAwait(false);

        if (result.IsAcknowledged)
        {
            return new() { Message = ConstMessages.SuccessfullyDone, Succeeded = true };
        }

        return new() { Message = ConstMessages.GeneralError, Succeeded = false };
    }

    public virtual async ValueTask<Result<TEntity>> DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        //DeleteOptions
        DeleteResult result = await Collection.DeleteManyAsync(predicate, cancellationToken).ConfigureAwait(false);

        if (result.IsAcknowledged)
        {
            return new() { Message = ConstMessages.SuccessfullyDone, Succeeded = true };
        }

        return new() { Message = ConstMessages.GeneralError, Succeeded = false };
    }


    private bool _bulkOperationMode;
    private readonly List<WriteModel<TEntity>> _bulkOperations = [];

    private void BeginBulkOperation()
    {
        if (_bulkOperationMode)
        {
            throw new();
        }

        //check for thread-safety
        _bulkOperationMode = true;
    }

    public virtual async ValueTask<BulkWriteResult<TEntity>> DoBulkOperation(List<WriteModel<TEntity>> writeModels)
    {
        return await DoBulkOperation(writeModels, default, default);
    }

    public virtual async ValueTask<BulkWriteResult<TEntity>> DoBulkOperation(List<WriteModel<TEntity>> writeModels, BulkWriteOptions options)
    {
        return await DoBulkOperation(writeModels, options, default);
    }

    public virtual async ValueTask<BulkWriteResult<TEntity>> DoBulkOperation(List<WriteModel<TEntity>> writeModels, CancellationToken cancellationToken)
    {
        return await DoBulkOperation(writeModels, default, cancellationToken);
    }

    public virtual async ValueTask<BulkWriteResult<TEntity>> DoBulkOperation(List<WriteModel<TEntity>> writeModels, BulkWriteOptions options, CancellationToken cancellationToken)
    {
        return await Collection.BulkWriteAsync(writeModels, options, cancellationToken).ConfigureAwait(false);
    }

    public virtual async ValueTask WithBulkOperation(Func<Task> func)
    {
        await WithBulkOperation(func, default, default);
    }

    public virtual async ValueTask WithBulkOperation(Func<Task> func, BulkWriteOptions options)
    {
        await WithBulkOperation(func, options, default);
    }

    public virtual async ValueTask WithBulkOperation(Func<Task> func, CancellationToken cancellationToken)
    {
        await WithBulkOperation(func, default, cancellationToken);
    }

    public virtual async ValueTask WithBulkOperation(Func<Task> func, BulkWriteOptions options, CancellationToken cancellationToken)
    {
        BeginBulkOperation();
        await func().ConfigureAwait(false);
        //await DoBulkOperation(options, cancellationToken).ConfigureAwait(false);
    }


    public virtual async ValueTask<string> CreateIndexAsync(Expression<Func<TEntity, object>> field, bool descending, bool unique, bool sparse = false, string name = null, CancellationToken cancellationToken = default)
    {
        //TODO: Convert new { p.Field1, p.Field2} to Combined index

        CreateIndexOptions options = new() { Unique = unique, Sparse = sparse, Name = name };

        if (descending)
        {
            return await Collection.Indexes.CreateOneAsync(new CreateIndexModel<TEntity>(Builders<TEntity>.IndexKeys.Descending(field), options), cancellationToken: cancellationToken);
        }

        return await Collection.Indexes.CreateOneAsync(new CreateIndexModel<TEntity>(Builders<TEntity>.IndexKeys.Ascending(field), options), cancellationToken: cancellationToken);
    }

    public virtual async ValueTask<List<string>> GetIndexesNamesAsync(CancellationToken cancellationToken = default)
    {
        IAsyncCursor<BsonDocument> indexCursor = await Collection.Indexes.ListAsync(cancellationToken).ConfigureAwait(false);
        List<BsonDocument> indexes = await indexCursor.ToListAsync(cancellationToken).ConfigureAwait(false);

        return indexes.ConvertAll(e => e["name"].ToString())!;
    }

    public virtual async ValueTask<bool> IndexExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return (await GetIndexesNamesAsync(cancellationToken)).Contains(name);
    }

    public virtual async ValueTask<string> CreateTextIndexAsync(Expression<Func<TEntity, object>> field, CreateIndexOptions options = null, CancellationToken cancellationToken = default)
    {
        return await Collection.Indexes.CreateOneAsync(new CreateIndexModel<TEntity>(Builders<TEntity>.IndexKeys.Text(field), options), cancellationToken: cancellationToken);
    }

    public virtual async ValueTask<string> CreateAscendingIndexAsync(Expression<Func<TEntity, object>> field, CreateIndexOptions options = null, CancellationToken cancellationToken = default)
    {
        return await Collection.Indexes.CreateOneAsync(new CreateIndexModel<TEntity>(Builders<TEntity>.IndexKeys.Ascending(field), options), cancellationToken: cancellationToken);
    }

    public virtual async ValueTask<string> CreateDescendingIndexAsync(Expression<Func<TEntity, object>> field, CreateIndexOptions options = null, CancellationToken cancellationToken = default)
    {
        return await Collection.Indexes.CreateOneAsync(new CreateIndexModel<TEntity>(Builders<TEntity>.IndexKeys.Descending(field), options), cancellationToken: cancellationToken);
    }

    public virtual async ValueTask<string> CreateHashedIndexAsync(Expression<Func<TEntity, object>> field, CreateIndexOptions options = null, CancellationToken cancellationToken = default)
    {
        return await Collection.Indexes.CreateOneAsync(new CreateIndexModel<TEntity>(Builders<TEntity>.IndexKeys.Hashed(field), options), cancellationToken: cancellationToken);
    }

    public virtual async ValueTask<string> CreateCombinedTextIndexAsync(IEnumerable<Expression<Func<TEntity, object>>> fields, CreateIndexOptions options = null, CancellationToken cancellationToken = default)
    {
        List<IndexKeysDefinition<TEntity>> indexKeys = fields.Select(field => Builders<TEntity>.IndexKeys.Text(field)).ToList();

        return await Collection.Indexes.CreateOneAsync(new CreateIndexModel<TEntity>(Builders<TEntity>.IndexKeys.Combine(indexKeys), options), cancellationToken: cancellationToken);
    }

    public virtual async ValueTask<string> CreateCombinedAscendingIndexAsync(IEnumerable<Expression<Func<TEntity, object>>> fields, CreateIndexOptions options = null, CancellationToken cancellationToken = default)
    {
        List<IndexKeysDefinition<TEntity>> indexKeys = fields.Select(field => Builders<TEntity>.IndexKeys.Ascending(field)).ToList();

        return await Collection.Indexes.CreateOneAsync(new CreateIndexModel<TEntity>(Builders<TEntity>.IndexKeys.Combine(indexKeys), options), cancellationToken: cancellationToken);
    }

    public virtual async ValueTask<string> CreateCombinedDescendingIndexAsync(IEnumerable<Expression<Func<TEntity, object>>> fields, CreateIndexOptions options = null, CancellationToken cancellationToken = default)
    {
        List<IndexKeysDefinition<TEntity>> indexKeys = fields.Select(field => Builders<TEntity>.IndexKeys.Descending(field)).ToList();

        return await Collection.Indexes.CreateOneAsync(new CreateIndexModel<TEntity>(Builders<TEntity>.IndexKeys.Combine(indexKeys), options), cancellationToken: cancellationToken);
    }

    public virtual async ValueTask<string> CreateCombinedHashedIndexAsync(IEnumerable<Expression<Func<TEntity, object>>> fields, CreateIndexOptions options = null, CancellationToken cancellationToken = default)
    {
        List<IndexKeysDefinition<TEntity>> indexKeys = fields.Select(field => Builders<TEntity>.IndexKeys.Hashed(field)).ToList();

        return await Collection.Indexes.CreateOneAsync(new CreateIndexModel<TEntity>(Builders<TEntity>.IndexKeys.Combine(indexKeys), options), cancellationToken: cancellationToken);
    }

    public Task DropIndexAsync(string indexName, CancellationToken cancellationToken = default)
    {
        return Collection.Indexes.DropOneAsync(indexName, cancellationToken);
    }


    private void SetId(List<TEntity> entities)
    {
        foreach (TEntity entity in entities)
        {
            SetId(entity);
        }
    }

    private void SetId(TEntity entity)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = DateTime.Now.ToString();
        }
    }

    private IEnumerable<ReplaceOneModel<TEntity>> CreateReplaceOneModels(List<TEntity> entities)
    {
        return from entity in entities let filter = Builders<TEntity>.Filter.Where(p => p.Id.Equals(entity.Id)) select new ReplaceOneModel<TEntity>(filter, entity);
    }

    private void AddInsertOneModel(TEntity entity)
    {
        InsertOneModel<TEntity> model = new(entity);
        _bulkOperations.Add(model);
    }

    private void AddDeleteOneModel(Expression<Func<TEntity, bool>> predicate)
    {
        FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Where(predicate);
        DeleteOneModel<TEntity> model = new(filter);
        _bulkOperations.Add(model);
    }

    private void AddDeleteManyModel(Expression<Func<TEntity, bool>> predicate)
    {
        FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Where(predicate);
        DeleteManyModel<TEntity> model = new(filter);
        _bulkOperations.Add(model);
    }

    private void AddReplaceOneModel(Expression<Func<TEntity, bool>> predicate, TEntity entity)
    {
        FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Where(predicate);
        ReplaceOneModel<TEntity> model = new(filter, entity);
        _bulkOperations.Add(model);
    }

    private void AddUpdateOneModel(Expression<Func<TEntity, bool>> predicate, UpdateDefinition<TEntity> updateDefinition)
    {
        FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Where(predicate);
        UpdateOneModel<TEntity> model = new(filter, updateDefinition);
        _bulkOperations.Add(model);
    }
}