using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SliderModule;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.SliderModule.Mongo;

public class SliderRepository : Repository<Entities.General.SliderModule.Slider>, ISliderRepository
{
    public SliderRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}