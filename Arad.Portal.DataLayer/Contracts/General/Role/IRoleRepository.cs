using Arad.Portal.DataLayer.Models.Role;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.Role
{
    public interface IRoleRepository
    {
        Task<RoleDTO> FetchRole(string roleId);
        Task<RoleDTO> FetchRoleByName(string roleName);
        Task<PagedItems<RoleDTO>> List(string queryString);
        Task<RepositoryOperationResult> Add(RoleDTO dto);
        Task<RepositoryOperationResult> Update(RoleDTO dto);
        Task<RepositoryOperationResult> Delete(string roleId, string modificationReason);
        Task<RepositoryOperationResult> ChangeActivation(string roleId, bool isActive, string modificationReason);
        //Task<List<RoleDTO>> ListRoles();
        
    }
}
