using Arad.Portal.DataLayer.Contracts.General.Notification;
using Arad.Portal.DataLayer.Models.Notification;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.General.Notification.Mongo
{
    public class NotificationRepository : BaseRepository, INotificationRepository
    {
        private readonly NotificationContext _context;
        private readonly IMapper _mapper;
        public NotificationRepository(NotificationContext notificationContext, IMapper mapper,
            IHttpContextAccessor httpContextAccessor): base(httpContextAccessor)
        {
            _context = notificationContext;
            _mapper = mapper;
        }
        public RepositoryOperationResult AddNewNotification(NotificationDTO dto)
        {
            RepositoryOperationResult result = new RepositoryOperationResult();
            var equallentModel = _mapper.Map<Entities.General.Notification.Notification>(dto);

            equallentModel.CreationDate = DateTime.Now;
            equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            try
            {
                equallentModel.NotificationId = Guid.NewGuid().ToString();
                _context.Collection.InsertOne(equallentModel);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }
            return result;
        }
    }
}
