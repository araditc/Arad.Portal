using Arad.Portal.DataLayer.Entities.General.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Models.Permission;
using Arad.Portal.DataLayer.Models.User;
using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.DataLayer.Contracts.General.User
{
    public interface IUserRepository
    {
        List<PermissionDTO> GetPermissionsOfUser(ApplicationUser user);
        Task<PagedItems<UserDTO>> List(UserSearchParams searchParam, string currentUserId);
        List<string> GetAccessibleRoutsOfUser(ApplicationUser user);
        List<string> GetRoleNamesOfUser(string userId);
        UserDTO GetUserWithPhone(string phoneNumber);
        List<UserDTO> GetAll();
        List<UserDTO> search(string word);
        bool CountPhone(string phoneNumber, string userPhone);
    }
}
