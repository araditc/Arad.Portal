using Arad.Portal.DataLayer.Contracts.General.Permission;
using Arad.Portal.DataLayer.Contracts.General.User;
using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Permission;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Collections.Specialized;
using System.Web;
using Arad.Portal.GeneralLibrary;
using System.Security.Claims;
using AspNetCore.Identity.Mongo.Mongo;
using MongoDB.Driver.Linq;
using Arad.Portal.DataLayer.Contracts.General.Role;
using Arad.Portal.DataLayer.Models.Role;
using Arad.Portal.DataLayer.Repositories.General.Role.Mongo;
using Microsoft.AspNetCore.Hosting;

namespace Arad.Portal.DataLayer.Repositories.General.Permission.Mongo
{
    public class PermissionRepository : BaseRepository, IPermissionRepository
    {
        private readonly PermissionContext _context;
        private readonly RoleContext _roleContext;
        private readonly FilterDefinitionBuilder<Entities.General.Permission.Permission> _builder = new();
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IRoleRepository _roleRepository;
        private readonly string _userId;
        private readonly string _userName;
        public PermissionRepository(PermissionContext context,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            RoleContext roleContext,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager, 
            IRoleRepository roleRepository,
            IUserRepository userRepository): base(httpContextAccessor, env)
        {
            _context = context;
            _roleContext = roleContext;
            _userManager = userManager;
            _mapper = mapper;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            if (httpContextAccessor.HttpContext != null)
            {
                _userId = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                _userName = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            }
        }
        public async Task<Result> Delete(string permissionId)
        {
            var result = new Result();
            try
            {
                var permissionEntity = _context.Collection
                    .Find(_ => _.PermissionId == permissionId).FirstOrDefault();
                if(permissionEntity != null)
                {
                    permissionEntity.IsDeleted = true;
                    var updateResult = await _context.Collection
                            .ReplaceOneAsync(_ => _.PermissionId == permissionId, permissionEntity);
                    if (updateResult.IsAcknowledged)
                    {
                        result.Message = ConstMessages.SuccessfullyDone;
                        result.Succeeded = true;
                    }
                    else
                    {
                        result.Message = ConstMessages.GeneralError;
                    }
                }
                else
                {
                    result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
                }
            }
            catch (Exception e)
            {
                result.Message = ConstMessages.GeneralError;
            }
            return result;
            
        }

        //public Result<PermissionDTO> GetForEdit(string permissionId)
        //{
        //    var result = new Result<PermissionDTO>();
        //    try
        //    {
        //        var per = _context.Collection.AsQueryable().FirstOrDefault(c => c.PermissionId == permissionId);
        //        if (per == null)
        //        {
        //           result.Message = ConstMessages.ObjectNotFound;
        //        }
        //        else
        //        {
        //            var viewModel = _mapper.Map<PermissionDTO>(per);
        //            result.ReturnValue = viewModel;
        //            result.Succeeded = true;
        //        }
               
        //    }
        //    catch (Exception e)
        //    {
        //        result.Message = ConstMessages.GeneralError;
        //    }
        //    return result;
        //}

        public Result<List<Modification>> GetModifications(string permissionId)
        {
            var result = new Result<List<Modification>>();
            var entity = _context.Collection.AsQueryable().FirstOrDefault(c => c.PermissionId == permissionId);
            if (entity != null)
            {
                result.Succeeded = true;
                result.ReturnValue = entity.Modifications.OrderByDescending(c => c.DateTime).Take(10).ToList();
               
            }else
            {
                result.Succeeded = false;
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");

            }
            return result;
        }
        public async Task<PagedItems<ListPermissionViewModel>> List(string queryString)
        {
            var result = new PagedItems<ListPermissionViewModel>();
            try
            {
                NameValueCollection filter = HttpUtility.ParseQueryString(queryString);

                if (string.IsNullOrWhiteSpace(filter["page"]))
                {
                    filter.Set("page", "1");
                }

                if (string.IsNullOrWhiteSpace(filter["PageSize"]))
                {
                    filter.Set("PageSize", "20");
                }

                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);

                long count = await _context.Collection.Find(c => true).CountDocumentsAsync();

                var mongoList = _context.Collection.AsQueryable().Skip((page - 1) * pageSize)
                    .Take(pageSize).ToList();
                var list =  mongoList.Select(_ => new ListPermissionViewModel()
                    {
                        Id = _.PermissionId,
                        ClientAddress = _.ClientAddress,
                        Title = _.Title,
                        //Type = _.Type,
                        //Method = _.Method,
                        //Routes =_.Routes.Count != 0 ?  string.Join(",", _.Routes) : "",
                        CreationDate = _.CreationDate,
                        CreatorName = _.CreatorUserName,
                        HasModification = _.Modifications != null && _.Modifications.Any(),
                        IsActive = _.IsActive,
                        IsDeleted = _.IsDeleted
                    }).ToList();

                result.CurrentPage = page;
                result.Items = list;
                result.ItemsCount = count;
                result.PageSize = pageSize;
                result.QueryString = queryString;
            }
            catch (Exception e)
            {
                result.CurrentPage = 1;
                result.Items = new List<ListPermissionViewModel>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<List<string>> GetUserPermissionsAsync()
        { 
            var id = this.GetUserId();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return new List<string>();
            }

            if (user.IsSystemAccount)
            {
                return _context.Collection.AsQueryable().Select(c => c.PermissionId).ToList();
            }

            return _userRepository.GetPermissionsOfUser(user).Select(_ => _.PermissionId).ToList();
        }
      
        public async Task<Entities.General.Permission.Permission> FetchPermission(string permissionId)
        {
            Entities.General.Permission.Permission result = null;
            result = await _context.Collection.Find(_ => _.PermissionId == permissionId).FirstOrDefaultAsync();
            return result;

        }

        public bool HasAny()
        {
            return _context.Collection.AsQueryable().Any();
        }

        public async Task InsertMany(List<Entities.General.Permission.Permission> permissions)
        {
            permissions.Select(c => { c.IsActive = true; return c; }).ToList();
            await _context.Collection.InsertManyAsync(permissions);
        }

        public List<Entities.General.Permission.Permission> GetAllPermissions()
        {
            var result = new List<Entities.General.Permission.Permission>();
            result = _context.Collection.AsQueryable()
                .Where(_ => _.IsActive && !_.IsDeleted).ToList();
            return result;
        }
        public List<string> GetAllPermissionIds()
        {
            var result = new List<string>();
            result = _context.Collection.AsQueryable()
                .Where(_ => _.IsActive && !_.IsDeleted).Select(_ => _.PermissionId).ToList();
            return result;
        }
       

        public async Task<Result> ChangeActivation(string permissionId, bool isActive, string modificationReason)
        {
            var result = new Result();
            try
            {

                var roleEntity = _context.Collection.Find(_ => _.PermissionId == permissionId).FirstOrDefault();
                if (roleEntity != null)
                {
                    roleEntity.IsActive = isActive;

                    #region Add Modification record
                    var currentModifications = roleEntity.Modifications;
                    var mod = GetCurrentModification(modificationReason);
                    currentModifications.Add(mod);
                    roleEntity.Modifications = currentModifications;
                    #endregion

                    var updateResult = await _context.Collection.ReplaceOneAsync(_ => _.PermissionId == permissionId, roleEntity);
                    if (updateResult.IsAcknowledged)
                    {
                        result.Succeeded = true;
                        result.Message = ConstMessages.SuccessfullyDone;
                    }
                    else
                    {
                        result.Message = ConstMessages.ErrorInSaving;
                    }
                }
                else
                {
                    result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
                }

            }
            catch (Exception e)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }

            return result;
        }

        public Task<List<Entities.General.Permission.Permission>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<List<PermissionTreeViewDto>> GetMenus(string currentUserId, string address)
        {
            List<PermissionTreeViewDto> result = new();

            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(currentUserId);
                List<Entities.General.Permission.Permission> permissions = GetPermissionsOfUser(user);


                void RemoveChild(ICollection<Entities.General.Permission.Permission> tmpPermissions, string title)
                {
                    foreach (Entities.General.Permission.Permission menu in tmpPermissions)
                    {
                        if (menu.Title.Equals(title))
                        {
                            tmpPermissions.Remove(menu);
                            break;
                        }

                        RemoveChild(menu.Children, title);
                    }
                }

                result = _mapper.Map<List<PermissionTreeViewDto>>(permissions);

                foreach (PermissionTreeViewDto dto in result)
                {
                    foreach (PermissionTreeViewDto child in dto.Children)
                    {
                        child.IsActive = GetAccessUrl(child).Any(a => a.Equals(address, StringComparison.OrdinalIgnoreCase));
                    }
                    dto.IsActive = !dto.Children.Any() ? GetAccessUrl(dto).Any(a => a.Equals(address, StringComparison.OrdinalIgnoreCase)) : dto.Children.Any(c => c.IsActive);
                }

                IEnumerable<string> GetAccessUrl(PermissionTreeViewDto dto)
                {
                    List<string> accessUrl = new() { dto.ClientAddress };
                    if (dto.Urls != null)
                    {
                        accessUrl.AddRange(dto.Urls.Select(u => u.ToLower()));
                    }

                    foreach (ActionDto permissionAction in dto.Actions)
                    {
                        accessUrl.Add(permissionAction.ClientAddress);
                        accessUrl.AddRange(permissionAction.Urls);
                    }

                    return accessUrl;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return result;
        }


        private List<Entities.General.Permission.Permission> GetPermissionsOfUser(ApplicationUser user)
        {
            List<Entities.General.Permission.Permission> permissionList;
            try
            {
                FilterDefinition<Entities.General.Permission.Permission> filterDefinition = _builder.Eq(p => p.IsActive, true);
                permissionList = _context.Collection.Find(filterDefinition).ToList();

                if (!user.IsSystemAccount)
                {
                    FilterDefinitionBuilder<Entities.General.Role.Role> roleBuilder = new();
                    FilterDefinition<Entities.General.Role.Role> roleFilterDefinition = roleBuilder.Eq(role => role.RoleId, user.UserRoleId);
                    Entities.General.Role.Role role = _roleContext.Collection.Find(roleFilterDefinition).SingleOrDefault();

                    if (role == null)
                    {
                        return new();
                    }

                    List<Entities.General.Permission.Permission> accessPermissions = new();

                    foreach (Entities.General.Permission.Permission permission in permissionList.Where(p => role.PermissionIds.Any(pId => pId.Equals(p.PermissionId))))
                    {
                        accessPermissions.Add(permission);
                        Remove(permission.Children);
                    }

                    void Remove(IList<Entities.General.Permission.Permission> children)
                    {
                        int count = children.Count;
                        for (int index = count - 1; index >= 0; index--)
                        {
                            Entities.General.Permission.Permission child = children[index];
                            if (role.PermissionIds.Any(p => p.Equals(child.PermissionId)))
                            {
                                Remove(child.Children);
                            }
                            else
                            {
                                children.RemoveAt(index);
                            }
                        }
                    }

                    return accessPermissions;
                }
            }
            catch
            {
                return new();
            }

            return permissionList;
        }
        public async Task<List<string>> GetUserPermissions()
        {
            ApplicationUser user = await _userManager.FindByIdAsync(_userId);

            if (user == null)
            {
                return new();
            }
           

            FilterDefinitionBuilder<Entities.General.Role.Role> roleBuilder = new();
            FilterDefinition <Entities.General.Role.Role> roleFilterDefinition = roleBuilder.Eq(role => role.RoleId, user.UserRoleId);
            Entities.General.Role.Role role = _roleContext.Collection.Find(roleFilterDefinition).SingleOrDefault();

            return role.PermissionIds;
        }
        public async Task<List<Entities.General.Permission.Permission>> GetAllListViewCustom()
        {
            List<Entities.General.Permission.Permission> result = new();
            try
            {
                IFindFluent<Entities.General.Permission.Permission, Entities.General.Permission.Permission> find =
                    _context.Collection.Find(FilterDefinition<Entities.General.Permission.Permission>.Empty);
                List<Entities.General.Permission.Permission> permissions = await find.ToListAsync();
                GenerateList(permissions, "");

                void GenerateList(List<Entities.General.Permission.Permission> tmPermissions, string parentName)
                {
                    foreach (Entities.General.Permission.Permission permission in tmPermissions)
                    {
                        Entities.General.Permission.Permission dto = new()
                        {
                            PermissionId = permission.PermissionId,
                            Urls = permission.Urls,
                            ClientAddress = permission.ClientAddress,
                            IsActive = permission.IsActive,
                            Actions = permission.Actions
                        };
                        result.Add(dto);

                        GenerateList(permission.Children, $"{parentName}{(string.IsNullOrWhiteSpace(parentName) ? "" : "-")}{dto.Title}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return result;
        }

        public async Task<List<PermissionSelectDto>> GetAllListView()
        {
            List<PermissionSelectDto> result = new();
            try
            {
                IFindFluent<Entities.General.Permission.Permission, Entities.General.Permission.Permission> find = _context.Collection.Find(FilterDefinition<Entities.General.Permission.Permission>.Empty);
                List<Entities.General.Permission.Permission> permissions = await find.ToListAsync();
                GenerateList(permissions, "");

                void GenerateList(List<Entities.General.Permission.Permission> tmPermissions, string parentName)
                {
                    foreach (Entities.General.Permission.Permission permission in tmPermissions)
                    {
                        PermissionSelectDto dto = new()
                        {
                            PermissionId = permission.PermissionId,
                            ClientAddress = permission.ClientAddress,
                            LevelNo = permission.LevelNo,
                            CreatorUserName = permission.CreatorUserName,
                            HasModification = permission.Modifications.Any(),
                            Icon = permission.Icon,
                            IsActive = permission.IsActive,
                            ParentTitle = parentName,
                            Priority = permission.Priority,
                            Title = Arad.Portal.GeneralLibrary.Utilities.Language.GetString($"PermissionTitle_{permission.Title}")
                        };
                        result.Add(dto);

                        GenerateList(permission.Children, $"{parentName}{(string.IsNullOrWhiteSpace(parentName) ? "" : "-")}{dto.Title}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return result;
        }


        public async Task<List<string>> GetAllNestedPermissionIds()
        {
            List<string> result = new();
            try
            {
                IFindFluent<Entities.General.Permission.Permission, Entities.General.Permission.Permission> find 
                    = _context.Collection.Find(FilterDefinition<Entities.General.Permission.Permission>.Empty);
                List<Entities.General.Permission.Permission> permissions = await find.ToListAsync();
                GenerateList(permissions);

                void GenerateList(List<Entities.General.Permission.Permission> tmPermissions)
                {
                    foreach (Entities.General.Permission.Permission permission in tmPermissions)
                    {
                        
                        result.Add(permission.PermissionId);
                        foreach (Arad.Portal.DataLayer.Entities.General.Permission.Action action in permission.Actions)
                        {
                            result.Add(permission.PermissionId);
                        }
                        GenerateList(permission.Children);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return result;
        }
        public async Task<PagedItems<PermissionSelectDto>> GetPage(string queryString)
        {
            try
            {
                int pageSize = 10;
                int page = 1;
                NameValueCollection queryParams = HttpUtility.ParseQueryString(queryString);

                if (!string.IsNullOrWhiteSpace(queryParams["page"]))
                {
                    page = Convert.ToInt32(queryParams["page"]);
                }

                if (!string.IsNullOrWhiteSpace(queryParams["pageSize"]))
                {
                    pageSize = Convert.ToInt32(queryParams["pageSize"]);
                }

                List<PermissionSelectDto> permissionSelectDtos = await GetAllListView();
                List<PermissionSelectDto> permissions = permissionSelectDtos.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                return new() { CurrentPage = page, Items = permissions, PageSize = pageSize, QueryString = queryString };
            }
            catch
            {
                return new() { QueryString = queryString, CurrentPage = 1, Items = new(), PageSize = 10 };
            }
        }

        public async Task<Entities.General.Permission.Permission> GetById(string id)
        {
            List<Entities.General.Permission.Permission> roots = await GetAll();
            Entities.General.Permission.Permission result = null;
            Get(roots);
            return result;

            void Get(List<Entities.General.Permission.Permission> permissions)
            {
                foreach (Entities.General.Permission.Permission permission in permissions)
                {
                    if (permission.PermissionId.Equals(id))
                    {
                        result = permission;
                        return;
                    }

                    if (permission.Children.Any())
                    {
                        Get(permission.Children);
                    }
                }
            }
        }

        public async Task<Entities.General.Permission.Permission> GetRootById(string id)
        {
            List<Entities.General.Permission.Permission> roots = await GetAll();
            Entities.General.Permission.Permission result = new();
            Get(roots, id);

            while (true)
            {
                if (string.IsNullOrWhiteSpace(result.ParentId))
                {
                    return result;
                }

                Get(roots, result.ParentId);
            }

            void Get(List<Entities.General.Permission.Permission> permissions, string childId)
            {
                foreach (Entities.General.Permission.Permission permission in permissions)
                {
                    if (permission.PermissionId.Equals(childId))
                    {
                        result = permission;
                        return;
                    }

                    if (permission.Children.Any())
                    {
                        Get(permission.Children, childId);
                    }
                }
            }
        }

        public Task<bool> Upsert(Entities.General.Permission.Permission permission)
        {
            throw new NotImplementedException();
        }

        public Task<Result<string>> GetPermissionType(string permissionId)
        {
            throw new NotImplementedException();
        }
    }
}
