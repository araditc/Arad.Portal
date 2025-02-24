using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.General.Comment;
using Arad.Portal.DataLayer.Entities.General.Content;
using Arad.Portal.DataLayer.Entities.General.ContentCategory;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Menu;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Entities.Shop.ProductGroup;
using Arad.Portal.DataLayer.Entities.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Entities.Shop.Transaction;

using Serilog;

namespace Arad.Portal.Helpers.Shared;

public class ConfigureMongoDbIndexesService : IHostedService
{
    private readonly IMongoDatabase _db;
    private readonly ILogger<ConfigureMongoDbIndexesService> _logger;
    public ConfigureMongoDbIndexesService(IConfiguration configuration, ILogger<ConfigureMongoDbIndexesService> logger)
    {
        _logger = logger;
        IConfiguration configuration1 = configuration;
        IMongoClient client = new MongoClient(configuration1["DatabaseConfig:ConnectionString"]);
        _db = client.GetDatabase(configuration1["DatabaseConfig:DbName"]);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        #region CreateIndexes

        CreateIndexOptions generalOptions = new() { Collation = new("en") };
        CreateIndexOptions uniqueOption = new() { Collation = new("en"), Unique = true };

        #region Content

        IMongoCollection<Content> contentCollection = _db.GetCollection<Content>("Content");
        if (!(await contentCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("CreationDate_-1")))
        {
            Log.Fatal("Dashboard : CreationDate_-1");
            IndexKeysDefinition<Content> creationDateIndex = Builders<Content>.IndexKeys.Descending(c => c.CreationDate);
            generalOptions.Name = "CreationDate_-1";
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<Content>(creationDateIndex, generalOptions),
                                                           cancellationToken: cancellationToken);
        }

        if (!(await contentCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_1")))
        {
            Log.Fatal("Dashboard : AssociatedDomainId_1");
            IndexKeysDefinition<Content> contentDomainIndex = Builders<Content>.IndexKeys.Ascending(c => c.AssociatedDomainId);
            generalOptions.Name = "AssociatedDomainId_1";
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<Content>(contentDomainIndex, generalOptions),
                                                           cancellationToken: cancellationToken);
        }


        if (!(await contentCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("ContentCategoryId_1")))
        {
            Log.Fatal("Dashboard : ContentCategoryId_1");
            IndexKeysDefinition<Content> categoryIdIndex = Builders<Content>.IndexKeys.Ascending(c => c.ContentCategoryId);
            generalOptions.Name = "ContentCategoryId_1";
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<Content>(categoryIdIndex, generalOptions),
                                                           cancellationToken: cancellationToken);
        }


        if (!(await contentCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("StartShowDate_1_EndShowDate_-1")))
        {
            Log.Fatal("Dashboard : StartShowDate_1_EndShowDate_-1");
            IndexKeysDefinition<Content> startEndShowDateIndex = Builders<Content>.IndexKeys.Ascending(c => c.StartShowDate).Descending(c => c.EndShowDate);
            generalOptions.Name = "StartShowDate_1_EndShowDate_-1";
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<Content>(startEndShowDateIndex, generalOptions),
                                                           cancellationToken: cancellationToken);
        }

        if (!(await contentCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("ContentCode_-1")))
        {
            Log.Fatal("Dashboard : ContentCode_-1");
            IndexKeysDefinition<Content> contentCodeIndex = Builders<Content>.IndexKeys.Descending(c => c.ContentCode);
            uniqueOption.Name = "ContentCode_-1";
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<Content>(contentCodeIndex, uniqueOption),
                                                           cancellationToken: cancellationToken);
        }

        if (!(await contentCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("IsActive_-1")))
        {
            Log.Fatal("Dashboard : IsActive_-1");
            IndexKeysDefinition<Content> isActiveIndex = Builders<Content>.IndexKeys.Descending(c => c.IsActive);
            generalOptions.Name = "IsActive_-1";
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<Content>(isActiveIndex, generalOptions),
                                                           cancellationToken: cancellationToken);
        }

        if (!(await contentCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("IsDeleted_-1")))
        {
            Log.Fatal("Dashboard : IsDeleted_-1");
            IndexKeysDefinition<Content> isDeletedIndex = Builders<Content>.IndexKeys.Descending(c => c.IsDeleted);
            generalOptions.Name = "IsDeleted_-1";
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<Content>(isDeletedIndex, generalOptions),
                                                           cancellationToken: cancellationToken);
        }

        if (!(await contentCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("LanguageId_-1")))
        {
            Log.Fatal("Dashboard : LanguageId_-1");
            IndexKeysDefinition<Content> contentLanguageIndex = Builders<Content>.IndexKeys.Descending(c => c.LanguageId);
            generalOptions.Name = "LanguageId_-1";
            await contentCollection.Indexes.CreateOneAsync(new CreateIndexModel<Content>(contentLanguageIndex, generalOptions),
                                                           cancellationToken: cancellationToken);
        }

        #endregion Content

        #region ContentCategory

        IMongoCollection<ContentCategory> contentCategoryCollection = _db.GetCollection<ContentCategory>("ContentCategory");

        if (!(await contentCategoryCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("ParentCategoryId_-1")))
        {
            Log.Fatal("Dashboard : ParentCategoryId_-1");
            IndexKeysDefinition<ContentCategory> parentCategoryIndex = Builders<ContentCategory>.IndexKeys.Descending(c => c.ParentCategoryId);
            generalOptions.Name = "ParentCategoryId_-1";
            await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<ContentCategory>(parentCategoryIndex, generalOptions),
                                                                   cancellationToken: cancellationToken);
        }

        if (!(await contentCategoryCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("CategoryType_-1")))
        {
            Log.Fatal("Dashboard : CategoryType_-1");
            IndexKeysDefinition<ContentCategory> categoryTypeIndex = Builders<ContentCategory>.IndexKeys.Descending(c => c.CategoryType);
            generalOptions.Name = "CategoryType_-1";
            await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<ContentCategory>(categoryTypeIndex, generalOptions),
                                                                   cancellationToken: cancellationToken);
        }

        if (!(await contentCategoryCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("CategoryCode_-1")))
        {
            Log.Fatal("Dashboard : CategoryCode_-1");
            IndexKeysDefinition<ContentCategory> categoryCodeIndex = Builders<ContentCategory>.IndexKeys.Descending(c => c.CategoryCode);
            uniqueOption.Name = "CategoryCode_-1";
            await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<ContentCategory>(categoryCodeIndex, uniqueOption),
                                                                   cancellationToken: cancellationToken);
        }

        if (!(await contentCategoryCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("IsActive_-1")))
        {
            Log.Fatal("Dashboard : IsActive_-1");
            IndexKeysDefinition<ContentCategory> categoryIsActiveIndex = Builders<ContentCategory>.IndexKeys.Descending(c => c.IsActive);
            generalOptions.Name = "IsActive_-1";
            await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<ContentCategory>(categoryIsActiveIndex, generalOptions),
                                                                   cancellationToken: cancellationToken);
        }

        if (!(await contentCategoryCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("IsDeleted_-1")))
        {
            Log.Fatal("Dashboard : IsDeleted_-1");
            IndexKeysDefinition<ContentCategory> categoryIsDeletedIndex = Builders<ContentCategory>.IndexKeys.Descending(c => c.IsDeleted);
            generalOptions.Name = "IsDeleted_-1";
            await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<ContentCategory>(categoryIsDeletedIndex, generalOptions),
                                                                   cancellationToken: cancellationToken);
        }
        if (!(await contentCategoryCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("CategoryNames.LanguageId_-1")))
        {
            Log.Fatal("Dashboard : CategoryNames.LanguageId_-1");
            IndexKeysDefinition<ContentCategory> contentCategoryLanguageIndex = Builders<ContentCategory>.IndexKeys.Descending("CategoryNames.LanguageId");
            generalOptions.Name = "CategoryNames.LanguageId_-1";
            await contentCategoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<ContentCategory>(contentCategoryLanguageIndex, generalOptions),
                                                                   cancellationToken: cancellationToken);
        }

        #endregion ContentCategory

        #region Domain
        IMongoCollection<Domain> domainCollection = _db.GetCollection<Domain>("Domain");
        if (!(await domainCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("DomainName_1")))
        {
            Log.Fatal("Dashboard : DomainName_1");
            IndexKeysDefinition<Domain> domainNameIndex = Builders<Domain>.IndexKeys.Ascending(c => c.DomainName);
            uniqueOption.Name = "DomainName_1";
            await domainCollection.Indexes.CreateOneAsync(new CreateIndexModel<Domain>(domainNameIndex, uniqueOption),
                                                          cancellationToken: cancellationToken);
        }
        #endregion 

        #region Menu
        IMongoCollection<Menu> menuCollection = _db.GetCollection<Menu>("Menu");

        if (!(await menuCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_-1")))
        {
            Log.Fatal("Dashboard : AssociatedDomainId_-1");
            IndexKeysDefinition<Menu> domainMenuIndex = Builders<Menu>.IndexKeys.Descending(c => c.AssociatedDomainId);
            generalOptions.Name = "AssociatedDomainId_-1";
            await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<Menu>(domainMenuIndex, generalOptions),
                                                        cancellationToken: cancellationToken);
        }

        if (!(await menuCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_1_MenuCode_1")))
        {
            Log.Fatal("Dashboard : AssociatedDomainId_1_MenuCode_1");
            IndexKeysDefinition<Menu> menuCodeIndex = Builders<Menu>.IndexKeys.Ascending(c => c.AssociatedDomainId).Ascending(c => c.MenuCode);
            generalOptions.Name = "AssociatedDomainId_1_MenuCode_1";
            await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<Menu>(menuCodeIndex, generalOptions),
                                                        cancellationToken: cancellationToken);
        }

        if (!(await menuCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("Url_-1_AssociatedDomainId_1")))
        {
            Log.Fatal("Dashboard : Url_-1_AssociatedDomainId_1");
            IndexKeysDefinition<Menu> menuUrlIndex = Builders<Menu>.IndexKeys.Descending(c => c.Url).Ascending(c => c.AssociatedDomainId);
            generalOptions.Name = "Url_-1_AssociatedDomainId_1";
            await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<Menu>(menuUrlIndex, generalOptions),
                                                        cancellationToken: cancellationToken);
        }

        if (!(await menuCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("MenuTitles.LanguageId_-1")))
        {
            Log.Fatal("Dashboard : MenuTitles.LanguageId_-1");
            IndexKeysDefinition<Menu> menuLanguageIndex = Builders<Menu>.IndexKeys.Descending("MenuTitles.LanguageId");
            generalOptions.Name = "MenuTitles.LanguageId_-1";
            await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<Menu>(menuLanguageIndex, generalOptions),
                                                        cancellationToken: cancellationToken);
        }


        if (!(await menuCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("IsDeleted_-1")))
        {
            Log.Fatal("Dashboard : IsDeleted_-1");
            IndexKeysDefinition<Menu> isDeletedMenuIndex = Builders<Menu>.IndexKeys.Descending(c => c.IsDeleted);
            generalOptions.Name = "IsDeleted_-1";
            await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<Menu>(isDeletedMenuIndex, generalOptions),
                                                        cancellationToken: cancellationToken);
        }


        if (!(await menuCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("IsActive_-1")))
        {
            Log.Fatal("Dashboard : IsActive_-1");
            IndexKeysDefinition<Menu> isActiveMenuIndex = Builders<Menu>.IndexKeys.Descending(c => c.IsActive);
            generalOptions.Name = "IsActive_-1";
            await menuCollection.Indexes.CreateOneAsync(new CreateIndexModel<Menu>(isActiveMenuIndex, generalOptions),
                                                        cancellationToken: cancellationToken);
        }

        #endregion Menu

        #region Product
        IMongoCollection<Product> productCollection = _db.GetCollection<Product>("Product");
        if (!(await productCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_-1")))
        {
            IndexKeysDefinition<Product> productDomainIndex = Builders<Product>.IndexKeys.Descending(c => c.AssociatedDomainId);
            generalOptions.Name = "AssociatedDomainId_-1";
            await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<Product>(productDomainIndex, generalOptions),
                                                           cancellationToken: cancellationToken);
        }

        if (!(await productCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("CreationDate_-1")))
        {
            IndexKeysDefinition<Product> productCreationDateIndex = Builders<Product>.IndexKeys.Descending(c => c.CreationDate);
            generalOptions.Name = "CreationDate_-1";
            await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<Product>(productCreationDateIndex, generalOptions),
                                                           cancellationToken: cancellationToken);
        }

        if (!(await productCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("ProductCode_1")))
        {
            IndexKeysDefinition<Product> productCodeIndex = Builders<Product>.IndexKeys.Ascending(c => c.ProductCode);
            uniqueOption.Name = "ProductCode_1";
            await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<Product>(productCodeIndex, uniqueOption),
                                                           cancellationToken: cancellationToken);
        }

        if (!(await productCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("UniqueCode_1_AssociatedDomainId_1")))
        {
            IndexKeysDefinition<Product> uniqueCodeIndex = Builders<Product>.IndexKeys.Ascending(c => c.UniqueCode).Ascending(c => c.AssociatedDomainId);
            uniqueOption.Name = "UniqueCode_1_AssociatedDomainId_1";
            await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<Product>(uniqueCodeIndex, uniqueOption),
                                                           cancellationToken: cancellationToken);
        }
        if (!(await productCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("MultiLingualProperties.LanguageId_1")))
        {
            IndexKeysDefinition<Product> productLanguageIndex = Builders<Product>.IndexKeys.Ascending("MultiLingualProperties.LanguageId");
            generalOptions.Name = "MultiLingualProperties.LanguageId_1";
            await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<Product>(productLanguageIndex, generalOptions),
                                                           cancellationToken: cancellationToken);
        }

        if (!(await productCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("IsDeleted_1")))
        {
            IndexKeysDefinition<Product> deletedProductIndex = Builders<Product>.IndexKeys.Ascending(c => c.IsDeleted);
            generalOptions.Name = "IsDeleted_1";
            await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<Product>(deletedProductIndex, generalOptions),
                                                           cancellationToken: cancellationToken);
        }

        if (!(await productCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("IsActive_1")))
        {
            IndexKeysDefinition<Product> activeProductIndex = Builders<Product>.IndexKeys.Ascending(c => c.IsActive);
            generalOptions.Name = "IsActive_1";
            await productCollection.Indexes.CreateOneAsync(new CreateIndexModel<Product>(activeProductIndex, generalOptions),
                                                           cancellationToken: cancellationToken);
        }
        #endregion Product

        #region ProductGroup
        IMongoCollection<ProductGroup> productGroupCollection = _db.GetCollection<ProductGroup>("ProductGroup");
        if (!(await productGroupCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("GroupCode_1")))
        {
            IndexKeysDefinition<ProductGroup> groupCodeIndex = Builders<ProductGroup>.IndexKeys.Ascending(c => c.GroupCode);
            uniqueOption.Name = "GroupCode_1";
            await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProductGroup>(groupCodeIndex, uniqueOption),
                                                                cancellationToken: cancellationToken);
        }

        if (!(await productGroupCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("ParentId_1")))
        {
            IndexKeysDefinition<ProductGroup> parentGroupIndex = Builders<ProductGroup>.IndexKeys.Ascending(c => c.ParentId);
            generalOptions.Name = "ParentId_1";
            await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProductGroup>(parentGroupIndex, generalOptions),
                                                                cancellationToken: cancellationToken);
        }

        if (!(await productGroupCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("MultiLingualProperties.LanguageId_1")))
        {
            IndexKeysDefinition<ProductGroup> productGroupLanguageIndex = Builders<ProductGroup>.IndexKeys.Ascending("MultiLingualProperties.LanguageId");
            generalOptions.Name = "MultiLingualProperties.LanguageId_1";
            await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProductGroup>(productGroupLanguageIndex, generalOptions),
                                                                cancellationToken: cancellationToken);
        }


        if (!(await productGroupCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("IsDeleted_1")))
        {
            IndexKeysDefinition<ProductGroup> deletedGroupIndex = Builders<ProductGroup>.IndexKeys.Ascending(c => c.IsDeleted);
            generalOptions.Name = "IsDeleted_1";
            await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProductGroup>(deletedGroupIndex, generalOptions),
                                                                cancellationToken: cancellationToken);
        }

        if (!(await productGroupCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("IsActive_1")))
        {
            IndexKeysDefinition<ProductGroup> activeGroupIndex = Builders<ProductGroup>.IndexKeys.Ascending(c => c.IsActive);
            generalOptions.Name = "IsActive_1";
            await productGroupCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProductGroup>(activeGroupIndex, generalOptions),
                                                                cancellationToken: cancellationToken);
        }
        #endregion

        #region Promotion
        IMongoCollection<Promotion> promotionCollection = _db.GetCollection<Promotion>("Promotion");

        if (!(await promotionCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_1")))
        {
            IndexKeysDefinition<Promotion> promotionDomainIndex = Builders<Promotion>.IndexKeys.Ascending(c => c.AssociatedDomainId);
            generalOptions.Name = "AssociatedDomainId_1";
            await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<Promotion>(promotionDomainIndex, generalOptions),
                                                             cancellationToken: cancellationToken);
        }

        if (!(await promotionCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("IsActive_1")))
        {
            IndexKeysDefinition<Promotion> activePromotionIndex = Builders<Promotion>.IndexKeys.Ascending(c => c.IsActive);
            generalOptions.Name = "IsActive_1";
            await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<Promotion>(activePromotionIndex, generalOptions),
                                                             cancellationToken: cancellationToken);
        }

        if (!(await promotionCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("IsDeleted_1")))
        {
            IndexKeysDefinition<Promotion> deletedPromotionIndex = Builders<Promotion>.IndexKeys.Ascending(c => c.IsDeleted);
            generalOptions.Name = "IsDeleted_1";
            await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<Promotion>(deletedPromotionIndex, generalOptions),
                                                             cancellationToken: cancellationToken);
        }

        if (!(await promotionCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("PromotionType_1")))
        {

            IndexKeysDefinition<Promotion> promotionTypeIndex = Builders<Promotion>.IndexKeys.Ascending(c => c.PromotionType);
            generalOptions.Name = "PromotionType_1";
            await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<Promotion>(promotionTypeIndex, generalOptions),
                                                             cancellationToken: cancellationToken);
        }

        if (!(await promotionCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("DiscountType_1")))
        {

            IndexKeysDefinition<Promotion> discountTypeIndex = Builders<Promotion>.IndexKeys.Ascending(c => c.DiscountType);
            generalOptions.Name = "DiscountType_1";
            await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<Promotion>(discountTypeIndex, generalOptions),
                                                             cancellationToken: cancellationToken);
        }


        if (!(await promotionCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("CurrencyId_1")))
        {
            IndexKeysDefinition<Promotion> currencyIndex = Builders<Promotion>.IndexKeys.Ascending(c => c.CurrencyId);
            generalOptions.Name = "CurrencyId_1";
            await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<Promotion>(currencyIndex, generalOptions),
                                                             cancellationToken: cancellationToken);
        }

        if (!(await promotionCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("SDate_1_EDate_-1")))
        {
            IndexKeysDefinition<Promotion> startEndShowDatePromotionIndex = Builders<Promotion>.IndexKeys.Ascending(c => c.SDate).Descending(c => c.EDate);
            generalOptions.Name = "SDate_1_EDate_-1";
            await promotionCollection.Indexes.CreateOneAsync(new CreateIndexModel<Promotion>(startEndShowDatePromotionIndex, generalOptions),
                                                             cancellationToken: cancellationToken);
        }
        #endregion

        #region ApplicationUser
        IMongoCollection<ApplicationUser> userCollection = _db.GetCollection<ApplicationUser>("Users");

        if (!(await userCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("UserName_1")))
        {
            IndexKeysDefinition<ApplicationUser> userNameInDomain = Builders<ApplicationUser>.IndexKeys.Ascending(c => c.UserName);
            uniqueOption.Name = "UserName_1";
            await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<ApplicationUser>(userNameInDomain, uniqueOption),
                                                        cancellationToken: cancellationToken);
        }

        if (!(await userCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("Profile.DefaultCurrencyId_1")))
        {
            IndexKeysDefinition<ApplicationUser> defaultCurrencyIdIndex = Builders<ApplicationUser>.IndexKeys.Ascending(c => c.Profile.DefaultCurrencyId);
            generalOptions.Name = "Profile.DefaultCurrencyId_1";
            await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<ApplicationUser>(defaultCurrencyIdIndex, generalOptions),
                                                        cancellationToken: cancellationToken);
        }

        if (!(await userCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("Profile.DefaultLanguageId_1")))
        {
            IndexKeysDefinition<ApplicationUser> defaultlanguageId = Builders<ApplicationUser>.IndexKeys.Ascending(c => c.Profile.DefaultLanguageId);
            generalOptions.Name = "Profile.DefaultLanguageId_1";
            await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<ApplicationUser>(defaultlanguageId, generalOptions),
                                                        cancellationToken: cancellationToken);
        }
        #endregion

        #region Comment

        IMongoCollection<Comment> commentCollection = _db.GetCollection<Comment>("Comment");
        if (!(await commentCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_1")))
        {
            IndexKeysDefinition<Comment> associatedDomainIdIndex = Builders<Comment>.IndexKeys.Ascending(c => c.AssociatedDomainId);
            generalOptions.Name = "AssociatedDomainId_1";
            await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<Comment>(associatedDomainIdIndex, generalOptions),
                                                           cancellationToken: cancellationToken);
        }
        if (!(await commentCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("ParentId_1")))
        {
            IndexKeysDefinition<Comment> parentIdIndex = Builders<Comment>.IndexKeys.Ascending(c => c.ParentId);
            generalOptions.Name = "ParentId_1";
            await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<Comment>(parentIdIndex, generalOptions),
                                                           cancellationToken: cancellationToken);
        }

        if (!(await commentCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("UserId_1")))
        {
            IndexKeysDefinition<Comment> commentUserIdIndex = Builders<Comment>.IndexKeys.Ascending(c => c.UserId);
            generalOptions.Name = "UserId_1";
            await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<Comment>(commentUserIdIndex, generalOptions),
                                                           cancellationToken: cancellationToken);
        }

        if (!(await commentCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("ReferenceId_1")))
        {
            IndexKeysDefinition<Comment> commentReferenceId = Builders<Comment>.IndexKeys.Ascending(c => c.ReferenceId);
            generalOptions.Name = "ReferenceId_1";
            await commentCollection.Indexes.CreateOneAsync(new CreateIndexModel<Comment>(commentReferenceId, generalOptions),
                                                           cancellationToken: cancellationToken);
        }

        #endregion

        #region ProductSpecification
        IMongoCollection<ProductSpecification> specificationCollection = _db.GetCollection<ProductSpecification>("ProductSpecification");
        if (!(await specificationCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_1")))
        {
            IndexKeysDefinition<ProductSpecification> associatedDomainIdspecIndex = Builders<ProductSpecification>.IndexKeys.Ascending(c => c.AssociatedDomainId);
            generalOptions.Name = "AssociatedDomainId_1";
            await specificationCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProductSpecification>(associatedDomainIdspecIndex, generalOptions),
                                                                 cancellationToken: cancellationToken);
        }
        if (!(await specificationCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("SpecificationGroupId_1")))
        {
            IndexKeysDefinition<ProductSpecification> specGroupIndex = Builders<ProductSpecification>.IndexKeys.Ascending(c => c.SpecificationGroupId);
            generalOptions.Name = "SpecificationGroupId_1";
            await specificationCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProductSpecification>(specGroupIndex, generalOptions),
                                                                 cancellationToken: cancellationToken);
        }
        #endregion

        #region ShoppingCart
        IMongoCollection<ShoppingCart> shoppingCartCollection = _db.GetCollection<ShoppingCart>("ShoppingCart");

        if (!(await shoppingCartCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_1")))
        {
            IndexKeysDefinition<ShoppingCart> shoppingCartDomainIndex = Builders<ShoppingCart>.IndexKeys.Ascending(c => c.AssociatedDomainId);
            generalOptions.Name = "AssociatedDomainId_1";
            await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<ShoppingCart>(shoppingCartDomainIndex, generalOptions),
                                                                cancellationToken: cancellationToken);
        }

        if (!(await shoppingCartCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("CreatorUserId_1")))
        {
            IndexKeysDefinition<ShoppingCart> shoppingCartCreatorUserIdIndex = Builders<ShoppingCart>.IndexKeys.Ascending(c => c.CreatorUserId);
            generalOptions.Name = "CreatorUserId_1";
            await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<ShoppingCart>(shoppingCartCreatorUserIdIndex, generalOptions),
                                                                cancellationToken: cancellationToken);
        }

        if (!(await shoppingCartCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("IsActive_1")))
        {
            IndexKeysDefinition<ShoppingCart> shoppingCartIsActiveIndex = Builders<ShoppingCart>.IndexKeys.Ascending(c => c.IsActive);
            generalOptions.Name = "IsActive_1";
            await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<ShoppingCart>(shoppingCartIsActiveIndex, generalOptions),
                                                                cancellationToken: cancellationToken);
        }

        if (!(await shoppingCartCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("IsDeleted_1")))
        {
            IndexKeysDefinition<ShoppingCart> shoppingCartIsDeletedIndex = Builders<ShoppingCart>.IndexKeys.Ascending(c => c.IsDeleted);
            generalOptions.Name = "IsDeleted_1";
            await shoppingCartCollection.Indexes.CreateOneAsync(new CreateIndexModel<ShoppingCart>(shoppingCartIsDeletedIndex, generalOptions),
                                                                cancellationToken: cancellationToken);
        }


        #endregion

        #region Transaction
        IMongoCollection<Transaction> transactionCollection = _db.GetCollection<Transaction>("Transaction");
        if (!(await transactionCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals($"AssociatedDomainId_1")))
        {
            IndexKeysDefinition<Transaction> transactionDomainIndex = Builders<Transaction>.IndexKeys.Ascending(c => c.AssociatedDomainId);
            generalOptions.Name = "AssociatedDomainId_1";
            await transactionCollection.Indexes.CreateOneAsync(new CreateIndexModel<Transaction>(transactionDomainIndex, generalOptions),
                                                               cancellationToken: cancellationToken);
        }

        if (!(await transactionCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name")
                                                                                                   .Equals("MainInvoiceNumber_1_AssociatedDomainId_1")))
        {
            IndexKeysDefinition<Transaction> invoiceNumberInDomain = Builders<Transaction>.IndexKeys.Ascending(c => c.MainInvoiceNumber).Ascending(c => c.AssociatedDomainId);
            generalOptions.Name = "MainInvoiceNumber_1_AssociatedDomainId_1";
            await transactionCollection.Indexes.CreateOneAsync(new CreateIndexModel<Transaction>(invoiceNumberInDomain, generalOptions),
                                                               cancellationToken: cancellationToken);
        }

        if (!(await transactionCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("CustomerData.UserId_1")))
        {
            IndexKeysDefinition<Transaction> transactionUserIdIndex = Builders<Transaction>.IndexKeys.Ascending(c => c.CustomerData.UserId);
            generalOptions.Name = "CustomerData.UserId_1";
            await transactionCollection.Indexes.CreateOneAsync(new CreateIndexModel<Transaction>(transactionUserIdIndex, generalOptions),
                                                               cancellationToken: cancellationToken);
        }

        #endregion

        #region UserCoupon
        IMongoCollection<UserCoupon> userCouponCollection = _db.GetCollection<UserCoupon>("UserCoupon");
        if (!(await userCouponCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name")
                                                                                                  .Equals("AssociatedDomainId_1_CouponCode_1")))
        {
            IndexKeysDefinition<UserCoupon> userCouponDomainIndex = Builders<UserCoupon>.IndexKeys.Ascending(c => c.AssociatedDomainId).Ascending(c => c.CouponCode);
            uniqueOption.Name = "AssociatedDomainId_1_CouponCode_1";
            await userCouponCollection.Indexes.CreateOneAsync(new CreateIndexModel<UserCoupon>(userCouponDomainIndex, uniqueOption),
                                                              cancellationToken: cancellationToken);
        }


        if (!(await userCouponCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("PromotionId_1")))
        {
            IndexKeysDefinition<UserCoupon> userCouponPromotionIndex = Builders<UserCoupon>.IndexKeys.Ascending(c => c.PromotionId);
            generalOptions.Name = "PromotionId_1";
            await userCouponCollection.Indexes.CreateOneAsync(new CreateIndexModel<UserCoupon>(userCouponPromotionIndex, generalOptions),
                                                              cancellationToken: cancellationToken);
        }
        #endregion

        #region UserFavorites
        IMongoCollection<UserFavorites> userFavouriteCollection = _db.GetCollection<UserFavorites>("UserFavorites");
        if (!(await userFavouriteCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("AssociatedDomainId_1")))
        {
            IndexKeysDefinition<UserFavorites> userFavoriteDomainIndex = Builders<UserFavorites>.IndexKeys.Ascending(c => c.AssociatedDomainId);
            generalOptions.Name = "AssociatedDomainId_1";
            await userFavouriteCollection.Indexes.CreateOneAsync(new CreateIndexModel<UserFavorites>(userFavoriteDomainIndex, generalOptions),
                                                                 cancellationToken: cancellationToken);
        }
        if (!(await userFavouriteCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("EntityId_1")))
        {
            IndexKeysDefinition<UserFavorites> userFavouriteEntityIdIndex = Builders<UserFavorites>.IndexKeys.Ascending(c => c.EntityId);
            generalOptions.Name = "EntityId_1";
            await userFavouriteCollection.Indexes.CreateOneAsync(new CreateIndexModel<UserFavorites>(userFavouriteEntityIdIndex, generalOptions),
                                                                 cancellationToken: cancellationToken);
        }

        if (!(await userFavouriteCollection.Indexes.ListAsync(cancellationToken)).ToList().Any(i => i.GetValue("name").Equals("FavoriteType_1")))
        {
            IndexKeysDefinition<UserFavorites> userFavoritesFavoriteTypeIndex = Builders<UserFavorites>.IndexKeys.Ascending(c => c.FavoriteType);
            generalOptions.Name = "FavoriteType_1";
            await userFavouriteCollection.Indexes.CreateOneAsync(new CreateIndexModel<UserFavorites>(userFavoritesFavoriteTypeIndex, generalOptions),
                                                                 cancellationToken: cancellationToken);
        }
        #endregion

        #endregion
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}