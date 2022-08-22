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
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Repositories.General.Role.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Permission.Mongo;
using Arad.Portal.GeneralLibrary.Utilities;
using static Arad.Portal.DataLayer.Models.Shared.Enums;


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
            IWebHostEnvironment env,
            IMapper mapper) : base(httpContextAccessor, env)
        {
            _context = userContext;
            _roleContext = roleContext;
            _permissionContext = permissionContext;
            _mapper = mapper;
        }

        public Result AddToUserFavoriteList(string userId, FavoriteType type, string entityId, string url, string domainId)
        {
            var result = new Result();
            try
            {
                var obj = new UserFavorites()
                {
                    UserFavoritesId = Guid.NewGuid().ToString(),
                    CreationDate = DateTime.Now,
                    CreatorUserId = userId,
                    AssociatedDomainId = domainId,
                    EntityId = entityId,
                    FavoriteType = type,
                    IsActive = true,
                    IsDeleted = false,
                    Url = url
                };

                _context.UserFavoritesCollection.InsertOne(obj);
                result.Succeeded = true;
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
            }
            return result;

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

                Entities.General.Role.Role userRoleEntity =
                    _roleContext.Collection.Find(_ => _.RoleId == user.UserRoleId).FirstOrDefault();

                //foreach (var item in userRolesEntities)
                //{
                //    var lst = _permissionContext.Collection.AsQueryable()
                //        .Where(_ => _.IsActive && item.PermissionIds.Contains(_.PermissionId)).SelectMany(_ => _.Routes).ToList();
                //    finalList.AddRange(lst);
                //}

                //finalList = finalList.Distinct().ToList();
                finalList = _permissionContext.Collection.AsQueryable()
                        .Where(_ => _.IsActive && userRoleEntity.PermissionIds.Contains(_.PermissionId)).SelectMany(_ => _.Urls).ToList();
            }
            catch (Exception ex)
            {

            }
            return finalList;
        }

        public List<SelectListModel> GetAddressTypes()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(AddressType)))
            {
                string name = Enum.GetName(typeof(AddressType), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel() { Text = GeneralLibrary.Utilities.Language.GetString("Choose"), Value = "-1" });
            return result;
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

        public List<SelectListModel> GetAllDownloadLimitationType()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(DownloadLimitationType)))
            {
                string name = Enum.GetName(typeof(DownloadLimitationType), i);
                var obj = new SelectListModel()
                {
                    Text = Arad.Portal.GeneralLibrary.Utilities.Language.GetString($"EnumDesc_{name}"),
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel() { Text = GeneralLibrary.Utilities.Language.GetString("Choose"), Value = "-1" });
            return result;
        }

        public List<SelectListModel> GetAllProductType()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(ProductType)))
            {
                string name = Enum.GetName(typeof(ProductType), i);
                var obj = new SelectListModel()
                {
                    Text = Arad.Portal.GeneralLibrary.Utilities.Language.GetString($"EnumDesc_{name}"),
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            //result.Insert(0, new SelectListModel() { Text = GeneralLibrary.Utilities.Language.GetString("Choose"), Value = "-1" });
            return result;
        }

        public List<Entities.General.Permission.Permission> GetPermissionsOfUser(ApplicationUser user)
        {
            List<Entities.General.Permission.Permission> permissionList = new();
            try
            {
                if(!user.IsSystemAccount)
                {

                   Entities.General.Role.Role userRoleEntity =
                     _roleContext.Collection.Find(_ => _.RoleId == user.UserRoleId).FirstOrDefault();
                    
                    permissionList = _permissionContext.Collection.AsQueryable()
                            .Where(_ => _.IsActive && userRoleEntity.PermissionIds.Contains(_.PermissionId)).ToList();
                      
                    permissionList = permissionList.Distinct().ToList();
                }
                else
                {
                    permissionList = _permissionContext.Collection.AsQueryable().Where(_ => _.IsActive).ToList();
                }
               
            }
            catch (Exception ex)
            {

            }
            return permissionList;
        }

        public string GetRoleNameOfUser(string userId)
        {
            string result = string.Empty;
            try
            {
                var user = _context.Collection.AsQueryable()
                    .FirstOrDefault(c => c.Id.ToString() == userId);

                if (user != null)
                {
                    result = _roleContext.Collection.Find(_ => _.RoleId == user.UserRoleId).Any() ? 
                        _roleContext.Collection.Find(_ => _.RoleId == user.UserRoleId).FirstOrDefault().RoleName : "";
                }

            }
            catch (Exception e)
            {

            }
            return result;
        }

        public List<UserFavorites> GetUserFavoriteList(string userId, FavoriteType type)
        {
            var list = _context.UserFavoritesCollection.Find(_ => _.CreatorUserId == userId && _.FavoriteType == type).ToList();
            return list;
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

        public List<UserDTO> Search(string word)
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
