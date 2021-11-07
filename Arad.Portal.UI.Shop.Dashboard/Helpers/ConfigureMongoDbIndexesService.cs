using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{
    public class ConfigureMongoDbIndexesService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoClient _client;
        private readonly IMongoDatabase db;
        private readonly ILogger<ConfigureMongoDbIndexesService> _logger;
        public ConfigureMongoDbIndexesService(IConfiguration configuration, ILogger<ConfigureMongoDbIndexesService> logger)
        {
            _configuration = configuration;
            _client = new MongoClient(_configuration["Database:ConnectionString"]);
            db = _client.GetDatabase(_configuration["Database:DbName"]);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            #region CreateIndexes
            
            var options = new CreateIndexOptions { Collation = new Collation("en") };


            #region Content
            var contentCollection = db.GetCollection<DataLayer.Entities.General.Content.Content>("Content");
            var creationDateIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.CreationDate);

            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(creationDateIndex),
               cancellationToken: cancellationToken);

            var contentDomainIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(contentDomainIndex , options),
                cancellationToken: cancellationToken);

            var categoryIdIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Ascending(_ => _.ContentCategoryId);
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(categoryIdIndex , options),
                cancellationToken: cancellationToken);


            var startShowDateIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Ascending(_ => _.StartShowDate);
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(startShowDateIndex),
                cancellationToken: cancellationToken);

            var endShowDateIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.EndShowDate);
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(endShowDateIndex),
                cancellationToken: cancellationToken);

            var contentCodeIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.ContentCode);
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(contentCodeIndex),
                cancellationToken: cancellationToken);

            var isActiveIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.IsActive);
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(isActiveIndex),
                cancellationToken: cancellationToken);

            var isDeletedIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.IsDeleted);
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(isDeletedIndex),
                cancellationToken: cancellationToken);
            #endregion Content

            #region ContentCategory
            var contentCategoryCollection = db.GetCollection<DataLayer.Entities.General.ContentCategory.ContentCategory>("ContentCategory");


            var parentCategoryIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.ParentCategoryId);
            await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(parentCategoryIndex),
                cancellationToken: cancellationToken);


            var categoryTypeIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.CategoryType);
            await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryTypeIndex),
                cancellationToken: cancellationToken);

            var categoryCodeIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.CategoryCode);
            await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryCodeIndex),
                cancellationToken: cancellationToken);
            #endregion ContentCategory

            #region Menu
            var menuCollection = db.GetCollection<DataLayer.Entities.General.Menu.Menu>("Menu");

            var domainMenuIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.AssociatedDomainId);
            await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(domainMenuIndex),
                cancellationToken: cancellationToken);


            var menuCodeIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.MenuCode);
            await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(menuCodeIndex),
                 cancellationToken: cancellationToken);

            var menuUrlIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.Url);
            await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(menuUrlIndex),
                cancellationToken: cancellationToken);

            #endregion Menu

            #region Product
            var productCollection = db.GetCollection<DataLayer.Entities.Shop.Product.Product>("Product");
            var productDomainIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Descending(_ => _.AssociatedDomainId);
            await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productDomainIndex),
                cancellationToken: cancellationToken);

            var productCreationDateIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Descending(_ => _.CreationDate);
                
            #endregion Product





            #endregion


        }

        public Task StopAsync(CancellationToken cancellationToken)
              => Task.CompletedTask;
    }
}
