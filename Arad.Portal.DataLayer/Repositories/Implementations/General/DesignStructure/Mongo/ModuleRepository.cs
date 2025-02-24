using Arad.Portal.DataLayer.Repositories.Interfaces.General.DesignStructure;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.DesignStructure.Mongo;

public class ModuleRepository : Repository<Entities.General.DesignStructure.Module>, IModuleRepository
{
    public ModuleRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}