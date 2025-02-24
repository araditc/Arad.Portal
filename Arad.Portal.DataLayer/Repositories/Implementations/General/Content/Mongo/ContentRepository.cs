using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Content;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.Content.Mongo;

public class ContentRepository : Repository<Entities.General.Content.Content>, IContentRepository
{
    public ContentRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}