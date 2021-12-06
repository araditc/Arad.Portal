using Arad.Portal.DataLayer.Entities.General.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.Email
{
    public interface ISMTPRepository
    {
        Task<SMTP> GetDefault();

        Task<bool> ExistsDefault(string id);
    }
}
