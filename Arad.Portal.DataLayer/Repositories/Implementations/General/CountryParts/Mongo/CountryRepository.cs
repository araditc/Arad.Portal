using Arad.Portal.DataLayer.Repositories.Interfaces.General.CountryParts;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;
using Arad.Portal.DataLayer.Entities.General.CountryParts;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.CountryParts.Mongo;

public class CountryRepository : Repository<Country>, ICountryRepository
{
    public CountryRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}