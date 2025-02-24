using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SystemSetting;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.SystemSetting.Mongo;

public class SystemSettingRepository : Repository<Entities.General.SystemSetting.SystemSetting>, ISystemSettingRepository
{
    public SystemSettingRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}