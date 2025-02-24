using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Setting;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.Shop.Setting.Mongo;

public class ShippingSettingRepository : Repository<Entities.Shop.Setting.ShippingSetting>, IShippingSettingRepository
{
    public ShippingSettingRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}