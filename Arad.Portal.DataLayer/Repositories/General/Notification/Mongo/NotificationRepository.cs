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
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Threading;
using static Arad.Portal.DataLayer.Models.Shared.Enums;
using Arad.Portal.DataLayer.Entities.General.Notify;
using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Entities.General.Email;
using Arad.Portal.GeneralLibrary.Utilities;

namespace Arad.Portal.DataLayer.Repositories.General.Notification.Mongo
{
    public class NotificationRepository : BaseRepository, INotificationRepository
    {
        private readonly NotificationContext _context;
        private readonly IMapper _mapper;
        private readonly Setting _setting;
        public NotificationRepository(NotificationContext notificationContext, 
            IMapper mapper,
            Setting setting,
            IHttpContextAccessor httpContextAccessor): base(httpContextAccessor)
        {
            _context = notificationContext;
            _mapper = mapper;
            _setting = setting;
        }
        public Result AddNewNotification(NotificationDTO dto)
        {
            Result result = new Result();
            var equallentModel = _mapper.Map<Entities.General.Notify.Notification>(dto);

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

        public async Task<Result> AddRange(IEnumerable<Entities.General.Notify.Notification> entities)
        {
            try
            {
                await _context.Collection.InsertManyAsync(entities);

                return new() { Succeeded = true, Message = Arad.Portal.GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_OperationSuccess") };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new() { Succeeded = false, Message = Arad.Portal.GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_OperationError") };
            }
        }

        public async Task<List<Entities.General.Notify.Notification>> GetForSend(NotificationType notificationType)
        {
           List<Entities.General.Notify.Notification> notifications = await _context.Collection.AsQueryable()
                                                              .Where(n => (n.SendStatus == NotificationSendStatus.Store || n.SendStatus == NotificationSendStatus.Error) &&
                                                                          n.ScheduleDate <= DateTime.Now.ToUniversalTime() && n.Type == notificationType)
                                                              .Take(notificationType == NotificationType.Email ? _setting.HowManyEmailsSendEachTime : _setting.HowManySmsSendEachTime)
                                                              .ToListAsync();

            foreach (Entities.General.Notify.Notification notification in notifications)
            {
                notification.SendStatus = NotificationSendStatus.Sending;
                await _context.Collection.ReplaceOneAsync(c => c.NotificationId == notification.NotificationId, notification);
            }

            return notifications.ToList();
        }

        public async Task<Result> Update(Entities.General.Notify.Notification entity, string modificationReason, CancellationToken cancellationToken)
        {
            try
            {
               Entities.General.Notify.Notification found = await _context.Collection.AsQueryable()
                    .FirstOrDefaultAsync(c => c.NotificationId == entity.NotificationId, cancellationToken);

                if (found == null)
                {
                    return new() { Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_NotFound"), Succeeded = false };
                }

                List<Modification> modifications = found.Modifications;
                modifications.Add(new() { UserName = GetUserName(), DateTime = DateTime.Now, ModificationReason = modificationReason, UserId = GetUserId() });

                entity.Modifications = modifications;
                entity.CreatorUserId = found.CreatorUserId;
                entity.CreationDate = found.CreationDate;
                entity.CreatorUserName = found.CreatorUserName;
                entity.IsActive = found.IsActive;
                await _context.Collection.ReplaceOneAsync(c => c.NotificationId == entity.NotificationId, entity, new ReplaceOptions(), cancellationToken);

                return new() { Succeeded = true, Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_OperationSuccess") };
            }
            catch
            {
                return new() { Succeeded = false, Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_OperationError") };
            }
        }

        public async Task<Result> UpdateMany(List<string> notificationIds, UpdateDefinition<Entities.General.Notify.Notification> definitions)
        {
            UpdateResult updateResult = await _context.Collection.UpdateManyAsync(_ => notificationIds.Contains(_.NotificationId), definitions);
            if(updateResult.IsAcknowledged)
            {
                return new() { Succeeded = true, Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_OperationSuccess") };
            }
            else
            {
                return new() { Succeeded = false, Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_OperationError") };
            }
            
        }
        public  async Task<Result> Update(Entities.General.Notify.Notification entity, string modificationReason = "", bool isWorker = false)
        {
            try
            {
                entity.Modifications.Add(new() { UserName = this.GetUserName(), DateTime = DateTime.Now, ModificationReason = modificationReason, UserId = this.GetUserId() });
                await _context.Collection.ReplaceOneAsync(c => c.NotificationId == entity.NotificationId, entity);

                return new() { Succeeded = true, Message = isWorker ? "Operation success" : GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_OperationSuccess") };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new() { Succeeded = false, Message = isWorker ? "Operation Failed" : GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_OperationError") };
            }
        }
        public async Task UpdateSmtp(SMTP smtp)
        {
            List<Entities.General.Notify.Notification> notifications = await _context.Collection.Find(t => t.SMTP.SMTPId.Equals(smtp.SMTPId)).ToListAsync();

            foreach (Entities.General.Notify.Notification notification in notifications)
            {
                notification.SMTP = smtp;
                await _context.Collection.ReplaceOneAsync(c => c.NotificationId == notification.NotificationId, notification);
            }
        }

       
    }
}
