using Arad.Portal.DataLayer.Repositories.Interfaces.General.BasicData;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.BasicData.Mongo;

public class BasicDataRepository : Repository<Entities.General.BasicData.BasicData>, IBasicDataRepository
{
    public BasicDataRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }    
}