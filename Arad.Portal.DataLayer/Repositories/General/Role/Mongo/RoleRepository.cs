using Arad.Portal.DataLayer.Contracts.General.Role;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Role;
using System.Security.Claims;
using MongoDB.Bson;
using System.Collections.Specialized;
using System.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;


namespace Arad.Portal.DataLayer.Repositories.General.Role.Mongo
{
    public class RoleRepository : BaseRepository, IRoleRepository
    {
        private readonly RoleContext _context;
        private readonly IMapper _mapper;
       
        public RoleRepository(RoleContext roleContext, 
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            IWebHostEnvironment env): base(httpContextAccessor, env)
        {
            _context = roleContext;
            _mapper = mapper;
        }

        public async Task<Result> Add(RoleDTO dto)
        {
            var result = new Result();
            var equallentModel = _mapper.Map<Entities.General.Role.Role>(dto);
            equallentModel.PermissionIds = dto.PermissionIds.Split(",").ToList();
                
            equallentModel.CreationDate = DateTime.Now;
            equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

            equallentModel.IsActive = true;

            try
            {
                equallentModel.RoleId = Guid.NewGuid().ToString();
                await _context.Collection.InsertOneAsync(equallentModel);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }

            return result;
        }


        public async Task InsertMany(List<Entities.General.Role.Role> roles)
        {
            await _context.Collection.InsertManyAsync(roles);
        }

        public async Task<Result> ChangeActivation(string roleId)
        {
            var result = new Result();
            try
            {
                var roleEntity = _context.Collection.Find(_ => _.RoleId == roleId).FirstOrDefault();
                if(roleEntity != null)
                {
                    roleEntity.IsActive = !roleEntity.IsActive;
                    var updateResult = await _context.Collection.ReplaceOneAsync(_=>_.RoleId == roleId, roleEntity);
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
            catch(Exception e)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }

            return result;
        }

        public async Task<Result> Delete(string roleId, string modificationReason)
        {
            var result = new Result();
            try
            {

                var roleEntity = _context.Collection.Find(_ => _.RoleId == roleId).FirstOrDefault();
                if (roleEntity != null)
                {
                    roleEntity.IsDeleted = true;

                    #region Add Modification record
                    var currentModifications = roleEntity.Modifications;
                    var mod = GetCurrentModification(modificationReason);
                    currentModifications.Add(mod);
                    roleEntity.Modifications = currentModifications;
                    #endregion

                    var updateResult = await _context.Collection.ReplaceOneAsync(_ => _.RoleId == roleId, roleEntity);
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

        public async Task<RoleDTO> FetchRole(string roleId)
        {
            var result = new RoleDTO();
            try
            {
                var role = await _context.Collection
                    .Find(c => c.RoleId == roleId).FirstOrDefaultAsync();
                if(role != null)
                {
                    result = _mapper.Map<RoleDTO>(role);
                    result.PermissionIds = string.Join(',', role.PermissionIds);
                }
                
            }
            catch (Exception e)
            {
                result = null;
            }
            return result;
        }

        public async Task<RoleDTO> FetchRoleByName(string roleName)
        {
            var result = new RoleDTO();
            try
            {
                var role = await _context.Collection
                    .Find(c => c.RoleName == roleName).FirstOrDefaultAsync();
                if(role != null)
                {
                    result = _mapper.Map<RoleDTO>(role);
                    result.PermissionIds = string.Join(",", role.PermissionIds);
                }
               
            }
            catch (Exception e)
            {
                result = null;
            }
            return result;
        }

        public async Task<PagedItems<RoleDTO>> List(string queryString)
        {
            PagedItems<RoleDTO> result = new PagedItems<RoleDTO>();
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

                long totalCount = await _context.Collection.Find(c => true).CountDocumentsAsync();
                var list = _context.Collection.Find(_=> true)
                   .Project(_ => new RoleDTO()
                   { 
                     RoleId = _.RoleId,
                     RoleName = _.RoleName,
                     PermissionIds = string.Join(',', _.PermissionIds),
                       //IsActive = _.IsActive,
                       //IsDeleted = _.IsDeleted
                   }).ToList();

                result.CurrentPage = page;
                result.Items = list;
                result.ItemsCount = totalCount;
                result.PageSize = pageSize;
                result.QueryString = queryString;

            }
            catch (Exception ex)
            {
                result.CurrentPage = 1;
                result.Items = new List<RoleDTO>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }


        public async Task<PagedItems<RoleListViewModel>> RoleList(string queryString)
        {
            PagedItems<RoleListViewModel> result = new PagedItems<RoleListViewModel>();
            try
            {
                NameValueCollection filter = HttpUtility.ParseQueryString(queryString);

                if (string.IsNullOrWhiteSpace(filter["page"]))
                {
                    filter.Set("page", "1");
                }

                if (string.IsNullOrWhiteSpace(filter["pageSize"]))
                {
                    filter.Set("pageSize", "20");
                }

                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["pageSize"]);

                long totalCount = await _context.Collection.Find(_=>!_.IsDeleted).CountDocumentsAsync();
                var list = _context.Collection.AsQueryable().Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new RoleListViewModel()
                   {
                       Id = _.RoleId,
                       RoleName = _.RoleName,
                       CreatorId = _.CreatorUserId,
                       CreatorUserName = _.CreatorUserName,
                       CreationDateTime = _.CreationDate,
                       HasModifications = _.Modifications.Any(),
                       IsActive = _.IsActive,
                       IsDeleted = _.IsDeleted
                   }).ToList();

                result.CurrentPage = page;
                result.Items = list;
                result.ItemsCount = totalCount;
                result.PageSize = pageSize;
                result.QueryString = queryString;

            }
            catch (Exception ex)
            {
                result.CurrentPage = 1;
                result.Items = new List<RoleListViewModel>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<Result> Update(RoleDTO dto)
        {
            var result = new Result();

            var equallentModel = _mapper.Map<Entities.General.Role.Role>(dto);
            equallentModel.PermissionIds = dto.PermissionIds.Split(',').ToList();

            var availableEntity = await _context.Collection
                    .Find(_ => _.RoleId.Equals(dto.RoleId)).FirstOrDefaultAsync();

            if (availableEntity != null)
            {
                #region Add Modification
                var currentModifications = availableEntity.Modifications;
                var mod = GetCurrentModification(dto.ModificationReason);

                currentModifications.Add(mod);
                #endregion

                equallentModel.Modifications = currentModifications;

                equallentModel.CreationDate = availableEntity.CreationDate;
                equallentModel.CreatorUserId = availableEntity.CreatorUserId;
                equallentModel.CreatorUserName = availableEntity.CreatorUserName;
                equallentModel.IsActive = true;
                var updateResult = await _context.Collection
                   .ReplaceOneAsync(_ => _.RoleId == availableEntity.RoleId, equallentModel);

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

        public bool HasAny()
        {
            return _context.Collection.AsQueryable().Any();
        }

        

        public async Task<Entities.General.Role.Role> FetchRoleEntity(string roleId)
        {
            var result = new Entities.General.Role.Role();
            try
            {
                result = await _context.Collection
                    .Find(c => c.RoleId == roleId).FirstOrDefaultAsync();
               
            }
            catch (Exception e)
            {
                result = null;
            }
            return result;
        }
    }
}
