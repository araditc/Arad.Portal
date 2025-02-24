using Arad.Portal.DataLayer.Repositories.Interfaces.General.Currency;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.Currency.Mongo;

public class CurrencyRepository : Repository<Entities.General.Currency.Currency>, ICurrencyRepository
{
    public CurrencyRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}