using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.Shop.ProductSpecification.Mongo;

public class ProductSpecificationRepository : Repository<Entities.Shop.ProductSpecification.ProductSpecification>, IProductSpecificationRepository
{
    public ProductSpecificationRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}