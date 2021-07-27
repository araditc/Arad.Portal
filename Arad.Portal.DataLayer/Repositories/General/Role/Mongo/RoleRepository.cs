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
using Arad.Portal.DataLayer.Entities.General.State;
using Arad.Portal.DataLayer.Entities.General.City;
using Arad.Portal.DataLayer.Entities.General.District;
using Arad.Portal.DataLayer.Entities.General.County;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Arad.Portal.DataLayer.Repositories.General.Role.Mongo
{
    public class RoleRepository : BaseRepository, IRoleRepository
    {
        private readonly RoleContext _context;
        private readonly IMapper _mapper;
        public IMongoCollection<State> States { get; set; }
        public IMongoCollection<City> Cities { get; set; }

        public IMongoCollection<District> Districts { get; set; }
        public IMongoCollection<County> Counties { get; set; }

        public RoleRepository(RoleContext roleContext, 
            IHttpContextAccessor httpContextAccessor, IMapper mapper): base(httpContextAccessor)
        {
            _context = roleContext;
            _mapper = mapper;
            States = roleContext.States;
            Cities = roleContext.Cities;
            Districts = roleContext.Districts;
            Counties = roleContext.Counties;
        }

        public async Task<RepositoryOperationResult> Add(RoleDTO dto)
        {
            var result = new RepositoryOperationResult();
            var equallentModel = _mapper.Map<Entities.General.Role.Role>(dto);

            equallentModel.CreationDate = DateTime.Now;
            equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

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

        public async Task<RepositoryOperationResult> ChangeActivation(string roleId)
        {
            var result = new RepositoryOperationResult();
            try
            {
                var roleEntity = _context.Collection.Find(_ => _.RoleId == roleId).First();
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

        public async Task<RepositoryOperationResult> Delete(string roleId, string modificationReason)
        {
            var result = new RepositoryOperationResult();
            try
            {

                var roleEntity = _context.Collection.Find(_ => _.RoleId == roleId).First();
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
                result = _mapper.Map<RoleDTO>(role);
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
                result = _mapper.Map<RoleDTO>(role);
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

                long totalCount = await _context.Collection.Find(c => true).CountDocumentsAsync();
                var list = _context.Collection.AsQueryable().Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new RoleDTO()
                   { 
                     RoleId = _.RoleId,
                     RoleName = _.RoleName,
                     PermissionIds = _.PermissionIds,
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

                long totalCount = await _context.Collection.Find(c => true).CountDocumentsAsync();
                var list = _context.Collection.AsQueryable().Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new RoleListViewModel()
                   {
                       Id = _.RoleId,
                       RoleName = _.RoleName,
                       CreatorId = _.CreatorUserId,
                       CreatorUserName = _.CreatorUserName,
                       CreationDateTime = _.CreationDate,
                       HasModifications = _.Modifications.Any(),
                       IsActive = _.IsActive
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

        public async Task<RepositoryOperationResult> Update(RoleDTO dto)
        {
            var result = new RepositoryOperationResult();

            var equallentModel = _mapper.Map<Entities.General.Role.Role>(dto);

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
            var result = false;
            if (_context.Collection.AsQueryable().Any())
            {
                result = true;
            }
            return result;
        }

        public List<SelectListModel> GetAllState()
        {
            var lst = _context.States.AsQueryable().Select(_ => new SelectListModel()
            {
                Value = _.Id,
                Text = _.Name
            }).ToList();
            return lst;
        }

        public List<SelectListModel> GetCounties(string stateId)
        {
            var state = _context.States.Find(_ => _.Id == stateId).FirstOrDefault();
            var lst = state.Counties.Select(_ => new SelectListModel()
            {
                Value = _.Id,
                Text = _.Name
            }).ToList();
            return lst;
        }

        public List<SelectListModel> GetDistricts(string countyId)
        {
            var county = _context.Counties.Find(_ => _.Id == countyId).FirstOrDefault();
            var lst = county.Districts.Select(_ => new SelectListModel()
            {
                Value = _.Id,
                Text = _.Name
            }).ToList();
            return lst;
        }

        public List<SelectListModel> GetCities(string districtId)
        {
            var district = _context.Districts.Find(_ => _.Id == districtId).FirstOrDefault();
            var lst = district.Cities.Select(_ => new SelectListModel()
            {
                Value = _.Id,
                Text = _.Name
            }).ToList();
            return lst;
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
