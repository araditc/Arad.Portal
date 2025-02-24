using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SendMessage;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.SendMessage.Mongo;

public class SendMessageRepository : Repository<Entities.General.SendMessage.SendMessage>, ISendMessageRepository
{
    public SendMessageRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}