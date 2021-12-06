using Arad.Portal.DataLayer.Contracts.General.Error;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.Utilities;

namespace Arad.Portal.DataLayer.Repositories.General.Error.Mongo
{
    public class ErrorLogRepository: BaseRepository, IErrorLogRepository
    {
        private readonly ErrorLogContext _context;
        public ErrorLogRepository(
            ErrorLogContext context,
            IHttpContextAccessor httpContextAccessor):base(httpContextAccessor)
        {
            _context = context;
        }

        public virtual async Task<Result> Add(Entities.General.Error.ErrorLog entity)
        {
            try
            {
                entity.ErrorLogId = Guid.NewGuid().ToString();
                entity.CreationDate = DateTime.Now;
                entity.CreatorUserId = this.GetUserId();
                entity.CreatorUserName = this.GetUserName();
                await _context.Collection.InsertOneAsync(entity);

                return new() { Succeeded = true, 
                    Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_OperationSuccess") };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new() { Succeeded = false, 
                    Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_OperationError") };
            }
        }
    }
}
