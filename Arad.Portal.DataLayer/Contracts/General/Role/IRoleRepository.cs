using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.Role
{
    public interface IRoleRepository
    {
        Task<Entities.General.Role.Role> FetchRole(string roleId);
    }
}
