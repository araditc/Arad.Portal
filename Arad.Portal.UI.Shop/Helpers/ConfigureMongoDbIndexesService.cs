using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

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
            _client = new MongoClient(_configuration["DatabaseConfig:ConnectionString"]);
            db = _client.GetDatabase(_configuration["DatabaseConfig:DbName"]);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            #region CreateIndexes

            var generalOptions = new CreateIndexOptions { Collation = new Collation("en") };
            var uniqueOption = new CreateIndexOptions { Collation = new Collation("en"), Unique = true };

            #region Content

            var contentCollection = db.GetCollection<DataLayer.Entities.General.Content.Content>("Content");
            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("CreationDate_-1")))
            {

                var creationDateIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.CreationDate);
                generalOptions.Name = "CreationDate_-1";
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(creationDateIndex, generalOptions),
                   cancellationToken: cancellationToken);
            }

            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_1")))
            {
                var contentDomainIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
                generalOptions.Name = "AssociatedDomainId_1";
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(contentDomainIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }


            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("ContentCategoryId_1")))
            {
                var categoryIdIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Ascending(_ => _.ContentCategoryId);
                generalOptions.Name = "ContentCategoryId_1";
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(categoryIdIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }


            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("StartShowDate_1_EndShowDate_-1")))
            {
                var startEndShowDateIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Ascending(_ => _.StartShowDate).Descending(_ => _.EndShowDate);
                generalOptions.Name = "StartShowDate_1_EndShowDate_-1";
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(startEndShowDateIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("ContentCode_-1")))
            {
                Log.Fatal("enter ContentCode_1 index in shop");
                var contentCodeIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.ContentCode);
                uniqueOption.Name = "ContentCode_-1";
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(contentCodeIndex, uniqueOption),
                    cancellationToken: cancellationToken);
            }

            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("IsActive_-1")))
            {
                var isActiveIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.IsActive);
                generalOptions.Name = "IsActive_-1";
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(isActiveIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("IsDeleted_-1")))
            {
                var isDeletedIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.IsDeleted);
                generalOptions.Name = "IsDeleted_-1";
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(isDeletedIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("LanguageId_-1")))
            {
                var contentLanguageIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.LanguageId);
                generalOptions.Name = "LanguageId_-1";
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(contentLanguageIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            #endregion Content

            #region ContentCategory

            var contentCategoryCollection = db.GetCollection<DataLayer.Entities.General.ContentCategory.ContentCategory>("ContentCategory");

            if (!contentCategoryCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("ParentCategoryId_-1")))
            {
                var parentCategoryIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.ParentCategoryId);
                generalOptions.Name = "ParentCategoryId_-1";
                await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(parentCategoryIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!contentCategoryCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("CategoryType_-1")))
            {
                var categoryTypeIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.CategoryType);
                generalOptions.Name = "CategoryType_-1";
                await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryTypeIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!contentCategoryCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("CategoryCode_-1")))
            {
                var categoryCodeIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.CategoryCode);
                uniqueOption.Name = "CategoryCode_-1";
                await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryCodeIndex, uniqueOption),
                    cancellationToken: cancellationToken);
            }

            if (!contentCategoryCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("IsActive_-1")))
            {
                var categoryIsActiveIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.IsActive);
                generalOptions.Name = "IsActive_-1";
                await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryIsActiveIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!contentCategoryCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("IsDeleted_-1")))
            {
                var categoryIsDeletedIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.IsDeleted);
                generalOptions.Name = "IsDeleted_-1";
                await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryIsDeletedIndex, generalOptions),
                     cancellationToken: cancellationToken);
            }
            if (!contentCategoryCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("CategoryNames.LanguageId_-1")))
            {
                var contentCategoryLanguageIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending("CategoryNames.LanguageId");
                generalOptions.Name = "CategoryNames.LanguageId_-1";
                await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(contentCategoryLanguageIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            #endregion ContentCategory

            #region Domain
            var domainCollection = db.GetCollection<DataLayer.Entities.General.Domain.Domain>("Domain");
            if (!domainCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("DomainName_1")))
            {
                var domainNameIndex = Builders<DataLayer.Entities.General.Domain.Domain>.IndexKeys.Ascending(_ => _.DomainName);
                uniqueOption.Name = "DomainName_1";
                await domainCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Domain.Domain>(domainNameIndex, uniqueOption),
                     cancellationToken: cancellationToken);
            }
            #endregion 

            #region Menu
            var menuCollection = db.GetCollection<DataLayer.Entities.General.Menu.Menu>("Menu");

            if (!menuCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_-1")))
            {
                var domainMenuIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.AssociatedDomainId);
                generalOptions.Name = "AssociatedDomainId_-1";
                await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(domainMenuIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!menuCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_1_MenuCode_1")))
            {
                var menuCodeIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Ascending(_ => _.AssociatedDomainId).Ascending(_ => _.MenuCode);
                generalOptions.Name = "AssociatedDomainId_1_MenuCode_1";
                await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(menuCodeIndex, generalOptions),
                     cancellationToken: cancellationToken);
            }

            if (!menuCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("Url_-1_AssociatedDomainId_1")))
            {
                var menuUrlIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.Url).Ascending(_ => _.AssociatedDomainId);
                generalOptions.Name = "Url_-1_AssociatedDomainId_1";
                await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(menuUrlIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!menuCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("MenuTitles.LanguageId_-1")))
            {
                var menuLanguageIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending("MenuTitles.LanguageId");
                generalOptions.Name = "MenuTitles.LanguageId_-1";
                await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(menuLanguageIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }


            if (!menuCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("IsDeleted_-1")))
            {
                var isDeletedMenuIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.IsDeleted);
                generalOptions.Name = "IsDeleted_-1";
                await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(isDeletedMenuIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }


            if (!menuCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("IsActive_-1")))
            {
                var isActiveMenuIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.IsActive);
                generalOptions.Name = "IsActive_-1";
                await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(isActiveMenuIndex, generalOptions),
                  cancellationToken: cancellationToken);
            }

            #endregion Menu

            #region Product
            var productCollection = db.GetCollection<DataLayer.Entities.Shop.Product.Product>("Product");
            if (!productCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_-1")))
            {
                var productDomainIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Descending(_ => _.AssociatedDomainId);
                generalOptions.Name = "AssociatedDomainId_-1";
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productDomainIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!productCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("CreationDate_-1")))
            {
                var productCreationDateIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Descending(_ => _.CreationDate);
                generalOptions.Name = "CreationDate_-1";
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productCreationDateIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!productCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("ProductCode_1")))
            {
                var productCodeIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending(_ => _.ProductCode);
                uniqueOption.Name = "ProductCode_1";
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productCodeIndex, uniqueOption),
                    cancellationToken: cancellationToken);
            }

            if (!productCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("UniqueCode_1_AssociatedDomainId_1")))
            {
                var uniqueCodeIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending(_ => _.UniqueCode).Ascending(_ => _.AssociatedDomainId);
                uniqueOption.Name = "UniqueCode_1_AssociatedDomainId_1";
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(uniqueCodeIndex, uniqueOption),
                    cancellationToken: cancellationToken);
            }
            if (!productCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("MultiLingualProperties.LanguageId_1")))
            {
                var productLanguageIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending("MultiLingualProperties.LanguageId");
                generalOptions.Name = "MultiLingualProperties.LanguageId_1";
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productLanguageIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!productCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("IsDeleted_1")))
            {
                var deletedProductIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending(_ => _.IsDeleted);
                generalOptions.Name = "IsDeleted_1";
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(deletedProductIndex, generalOptions),
                     cancellationToken: cancellationToken);
            }

            if (!productCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("IsActive_1")))
            {
                var activeProductIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending(_ => _.IsActive);
                generalOptions.Name = "IsActive_1";
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(activeProductIndex, generalOptions),
                  cancellationToken: cancellationToken);
            }
            #endregion Product

            #region ProductGroup
            var productGroupCollection = db.GetCollection<DataLayer.Entities.Shop.ProductGroup.ProductGroup>("ProductGroup");
            if (!productGroupCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("GroupCode_1")))
            {
                var groupCodeIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending(_ => _.GroupCode);
                uniqueOption.Name = "GroupCode_1";
                await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(groupCodeIndex, uniqueOption),
                    cancellationToken: cancellationToken);
            }

            if (!productGroupCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("ParentId_1")))
            {
                var parentGroupIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending(_ => _.ParentId);
                generalOptions.Name = "ParentId_1";
                await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(parentGroupIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!productGroupCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("MultiLingualProperties.LanguageId_1")))
            {
                var productGroupLanguageIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending("MultiLingualProperties.LanguageId");
                generalOptions.Name = "MultiLingualProperties.LanguageId_1";
                await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(productGroupLanguageIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }


            if (!productGroupCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("IsDeleted_1")))
            {
                var deletedGroupIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending(_ => _.IsDeleted);
                generalOptions.Name = "IsDeleted_1";
                await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(deletedGroupIndex, generalOptions),
                   cancellationToken: cancellationToken);
            }

            if (!productGroupCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("IsActive_1")))
            {
                var activeGroupIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending(_ => _.IsActive);
                generalOptions.Name = "IsActive_1";
                await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(activeGroupIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }
            #endregion

            #region Promotion
            var promotionCollection = db.GetCollection<DataLayer.Entities.Shop.Promotion.Promotion>("Promotion");

            if (!promotionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_1")))
            {
                var promotionDomainIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
                generalOptions.Name = "AssociatedDomainId_1";
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(promotionDomainIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!promotionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("IsActive_1")))
            {
                var activePromotionIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.IsActive);
                generalOptions.Name = "IsActive_1";
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(activePromotionIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!promotionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("IsDeleted_1")))
            {
                var deletedPromotionIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.IsDeleted);
                generalOptions.Name = "IsDeleted_1";
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(deletedPromotionIndex, generalOptions),
                   cancellationToken: cancellationToken);
            }

            if (!promotionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("PromotionType_1")))
            {

                var promotionTypeIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.PromotionType);
                generalOptions.Name = "PromotionType_1";
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(promotionTypeIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!promotionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("DiscountType_1")))
            {

                var discountTypeIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.DiscountType);
                generalOptions.Name = "DiscountType_1";
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(discountTypeIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }


            if (!promotionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("CurrencyId_1")))
            {
                var currencyIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.CurrencyId);
                generalOptions.Name = "CurrencyId_1";
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(currencyIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!promotionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("SDate_1_EDate_-1")))
            {
                var startEndShowDatePromotionIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.SDate).Descending(_ => _.EDate);
                generalOptions.Name = "SDate_1_EDate_-1";
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(startEndShowDatePromotionIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }
            #endregion

            #region ApplicationUser
            var userCollection = db.GetCollection<DataLayer.Entities.General.User.ApplicationUser>("ApplicationUser");

            if (!userCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("UserName_1")))
            {
                var userNameInDomain = Builders<DataLayer.Entities.General.User.ApplicationUser>.IndexKeys.Ascending(_ => _.UserName);
                uniqueOption.Name = "UserName_1";
                await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.ApplicationUser>(userNameInDomain, uniqueOption),
                    cancellationToken: cancellationToken);
            }

            if (!userCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("Profile.DefaultCurrencyId_1")))
            {
                var defaultCurrencyIdIndex = Builders<DataLayer.Entities.General.User.ApplicationUser>.IndexKeys.Ascending(_ => _.Profile.DefaultCurrencyId);
                generalOptions.Name = "Profile.DefaultCurrencyId_1";
                await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.ApplicationUser>(defaultCurrencyIdIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!userCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("Profile.DefaultLanguageId_1")))
            {
                var defaultlanguageId = Builders<DataLayer.Entities.General.User.ApplicationUser>.IndexKeys.Ascending(_ => _.Profile.DefaultLanguageId);
                generalOptions.Name = "Profile.DefaultLanguageId_1";
                await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.ApplicationUser>(defaultlanguageId, generalOptions),
                    cancellationToken: cancellationToken);
            }
            #endregion

            #region Comment

            var commentCollection = db.GetCollection<DataLayer.Entities.General.Comment.Comment>("Comment");
            if (!commentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_1")))
            {
                var associatedDomainIdIndex = Builders<DataLayer.Entities.General.Comment.Comment>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
                generalOptions.Name = "AssociatedDomainId_1";
                await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Comment.Comment>(associatedDomainIdIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }
            if (!commentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("ParentId_1")))
            {
                var parentIdIndex = Builders<DataLayer.Entities.General.Comment.Comment>.IndexKeys.Ascending(_ => _.ParentId);
                generalOptions.Name = "ParentId_1";
                await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Comment.Comment>(parentIdIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!commentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("UserId_1")))
            {
                var commentUserIdIndex = Builders<DataLayer.Entities.General.Comment.Comment>.IndexKeys.Ascending(_ => _.UserId);
                generalOptions.Name = "UserId_1";
                await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Comment.Comment>(commentUserIdIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!commentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("ReferenceId_1")))
            {
                var commentReferenceId = Builders<DataLayer.Entities.General.Comment.Comment>.IndexKeys.Ascending(_ => _.ReferenceId);
                generalOptions.Name = "ReferenceId_1";
                await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Comment.Comment>(commentReferenceId, generalOptions),
                    cancellationToken: cancellationToken);
            }

            #endregion

            #region ProductSpecification
            var specificationCollection = db.GetCollection<DataLayer.Entities.Shop.ProductSpecification.ProductSpecification>("ProductSpecification");
            if (!specificationCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_1")))
            {
                var associatedDomainIdspecIndex = Builders<DataLayer.Entities.Shop.ProductSpecification.ProductSpecification>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
                generalOptions.Name = "AssociatedDomainId_1";
                await specificationCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductSpecification.ProductSpecification>(associatedDomainIdspecIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }
            if (!specificationCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("SpecificationGroupId_1")))
            {
                var specGroupIndex = Builders<DataLayer.Entities.Shop.ProductSpecification.ProductSpecification>.IndexKeys.Ascending(_ => _.SpecificationGroupId);
                generalOptions.Name = "SpecificationGroupId_1";
                await specificationCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductSpecification.ProductSpecification>(specGroupIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }
            #endregion

            #region ShoppingCart
            var shoppingCartCollection = db.GetCollection<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>("ShoppingCart");

            if (!shoppingCartCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_1")))
            {
                var shoppingCartDomainIndex = Builders<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
                generalOptions.Name = "AssociatedDomainId_1";
                await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>(shoppingCartDomainIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!shoppingCartCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("CreatorUserId_1")))
            {
                var shoppingCartCreatorUserIdIndex = Builders<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>.IndexKeys.Ascending(_ => _.CreatorUserId);
                generalOptions.Name = "CreatorUserId_1";
                await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>(shoppingCartCreatorUserIdIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!shoppingCartCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("IsActive_1")))
            {
                var shoppingCartIsActiveIndex = Builders<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>.IndexKeys.Ascending(_ => _.IsActive);
                generalOptions.Name = "IsActive_1";
                await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>(shoppingCartIsActiveIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!shoppingCartCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("IsDeleted_1")))
            {
                var shoppingCartIsDeletedIndex = Builders<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>.IndexKeys.Ascending(_ => _.IsDeleted);
                generalOptions.Name = "IsDeleted_1";
                await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>(shoppingCartIsDeletedIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }


            #endregion

            #region Transaction
            var transactionCollection = db.GetCollection<DataLayer.Entities.Shop.Transaction.Transaction>("Transaction");
            if (!transactionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"AssociatedDomainId_1")))
            {
                var transactionDomainIndex = Builders<DataLayer.Entities.Shop.Transaction.Transaction>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
                generalOptions.Name = "AssociatedDomainId_1";
                await transactionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Transaction.Transaction>(transactionDomainIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!transactionCollection.Indexes.List().ToList().Any(i => i.GetValue("name")
            .Equals("MainInvoiceNumber_1_AssociatedDomainId_1")))
            {
                var invoiceNumberInDomain = Builders<DataLayer.Entities.Shop.Transaction.Transaction>.IndexKeys.Ascending(_ => _.MainInvoiceNumber).Ascending(_ => _.AssociatedDomainId);
                generalOptions.Name = "MainInvoiceNumber_1_AssociatedDomainId_1";
                await transactionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Transaction.Transaction>(invoiceNumberInDomain, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!transactionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("CustomerData.UserId_1")))
            {
                var transactionUserIdIndex = Builders<DataLayer.Entities.Shop.Transaction.Transaction>.IndexKeys.Ascending(_ => _.CustomerData.UserId);
                generalOptions.Name = "CustomerData.UserId_1";
                await transactionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Transaction.Transaction>(transactionUserIdIndex, generalOptions),
                     cancellationToken: cancellationToken);
            }

            #endregion

            #region UserCoupon
            var userCouponCollection = db.GetCollection<DataLayer.Entities.Shop.Promotion.UserCoupon>("UserCoupon");
            if (!userCouponCollection.Indexes.List().ToList().Any(i => i.GetValue("name")
            .Equals("AssociatedDomainId_1_CouponCode_1")))
            {
                var userCouponDomainIndex = Builders<DataLayer.Entities.Shop.Promotion.UserCoupon>.IndexKeys.Ascending(_ => _.AssociatedDomainId).Ascending(_ => _.CouponCode);
                uniqueOption.Name = "AssociatedDomainId_1_CouponCode_1";
                await userCouponCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.UserCoupon>(userCouponDomainIndex, uniqueOption),
                   cancellationToken: cancellationToken);
            }


            if (!userCouponCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("PromotionId_1")))
            {
                var userCouponPromotionIndex = Builders<DataLayer.Entities.Shop.Promotion.UserCoupon>.IndexKeys.Ascending(_ => _.PromotionId);
                generalOptions.Name = "PromotionId_1";
                await userCouponCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.UserCoupon>(userCouponPromotionIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }
            #endregion

            #region UserFavorites
            var userFavouriteCollection = db.GetCollection<DataLayer.Entities.General.User.UserFavorites>("UserFavorites");
            if (!userFavouriteCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_1")))
            {
                var userFavoriteDomainIndex = Builders<DataLayer.Entities.General.User.UserFavorites>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
                generalOptions.Name = "AssociatedDomainId_1";
                await userFavouriteCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.UserFavorites>(userFavoriteDomainIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }
            if (!userFavouriteCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("EntityId_1")))
            {
                var userFavouriteEntityIdIndex = Builders<DataLayer.Entities.General.User.UserFavorites>.IndexKeys.Ascending(_ => _.EntityId);
                generalOptions.Name = "EntityId_1";
                await userFavouriteCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.UserFavorites>(userFavouriteEntityIdIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!userFavouriteCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("FavoriteType_1")))
            {
                var userFavoritesFavoriteTypeIndex = Builders<DataLayer.Entities.General.User.UserFavorites>.IndexKeys.Ascending(_ => _.FavoriteType);
                generalOptions.Name = "FavoriteType_1";
                await userFavouriteCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.UserFavorites>(userFavoritesFavoriteTypeIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }
            #endregion

            #endregion
        }

        public Task StopAsync(CancellationToken cancellationToken)
              => Task.CompletedTask;
    }
}
