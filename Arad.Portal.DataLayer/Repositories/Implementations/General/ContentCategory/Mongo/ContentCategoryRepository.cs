using Arad.Portal.DataLayer.Repositories.Interfaces.General.ContentCategory;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.ContentCategory.Mongo;

public class ContentCategoryRepository : Repository<Entities.General.ContentCategory.ContentCategory>, IContentCategoryRepository
{
    public ContentCategoryRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}