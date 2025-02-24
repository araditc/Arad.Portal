using Arad.Portal.DataLayer.Repositories.Interfaces.General.Menu;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.Menu.Mongo;

public class MenuRepository : Repository<Entities.General.Menu.Menu>, IMenuRepository
{
    public MenuRepository(IMongoDbContext dbContext) : base(dbContext)
    {
            
    }
}