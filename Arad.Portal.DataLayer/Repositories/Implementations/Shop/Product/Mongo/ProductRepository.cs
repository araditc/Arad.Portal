using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.Shop.Product.Mongo;

public class ProductRepository : Repository<Entities.Shop.Product.Product>, IProductRepository
{
    public ProductRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}