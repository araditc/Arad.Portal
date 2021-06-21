using Arad.Portal.DataLayer.Contracts.General.User;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Permission;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.User;
using Arad.Portal.DataLayer.Repositories.General.Permission;
using Arad.Portal.DataLayer.Repositories.General.Role;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

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

        public bool CountPhone(string phoneNumber, string userPhone)
        {
            bool result;
            try
            {
                var user = _context.Collection.AsQueryable()
                    .Where(u => u.PhoneNumber == phoneNumber).ToList();

                var count = user.Count;

                if (count == 0)
                {
                    result = true;
                }

                if (count == 1)
                {
                    if (phoneNumber == userPhone)
                    {
                        result = true;
                    }
                }

                result = false;
            }
            catch (Exception e)
            {
                result = false;
            }
            return result;
        }

        public List<string> GetAccessibleRoutsOfUser(ApplicationUser user)
        {
            List<string> finalList = new List<string>();
            try
            {
                List<string> roleIdList = user.UserRoles;
                List<Entities.General.Role.Role> userRolesEntities = new();
                userRolesEntities = _roleContext.Collection.AsQueryable()
                    .Where(_ => roleIdList.Contains(_.RoleId)).ToList();


                foreach (var item in userRolesEntities)
                {
                    var lst = _permissionContext.Collection.AsQueryable()
                        .Where(_ => _.IsActive && item.PermissionIds.Contains(_.PermissionId)).SelectMany(_ => _.Routes).ToList();
                    finalList.AddRange(lst);
                }

                finalList = finalList.Distinct().ToList();
            }
            catch (Exception ex)
            {

            }
            return finalList;
        }

        public List<UserDTO> GetAll()
        {
            List<UserDTO> result = new List<UserDTO>();
            try
            {
                var users = _context.Collection.AsQueryable()
                    .Where(u => u.IsActive && !u.IsSystemAccount && !u.IsDeleted).ToList();
                if (users != null)
                {
                    result = _mapper.Map<List<UserDTO>>(users);
                }
            }
            catch (Exception e)
            {

            }
            return result;
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


                List<Entities.General.Permission.Permission> permissionList = new();
                //permissionList = userRolesEntities.SelectMany(x => x.Permissions).ToList();
                foreach (var item in userRolesEntities)
                {
                    var lst = _permissionContext.Collection.AsQueryable()
                        .Where(_ => _.IsActive && item.PermissionIds.Contains(_.PermissionId)).ToList();
                    permissionList.AddRange(lst);
                }

                permissionList = permissionList.Distinct().ToList();
                finalList = _mapper.Map<List<PermissionDTO>>(permissionList);

            }
            catch (Exception ex)
            {

            }
            return finalList;
        }

        public List<string> GetRoleNamesOfUser(string userId)
        {
            List<string> result = new List<string>();
            try
            {
                var user = _context.Collection.AsQueryable()
                    .FirstOrDefault(c => c.Id == userId);

                if (user != null)
                {

                    result = _roleContext.Collection
                       .AsQueryable().Where(_ => user.UserRoles.Contains(_.RoleId)).Select(_ => _.RoleName).ToList();
                }

            }
            catch (Exception e)
            {

            }
            return result;
        }

        public UserDTO GetUserWithPhone(string phoneNumber)
        {
            var result = new UserDTO();
            try
            {
                var user = _context.Collection.AsQueryable()
                    .FirstOrDefault(u => u.PhoneNumber == phoneNumber && !u.IsDeleted);
                if (user != null)
                {
                    result = _mapper.Map<UserDTO>(user);
                }
            }
            catch (Exception e)
            {

            }
            return result;
        }

        public async Task<PagedItems<UserDTO>> List(UserSearchParams searchParam, string currentUserId)
        {
            var result = new PagedItems<UserDTO>();
            try
            {
                var query = _context.Collection.AsQueryable()
                    .Where(u => !u.IsSystemAccount && u.Id != currentUserId);

                if (!string.IsNullOrEmpty(searchParam.UserName))
                {
                    query = query.Where(_ => _.UserName.Contains(searchParam.UserName.Trim()));
                }

                if (!string.IsNullOrEmpty(searchParam.Name))
                {
                    query = query.Where(c => c.Profile.FirstName.Contains(searchParam.Name.Trim()));
                }

                if (!string.IsNullOrEmpty(searchParam.LastName))
                {
                    query = query.Where(_ => _.Profile.LastName.Contains(searchParam.LastName.Trim()));
                }

                if (!string.IsNullOrEmpty(searchParam.PhoneNumber))
                {
                    query = query.Where(_ => _.PhoneNumber.Contains(searchParam.PhoneNumber.Trim()));
                }

                query = query.Where(_ => _.IsActive == searchParam.IsActive);


                if (searchParam.StartRegisterDate != null)
                {
                    //???? univeral or not
                    query = query.Where(_ => _.CreationDate >= searchParam.StartRegisterDate);
                }

                if (searchParam.EndRegisterDate != null)
                {
                   //??? adding one day or not
                   query = query.Where(_ => _.CreationDate < searchParam.EndRegisterDate);
                }


                if (searchParam.UserRoles != null && searchParam.UserRoles.Any())
                {
                    query = query.Where(c => c.UserRoles.Any(r => searchParam.UserRoles.Contains(r)));
                }

                var count = query.Count();

                query = query.Skip((searchParam.CurrentPage - 1) * searchParam.PageSize)
                    .Take(searchParam.PageSize);

                List<ApplicationUser> users = await query.ToListAsync();

                var res = new PagedItems<UserDTO>()
                {
                    CurrentPage = searchParam.CurrentPage,
                    ItemsCount = count,
                    PageSize = searchParam.PageSize,
                    Items = users.Select(_ => new UserDTO()
                    {
                        UserId = _.Id.ToString(),
                        UserName = _.UserName,
                        UserProfile = _.Profile,
                        IsActive = _.IsActive,
                        CreationDate = _.CreationDate,
                        IsDeleted =_.IsDeleted
                    }).ToList()
                };
            }
            catch (Exception e)
            {
              
            }
            return result;
        }

        public List<UserDTO> search(string word)
        {
            var result = new List<UserDTO>();
            try
            {
                var users = _context.Collection.AsQueryable()
                    .Where(_ => _.IsActive && !_.IsSystemAccount && !_.IsDeleted &&
                    (_.UserName.Contains(word) || _.Profile.FatherName.Contains(word) ||
                                _.Profile.LastName.Contains(word))).ToList();

                result = _mapper.Map<List<UserDTO>>(users);
            }
            catch (Exception e)
            {
              
            }
            return result;
        }
    }
}
