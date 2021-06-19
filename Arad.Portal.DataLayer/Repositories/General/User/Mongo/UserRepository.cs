using Arad.Portal.DataLayer.Contracts.General.User;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Permission;
using Arad.Portal.DataLayer.Repositories.General.Permission;
using Arad.Portal.DataLayer.Repositories.General.Role;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arad.Portal.DataLayer.Repositories.General.User.Mongo
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        private readonly UserContext _context;
        private readonly RoleContext _roleContext;
        private readonly PermissionContext _permissionContext;
        private readonly IMapper _mapper;
        public UserRepository(UserContext userContext,
            RoleContext roleContext,
            PermissionContext permissionContext,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper) : base(httpContextAccessor)
        {
            _context = userContext;
            _roleContext = roleContext;
            _permissionContext = permissionContext;
            _mapper = mapper;
        }

        public List<PermissionDTO> GetPermissionsOfUser(ApplicationUser user)
        {
            List<PermissionDTO> finalList = new List<PermissionDTO>();
            try
            {
                List<string> roleIdList = user.UserRoles;
                List<Entities.General.Role.Role> userRolesEntities = new();
                userRolesEntities = _roleContext.Collection.AsQueryable()
                    .Where(_ => roleIdList.Contains(_.RoleId)).ToList();

                List<Entities.General.Permission.Permission> permissions =
                    _permissionContext.Collection.AsQueryable()
                    .Where(c => c.IsActive).Distinct()
                    .ToList();

                List<Entities.General.Permission.Permission> permissionList = new();
                permissionList = userRolesEntities.SelectMany(x => x.Permissions).ToList();
                permissionList = permissionList.Distinct().ToList();
                finalList = _mapper.Map<List<PermissionDTO>>(permissionList);

            }
            catch (Exception ex)
            {

            }
            return finalList;
        }
    }
}
