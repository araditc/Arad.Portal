using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.SystemSetting
{
    public interface ISystemSettingRepository
    {
        Task<List<Entities.General.SystemSetting.SystemSetting>> GetAll();
    }
}
