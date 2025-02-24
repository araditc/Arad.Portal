using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Promotion;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.Shop.Promotion.Mongo;

public class PromotionRepository : Repository<Entities.Shop.Promotion.Promotion>, IPromotionRepository
{
    public PromotionRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}