using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.User.Mongo;

public class UserRepository : Repository<ApplicationUser>, IUserRepository
{
    public UserRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}