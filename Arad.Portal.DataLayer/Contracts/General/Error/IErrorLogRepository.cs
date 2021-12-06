
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.Error
{
    public interface IErrorLogRepository
    {
         Task<Result> Add(Entities.General.Error.ErrorLog entity);
    }
}
