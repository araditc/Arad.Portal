using Arad.Portal.DataLayer.Models.Notification;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.Notification
{
    public interface INotificationRepository
    {
        RepositoryOperationResult AddNewNotification(NotificationDTO model);
    }
}
