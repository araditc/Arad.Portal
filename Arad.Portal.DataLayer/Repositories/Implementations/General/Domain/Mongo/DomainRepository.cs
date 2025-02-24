using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.Domain.Mongo;

public class DomainRepository : Repository<Entities.General.Domain.Domain>, IDomainRepository
{
    public DomainRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}