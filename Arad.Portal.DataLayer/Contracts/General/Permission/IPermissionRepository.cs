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
        Task<List<MenuLinkModel>> ListOfMenues(string currentUserId, string address, string domain);

        Task<Result<string>> GetPermissionType(string permissionId);
        List<MenuLinkModel> GetChildren(List<Entities.General.Permission.Permission> context,
           string permissionId, string address, string domain);
        //Task<List<PermissionDTO>> MenusPermission(Enums.PermissionType typeMenu);
        //Task<Result> Save(PermissionDTO permission);
        //Result<PermissionDTO> GetForEdit(string permissionId);
        Task<Result> Delete(string permissionId);
        Result<List<Modification>> GetModifications(string permissionId);
        Task<List<string>> GetUserPermissionsAsync();
        Task<Entities.General.Permission.Permission> FetchPermission(string permissionId);
        Task<List<TreeviewModel>> ListPermissions(string currentUserId, string currentRoleId);
        List<Entities.General.Permission.Permission> GetAllPermissions();

        Task<Result> ChangeActivation(string permissionId, bool isActive, string modificationReason);

        Task<List<Entities.General.Permission.Permission>> GetAll();

        Task<List<PermissionTreeViewDto>> GetMenus(string currentUserId, string address, bool isAdmin);

        Task<List<string>> GetUserPermissions();

        Task<List<Entities.General.Permission.Permission>> GetAllListViewCustom();

        Task<List<PermissionSelectDto>> GetAllListView();

        Task<PagedItems<PermissionSelectDto>> GetPage(string queryString);

        Task<Entities.General.Permission.Permission> GetById(string id);

        Task<Entities.General.Permission.Permission> GetRootById(string id);

        Task<bool> Upsert(Entities.General.Permission.Permission permission);

      
    }
}
