using Arad.Portal.DataLayer.Repositories.Interfaces.General.Notification;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.Notification.Mongo;

public class NotificationRepository(IMongoDbContext dbContext) : Repository<Entities.General.Notify.Notification>(dbContext), INotificationRepository;