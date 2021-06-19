using Arad.Portal.DataLayer.Entities.General.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Models.Permission;

namespace Arad.Portal.DataLayer.Contracts.General.User
{
    public interface IUserRepository
    {
        List<PermissionDTO> GetPermissionsOfUser(ApplicationUser user);
    }
}
