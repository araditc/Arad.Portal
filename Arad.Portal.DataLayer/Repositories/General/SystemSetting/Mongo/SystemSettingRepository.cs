using Arad.Portal.DataLayer.Contracts.General.SystemSetting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.General.SystemSetting.Mongo
{
    public class SystemSettingRepository : BaseRepository, ISystemSettingRepository
    {
        private readonly SystemSettingContext _context;
        public SystemSettingRepository(SystemSettingContext context, IHttpContextAccessor accessor):base(accessor)
        {
            _context = context;
        }

        public  async Task<List<Entities.General.SystemSetting.SystemSetting>> GetAll() => await _context.Collection.Find(t => true).ToListAsync();
    }
}
