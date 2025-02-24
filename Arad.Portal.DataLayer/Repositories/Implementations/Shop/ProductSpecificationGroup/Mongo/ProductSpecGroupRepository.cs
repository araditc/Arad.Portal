using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.Shop.ProductSpecificationGroup.Mongo;

public class ProductSpecGroupRepository : Repository<Entities.Shop.ProductSpecificationGroup.ProductSpecGroup>, IProductSpecGroupRepository
{
    public ProductSpecGroupRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}