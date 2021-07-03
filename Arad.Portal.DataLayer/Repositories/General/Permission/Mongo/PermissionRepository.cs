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

namespace Arad.Portal.DataLayer.Repositories.General.Permission.Mongo
{
    public class PermissionRepository : BaseRepository, IPermissionRepository
    {
        private readonly PermissionContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public PermissionRepository(PermissionContext context,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            UserManager<ApplicationUser> userManager, 
            IUserRepository userRepository): base(httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _userRepository = userRepository;
        }
        public async Task<RepositoryOperationResult> Delete(string permissionId, string modificationReason)
        {
            var result = new RepositoryOperationResult();
            try
            {
                var permissionEntity = _context.Collection
                    .Find(_ => _.PermissionId == permissionId).FirstOrDefault();
                if(permissionEntity != null)
                {
                    permissionEntity.IsDeleted = true;

                    #region Add Modification
                    var currentModification = permissionEntity.Modifications;
                    var mod = GetCurrentModification(modificationReason);
                    currentModification.Add(mod);
                    permissionEntity.Modifications = currentModification;
                    #endregion

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

        public RepositoryOperationResult<PermissionDTO> GetForEdit(string permissionId)
        {
            var result = new RepositoryOperationResult<PermissionDTO>();
            try
            {
                var per = _context.Collection.AsQueryable().FirstOrDefault(c => c.PermissionId == permissionId);
                if (per == null)
                {
                   result.Message = ConstMessages.ObjectNotFound;
                }
                else
                {
                    var viewModel = _mapper.Map<PermissionDTO>(per);
                    result.ReturnValue = viewModel;
                    result.Succeeded = true;
                }
               
            }
            catch (Exception e)
            {
                result.Message = ConstMessages.GeneralError;
            }
            return result;
        }

        public RepositoryOperationResult<List<Modification>> GetModifications(string permissionId)
        {
            var result = new RepositoryOperationResult<List<Modification>>();
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

                if (string.IsNullOrWhiteSpace(filter["CurrentPage"]))
                {
                    filter.Set("CurrentPage", "1");
                }

                if (string.IsNullOrWhiteSpace(filter["PageSize"]))
                {
                    filter.Set("PageSize", "20");
                }

                var page = Convert.ToInt32(filter["CurrentPage"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);

                long count = await _context.Collection.Find(c => true).CountDocumentsAsync();

                var list = _context.Collection.AsQueryable().Where(per => !per.IsDeleted).Skip((page - 1) * pageSize)
                    .Take(pageSize).Select(_ => new ListPermissionViewModel()
                    {
                        Id = _.PermissionId,
                        ClientAddress = _.ClientAddress,
                        Title = _.Title,
                        Type = _.Type,
                        CreationDate = _.CreationDate,
                        CreatorName = _.CreatorUserName,
                        HasModification = _.Modifications != null && _.Modifications.Any(),
                        IsActive = _.IsActive
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

        public async Task<List<MenuLinkModel>> ListOfMenues(string currentUserId, string address)
        {
            var result = new List<MenuLinkModel>();
            try
            {
                var user = await _userManager.FindByIdAsync(currentUserId);
              
                if (user.IsSystemAccount)
                {
                    var query = _context.Collection.AsQueryable();
                    var allBaseMenues = query.Where(p => p.Type == Enums.PermissionType.baseMenu).ToList();
                    var allMenues = query.Where(p => p.Type == Enums.PermissionType.Menu).ToList();


                    result = allBaseMenues.Select(_ => new MenuLinkModel()
                    {
                        MenuId = _.PermissionId,
                        MenuTitle = GeneralLibrary.Utilities.Language.GetString("PermissionTitle_" + _.Title),
                        Icon = _.Icon,
                        Priority = _.Priority,
                        IsActive = _.ClientAddress != null && _.Routes.Contains(address),
                        Children = GetChildren(allMenues, _.PermissionId, address)
                    }).ToList().OrderBy(_=>_.Priority).ToList();
                }
                else
                {
                    var pers = _userRepository.GetPermissionsOfUser(user);
                    var baseMenues = pers.Where(p => p.Type == Enums.PermissionType.baseMenu).ToList();

                    var menues = pers.Where(p => p.Type == Enums.PermissionType.Menu).ToList();

                    result = baseMenues.Select(_ => new MenuLinkModel()
                    {
                        MenuId = _.PermissionId,
                        MenuTitle = GeneralLibrary.Utilities.Language.GetString("PermissionTitle_" + _.Title),
                        Icon = _.Icon,
                        Priority = _.Priority,
                        IsActive = _.ClientAddress != null && _.Routes.Contains(address),
                        Children = GetChildren(menues, _.PermissionId, address)
                    }).OrderBy(o => o.Priority).ToList();
                }
            }
            catch (Exception)
            {
              
            }
            return result;
        }

        public List<MenuLinkModel> GetChildren(List<Entities.General.Permission.Permission> context,
           string permissionId, string address)
        {
            if (context.Any(_ => _.ParentMenuId == permissionId))
            {
                return context.Select(_ => new MenuLinkModel()
                {
                    Icon = _.Icon,
                    IsActive = _.Routes != null && _.Routes.Contains(address),
                    Link = _.ClientAddress,
                    MenuTitle = GeneralLibrary.Utilities.Language.GetString("PermissionTitle_" + _.Title),
                    MenuId = _.PermissionId,
                    Priority = _.Priority,
                    Children = GetChildren(context, _.PermissionId, address)
                }).ToList();
            }
            else
            {
                return new List<MenuLinkModel>();
            }
        }
        public async Task<List<PermissionDTO>> MenusPermission(Enums.PermissionType typeMenu)
        {
            var result = new List<PermissionDTO>();
            try
            {
                var list = await _context.Collection.Find(_ => _.Type == typeMenu).ToListAsync();
                result = _mapper.Map<List<PermissionDTO>>(list);
            }
            catch (Exception)
            {
            }
            return result;
        }

        public async Task<RepositoryOperationResult> Save(PermissionDTO dto)
        {
            RepositoryOperationResult result;

            //mapping the input model to equallent object of database
            var equallentModel = _mapper.Map<Entities.General.Permission.Permission>(dto);

            if (!string.IsNullOrWhiteSpace(dto.PermissionId))//it is update case
            {
                result = await UpdatePermissionAsync(equallentModel, dto.ModificationReason);
            }
            else //it is insert case
            {
                result = await InsertPermissionAsync(equallentModel);
            }

            return result;
        }
        private async Task<RepositoryOperationResult> UpdatePermissionAsync
            (Entities.General.Permission.Permission equallentModel, string modificationReason)
        {
            var result = new RepositoryOperationResult();

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


        private async Task<RepositoryOperationResult> InsertPermissionAsync(
            Entities.General.Permission.Permission equallentModel)
        {
            var result = new RepositoryOperationResult();
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

        public async Task<RepositoryOperationResult<string>> GetPermissionType(string permissionId)
        {
            var result = new RepositoryOperationResult<string>();
           if (!string.IsNullOrWhiteSpace(permissionId))
            {
                var per = await _context.Collection
                    .Find(_ => _.PermissionId == permissionId).FirstOrDefaultAsync();

                if(per != null)
                {
                   result.ReturnValue =  per.Type.ToString();
                   result.Message = ConstMessages.SuccessfullyDone;
                   result.Succeeded = true;
                }else
                {
                    result.Message = ConstMessages.ObjectNotFound;
                }
            }else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public async Task<Entities.General.Permission.Permission> FetchPermission(string permissionId)
        {
            Entities.General.Permission.Permission result = null;
            result = await _context.Collection.Find(_ => _.PermissionId == permissionId).FirstOrDefaultAsync();
            return result;

        }
    }
}
