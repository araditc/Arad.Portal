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
        Task<RoleDTO> FetchRole(string roleId);

        Task<Entities.General.Role.Role> FetchRoleEntity(string roleId);
        Task<RoleDTO> FetchRoleByName(string roleName);
        Task<PagedItems<RoleDTO>> List(string queryString);
        Task<PagedItems<RoleListViewModel>> RoleList(string queryString);
        Task<RepositoryOperationResult> Add(RoleDTO dto);

        List<SelectListModel> GetAllState();
        List<SelectListModel> GetCounties(string stateId);
        List<SelectListModel> GetDistricts(string countyId);
        List<SelectListModel> GetCities(string districtId);
      
        Task<RepositoryOperationResult> Update(RoleDTO dto);
        Task<RepositoryOperationResult> Delete(string roleId, string modificationReason);
        Task<RepositoryOperationResult> ChangeActivation(string roleId);
        //Task<List<RoleDTO>> ListRoles();
        
    }
}
