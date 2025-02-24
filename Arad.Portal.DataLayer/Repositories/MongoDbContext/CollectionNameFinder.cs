using Arad.Portal.GeneralLibrary.CustomAttributes;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Arad.Portal.DataLayer.Repositories.MongoDbContext;

public static class CollectionNameFinder
{
    private static readonly ConcurrentDictionary<Type, string> _dictionary = new();

    public static string GetCollectionName<TEntity>()
    {
        return GetCollectionName(typeof(TEntity));
    }

    public static string GetCollectionName(Type type)
    {
        return _dictionary.GetOrAdd(type,
                                    _ =>
                                    {
                                        string collectionName = FindNameByAttribute(type);

                                        return collectionName;
                                    });
    }

    private static string FindNameByAttribute(Type type)
    {
        TypeInfo typeInfo = type.GetTypeInfo();
        CustomCollectionNameAttribute tableAttribute = typeInfo.GetCustomAttribute<CustomCollectionNameAttribute>(false);

        return tableAttribute is null ? typeInfo.Name : tableAttribute.Name;
    }

    public static IMongoCollection<TDocument> GetCollection<TDocument>(this IMongoDatabase database, MongoCollectionSettings settings = null)
    {
        string name = GetCollectionName<TDocument>();

        return database.GetCollection<TDocument>(name, settings);
    }
}