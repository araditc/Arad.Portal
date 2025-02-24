using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.Shop.ShoppingCart.Mongo;

public class ShoppingCartRepository : Repository<Entities.Shop.ShoppingCart.ShoppingCart>, IShoppingCartRepository
{
    public ShoppingCartRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}