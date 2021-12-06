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
using Arad.Portal.DataLayer.Repositories.General.Content.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;

namespace Arad.Portal.DataLayer.Repositories.General.Menu.Mongo
{
    public class MenuRepository : BaseRepository, IMenuRepository
    {
        private readonly MenuContext _context;
        private readonly ProductContext _productContext;
        private readonly ContentContext _contentContext;
        private readonly LanguageContext _languageContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly DomainContext _domainContext;
        public MenuRepository(MenuContext context,
                                UserManager<ApplicationUser> userManager,
                                DomainContext domainContext,
                                ContentContext contentContext,
                                LanguageContext languageContext,
                                IHttpContextAccessor httpContextAccessor,
                                ProductContext productContext,
                                IMapper mapper) : base(httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _domainContext = domainContext;
            _productContext = productContext;
            _contentContext = contentContext;
            _languageContext = languageContext;
        }

        public async Task<Result> AddMenu(MenuDTO dto)
        {
            Result result = new Result();
            try
            {
                var equallentModel = _mapper.Map<Entities.General.Menu.Menu>(dto);


                equallentModel.MenuId = Guid.NewGuid().ToString();
                equallentModel.CreationDate = DateTime.Now;
                equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
                equallentModel.IsActive = true;
                
                var domainId = _httpContextAccessor.HttpContext.User.FindFirst("RelatedDomain")?.Value;
                
                equallentModel.AssociatedDomainId = domainId;
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
            var lan = new Entities.General.Language.Language();
            string languageId = "";
            try
            {
                var userId = this.GetUserId();
                var userEntity = await _userManager.FindByIdAsync(userId);

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
                var domainId = filter["domainId"].ToString();
                var parentId = "";
                if (string.IsNullOrWhiteSpace(filter["LanguageId"]))
                {
                    lan = _languageContext.Collection.Find(_ => _.IsDefault).FirstOrDefault();
                    languageId = lan.LanguageId;
                    filter.Set("LanguageId", lan.LanguageId);
                    
                }else
                {
                    languageId = filter["LanguageId"].ToString();
                }
                if(!string.IsNullOrWhiteSpace(filter["Id"]))
                {
                    parentId = filter["Id"].ToString();
                }
                var domainEntity = _domainContext.Collection.Find(_ => _.DomainId == domainId).FirstOrDefault();

                FilterDefinitionBuilder<Entities.General.Menu.Menu> builder = new();
                FilterDefinition<Entities.General.Menu.Menu> filterDef;
                if (userEntity.IsSystemAccount)
                    filterDef = builder.Empty;
                else
                    filterDef = builder.Eq(nameof(Entities.General.Menu.Menu.AssociatedDomainId), domainEntity.DomainId);
               
                filterDef = builder.And(filterDef, builder.Eq(nameof(Entities.General.Menu.Menu.IsActive), true));
                if(parentId != "")
                {
                    filterDef = builder.And(filterDef, builder.Eq(nameof(Entities.General.Menu.Menu.ParentId), parentId));
                }
              
                long totalCount = 0;
                if(parentId == "")
                {
                    if(userEntity.IsSystemAccount)
                    {
                        totalCount = await _context.Collection.Find(new FilterDefinitionBuilder<Entities.General.Menu.Menu>().Empty).CountDocumentsAsync();
                    }
                    else
                    {
                        totalCount = await _context.Collection.Find(_ => _.AssociatedDomainId == domainEntity.DomainId).CountDocumentsAsync();
                    }
                   
                }
                else
                {
                    if (userEntity.IsSystemAccount)
                    {
                        totalCount = await _context.Collection.Find(_ => _.ParentId == parentId).CountDocumentsAsync();
                    }
                    else
                    {
                        totalCount = await _context.Collection.Find(_ => _.AssociatedDomainId == domainEntity.DomainId && _.ParentId == parentId).CountDocumentsAsync();
                    }
                }

                var lst = _context.Collection
                  .Find(filterDef)
                    .Project(_ =>
                        new MenuDTO()
                        {
                            MenuId = _.MenuId,
                            Icon = _.Icon,
                            MenuTitle = _.MenuTitles.FirstOrDefault(_=>_.LanguageId == languageId).Name,
                            LanguageId = languageId,
                            MenuTitles = _.MenuTitles,
                            MenuType = _.MenuType,
                            Order = _.Order,
                            ParentName = _.ParentName,
                            ParentId = _.ParentId,
                            Url = _.Url,
                            SubId = _.SubId,
                            SubName = _.SubName,
                            SubGroupId = _.SubGroupId,
                            SubGroupName = _.SubGroupName,
                            CreatorUserName = _.CreatorUserName,
                            CreatorUserId = _.CreatorUserId,
                            IsDeleted = _.IsDeleted
                        }).Sort(Builders<Entities.General.Menu.Menu>.Sort.Ascending(a => a.Order)).Skip((page -1) * pageSize).Limit(pageSize).ToList();
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
            var domainEntity = _domainContext.Collection.Find(Builders<Entities.General.Domain.Domain>.Filter.Eq(_ => _.DomainId, domainId)).First();
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
                    SubId = _.SubId,
                    SubName = _.SubName,
                    SubGroupId = _.SubGroupId,
                    SubGroupName = _.SubGroupName,
                    MenuCode = _.MenuCode,
                    Childrens = GetChildren(_.MenuId, finalLangId, domainEntity),
                    IsFull = true
                }).ToList();
            //those menues which have isFull = true will be shown in UI
            #region check isFull
            var productGroupsMenu = result.Where(_ => _.MenuType == MenuType.ProductGroup);
            var prodctGroupMenuLeaves = productGroupsMenu.Where(_ => _.Childrens.Count() == 0);
            foreach (var leaf in prodctGroupMenuLeaves)
            {
                var tmp = new StoreMenuVM();
                var productsInGroupCounts = _productContext.ProductCollection.Find(_ => _.GroupIds.Contains(leaf.SubGroupId)).CountDocuments();
                if (productsInGroupCounts == 0)
                {
                    leaf.IsFull = false;
                    tmp = leaf;
                    while (tmp.ParentId != null)
                    {
                        tmp.IsFull = false;
                        tmp = result.FirstOrDefault(_ => _.MenuId == tmp.ParentId);
                    }
                    tmp.IsFull = false;
                }
            }
            var contentCategoryMenu = result.Where(_ => _.MenuType == MenuType.CategoryContent);
            var contentCategoryLeaves = contentCategoryMenu.Where(_ => _.Childrens.Count() == 0);
            foreach (var leaf in contentCategoryLeaves)
            {
                var tmp = new StoreMenuVM();
                var contentsInCategoryCounts = _contentContext.Collection
                    .Find(_ => _.ContentCategoryId == leaf.SubGroupId).CountDocuments();
                if (contentsInCategoryCounts == 0)
                {
                    leaf.IsFull = false;
                    tmp = leaf;
                    while (tmp.ParentId != null)
                    {
                        tmp.IsFull = false;
                        tmp = result.FirstOrDefault(_ => _.MenuId == tmp.ParentId);
                    }
                    tmp.IsFull = false;
                }
            }
            #endregion

            return result;
        }

        public List<StoreMenuVM> GetChildren(string menuId, string finalLangId, Entities.General.Domain.Domain domainEntity)
        {
            var result = new List<StoreMenuVM>();
            var menuEntity = _context.Collection.Find(_ => _.MenuId == menuId).First();
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
                        SubId = _.SubId,
                        SubName = _.SubName,
                        SubGroupId = _.SubGroupId,
                        SubGroupName = _.SubGroupName,
                        MenuCode = _.MenuCode,
                        Childrens = GetChildren(_.MenuId, finalLangId, domainEntity)
                    }).ToList();
            }
            return result;
        }

        public async Task<Result> DeleteMenu(string menuId)
        {
            Result result = new Result();
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

        public Result<MenuDTO> FetchMenu(string menuId)
        {
            Result<MenuDTO> result
               = new Result<MenuDTO>();
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

        public List<SelectListModel> GetAllMenuType()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(MenuType)))
            {
                string name = Enum.GetName(typeof(MenuType), i);
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

        public async Task<Result> EditMenu(MenuDTO dto)
        {
            var result = new Result();
            var entity = _context.Collection
                .Find(_ => _.MenuId == dto.MenuId).FirstOrDefault();
            var userId = this.GetUserId();
            var userName = this.GetUserName();
            if (entity != null)
            {
                entity.ParentId = dto.ParentId;
                #region Add Modification
                var currentModifications = entity.Modifications;
                var mod = GetCurrentModification($"Update menu by UserId={userId} and userName={userName} at Date={DateTime.Now.ToPersianDdate()}");
                currentModifications.Add(mod);
                #endregion

                entity.Modifications = currentModifications;
                entity.MenuTitles = dto.MenuTitles;
                entity.Url = dto.Url;
                entity.ParentId = dto.ParentId;
                entity.MenuType = dto.MenuType;
                entity.ParentId = dto.ParentId;
                entity.SubId = dto.SubId;
                entity.SubName = dto.SubName;
                entity.SubGroupId = dto.SubGroupId;
                entity.SubGroupName = dto.SubGroupName;
                entity.MenuCode = dto.MenuCode;
                entity.Order = dto.Order;
                entity.Icon = dto.Icon;


                var updateResult = await _context.Collection
               .ReplaceOneAsync(_ => _.MenuId == dto.MenuId, entity);
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
            else
            {
                result.Succeeded = false;
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public async Task<List<SelectListModel>> AllActiveMenues(string domainId, string langId)
        {
            var userId  = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var userEntity = await _userManager.FindByIdAsync(userId);

            var result = new List<SelectListModel>();

            if(userEntity.IsSystemAccount)
            {
                result = _context.Collection.Find(_ => _.IsActive)
                .Project(_ => new SelectListModel()
                {
                    Value = _.MenuId,
                    Text = _.MenuTitles.First(a => a.LanguageId == langId).Name
                }).ToList();
            }else
            {
                result = _context.Collection.Find(_ => _.IsActive && _.AssociatedDomainId == domainId)
                .Project(_ => new SelectListModel()
                {
                    Value = _.MenuId,
                    Text = _.MenuTitles.First(a => a.LanguageId == langId).Name
                }).ToList();
            }
            
            result.Insert(0, new SelectListModel() { Text = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_Choose"), Value = "-1" });
            return result;
        }

        public StoreMenuVM GetByCode(long menuCode)
        {
            var result = new StoreMenuVM();
            var entity = _context.Collection
                .Find(_ => _.MenuCode == menuCode).FirstOrDefault();
            result.MenuId = entity.MenuId;
            result.Icon = entity.Icon;
            result.MenuType = entity.MenuType;
            result.Order = entity.Order;
            result.Url = entity.Url;
            result.SubId = entity.SubId;
            result.SubName = entity.SubName;
            result.SubGroupId = entity.SubGroupId;
            result.SubGroupName = entity.SubGroupName;
            result.MenuCode = entity.MenuCode;
            return result;
        }
    }
}
