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
            UserManager<ApplicationUser> userManager, 
            IUserRepository userRepository): base(httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _userRepository = userRepository;
        }
        public async Task<RepositoryOperationResult> Delete(string permissionId)
        {
            var result = new RepositoryOperationResult();
            try
            {
                var delResult = await _context.Collection.DeleteOneAsync(_=>_.PermissionId == permissionId);
                if (delResult.IsAcknowledged)
                {
                   
                    result.Message = ConstMessages.SuccessfullyDone;
                    result.Succeeded = true;
                }
                else
                {
                    result.Message = ConstMessages.GeneralError;
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

        public async Task<List<LinkViewModel>> ListOfMenu(string currentUserId, string address)
        {
            var result = new List<LinkViewModel>();
            try
            {
                var user = await _userManager.FindByIdAsync(currentUserId);

                if (user.IsSystemAccount)
                {
                    var query = _context.Collection.AsQueryable();
                    var baseMenusAll = query.Where(p => p.Type == Enums.PermissionType.Category).ToList();

                    var menusAll = query.Where(p => p.Type == Enums.PermissionType.CategoryItem).ToList();

                    result = baseMenusAll.Select(_ => new LinkViewModel()
                    {
                        PerId = _.PermissionId,
                        MenuTitle = GeneralLibrary.Utilities.Language.GetString("PermissionTitle_" + _.Title),
                        Icon = _.Icon,
                        Priority = _.Priority,
                        IsActive = _.ClientAddress != null && _.Routes.Contains(address),
                        Links = menusAll.Where(b => b.ParentMenuId == _.PermissionId)
                            .Select(d => new LinkViewModel()
                            {
                                PerId = d.PermissionId,
                                MenuTitle = GeneralLibrary.Utilities.Language.GetString("PermissionTitle_" + d.Title),
                                Icon = d.Icon,
                                Priority = d.Priority,
                                Link = d.ClientAddress,
                                IsActive = d.ClientAddress != null && d.Routes.Contains(address),
                            }).ToList().OrderBy(o => o.Priority).ToList(),
                    }).ToList();

                    var listAllWithoutBaseMenu = menusAll.Where(b => b.ParentMenuId == null)
                        .Select(h => new LinkViewModel()
                        {
                            PerId = h.PermissionId,
                            MenuTitle = GeneralLibrary.Utilities.Language.GetString("PermissionTitle_" + h.Title),
                            Icon = h.Icon,
                            Priority = h.Priority,
                            Link = h.ClientAddress,
                            IsActive = h.ClientAddress != null && h.Routes.Contains(address),
                            Links = new List<LinkViewModel>()
                        }).ToList();

                    result.AddRange(listAllWithoutBaseMenu);
                    result = result.OrderBy(o => o.Priority).ToList();
                }
                else
                {
                    var pers = _userRepository.GetPermissionsOfUser(user);
                    var baseMenus = pers.Where(p => p.Type == Enums.PermissionType.Category).ToList();

                    var menus = pers.Where(p => p.Type == Enums.PermissionType.CategoryItem).ToList();

                    result = baseMenus.Select(_ => new LinkViewModel()
                    {
                        PerId = _.PermissionId,
                        MenuTitle = _.Title,
                        Icon = _.Icon,
                        Priority = _.Priority,
                        IsActive = _.ClientAddress != null && _.Routes.Contains(address),
                        Links = menus.Where(b => b.ParentMenuId == _.PermissionId && !b.Routes.Contains("permission"))
                            .Select(d => new LinkViewModel()
                            {
                                PerId = d.PermissionId,
                                MenuTitle = d.Title,
                                Icon = d.Icon,
                                Priority = d.Priority,
                                Link = d.ClientAddress,
                                IsActive = d.ClientAddress != null && d.Routes.Contains(address)
                            }).ToList().OrderBy(o => o.Priority).ToList(),
                    }).ToList();

                    var list = menus.Where(b => b.ParentMenuId.Equals("-1") && !b.Routes.Contains("permission"))
                        .Select(h => new LinkViewModel()
                        {
                            PerId = h.PermissionId,
                            MenuTitle = h.Title,
                            Icon = h.Icon,
                            Priority = h.Priority,
                            Link = h.ClientAddress,
                            IsActive = h.ClientAddress != null && h.Routes.Contains(address),
                            Links = new List<LinkViewModel>()
                        }).ToList();

                    result.AddRange(list);
                    result = result.OrderBy(o => o.Priority).ToList();
                }
            }
            catch (Exception)
            {
              
            }
            return result;
        }

        public List<PermissionDTO> MenusPermission(Enums.PermissionType typeMenu)
        {
            var result = new List<PermissionDTO>();
            try
            {
                var list = _context.Collection.AsQueryable().Where(c => c.Type == typeMenu).ToList();
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
                result = await UpdatePermissionAsync(dto, equallentModel);
            }
            else //it is insert case
            {
                result = await InsertPermissionAsync(equallentModel);
            }

            return result;
        }
        private async Task<RepositoryOperationResult> UpdatePermissionAsync(PermissionDTO dto,
            Entities.General.Permission.Permission equallentModel)
        {
            var result = new RepositoryOperationResult();

            var availableEntity = await _context.Collection
                    .Find(_ => _.PermissionId.Equals(dto.PermissionId)).FirstOrDefaultAsync();

            if (availableEntity != null)
            {
                #region Add Modification
                var currentModification = availableEntity.Modifications;
                currentModification ??= new List<Modification>();
                var mod = GetCurrentModification(dto.ModificationReason);

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
    }
}
