using Arad.Portal.DataLayer.Repositories.Interfaces.General.Role;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;
using Arad.Portal.DataLayer.Entities.General.ApplicationRole;


namespace Arad.Portal.DataLayer.Repositories.Implementations.General.Role.Mongo;

public class RoleRepository : Repository<ApplicationRole>, IRoleRepository
{
    public RoleRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}