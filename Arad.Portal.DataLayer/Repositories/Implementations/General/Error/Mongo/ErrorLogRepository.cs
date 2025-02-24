using Arad.Portal.DataLayer.Repositories.Interfaces.General.Error;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.Error.Mongo;

public class ErrorLogRepository : Repository<Entities.General.Error.ErrorLog>, IErrorLogRepository
{
    public ErrorLogRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}