using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductUnit;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.Shop.ProductUnit.Mongo;

public class ProductUnitRepository : Repository<Entities.Shop.ProductUnit.ProductUnit>, IProductUnitRepository
{
    public ProductUnitRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}