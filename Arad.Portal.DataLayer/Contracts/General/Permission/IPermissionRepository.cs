using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Models.Permission;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Contracts.General.Permission
{
    public interface IPermissionRepository
    {
        Task<PagedItems<ListPermissionViewModel>> List(string queryString);

        /// <summary>
        /// لیست منوهایی که این یوزر دسترسی داره
        /// </summary>
        Task<List<MenuLinkModel>> ListOfMenues(string currentUserId, string address);

        Task<RepositoryOperationResult<string>> GetPermissionType(string permissionId);
        List<MenuLinkModel> GetChildren(List<Entities.General.Permission.Permission> context,
           string permissionId, string address);
        Task<List<PermissionDTO>> MenusPermission(Enums.PermissionType typeMenu);
        Task<RepositoryOperationResult> Save(PermissionDTO permission);
        RepositoryOperationResult<PermissionDTO> GetForEdit(string permissionId);
        Task<RepositoryOperationResult> Delete(string permissionId);
        RepositoryOperationResult<List<Modification>> GetModifications(string permissionId);
        Task<List<string>> GetUserPermissionsAsync();
        Task<Entities.General.Permission.Permission> FetchPermission(string permissionId);
        Task<List<TreeviewModel>> ListPermissions(string currentUserId, string currentRoleId);
        List<Entities.General.Permission.Permission> GetAllPermissions();

        Task<RepositoryOperationResult> ChangeActivation(string permissionId, bool isActive, string modificationReason);




    }
}
