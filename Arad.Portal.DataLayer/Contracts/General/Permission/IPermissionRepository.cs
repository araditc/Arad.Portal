using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Models.Permission;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.Permission
{
    public interface IPermissionRepository
    {
        Task<PagedItems<ListPermissionViewModel>> List(string queryString);

        /// <summary>
        /// لیست منوهایی که این یوزر دسترسی داره
        /// </summary>
        Task<List<MenuLinkModel>> ListOfMenues(string currentUserId, string address);
        List<PermissionDTO> MenusPermission(Enums.PermissionType typeMenu);
        Task<RepositoryOperationResult> Save(PermissionDTO permission);
        RepositoryOperationResult<PermissionDTO> GetForEdit(string permissionId);
        Task<RepositoryOperationResult> Delete(string permissionId, string modificationReason);
        RepositoryOperationResult<List<Modification>> GetModifications(string permissionId);
    }
}
