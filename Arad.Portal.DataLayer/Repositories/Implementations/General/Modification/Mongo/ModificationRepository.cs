using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.Modification.Mongo;

public class ModificationRepository(IMongoDbContext dbContext) : Repository<Entities.General.Modification.Modification>(dbContext), IModificationRepository;
