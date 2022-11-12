using Arad.Portal.DataLayer.Models.Role;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.Role
{
    public interface IRoleRepository
    {
        RoleDTO FetchRole(string roleId);

        Task<Entities.General.Role.Role> FetchRoleEntity(string roleId);
        Task<RoleDTO> FetchRoleByName(string roleName);
        Task<PagedItems<RoleDTO>> List(string queryString);
        Task<PagedItems<RoleListViewModel>> RoleList(string queryString);
        Task<Result> Add(RoleDTO dto);
        Task<Result> Update(RoleDTO dto);
        Task<Result> Restore(string roleId);
        Task<Result> Delete(string roleId, string modificationReason);
        Task<Result> ChangeActivation(string roleId);
        //Task<List<RoleDTO>> ListRoles();
        
    }
}
