using Arad.Portal.DataLayer.Entities.General.Email;
using Arad.Portal.DataLayer.Models.Notification;
using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Contracts.General.Notification
{
    public interface INotificationRepository
    {
        Result AddNewNotification(NotificationDTO model);
        Task<List<Entities.General.Notify.Notification>> GetForSend(NotificationType notificationType);
        Task UpdateSmtp(SMTP smtp);
        Task<Result> Update(Entities.General.Notify.Notification entity, string modificationReason,
            CancellationToken cancellationToken);
        Task<Result> Update(Entities.General.Notify.Notification entity, 
            string modificationReason = "", bool isWorker = false);
        Task<Result> AddRange(IEnumerable<Entities.General.Notify.Notification> entities);
        Task<Result> UpdateMany(List<string> notificationIds, 
            UpdateDefinition<Entities.General.Notify.Notification> definitions);
    }
}
