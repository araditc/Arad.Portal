using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductGroup;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.Shop.ProductGroup.Mongo;

public class ProductGroupRepository : Repository<Entities.Shop.ProductGroup.ProductGroup>, IProductGroupRepository
{
    public ProductGroupRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}