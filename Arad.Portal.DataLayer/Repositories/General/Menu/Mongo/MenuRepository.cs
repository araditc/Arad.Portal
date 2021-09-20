using Arad.Portal.DataLayer.Contracts.General.Menu;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Menu;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.General.Menu.Mongo
{
    public class MenuRepository : BaseRepository, IMenuRepository
    {
        private readonly MenuContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        public MenuRepository(MenuContext context,
                                UserManager<ApplicationUser> userManager,
                                IHttpContextAccessor httpContextAccessor,
                                IMapper mapper) : base(httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }
        public async Task<RepositoryOperationResult> AddMenu(MenuDTO dto)
        {
            RepositoryOperationResult result = new RepositoryOperationResult();
            try
            {
                var equallentModel = _mapper.Map<Entities.General.Menu.Menu>(dto);

                equallentModel.CreationDate = DateTime.Now;
                equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

                await _context.Collection.InsertOneAsync(equallentModel);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;

            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.InternalServerErrorMessage;
            }
            return result;
        }

        public async Task<PagedItems<MenuLinkModel>> AllShopMenuList(string domainId)
        {
            var result = new List<MenuLinkModel>();
            try
            {
                var query = _context.Collection.AsQueryable();
                var allBaseMenues = query.Where(p => p.Type == Enums.PermissionType.BaseMenu).ToList();
                var allMenues = query.Where(p => p.Type == Enums.PermissionType.Menu).ToList();


                result = allBaseMenues.Select(_ => new MenuLinkModel()
                {
                    MenuId = _.PermissionId,
                    MenuTitle = GeneralLibrary.Utilities.Language.GetString("PermissionTitle_" + _.Title),
                    Icon = _.Icon,
                    Link = _.ClientAddress,
                    Priority = _.Priority,
                    IsActive = (!string.IsNullOrWhiteSpace(_.ClientAddress) && _.ClientAddress.Contains(address)) || (_.ClientAddress == "" && _.Routes.Contains(address)),
                    Children = GetChildren(allMenues, _.PermissionId, address, domain)
                }).ToList().OrderBy(_ => _.Priority).ToList();
               
            }
            catch (Exception)
            {

            }
            return result;
        }

        public List<MenuLinkModel> GetChildren(List<Entities.General.Permission.Permission> context,
          string permissionId, string address, string domain)
        {
            var result = new List<MenuLinkModel>();
            if (context.Any(_ => _.ParentMenuId == permissionId))
            {
                result = context.Where(_ => _.ParentMenuId == permissionId)
                    .Select(_ => new MenuLinkModel()
                    {
                        Icon = _.Icon,
                        IsActive = !string.IsNullOrWhiteSpace(_.ClientAddress) && _.ClientAddress.Equals(address),
                        Link = _.ClientAddress,
                        MenuTitle = GeneralLibrary.Utilities.Language.GetString("PermissionTitle_" + _.Title),
                        MenuId = _.PermissionId,
                        Priority = _.Priority,
                        Children = GetChildren(context, _.PermissionId, address, domain)
                    }).ToList();
            }

            return result;

        }

        public Task<RepositoryOperationResult> DeleteMenu(string menuId)
        {
            throw new NotImplementedException();
        }

        public RepositoryOperationResult<MenuDTO> FetchMenu(string menuId)
        {
            throw new NotImplementedException();
        }
    }
}
