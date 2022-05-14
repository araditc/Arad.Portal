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
            UserManager<ApplicationUser> userManager, 
            IRoleRepository roleRepository,
            IUserRepository userRepository): base(httpContextAccessor)
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
                    result.Message = ConstMessages.ObjectNotFound;
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
                result.Message = ConstMessages.ObjectNotFound;
                    
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
        public async Task<List<MenuLinkModel>> ListOfMenues(string currentUserId, string address, string domain)
        {
            var result = new List<MenuLinkModel>();
            //address = address.Length > 1 ? address.Substring(1, address.Length) : address;
            //try
            //{
            //    var user = await _userManager.FindByIdAsync(currentUserId);

            //    if (user.IsSystemAccount)
            //    {
            //        var query = _context.Collection.AsQueryable();
            //        //var allBaseMenues = query.Where(p => p.Type == Enums.PermissionType.BaseMenu).ToList();
            //        //var allMenues = query.Where(p => p.Type == Enums.PermissionType.Menu).ToList();


            //        result = allBaseMenues.Select(_ => new MenuLinkModel()
            //        {
            //            MenuId = _.PermissionId,
            //            MenuTitle = GeneralLibrary.Utilities.Language.GetString("PermissionTitle_" + _.Title),
            //            Icon = _.Icon,
            //            Link = _.ClientAddress,
            //            Priority = _.Priority,
            //            IsActive = (!string.IsNullOrWhiteSpace(_.ClientAddress)  && _.ClientAddress.Contains(address)) || (_.ClientAddress == "" 
            //            && _.Routes.Contains(address)),
            //            Children = GetChildren(allMenues, _.PermissionId, address, domain)
            //        }).ToList().OrderBy(_ => _.Priority).ToList();
            //    }
            //    else
            //    {
            //        var pers = _userRepository.GetPermissionsOfUser(user);
            //        var baseMenues = pers.Where(p => p.Type == Enums.PermissionType.BaseMenu).ToList();

            //        var menues = pers.Where(p => p.Type == Enums.PermissionType.Menu).ToList();

            //        result = baseMenues.Select(_ => new MenuLinkModel()
            //        {
            //            MenuId = _.PermissionId,
            //            MenuTitle = GeneralLibrary.Utilities.Language.GetString("PermissionTitle_" + _.Title),
            //            Icon = _.Icon,
            //            Link = _.ClientAddress,
            //            Priority = _.Priority,
            //            IsActive = (!string.IsNullOrWhiteSpace(_.ClientAddress) && _.ClientAddress.Contains(address)) || (_.ClientAddress == "" 
            //            && _.Routes.Contains(address)),
            //            Children = GetChildren(menues, _.PermissionId, address, domain)
            //        }).OrderBy(o => o.Priority).ToList();
            //    }
            //}
            //catch (Exception)
            //{

            //}
            return result;
        }

        public List<MenuLinkModel> GetChildren(List<Entities.General.Permission.Permission> context,
           string permissionId, string address, string domain)
        {
            var result = new List<MenuLinkModel>();
            //if (context.Any(_ => _.ParentMenuId == permissionId))
            //{
            //    result = context.Where(_ => _.ParentMenuId == permissionId)
            //        .Select(_ => new MenuLinkModel()
            //    {
            //        Icon = _.Icon,
            //        IsActive = !string.IsNullOrWhiteSpace(_.ClientAddress) && _.ClientAddress.Equals(address),
            //        Link = _.ClientAddress,
            //        MenuTitle = GeneralLibrary.Utilities.Language.GetString("PermissionTitle_" + _.Title),
            //        MenuId = _.PermissionId,
            //        Priority = _.Priority,
            //        Children = GetChildren(context, _.PermissionId, address, domain)
            //    }).ToList();
            //}

            return result;
            
        }
        //public async Task<List<PermissionDTO>> MenusPermission(Enums.PermissionType typeMenu)
        //{
        //    var result = new List<PermissionDTO>();
        //    try
        //    {
        //        var list = await _context.Collection.Find(_ => _.Type == typeMenu).ToListAsync();
        //        result = _mapper.Map<List<PermissionDTO>>(list);
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    return result;
        //}

        //public async Task<Result> Save(PermissionDTO dto)
        //{
        //    Result result;

        //    //mapping the input model to equallent object of database
        //    var equallentModel = _mapper.Map<Entities.General.Permission.Permission>(dto);
        //    equallentModel.Routes = dto.Routes.Split(",").ToList();

        //    if (!string.IsNullOrWhiteSpace(dto.PermissionId))//it is update case
        //    {
        //        result = await UpdatePermissionAsync(equallentModel, dto.ModificationReason);
        //    }
        //    else //it is insert case
        //    {
        //        result = await InsertPermissionAsync(equallentModel);
        //    }

        //    return result;
        //}
        private async Task<Result> UpdatePermissionAsync
            (Entities.General.Permission.Permission equallentModel, string modificationReason)
        {
            var result = new Result();

            var availableEntity = await _context.Collection
                    .Find(_ => _.PermissionId.Equals(equallentModel.PermissionId)).FirstOrDefaultAsync();

            if (availableEntity != null)
            {
                #region Add Modification
                var currentModification = availableEntity.Modifications;
                var mod = GetCurrentModification(modificationReason);
                currentModification.Add(mod);
                #endregion

                equallentModel.Modifications = currentModification;

                equallentModel.CreationDate = availableEntity.CreationDate;
                equallentModel.CreatorUserId = availableEntity.CreatorUserId;
                equallentModel.CreatorUserName = availableEntity.CreatorUserName;

                var updateResult = await _context.Collection
                   .ReplaceOneAsync(_ => _.PermissionId == availableEntity.PermissionId, equallentModel);

                if (updateResult.IsAcknowledged)
                {
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = ConstMessages.ErrorInSaving;
                }
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
        private async Task<Result> InsertPermissionAsync(
            Entities.General.Permission.Permission equallentModel)
        {
            var result = new Result();
            equallentModel.Modifications = new List<Modification>();

            equallentModel.CreationDate = DateTime.Now;
            equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            try
            {
                equallentModel.PermissionId = Guid.NewGuid().ToString();
                await _context.Collection.InsertOneAsync(equallentModel);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            catch (Exception e)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }

            return result;
        }

        //public async Task<Result<string>> GetPermissionType(string permissionId)
        //{
        //    var result = new Result<string>();
        //   if (!string.IsNullOrWhiteSpace(permissionId))
        //    {
        //        var per = await _context.Collection
        //            .Find(_ => _.PermissionId == permissionId).FirstOrDefaultAsync();

        //        if(per != null)
        //        {
        //           result.ReturnValue =  per.Type.ToString();
        //           result.Message = ConstMessages.SuccessfullyDone;
        //           result.Succeeded = true;
        //        }else
        //        {
        //            result.Message = ConstMessages.ObjectNotFound;
        //        }
        //    }else
        //    {
        //        result.Message = ConstMessages.ObjectNotFound;
        //    }
        //    return result;
        //}

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

        public async Task<List<TreeviewModel>> ListPermissions(string currentUserId, string currentRoleId)
        { 
            var result = new List<TreeviewModel>();
            //try
            //{
            //    RoleDTO role = new RoleDTO();
            //    if(!string.IsNullOrWhiteSpace(currentRoleId))
            //    {
            //       role = await _roleRepository.FetchRole(currentRoleId);
            //    }
            //    var user = await _userManager.FindByIdAsync(currentUserId);
            //    if(user.IsSystemAccount)
            //    {
            //        var query = _context.Collection.AsQueryable();
            //        var baseMenu = await query.Where(a => a.Type == Enums.PermissionType.BaseMenu).ToListAsync();
            //        var menuesAndModules = await query.Where(a => a.Type == Enums.PermissionType.Menu || a.Type == Enums.PermissionType.Module).ToListAsync();
            //        //var modules = await query.Where(a => a.Type == Enums.PermissionType.Module).ToListAsync();
            //        var list = baseMenu.OrderBy(_=>_.Priority).Select(d => new TreeviewModel()
            //        {
            //            id = d.PermissionId,
            //            text = GeneralLibrary.Utilities.Language.GetString($"PermissionTitle_{d.Title}"),
            //            type = d.Type,
            //            children = GetChildren(menuesAndModules, d.PermissionId, role).ToList(),
            //            Checked = !string.IsNullOrWhiteSpace(currentRoleId) && role.PermissionIds.Contains(d.PermissionId),

            //            isActive = d.IsActive,
            //            priority = d.Priority
            //        }).ToList();

            //        result = list;
                   
            //    }
            //    else
            //    {
            //        var pers = _userRepository.GetPermissionsOfUser(user);
            //        var baseMenues = pers.Where(p => p.Type == Enums.PermissionType.BaseMenu).ToList();
            //        var menuesAndModules = pers.Where(p => p.Type == Enums.PermissionType.Menu || p.Type == Enums.PermissionType.Module).ToList();
            //        result = baseMenues.OrderBy(_=>_.Priority).Select(d => new TreeviewModel()
            //        {
            //            id = d.PermissionId,
            //            text = GeneralLibrary.Utilities.Language.GetString($"PermissionTitle_{d.Title}"),
            //            type = d.Type,
            //            children = GetChildren(menuesAndModules, d.PermissionId, role).ToList(),
            //            Checked = !string.IsNullOrWhiteSpace(currentRoleId) && role.PermissionIds.Contains(d.PermissionId),
                       
            //            isActive = d.IsActive,
            //            priority = d.Priority
            //        }).ToList();
            //    }
            //}
            //catch(Exception ex)
            //{
            //}
            return result;
        }

        public List<TreeviewModel> GetChildren(List<Entities.General.Permission.Permission> context, string parentId, RoleDTO? currentRole)
        {
            var result = new List<TreeviewModel>();
            //if (context.Any(_ => _.ParentMenuId == parentId))
            //{
            //    result = context.OrderBy(_=>_.Priority).Where(_ => _.ParentMenuId == parentId)
            //        .Select(d => new TreeviewModel()
            //        {
            //            id = d.PermissionId,
            //            text = GeneralLibrary.Utilities.Language.GetString($"PermissionTitle_{d.Title}"),
            //            type = d.Type,
            //            children = GetChildren(context, d.PermissionId, currentRole).ToList(),
            //            Checked = currentRole != null  && currentRole.PermissionIds.Contains(d.PermissionId),

            //            isActive = d.IsActive,
            //            priority = d.Priority
            //        }).ToList();
            //}
            return result;
        }

        public async Task<Result> ChangeActivation(string permissionId, bool isActive, string modificationReason)
        {
            var result = new Result();
            try
            {

                var roleEntity = _context.Collection.Find(_ => _.PermissionId == permissionId).First();
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
                    result.Message = ConstMessages.ObjectNotFound;
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
        public Task<List<Entities.General.Permission.Permission>> GetAllListViewCustom()
        {
            throw new NotImplementedException();
        }

        public Task<List<PermissionSelectDto>> GetAllListView()
        {
            throw new NotImplementedException();
        }

        public Task<PagedItems<PermissionSelectDto>> GetPage(string queryString)
        {
            throw new NotImplementedException();
        }

        public Task<Entities.General.Permission.Permission> GetById(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Entities.General.Permission.Permission> GetRootById(string id)
        {
            throw new NotImplementedException();
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
