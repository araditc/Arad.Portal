using Arad.Portal.DataLayer.Repositories.Interfaces.General.Permission;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.Permission.Mongo;

public class PermissionRepository : Repository<Entities.General.Permission.Permission>, IPermissionRepository
{
    public PermissionRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}