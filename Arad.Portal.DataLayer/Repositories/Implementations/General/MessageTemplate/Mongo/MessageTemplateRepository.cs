using Arad.Portal.DataLayer.Repositories.Interfaces.General.MessageTemplate;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.MessageTemplate.Mongo;

public class MessageTemplateRepository(IMongoDbContext dbContext) : Repository<Entities.General.MessageTemplate.MessageTemplate>(dbContext), IMessageTemplateRepository;