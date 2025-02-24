using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.Language.Mongo;

public class LanguageRepository : Repository<Entities.General.Language.Language>, ILanguageRepository
{
    public LanguageRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}