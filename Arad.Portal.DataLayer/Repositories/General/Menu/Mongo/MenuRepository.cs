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
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Arad.Portal.GeneralLibrary.Utilities;
using System.Collections.Specialized;
using System.Web;
using Arad.Portal.DataLayer.Entities.General.Menu;
using Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo;

namespace Arad.Portal.DataLayer.Repositories.General.Menu.Mongo
{
    public class MenuRepository : BaseRepository, IMenuRepository
    {
        private readonly MenuContext _context;
        private readonly ProductContext _productContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly DomainContext _domainContext;
        public MenuRepository(MenuContext context,
                                UserManager<ApplicationUser> userManager,
                                DomainContext domainContext,
                                IHttpContextAccessor httpContextAccessor,
                                ProductContext productContext,
                                IMapper mapper) : base(httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _domainContext = domainContext;
            _productContext = productContext;
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

        public async Task<PagedItems<MenuDTO>> AdminList(string queryString)
        {
            PagedItems<MenuDTO> result = new PagedItems<MenuDTO>();
            try
            {
                var userId = this.GetUserId();
                var userEntity = await _userManager.FindByIdAsync(userId);

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
                var domainId = filter["domainId"].ToString();
                var domainEntity = _domainContext.Collection.Find(_ => _.DomainId == domainId).FirstOrDefault();

                long totalCount = await _context.Collection.Find(_ => _.AssociatedDomainId == domainEntity.DomainId).CountDocumentsAsync();
                var lst = _context.Collection
                  //  .Find(_ => _.AssociatedDomainId == domainId && _.ParentId == null)
                  .Find(Builders<Entities.General.Menu.Menu>.Filter.And(Builders< Entities.General.Menu.Menu>.Filter.Eq(x=>x.AssociatedDomainId, domainId),
                                                                       Builders<Entities.General.Menu.Menu>.Filter.Eq(x => x.ParentId, null)))
                    .Project(_ =>
                        new MenuDTO()
                        {
                            MenuId = _.MenuId,
                            Icon = _.Icon,
                            MenuTitles = _.MenuTitles,
                            MenuType = _.MenuType,
                            Order = _.Order,
                            //????
                            //ParentTitles = _context.Collection.Find(b => b.MenuId == _.MenuId).First().MenuTitles,
                            ParentId = _.ParentId,
                            Url = _.Url
                        }).Sort(Builders<Entities.General.Menu.Menu>.Sort.Ascending(a => a.Order)).ToList();
                result.CurrentPage = page;
                result.Items = lst;
                result.ItemsCount = totalCount;
                result.PageSize = pageSize;
                result.QueryString = queryString;
            }
            catch (Exception e)
            {
                throw;
            }
            return result;
        }

        public  List<StoreMenuVM> StoreList(string domainId, string langId)
        {
            var result = new List<StoreMenuVM>();
            string finalLangId;
            var domainEntity = _domainContext.Collection.Find(Builders<Entities.General.Domain.Domain>.Filter.Eq(_ => _.AssociatedDomainId, domainId)).First();
            if(!string.IsNullOrWhiteSpace(langId))
            {
                finalLangId = langId;
            }else
            {
                finalLangId = domainEntity.DefaultLanguageId;
            }
            result = _context.Collection.Find(_ => _.AssociatedDomainId == domainId && _.ParentId == null)
                .Project(_ => new StoreMenuVM()
                {
                    MenuId = _.MenuId,
                    MenuTitle = _.MenuTitles.Count(_ => _.LanguageId == finalLangId) > 0 ?
                                _.MenuTitles.First(_ => _.LanguageId == finalLangId):
                                (_.MenuTitles.Count(_ => _.LanguageId == domainEntity.DefaultLanguageId) > 0 ?
                                _.MenuTitles.First(_ => _.LanguageId == domainEntity.DefaultLanguageId):
                                _.MenuTitles.First()),
                    Icon = _.Icon,
                    MenuType = _.MenuType,
                    Order = _.Order,
                    Url = _.Url,
                    Childrens = GetChildren(_.MenuId,finalLangId, domainEntity)
                }).ToList();
            
            return result;
        }

        public List<StoreMenuVM> GetChildren(string menuId, string finalLangId, Entities.General.Domain.Domain domainEntity)
        {
            var result = new List<StoreMenuVM>();
            if (_context.Collection.Find(_ => _.ParentId == menuId).CountDocuments() > 0)
            {
                result = _context.Collection.Find(_=>_.AssociatedDomainId == domainEntity.DomainId && _.ParentId == menuId)

                    .Project(_ => new StoreMenuVM()
                    {
                        MenuId = _.MenuId,
                        MenuTitle = _.MenuTitles.Count(_ => _.LanguageId == finalLangId) > 0 ?
                                _.MenuTitles.First(_ => _.LanguageId == finalLangId) :
                                (_.MenuTitles.Count(_ => _.LanguageId == domainEntity.DefaultLanguageId) > 0 ?
                                _.MenuTitles.First(_ => _.LanguageId == domainEntity.DefaultLanguageId) :
                                _.MenuTitles.First()),
                        Icon = _.Icon,
                        MenuType = _.MenuType,
                        Order = _.Order,
                        Url = _.Url,
                        Childrens = GetChildren(_.MenuId, finalLangId, domainEntity)
                    }).ToList();
            }
            return result;
        }

        public async Task<RepositoryOperationResult> DeleteMenu(string menuId)
        {
            RepositoryOperationResult result = new RepositoryOperationResult();
            try
            {
                var userId = this.GetUserId();
                var userName = this.GetUserName();
                #region check object dependency 
                //check whether it has any submenues or not
                var allowDeletion = true;
                if (_context.Collection
                        .Find(_ => _.ParentId == menuId && _.IsActive && !_.IsDeleted).CountDocuments() > 0)
                {
                    allowDeletion = false;
                }
                #endregion
                if (allowDeletion)
                {
                    var entity = _context.Collection.Find(_ => _.MenuId == menuId).First();
                    if (entity != null)
                    {
                        entity.IsDeleted = true;
                        #region add modification
                        var mod = GetCurrentModification($"Delete this menu by UserId={userId} and UserName={userName} at {DateTime.Now.ToPersianDdate()}");
                        entity.Modifications.Add(mod);
                        #endregion

                        var updateResult = await _context.Collection.ReplaceOneAsync(_ => _.MenuId == menuId, entity);
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
                else
                {
                    result.Message = ConstMessages.DeletedNotAllowedForDependencies;
                }

            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public RepositoryOperationResult<MenuDTO> FetchMenu(string menuId)
        {
            RepositoryOperationResult<MenuDTO> result
               = new RepositoryOperationResult<MenuDTO>();
            var entity = _context.Collection
                .Find(_ => _.MenuId == menuId).First();
            try
            {
                if (entity != null)
                {
                    var dto = _mapper.Map<MenuDTO>(entity);
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                    result.ReturnValue = dto;
                }
                else
                {
                    result.Message = ConstMessages.ObjectNotFound;
                }
            }
            catch (Exception)
            {
                result.Message = ConstMessages.ExceptionOccured;
            }

            return result;
        }
    }
}
