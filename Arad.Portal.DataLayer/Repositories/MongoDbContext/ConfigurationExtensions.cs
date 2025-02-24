using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.Interfaces.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.MongoDbContext;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddMongoDbContext(this IServiceCollection services, string connectionString, string dbName)
    {
        MongoClient client = new(connectionString);

        // new MongoUrl(connectionString).DatabaseName
        return services.AddMongoDbContext(client, dbName);
    }

    public static IServiceCollection AddMongoDbContext(this IServiceCollection services, MongoClient client, string dbName)
    {
        //BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

        services.AddSingleton(client);

        //MongoDatabaseSettings
        services.AddSingleton(client.GetDatabase(dbName));

        //services.AddSingleton<IMongoDbContext>(provider => new MongoDbContext(provider.GetRequiredService<IMongoDatabase>(), provider));
        //services.AddSingleton(typeof(IMongoRepository<>), typeof(MongoRepository<>));
        //services.AddSingleton(typeof(IMongoRepository<,>), typeof(MongoRepository<,>));

        services.AddTransient<IMongoDbContext, MongoDbContext>();

        //services.AddScoped<IMongoDbContext>(provider => new MongoDbContext(provider.GetRequiredService<IMongoDatabase>(), provider));

        services.AddTransient(typeof(IRepository<>), typeof(Repository<>));

        //services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
        //services.AddScoped(typeof(IMongoRepository<,>), typeof(MongoRepository<,>));

        return services;
    }
}