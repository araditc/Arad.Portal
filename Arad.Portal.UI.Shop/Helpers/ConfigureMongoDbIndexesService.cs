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


            var creationDateIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.CreationDate);
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(creationDateIndex, generalOptions),
               cancellationToken: cancellationToken);

            var contentDomainIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(contentDomainIndex, generalOptions),
                cancellationToken: cancellationToken);

            var categoryIdIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Ascending(_ => _.ContentCategoryId);
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(categoryIdIndex, generalOptions),
                cancellationToken: cancellationToken);


            var startEndShowDateIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Ascending(_ => _.StartShowDate).Descending(_ => _.EndShowDate);

            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(startEndShowDateIndex, generalOptions),
                cancellationToken: cancellationToken);


            var contentCodeIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.ContentCode);
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(contentCodeIndex, uniqueOption),
                cancellationToken: cancellationToken);

            var isActiveIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.IsActive);
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(isActiveIndex, generalOptions),
                cancellationToken: cancellationToken);

            var isDeletedIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.IsDeleted);
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(isDeletedIndex, generalOptions),
                cancellationToken: cancellationToken);

            var contentLanguageIndex = Builders<DataLayer.Entities.General.Content.Content>.IndexKeys.Descending(_ => _.LanguageId);
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Content.Content>(contentLanguageIndex, generalOptions),
                cancellationToken: cancellationToken);
            #endregion Content

            #region ContentCategory
            var contentCategoryCollection = db.GetCollection<DataLayer.Entities.General.ContentCategory.ContentCategory>("ContentCategory");


            var parentCategoryIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.ParentCategoryId);
            await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(parentCategoryIndex, generalOptions),
                cancellationToken: cancellationToken);


            var categoryTypeIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.CategoryType);
            await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryTypeIndex, generalOptions),
                cancellationToken: cancellationToken);

            var categoryCodeIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.CategoryCode);
            await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryCodeIndex, uniqueOption),
                cancellationToken: cancellationToken);

            var categoryIsActiveIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.IsActive);
            await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryIsActiveIndex, generalOptions),
                cancellationToken: cancellationToken);

            var categoryIsDeletedIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending(_ => _.IsDeleted);
            await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(categoryIsDeletedIndex, generalOptions),
                 cancellationToken: cancellationToken);

            var contentCategoryLanguageIndex = Builders<DataLayer.Entities.General.ContentCategory.ContentCategory>.IndexKeys.Descending("CategoryNames.LanguageId");
            await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.ContentCategory.ContentCategory>(contentCategoryLanguageIndex, generalOptions),
                cancellationToken: cancellationToken);
            #endregion ContentCategory

            #region Menu
            var menuCollection = db.GetCollection<DataLayer.Entities.General.Menu.Menu>("Menu");

            var domainMenuIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.AssociatedDomainId);
            await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(domainMenuIndex, generalOptions),
                cancellationToken: cancellationToken);


            var menuCodeIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.MenuCode);
            await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(menuCodeIndex, uniqueOption),
                 cancellationToken: cancellationToken);

            var menuUrlIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.Url);
            await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(menuUrlIndex, uniqueOption),
                cancellationToken: cancellationToken);

            var menuLanguageIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending("MenuTitles.LanguageId");
            await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(menuLanguageIndex, generalOptions),
                cancellationToken: cancellationToken);

            var isDeletedMenuIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.IsDeleted);
            await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(isDeletedMenuIndex, generalOptions),
                cancellationToken: cancellationToken);

            var isActiveMenuIndex = Builders<DataLayer.Entities.General.Menu.Menu>.IndexKeys.Descending(_ => _.IsActive);
            await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Menu.Menu>(isActiveMenuIndex, generalOptions),
              cancellationToken: cancellationToken);

            #endregion Menu

            #region Product
            var productCollection = db.GetCollection<DataLayer.Entities.Shop.Product.Product>("Product");
            var productDomainIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Descending(_ => _.AssociatedDomainId);
            await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productDomainIndex, generalOptions),
                cancellationToken: cancellationToken);

            var productCreationDateIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Descending(_ => _.CreationDate);
            await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productCreationDateIndex, generalOptions),
                cancellationToken: cancellationToken);

            var productCodeIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending(_ => _.ProductCode);
            await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productCodeIndex, uniqueOption),
                cancellationToken: cancellationToken);

            var uniqueCodeIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending(_ => _.UniqueCode).Ascending(_ => _.AssociatedDomainId);
            await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(uniqueCodeIndex, uniqueOption),
                cancellationToken: cancellationToken);

            var productLanguageIndex = Builders<DataLayer.Entities.Shop.Product.Product>.IndexKeys.Ascending("MultiLingualProperties.LanguageId");
            await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Product.Product>(productLanguageIndex, generalOptions),
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
            await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(groupCodeIndex, uniqueOption),
                cancellationToken: cancellationToken);

            var parentGroupIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending(_ => _.ParentId);
            await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(parentGroupIndex, generalOptions),
                cancellationToken: cancellationToken);

            var productGroupLanguageIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending("MultiLingualProperties.LanguageId");
            await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(productGroupLanguageIndex, generalOptions),
                cancellationToken: cancellationToken);

            var deletedGroupIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending(_ => _.IsDeleted);
            await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(deletedGroupIndex, uniqueOption),
               cancellationToken: cancellationToken);

            var activeGroupIndex = Builders<DataLayer.Entities.Shop.ProductGroup.ProductGroup>.IndexKeys.Ascending(_ => _.IsActive);
            await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductGroup.ProductGroup>(activeGroupIndex, uniqueOption),
                cancellationToken: cancellationToken);
            #endregion

            #region Promotion
            var promotionCollection = db.GetCollection<DataLayer.Entities.Shop.Promotion.Promotion>("Promotion");

            var promotionDomainIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
            await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(promotionDomainIndex, generalOptions),
                cancellationToken: cancellationToken);

            var activePromotionIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.IsActive);
            await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(activePromotionIndex, generalOptions),
                cancellationToken: cancellationToken);

            var deletedPromotionIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.IsDeleted);
            await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(deletedPromotionIndex, generalOptions),
               cancellationToken: cancellationToken);

            var promotionTypeIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.PromotionType);
            await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(promotionTypeIndex, generalOptions),
                cancellationToken: cancellationToken);

            var discountTypeIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.DiscountType);
            await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(discountTypeIndex, generalOptions),
                cancellationToken: cancellationToken);

            var currencyIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.CurrencyId);
            await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(currencyIndex, generalOptions),
                cancellationToken: cancellationToken);

            var startEndShowDatePromotionIndex = Builders<DataLayer.Entities.Shop.Promotion.Promotion>.IndexKeys.Ascending(_ => _.SDate).Descending(_ => _.EDate);
            await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.Promotion>(startEndShowDatePromotionIndex, generalOptions),
                cancellationToken: cancellationToken);
            #endregion

            #region ApplicationUser
            var userCollection = db.GetCollection<DataLayer.Entities.General.User.ApplicationUser>("ApplicationUser");
            var userNameInDomain = Builders<DataLayer.Entities.General.User.ApplicationUser>.IndexKeys.Ascending(_ => _.UserName).Ascending(_ => _.DomainId);
            await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.ApplicationUser>(userNameInDomain, uniqueOption),
                cancellationToken: cancellationToken);

            var defaultCurrencyIdIndex = Builders<DataLayer.Entities.General.User.ApplicationUser>.IndexKeys.Ascending(_ => _.Profile.DefaultCurrencyId);
            await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.ApplicationUser>(defaultCurrencyIdIndex, generalOptions),
                cancellationToken: cancellationToken);

            var defaultlanguageId = Builders<DataLayer.Entities.General.User.ApplicationUser>.IndexKeys.Ascending(_ => _.Profile.DefaultLanguageId);
            await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.ApplicationUser>(defaultlanguageId, generalOptions),
                cancellationToken: cancellationToken);
            #endregion

            #region Comment

            var commentCollection = db.GetCollection<DataLayer.Entities.General.Comment.Comment>("Comment");

            var associatedDomainIdIndex = Builders<DataLayer.Entities.General.Comment.Comment>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
            await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Comment.Comment>(associatedDomainIdIndex, generalOptions),
                cancellationToken: cancellationToken);

            var parentIdIndex = Builders<DataLayer.Entities.General.Comment.Comment>.IndexKeys.Ascending(_ => _.ParentId);
            await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Comment.Comment>(parentIdIndex, generalOptions),
                cancellationToken: cancellationToken);

            var commentUserIdIndex = Builders<DataLayer.Entities.General.Comment.Comment>.IndexKeys.Ascending(_ => _.UserId);
            await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Comment.Comment>(commentUserIdIndex, generalOptions),
                cancellationToken: cancellationToken);

            var commentReferenceId = Builders<DataLayer.Entities.General.Comment.Comment>.IndexKeys.Ascending(_ => _.ReferenceId);
            await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.Comment.Comment>(commentReferenceId, generalOptions),
                cancellationToken: cancellationToken);

            #endregion

            #region ProductSpecification
            var specificationCollection = db.GetCollection<DataLayer.Entities.Shop.ProductSpecification.ProductSpecification>("ProductSpecification");

            var associatedDomainIdspecIndex = Builders<DataLayer.Entities.Shop.ProductSpecification.ProductSpecification>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
            await specificationCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductSpecification.ProductSpecification>(associatedDomainIdspecIndex, generalOptions),
                cancellationToken: cancellationToken);

            var specGroupIndex = Builders<DataLayer.Entities.Shop.ProductSpecification.ProductSpecification>.IndexKeys.Ascending(_ => _.SpecificationGroupId);
            await specificationCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ProductSpecification.ProductSpecification>(specGroupIndex, generalOptions),
                cancellationToken: cancellationToken);
            #endregion

            #region ShoppingCart
            var shoppingCartCollection = db.GetCollection<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>("ShoppingCart");

            var shoppingCartDomainIndex = Builders<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
            await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>(shoppingCartDomainIndex, generalOptions),
                cancellationToken: cancellationToken);

            var shoppingCartCreatorUserIdIndex = Builders<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>.IndexKeys.Ascending(_ => _.CreatorUserId);
            await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>(shoppingCartCreatorUserIdIndex, generalOptions),
                cancellationToken: cancellationToken);

            var shoppingCartIsActiveIndex = Builders<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>.IndexKeys.Ascending(_ => _.IsActive);
            await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>(shoppingCartIsActiveIndex, generalOptions),
                cancellationToken: cancellationToken);

            var shoppingCartIsDeletedIndex = Builders<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>.IndexKeys.Ascending(_ => _.IsDeleted);
            await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.ShoppingCart.ShoppingCart>(shoppingCartIsDeletedIndex, generalOptions),
                cancellationToken: cancellationToken);


            #endregion

            #region Transaction
            var transactionCollection = db.GetCollection<DataLayer.Entities.Shop.Transaction.Transaction>("Transaction");

            var transactionDomainIndex = Builders<DataLayer.Entities.Shop.Transaction.Transaction>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
            await transactionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Transaction.Transaction>(transactionDomainIndex, generalOptions),
                cancellationToken: cancellationToken);

            var invoiceNumberInDomain = Builders<DataLayer.Entities.Shop.Transaction.Transaction>.IndexKeys.Ascending(_ => _.MainInvoiceNumber).Ascending(_ => _.AssociatedDomainId);
            await transactionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Transaction.Transaction>(invoiceNumberInDomain, generalOptions),
                cancellationToken: cancellationToken);

            var transactionUserIdIndex = Builders<DataLayer.Entities.Shop.Transaction.Transaction>.IndexKeys.Ascending(_ => _.CustomerData.UserId);
            await transactionCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Transaction.Transaction>(transactionUserIdIndex, generalOptions),
                 cancellationToken: cancellationToken);

            #endregion

            #region UserCoupon
            var userCouponCollection = db.GetCollection<DataLayer.Entities.Shop.Promotion.UserCoupon>("UserCoupon");

            var userCouponDomainIndex = Builders<DataLayer.Entities.Shop.Promotion.UserCoupon>.IndexKeys.Ascending(_ => _.AssociatedDomainId).Ascending(_ => _.CouponCode);
            await userCouponCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.UserCoupon>(userCouponDomainIndex, generalOptions),
               cancellationToken: cancellationToken);

            var userCouponPromotionIndex = Builders<DataLayer.Entities.Shop.Promotion.UserCoupon>.IndexKeys.Ascending(_ => _.PromotionId);
            await userCouponCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.Shop.Promotion.UserCoupon>(userCouponPromotionIndex, generalOptions),
                cancellationToken: cancellationToken);
            #endregion

            #region UserFavorites
            var userFavouriteCollection = db.GetCollection<DataLayer.Entities.General.User.UserFavorites>("UserFavorites");

            var userFavoriteDomainIndex = Builders<DataLayer.Entities.General.User.UserFavorites>.IndexKeys.Ascending(_ => _.AssociatedDomainId);
            await userFavouriteCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.UserFavorites>(userFavoriteDomainIndex, generalOptions),
                cancellationToken: cancellationToken);

            var userFavouriteEntityIdIndex = Builders<DataLayer.Entities.General.User.UserFavorites>.IndexKeys.Ascending(_ => _.EntityId);
            await userFavouriteCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.UserFavorites>(userFavouriteEntityIdIndex, generalOptions),
                cancellationToken: cancellationToken);

            var userFavoritesFavoriteTypeIndex = Builders<DataLayer.Entities.General.User.UserFavorites>.IndexKeys.Ascending(_ => _.FavoriteType);
            await userFavouriteCollection.Indexes.CreateOneAsync(new CreateIndexModel<DataLayer.Entities.General.User.UserFavorites>(userFavoritesFavoriteTypeIndex, generalOptions),
                cancellationToken: cancellationToken);
            #endregion

            #endregion
        }

        public Task StopAsync(CancellationToken cancellationToken)
              => Task.CompletedTask;
    }
}
