﻿using Microsoft.Extensions.Configuration;
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
            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Content.Content.CreationDate)}_1")))
            {

                var creationDateIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.CreationDate);
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(creationDateIndex, generalOptions),
                   cancellationToken: cancellationToken);
            }

            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Content.Content.AssociatedDomainId)}_1")))
            {
                var contentDomainIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(contentDomainIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }


            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Content.Content.ContentCategoryId)}_1")))
            {
                var categoryIdIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Ascending(_ => _.ContentCategoryId);
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(categoryIdIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }


            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Content.Content.StartShowDate)}_1_{nameof(DataLayer.Entities.General.Content.Content.EndShowDate)}_1")))
            {
                var startEndShowDateIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Ascending(_ => _.StartShowDate).Descending(_ => _.EndShowDate);
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(startEndShowDateIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Content.Content.ContentCode)}_1")))
            {
                var contentCodeIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.ContentCode);
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(contentCodeIndex, uniqueOption),
                    cancellationToken: cancellationToken);
            }

            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Content.Content.IsActive)}_1")))
            {
                var isActiveIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.IsActive);
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(isActiveIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Content.Content.IsDeleted)}_1")))
            {
                var isDeletedIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.IsDeleted);
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(isDeletedIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Content.Content.LanguageId)}_1")))
            {
                var contentLanguageIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.LanguageId);
                await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(contentLanguageIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            #endregion Content

            #region ContentCategory

            var contentCategoryCollection = db.GetCollection<DataLayer.Entities.General.ContentCategory.ContentCategory>("ContentCategory");

            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.ContentCategory.ContentCategory.ParentCategoryId)}_1")))
            {
                var parentCategoryIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.ParentCategoryId);
                await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(parentCategoryIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.ContentCategory.ContentCategory.CategoryType)}_1")))
            {
                var categoryTypeIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.CategoryType);
                await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryTypeIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.ContentCategory.ContentCategory.CategoryCode)}_1")))
            {
                var categoryCodeIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.CategoryCode);
                await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryCodeIndex, uniqueOption),
                    cancellationToken: cancellationToken);
            }

            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.ContentCategory.ContentCategory.IsActive)}_1")))
            {
                var categoryIsActiveIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.IsActive);
                await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryIsActiveIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.ContentCategory.ContentCategory.IsDeleted)}_1")))
            {
                var categoryIsDeletedIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.IsDeleted);
                await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryIsDeletedIndex, generalOptions),
                     cancellationToken: cancellationToken);
            }
            if (!contentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("CategoryNames.LanguageId_1")))
            {
                var contentCategoryLanguageIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending("CategoryNames.LanguageId");
                await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(contentCategoryLanguageIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            #endregion ContentCategory

            #region Menu
            var menuCollection = db.GetCollection<DataLayer.Entities.General.Menu.Menu>("Menu");

            if (!menuCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Menu.Menu.AssociatedDomainId)}_1")))
            {
                var domainMenuIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.AssociatedDomainId);
                await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(domainMenuIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!menuCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Menu.Menu.MenuCode)}_1")))
            {
                var menuCodeIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.MenuCode);
                await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(menuCodeIndex, uniqueOption),
                     cancellationToken: cancellationToken);
            }

            if (!menuCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Menu.Menu.Url)}_1")))
            {
                var menuUrlIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.Url);
                await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(menuUrlIndex, uniqueOption),
                    cancellationToken: cancellationToken);
            }

            if (!menuCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("MenuTitles.LanguageId_1")))
            {
                var menuLanguageIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending("MenuTitles.LanguageId");
                await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(menuLanguageIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }


            if (!menuCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Menu.Menu.IsDeleted)}_1")))
            {
                var isDeletedMenuIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.IsDeleted);
                await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(isDeletedMenuIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }


            if (!menuCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Menu.Menu.IsActive)}_1")))
            {
                var isActiveMenuIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.IsActive);
                await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(isActiveMenuIndex, generalOptions),
                  cancellationToken: cancellationToken);
            }

            #endregion Menu

            #region Product
            var productCollection = db.GetCollection<DataLayer.Entities.Shop.Product.Product>("Product");
            if (!productCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Product.Product.AssociatedDomainId)}_1")))
            {
                var productDomainIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Descending(_ => _.AssociatedDomainId);
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productDomainIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!productCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Product.Product.CreationDate)}_1")))
            {
                var productCreationDateIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Descending(_ => _.CreationDate);
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productCreationDateIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!productCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Product.Product.ProductCode)}_1")))
            {
                var productCodeIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending(_ => _.ProductCode);
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productCodeIndex, uniqueOption),
                    cancellationToken: cancellationToken);
            }

            if (!productCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Product.Product.UniqueCode)}_1_{nameof(DataLayer.Entities.Shop.Product.Product.AssociatedDomainId)}_1")))
            {
                var uniqueCodeIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending(_ => _.UniqueCode).Ascending(_ => _.AssociatedDomainId);
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(uniqueCodeIndex, uniqueOption),
                    cancellationToken: cancellationToken);
            }
            if (!productCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("MultiLingualProperties.LanguageId_1")))
            {
                var productLanguageIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending("MultiLingualProperties.LanguageId");
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productLanguageIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!productCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Product.Product.IsDeleted)}_1")))
            {
                var deletedProductIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending(_ => _.IsDeleted);
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(deletedProductIndex),
                     cancellationToken: cancellationToken);
            }

            if (!productCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Product.Product.IsActive)}_1")))
            {
                var activeProductIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending(_ => _.IsActive);
                await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(activeProductIndex),
                  cancellationToken: cancellationToken);
            }
            #endregion Product

            #region ProductGroup
            var productGroupCollection = db.GetCollection<DataLayer.Entities.Shop.ProductGroup.ProductGroup>("ProductGroup");
            if (!productGroupCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.ProductGroup.ProductGroup.GroupCode)}_1")))
            {
                var groupCodeIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending(_ => _.GroupCode);
                await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(groupCodeIndex, uniqueOption),
                    cancellationToken: cancellationToken);
            }

            if (!productGroupCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.ProductGroup.ProductGroup.ParentId)}_1")))
            {
                var parentGroupIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending(_ => _.ParentId);
                await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(parentGroupIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!productGroupCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals("MultiLingualProperties.LanguageId_1")))
            {
                var productGroupLanguageIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending("MultiLingualProperties.LanguageId");
                await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(productGroupLanguageIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }


            if (!productGroupCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.ProductGroup.ProductGroup.IsDeleted)}_1")))
            {
                var deletedGroupIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending(_ => _.IsDeleted);
                await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(deletedGroupIndex, uniqueOption),
                   cancellationToken: cancellationToken);
            }

            if (!productGroupCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.ProductGroup.ProductGroup.IsActive)}_1")))
            {
                var activeGroupIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending(_ => _.IsActive);
                await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(activeGroupIndex, uniqueOption),
                    cancellationToken: cancellationToken);
            }
            #endregion

            #region Promotion
            var promotionCollection = db.GetCollection<DataLayer.Entities.Shop.Promotion.Promotion>("Promotion");

            if (!promotionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Promotion.Promotion.AssociatedDomainId)}_1")))
            {
                var promotionDomainIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(promotionDomainIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!promotionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Promotion.Promotion.IsActive)}_1")))
            {
                var activePromotionIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.IsActive);
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(activePromotionIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!promotionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Promotion.Promotion.IsDeleted)}_1")))
            {
                var deletedPromotionIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.IsDeleted);
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(deletedPromotionIndex, generalOptions),
                   cancellationToken: cancellationToken);
            }

            if (!promotionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Promotion.Promotion.PromotionType)}_1")))
            {

                var promotionTypeIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.PromotionType);
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(promotionTypeIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!promotionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Promotion.Promotion.DiscountType)}_1")))
            {

                var discountTypeIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.DiscountType);
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(discountTypeIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }


            if (!promotionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Promotion.Promotion.CurrencyId)}_1")))
            {
                var currencyIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.CurrencyId);
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(currencyIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (!promotionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Promotion.Promotion.SDate)}_1_{nameof(DataLayer.Entities.Shop.Promotion.Promotion.EDate)}")))
            {
                var startEndShowDatePromotionIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.SDate).Descending(_ => _.EDate);
                await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(startEndShowDatePromotionIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }
            #endregion

            #region ApplicationUser
            var userCollection = db.GetCollection<DataLayer.Entities.General.User.ApplicationUser>("ApplicationUser");

            if (userCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.User.ApplicationUser.UserName)}_1")))
            {
                var userNameInDomain = Builders<DataLayer.Entities.General.User.ApplicationUser>.IndexKeys.Ascending(_ => _.UserName).Ascending(_ => _.DomainId);
                await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.ApplicationUser>(userNameInDomain, uniqueOption),
                    cancellationToken: cancellationToken);
            }

            if (userCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.User.ApplicationUser.Profile.DefaultCurrencyId)}_1")))
            {
                var defaultCurrencyIdIndex = Builders<DataLayer.Entities.General.User.ApplicationUser>.IndexKeys.Ascending(_ => _.Profile.DefaultCurrencyId);
                await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.ApplicationUser>(defaultCurrencyIdIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (userCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.User.ApplicationUser.Profile.DefaultLanguageId)}_1")))
            {
                var defaultlanguageId = Builders<DataLayer.Entities.General.User.ApplicationUser>.IndexKeys.Ascending(_ => _.Profile.DefaultLanguageId);
                await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.ApplicationUser>(defaultlanguageId, generalOptions),
                    cancellationToken: cancellationToken);
            }
            #endregion

            #region Comment

            var commentCollection = db.GetCollection<DataLayer.Entities.General.Comment.Comment>("Comment");
            if (commentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Comment.Comment.AssociatedDomainId)}_1")))
            {
                var associatedDomainIdIndex = Builders<DataLayer.Entities.General.Comment.Comment>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
                await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Comment.Comment>(associatedDomainIdIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }
            if (commentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Comment.Comment.ParentId)}_1")))
            {
                var parentIdIndex = Builders<DataLayer.Entities.General.Comment.Comment>.IndexKeys.Ascending(_ => _.ParentId);
                await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Comment.Comment>(parentIdIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (commentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Comment.Comment.UserId)}_1")))
            {
                var commentUserIdIndex = Builders<DataLayer.Entities.General.Comment.Comment>.IndexKeys.Ascending(_ => _.UserId);
                await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Comment.Comment>(commentUserIdIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (commentCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.Comment.Comment.ReferenceId)}_1")))
            {
                var commentReferenceId = Builders<DataLayer.Entities.General.Comment.Comment>.IndexKeys.Ascending(_ => _.ReferenceId);
                await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Comment.Comment>(commentReferenceId, generalOptions),
                    cancellationToken: cancellationToken);
            }

            #endregion

            #region ProductSpecification
            var specificationCollection = db.GetCollection<DataLayer.Entities.Shop.ProductSpecification.ProductSpecification>("ProductSpecification");
            if (specificationCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.ProductSpecification.ProductSpecification.AssociatedDomainId)}_1")))
            {
                var associatedDomainIdspecIndex = Builders<DataLayer.Entities.Shop.ProductSpecification.ProductSpecification>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
                await specificationCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductSpecification.ProductSpecification>(associatedDomainIdspecIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }
            if (specificationCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.ProductSpecification.ProductSpecification.SpecificationGroupId)}_1")))
            {
                var specGroupIndex = Builders<DataLayer.Entities.Shop.ProductSpecification.ProductSpecification>.IndexKeys.Ascending(_ => _.SpecificationGroupId);
                await specificationCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductSpecification.ProductSpecification>(specGroupIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }
            #endregion

            #region ShoppingCart
            var shoppingCartCollection = db.GetCollection<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>("ShoppingCart");

            if (shoppingCartCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.ShoppingCart.ShoppingCart.AssociatedDomainId)}_1")))
            {
                var shoppingCartDomainIndex = Builders<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
                await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>(shoppingCartDomainIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (shoppingCartCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.ShoppingCart.ShoppingCart.CreatorUserId)}_1")))
            {
                var shoppingCartCreatorUserIdIndex = Builders<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>.IndexKeys.Ascending(_ => _.CreatorUserId);
                await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>(shoppingCartCreatorUserIdIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (shoppingCartCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.ShoppingCart.ShoppingCart.IsActive)}_1")))
            {
                var shoppingCartIsActiveIndex = Builders<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>.IndexKeys.Ascending(_ => _.IsActive);
                await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>(shoppingCartIsActiveIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (shoppingCartCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.ShoppingCart.ShoppingCart.IsDeleted)}_1")))
            {
                var shoppingCartIsDeletedIndex = Builders<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>.IndexKeys.Ascending(_ => _.IsDeleted);
                await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>(shoppingCartIsDeletedIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }


            #endregion

            #region Transaction
            var transactionCollection = db.GetCollection<DataLayer.Entities.Shop.Transaction.Transaction>("Transaction");
            if (transactionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Transaction.Transaction.AssociatedDomainId)}_1")))
            {
                var transactionDomainIndex = Builders<DataLayer.Entities.Shop.Transaction.Transaction>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
                await transactionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Transaction.Transaction>(transactionDomainIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (transactionCollection.Indexes.List().ToList().Any(i => i.GetValue("name")
            .Equals($"{nameof(DataLayer.Entities.Shop.Transaction.Transaction.MainInvoiceNumber)}_1_{nameof(DataLayer.Entities.Shop.Transaction.Transaction.AssociatedDomainId)}_1")))
            {
                var invoiceNumberInDomain = Builders<DataLayer.Entities.Shop.Transaction.Transaction>.IndexKeys.Ascending(_ => _.MainInvoiceNumber).Ascending(_ => _.AssociatedDomainId);
                await transactionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Transaction.Transaction>(invoiceNumberInDomain, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (transactionCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Transaction.Transaction.CustomerData.UserId)}_1")))
            {
                var transactionUserIdIndex = Builders<DataLayer.Entities.Shop.Transaction.Transaction>.IndexKeys.Ascending(_ => _.CustomerData.UserId);
                await transactionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Transaction.Transaction>(transactionUserIdIndex, generalOptions),
                     cancellationToken: cancellationToken);
            }

            #endregion

            #region UserCoupon
            var userCouponCollection = db.GetCollection<DataLayer.Entities.Shop.Promotion.UserCoupon>("UserCoupon");
            if (userCouponCollection.Indexes.List().ToList().Any(i => i.GetValue("name")
            .Equals($"{nameof(DataLayer.Entities.Shop.Promotion.UserCoupon.AssociatedDomainId)}_1_{nameof(DataLayer.Entities.Shop.Promotion.UserCoupon.CouponCode)}_1")))
            {
                var userCouponDomainIndex = Builders<DataLayer.Entities.Shop.Promotion.UserCoupon>.IndexKeys.Ascending(_ => _.AssociatedDomainId).Ascending(_ => _.CouponCode);
                await userCouponCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.UserCoupon>(userCouponDomainIndex, generalOptions),
                   cancellationToken: cancellationToken);
            }


            if (userCouponCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.Shop.Promotion.UserCoupon.PromotionId)}_1")))
            {
                var userCouponPromotionIndex = Builders<DataLayer.Entities.Shop.Promotion.UserCoupon>.IndexKeys.Ascending(_ => _.PromotionId);
                await userCouponCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.UserCoupon>(userCouponPromotionIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }
            #endregion

            #region UserFavorites
            var userFavouriteCollection = db.GetCollection<DataLayer.Entities.General.User.UserFavorites>("UserFavorites");
            if (userFavouriteCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.User.UserFavorites.AssociatedDomainId)}_1")))
            {
                var userFavoriteDomainIndex = Builders<DataLayer.Entities.General.User.UserFavorites>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
                await userFavouriteCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.UserFavorites>(userFavoriteDomainIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }
            if (userFavouriteCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.User.UserFavorites.EntityId)}_1")))
            {
                var userFavouriteEntityIdIndex = Builders<DataLayer.Entities.General.User.UserFavorites>.IndexKeys.Ascending(_ => _.EntityId);
                await userFavouriteCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.UserFavorites>(userFavouriteEntityIdIndex, generalOptions),
                    cancellationToken: cancellationToken);
            }

            if (userFavouriteCollection.Indexes.List().ToList().Any(i => i.GetValue("name").Equals($"{nameof(DataLayer.Entities.General.User.UserFavorites.FavoriteType)}_1")))
            {
                var userFavoritesFavoriteTypeIndex = Builders<DataLayer.Entities.General.User.UserFavorites>.IndexKeys.Ascending(_ => _.FavoriteType);
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
