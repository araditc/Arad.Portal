using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;


namespace Arad.Portal.DataLayer.Repositories.Implementations.Shop.Product.Mongo;

public class BaseProductRepository : Repository<Entities.Shop.Product.BaseProduct>, IBaseProductRepository
{
    public BaseProductRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}