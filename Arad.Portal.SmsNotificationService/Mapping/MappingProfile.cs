using Arad.Portal.DataLayer.Entities.General.Notify;
using Arad.Portal.DataLayer.Models.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.SmsNotificationService.Mapping
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            CreateMap<NotificationDTO, Notification>().ReverseMap();
        }
       
    }
}
