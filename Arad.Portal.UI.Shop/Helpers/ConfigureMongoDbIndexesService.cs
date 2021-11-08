using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Helpers
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

                var contentLanguageIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.LanguageId);
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(contentLanguageIndex, options),
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

                var categoryIsActiveIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.IsActive);
                await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryIsActiveIndex),
                    cancellationToken: cancellationToken);

                var categoryIsDeletedIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.IsDeleted);
                await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryIsDeletedIndex),
                     cancellationToken: cancellationToken);

                var contentCategoryLanguageIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending("CategoryNames.LanguageId");
                await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(contentCategoryLanguageIndex, options),
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

                var menuLanguageIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending("MenuTitles.LanguageId");
                await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(menuLanguageIndex, options),
                    cancellationToken: cancellationToken);

                var isDeletedMenuIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.IsDeleted);
                await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(isDeletedMenuIndex),
                    cancellationToken: cancellationToken);

                 var isActiveMenuIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.IsActive);
                 await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(isActiveMenuIndex),
                   cancellationToken: cancellationToken);

                #endregion Menu

                #region Product
                var productCollection = db.GetCollection<DataLayer.Entities.Shop.Product.Product>("Product");
                var productDomainIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Descending(_ => _.AssociatedDomainId);
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productDomainIndex),
                    cancellationToken: cancellationToken);

                var productCreationDateIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Descending(_ => _.CreationDate);
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productCreationDateIndex),
                    cancellationToken: cancellationToken);

                var productCodeIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending(_ => _.ProductCode);
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productCodeIndex),
                    cancellationToken: cancellationToken);

                var uniqueCodeIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending(_ => _.UniqueCode);
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(uniqueCodeIndex),
                    cancellationToken: cancellationToken);

                var productLanguageIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending("MultiLingualProperties.LanguageId");
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productLanguageIndex, options),
                    cancellationToken: cancellationToken);

                var deletedProductIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending(_ => _.IsDeleted);
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(deletedProductIndex),
                     cancellationToken: cancellationToken);

                var activeProductIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending(_ => _.IsActive);
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(activeProductIndex),
                  cancellationToken: cancellationToken);  
            #endregion Product

                #region ProductGroup
                var productGroupCollection = db.GetCollection<DataLayer.Entities.Shop.ProductGroup.ProductGroup>("ProductGroup");

                var groupCodeIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending(_ => _.GroupCode);
                await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(groupCodeIndex),
                    cancellationToken: cancellationToken);

                var productGroupLanguageIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending("MultiLingualProperties.LanguageId");
                await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(productGroupLanguageIndex, options),
                    cancellationToken: cancellationToken);

                 var deletedGroupIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending(_ => _.IsDeleted);
                 await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(deletedGroupIndex),
                    cancellationToken: cancellationToken);

                 var activeGroupIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending(_ => _.IsActive);
                 await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(activeGroupIndex),
                     cancellationToken: cancellationToken);
            #endregion

            #region Promotion
                var promotionCollection = db.GetCollection<DataLayer.Entities.Shop.Promotion.Promotion>("Promotion");

                var promotionDomainIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(promotionDomainIndex, options),
                    cancellationToken: cancellationToken);

                var activePromotionIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.IsActive);
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(activePromotionIndex),
                    cancellationToken: cancellationToken);

                var deletedPromotionIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.IsDeleted);
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(deletedPromotionIndex),
                   cancellationToken: cancellationToken);
            #endregion

            #endregion
        }

        public Task StopAsync(CancellationToken cancellationToken)
              => Task.CompletedTask;
    }
}
