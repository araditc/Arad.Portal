using Arad.Portal.DataLayer.Repositories.Interfaces.General.Services;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;


namespace Arad.Portal.DataLayer.Repositories.Implementations.General.Service.Mongo;

public class ProviderRepository : Repository<Entities.General.Service.Provider>, IProviderRepository
{
    public ProviderRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}