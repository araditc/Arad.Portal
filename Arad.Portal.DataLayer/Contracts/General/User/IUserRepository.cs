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
        List<Entities.General.Permission.Permission> GetPermissionsOfUser(ApplicationUser user);
        List<string> GetAccessibleRoutsOfUser(ApplicationUser user);
        string GetRoleNameOfUser(string userId);
        UserDTO GetUserWithPhone(string phoneNumber);
        List<UserDTO> GetAll();
        List<UserDTO> Search(string word);
        List<SelectListModel> GetAllProductType();

        List<SelectListModel> GetAllDownloadLimitationType();
        List<SelectListModel> GetAddressTypes();
        bool CountPhone(string phoneNumber, string userPhone);

        List<UserFavorites> GetUserFavoriteList(string userId, FavoriteType type);

        Result AddToUserFavoriteList(string userId, FavoriteType type, string entityId, string url, string domainId);
    }
}
