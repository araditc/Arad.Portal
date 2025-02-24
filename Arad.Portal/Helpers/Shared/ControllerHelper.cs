using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.ApplicationRole;
using Arad.Portal.DataLayer.Entities.General.BasicData;
using Arad.Portal.DataLayer.Entities.General.Comment;
using Arad.Portal.DataLayer.Entities.General.Content;
using Arad.Portal.DataLayer.Entities.General.ContentCategory;
using Arad.Portal.DataLayer.Entities.General.CountryParts;
using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Error;
using Arad.Portal.DataLayer.Entities.General.Menu;
using Arad.Portal.DataLayer.Entities.General.MessageTemplate;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Entities.General.Permission;
using Arad.Portal.DataLayer.Entities.General.Service;
using Arad.Portal.DataLayer.Entities.General.SliderModule;
using Arad.Portal.DataLayer.Entities.General.SystemSetting;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Entities.Shop.ProductGroup;
using Arad.Portal.DataLayer.Entities.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Entities.Shop.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Entities.Shop.ProductUnit;
using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using Arad.Portal.DataLayer.Entities.Shop.Setting;
using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Entities.Shop.Transaction;
using Arad.Portal.DataLayer.Helpers;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Shared.DesignStructure;
using Arad.Portal.DataLayer.Models.Shared.Product;
using Arad.Portal.DataLayer.Models.Shared.User;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.BasicData;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Comment;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Content;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.ContentCategory;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.CountryParts;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Currency;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Error;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Menu;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.MessageTemplate;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Permission;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Role;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Services;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SliderModule;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SystemSetting;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductGroup;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductUnit;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Promotion;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Setting;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Transaction;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.Content;
using Arad.Portal.Models.Shared.Domain;
using Arad.Portal.Models.Shared.Permission;
using Arad.Portal.Models.Shared.Product;
using Arad.Portal.Models.Shared.Role;
using Arad.Portal.Models.Shared.ShoppingCart;

using AutoMapper;

using Microsoft.AspNetCore.Http;

using MongoDB.Driver;

using static Arad.Portal.DataLayer.Models.Shared.Enums;

using Language = Arad.Portal.DataLayer.Entities.General.Language.Language;

namespace Arad.Portal.Helpers.Shared;

public sealed class ControllerHelper(
    IDomainRepository domainRepository,
    IBasicDataRepository basicDataRepository,
    IHttpContextAccessor httpContextAccessor,
    ILanguageRepository languageRepository,
    IContentRepository contentRepository,
    ICommentRepository commentRepository,
    IContentCategoryRepository contentCategoryRepository,
    ICountryRepository countryRepository,
    ICurrencyRepository currencyRepository,
    IMapper mapper,
    IErrorLogRepository errorLogRepository,
    IMessageTemplateRepository messageTemplateRepository,
    IRoleRepository roleRepository,
    IProviderRepository providerRepository,
    ISliderRepository sliderRepository,
    ISystemSettingRepository systemSettingRepository,
    IModificationRepository modificationRepository,
    IUserRepository userRepository,
    IProductRepository productRepository,
    IPromotionRepository promotionRepository,
    IProductGroupRepository productGroupRepository,
    IProductSpecificationRepository productSpecificationRepository,
    IProductSpecGroupRepository productSpecGroupRepository,
    IProductUnitRepository productUnitRepository,
    IShoppingCartRepository shoppingCartRepository,
    IShippingSettingRepository shippingSettingRepository,
    ITransactionRepository transactionRepository,
    IMenuRepository menuRepository,
    IPermissionRepository permissionRepository)
{
    public string GetCurrentUserId()
    {
        string currentUserId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        return currentUserId;
    }

    public async Task<ApplicationUser> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        string currentUserId = GetCurrentUserId();
        ApplicationUser currentUser = await userRepository.FirstOrDefaultAsync(c => c.Id == currentUserId, cancellationToken);

        return currentUser;
    }

    public string GetUserIpAddress()
    {
        if (httpContextAccessor.HttpContext == null)
        {
            return "";
        }

        IPAddress remoteIpAddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
        return remoteIpAddress != null ? remoteIpAddress.ToString() : "IP not found";

    }

    #region ICountryRepository
    public List<SelectListModel> GetAllCountries()
    {
        List<Country> countries = countryRepository.GetAll();
        List<SelectListModel> lst = countries.Select(c => new SelectListModel { Value = c.Id.ToString(), Text = c.Name }).ToList();
        lst.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });

        return lst;
    }
    #endregion

    #region IErrorLogRepository
    public async Task<Result> AddErrorLog(ErrorLog entity, CancellationToken cancellationToken = default)
    {
        try
        {
            Task<ApplicationUser> userDb = GetCurrentUser(cancellationToken);
            entity.Id = Guid.NewGuid().ToString();
            entity.CreationDate = DateTime.Now;
            entity.CreatorUserId = userDb.Result.Id;
            entity.CreatorUserName = userDb.Result.UserName;
            await errorLogRepository.InsertAsync(entity, cancellationToken);

            return new() { Succeeded = true, Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_OperationError") };
        }
    }
    #endregion

    #region IMessageTemplateRepository
    public async Task<List<MessageTemplate>> GetAllMessageTemplatesByName(string templateName)
    {
        return await messageTemplateRepository.GetListAsync(m => m.TemplateName.Equals(templateName));
    }
    #endregion

    #region IRoleRepository
    public RoleDto FetchRole(string roleId)
    {
        RoleDto result = new();

        try
        {
            ApplicationRole role = roleRepository.FirstOrDefault(c => c.Id == roleId);

            if (role != null)
            {
                result = new() { Name = role.Name, Id = role.Id, IsActive = role.IsActive, PermissionIds = string.Join(',', role.PermissionIds) };
            }
        }
        catch (Exception)
        {
            result = null;
        }

        return result;
    }
    #endregion

    #region IProviderRepository
    public List<SelectListModel> GetProvidersPerType(ProviderType type)
    {
        List<Provider> lst = providerRepository.GetList(p => p.ProviderType == type);

        List<SelectListModel> res = lst.Select(p => new SelectListModel { Text = p.ProviderName, Value = p.Id.ToString() }).ToList();

        res.Insert(0, new() { Value = "-1", Text = UtilityLanguage.GetString("AlertAndMessage_Choose") });

        return res;
    }
    #endregion

    #region ISystemSettingRepository
    public async Task<List<SystemSetting>> GetAll()
    {
        return await systemSettingRepository.GetListAsync(t => true);
    }
    #endregion

    #region ICommentRepository
    public List<CommentVm> CreateNestedTreeComment(List<Comment> comments, string currentUserId)
    {
        List<CommentVm> result = comments.Where(c => c.ParentId == null && c.IsApproved && !c.IsDeleted)
                                         .Select(c => new CommentVm
                                         {
                                             Id = c.Id,
                                             Content = c.Content,
                                             CreationDate = c.CreationDate,
                                             PersianCreationDate = c.CreationDate.ToLocalTime().ToPersianDdate(),
                                             CreatorUserId = c.CreatorUserId,
                                             CreatorUserName = c.CreatorUserName,
                                             DislikeCount = c.DislikeCount,
                                             ReferenceId = c.ReferenceId,
                                             ReferenceType = c.ReferenceType,
                                             LikeCount = c.LikeCount,
                                             UserStatus = !string.IsNullOrEmpty(currentUserId)
                                                                           ? httpContextAccessor.HttpContext?.Request.Cookies[$"{currentUserId}_cmt{c.Id}"] != null
                                                                                 ? httpContextAccessor.HttpContext.Request.Cookies[$"{currentUserId}_cmt{c.Id}"] == "true" ? UserStatus.Like : UserStatus.Dislike
                                                                                 : UserStatus.NoAction
                                                                           : UserStatus.UnAuthorized,
                                             Childrens = GetChildren(comments, c.Id, currentUserId)
                                         })
                                         .ToList();

        return result;
    }
    #endregion

    #region IProductSpecificationRepository
    public async Task<ProductSpecification> SpecificationFetch(string specId, CancellationToken cancellationToken = default)
    {
        ProductSpecification entity = await productSpecificationRepository.FirstOrDefaultAsync(c => c.Id == specId, cancellationToken);

        return entity;
    }
    #endregion

    #region IProductUnitRepository
    public async Task<List<SelectListModel>> GetAllActiveProductUnit(string langId, string domainId)
    {
        List<SelectListModel> result = [];

        try
        {
            Task<ApplicationUser> userEntity = GetCurrentUser();

            if (userEntity.Result.IsSystemAccount && string.IsNullOrEmpty(domainId))
            {
                result = (await productUnitRepository.GetListAsync(c => c.IsActive && !c.IsDeleted))
                         .Select(u => new SelectListModel { Text = u.UnitNames.Count(a => a.LanguageId == langId) != 0 ? u.UnitNames.FirstOrDefault(a => a.LanguageId == langId)?.Name : "", Value = u.Id.ToString() })
                         .Where(item => !string.IsNullOrEmpty(item.Text))
                         .ToList();
            }
            else
            {
                string? finalDomainId = !string.IsNullOrEmpty(domainId) ? domainId : userEntity.Result.Domains.FirstOrDefault(a => a.IsOwner)?.DomainId;
                List<ProductUnit> lst = (await productUnitRepository.GetAllAsync())
                                        .AsQueryable()
                                        .Where(c => c.IsActive &&
                                                    !c.IsDeleted &&
                                                    c.AssociatedDomainId == finalDomainId)
                                        .ToList();

                result = lst
                         .Select(c => new SelectListModel { Text = c.UnitNames.Count(a => a.LanguageId == langId) != 0 ? c.UnitNames.FirstOrDefault(a => a.LanguageId == langId)?.Name : "", Value = c.Id.ToString() })
                         .Where(item => !string.IsNullOrEmpty(item.Text))
                         .ToList();
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return result;
    }
    #endregion

    #region IPromotionRepository
    public List<SelectListModel> GetAllDiscountType(bool asCoupon)
    {
        List<SelectListModel> result = [];

        foreach (int i in Enum.GetValues(typeof(DiscountType)))
        {
            if (!asCoupon)
            {
                string name = Enum.GetName(typeof(DiscountType), i);
                SelectListModel obj = new() { Text = name, Value = i.ToString() };
                result.Add(obj);
            }
            else if (i != 2)
            {
                string name = Enum.GetName(typeof(DiscountType), i);
                SelectListModel obj = new() { Text = name, Value = i.ToString() };
                result.Add(obj);
            }
        }

        result.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });

        return result;
    }
    #endregion

    #region ITransactionRepository
    public async Task UpdateTransaction(Transaction transaction, CancellationToken cancellationToken)
    {
        Transaction entity
            = await transactionRepository.FirstOrDefaultAsync(t => t.Id == transaction.Id, cancellationToken);

        if (entity != null)
        {
            await transactionRepository.UpdateAsync(t => t.Id == transaction.Id, m => m, transaction, cancellationToken);
        }
    }
    #endregion

    #region ILanguageRepository
    public Language GetDefaultLanguage()
    {
        Language lan = null;

        if (httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
        {
            Task<ApplicationUser> userDb = GetCurrentUser();
            lan = !string.IsNullOrWhiteSpace(userDb.Result.Profile.DefaultLanguageId) ? languageRepository.FirstOrDefault(c => c.Id == userDb.Result.Profile.DefaultLanguageId) : languageRepository.FirstOrDefault(l => l.IsDefault);
        }
        else
        {
            lan = languageRepository.FirstOrDefault(l => l.IsDefault);
        }

        return lan;
    }

    public string FetchLanguageBySymbol(string symbol)
    {
        Language entity = languageRepository.FirstOrDefault(c => c.Symbol.ToLower() == symbol.ToLower());

        return entity != null ? entity.Id : "";
    }

    public List<SelectListModel> GetAllActiveLanguage()
    {
        List<SelectListModel> result = languageRepository.GetList(l => l.IsActive).Select(c => new SelectListModel { Text = c.LanguageName, Value = c.Id.ToString() }).ToList();

        return result;
    }

    public Language FetchLanguage(string languageId)
    {
        Language entity = languageRepository.FirstOrDefault(c => c.Id == languageId);

        return entity;
    }
    #endregion

    #region IBasicDataRepository
    public async Task<Result> InsertNewRecord(BasicData model, CancellationToken cancellationToken = default)
    {
        Result result = new();

        try
        {
            await basicDataRepository.InsertAsync(model, cancellationToken);
            result.Succeeded = true;
            result.Message = ConstMessages.SuccessfullyDone;
        }
        catch (Exception ex)
        {
            result.Message = ConstMessages.InternalServerErrorMessage;
        }

        return result;
    }

    public List<BasicData> GetBasicDataList(string groupKey, bool withChooseItem, bool isDomain = true, CancellationToken cancellationToken = default)
    {
        string domainName = GetCurrentDomainName();
        Domain domainEntity = domainRepository.Any(d => d.DomainName == domainName) ? domainRepository.First(d => d.DomainName == domainName) : domainRepository.First(d => d.IsDefault);

        if (groupKey.ToLower() == "shippingtype")
        {
            bool hasShippingType = HasShippingType();

            if (!hasShippingType)
            {
                BasicData post = new() { Id = Guid.NewGuid().ToString(), GroupKey = "ShippingType", Text = "Post", Value = "1", Order = 1, AssociatedDomainId = domainEntity.Id };
                basicDataRepository.InsertAsync(post, cancellationToken);

                BasicData courier = new() { Id = Guid.NewGuid().ToString(), GroupKey = "ShippingType", Text = "Courier", Value = "2", Order = 2, AssociatedDomainId = domainEntity.Id };
                basicDataRepository.InsertAsync(courier, cancellationToken);
            }
        }

        List<BasicData> result = !isDomain
                                     ? basicDataRepository.GetList(d => d.GroupKey.ToLower() == groupKey.ToLower() && string.IsNullOrEmpty(d.AssociatedDomainId)).ToList()
                                     : basicDataRepository.GetList(d => d.GroupKey.ToLower() == groupKey.ToLower() && d.AssociatedDomainId == domainEntity.Id).ToList();

        if (withChooseItem)
        {
            result.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });
        }

        return result;
    }

    private bool HasShippingType()
    {
        bool result = false;
        string domainName = GetCurrentDomainName();
        Domain domainEntity = domainRepository.Any(d => d.DomainName == domainName) ? domainRepository.FirstOrDefault(d => d.DomainName == domainName) : domainRepository.FirstOrDefault(d => d.IsDefault);

        if (basicDataRepository.Any(d => d.GroupKey.ToLower() == "shippingtype" && d.AssociatedDomainId == domainEntity.Id))
        {
            result = true;
        }

        return result;
    }
    #endregion

    #region IContentRepository
    public async Task<Content> ContentFetch(string contentId, bool isDeleted = false, CancellationToken cancellationToken = default)
    {
        Content result = new();

        ApplicationUser userDb = await GetCurrentUser(cancellationToken);
        string domainId = GetCurrentUserDomain().Id;
        try
        {
            if (userDb.IsSystemAccount)
            {
                result = await contentRepository.FirstOrDefaultAsync(c => c.Id == contentId && !c.IsDeleted, cancellationToken);
            }
            else
            {
                if (isDeleted)
                {
                    result = await contentRepository.FirstOrDefaultAsync(c => c.Id == contentId && c.AssociatedDomainId == domainId, cancellationToken);
                }
                else
                {
                    result = await contentRepository.FirstOrDefaultAsync(c => c.Id == contentId && !c.IsDeleted && c.AssociatedDomainId == domainId, cancellationToken);
                }
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return result;
    }

    public List<SelectListModel> GetContentsList(string domainId, string categoryId)
    {
        List<SelectListModel> result;
        Domain domainEntity = domainRepository.FirstOrDefault(d => d.Id == domainId) ?? domainRepository.FirstOrDefault(d => d.IsDefault);

        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            result = contentRepository.GetList(c => c.ContentCategoryId == categoryId && !c.IsDeleted && c.LanguageId == domainEntity.DefaultLanguageId && c.AssociatedDomainId == domainEntity.Id)
                                      .Select(c => new SelectListModel { Text = c.Title, Value = c.Id.ToString() })
                                      .ToList();
        }
        else
        {
            result = contentRepository.GetList(c => c.IsActive && !c.IsDeleted && c.AssociatedDomainId == domainEntity.Id)
                                      .Select(c => new SelectListModel { Text = c.Title, Value = c.Id.ToString() })
                                      .ToList();
        }

        return result;
    }

    public List<SelectListModel> AllActiveContentCategory(string langId, string domainId = "")
    {
        List<SelectListModel> result = [];
        Task<ApplicationUser> userDb = GetCurrentUser();

        try
        {
            if (userDb.Result.IsSystemAccount && string.IsNullOrWhiteSpace(domainId))
            {
                result = contentCategoryRepository.GetList(c => c.IsActive && !c.IsDeleted)
                                                  .Select(c => new SelectListModel
                                                  {
                                                      Text = c.CategoryNames.Count(a => a.LanguageId == langId) != 0 ? c.CategoryNames.First(a => a.LanguageId == langId).Name : "",
                                                      Value = c.Id.ToString()
                                                  })
                                                  .ToList();
            }
            else
            {
                string lastDomainId = !string.IsNullOrWhiteSpace(domainId) ? domainId : userDb.Result.Domains.FirstOrDefault(a => a.IsOwner)?.DomainId;
                List<ContentCategory> lst = contentCategoryRepository.GetAll()
                                                                     .AsQueryable()
                                                                     .Where(c => c.IsActive &&
                                                                                 !c.IsDeleted &
                                                                                 //dbUser.Profile.Access.AccessibleContentCategoryIds.Contains(_.ContentCategoryId) &&
                                                                                 c.AssociatedDomainId == lastDomainId)
                                                                     .ToList();
                result = lst
                         .Select(c => new SelectListModel { Text = c.CategoryNames.Any(a => a.LanguageId == langId) ? c.CategoryNames.First(a => a.LanguageId == langId).Name : "", Value = c.Id.ToString() })
                         .ToList();
            }
        }
        catch (Exception)
        {
            // ignored
        }

        if (result.Count > 0)
        {
            result.Insert(0, new() { Text = UtilityLanguage.GetString("AlertAndMessage_Choose"), Value = "-1" });
        }

        return result;

        ;
    }

    public async Task<ContentCategory> ContentCategoryFetch(string contentCategoryId, bool isDeleted = false)
    {
        ContentCategory category;

        try
        {
            if (isDeleted)
            {
                category = await contentCategoryRepository.FirstOrDefaultAsync(c => c.Id == contentCategoryId);
            }
            else
            {
                category = await contentCategoryRepository.FirstOrDefaultAsync(c => c.Id == contentCategoryId && !c.IsDeleted);
            }
        }
        catch (Exception)
        {
            category = null;
        }

        return category;
    }

    public async Task<PagedItems<ContentViewModel>> ContentList(string queryString, ApplicationUser user)
    {
        PagedItems<ContentViewModel> result = new();

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

            if (string.IsNullOrWhiteSpace(filter["LanguageId"]))
            {
                Language lan = await languageRepository.FirstOrDefaultAsync(l => l.IsDefault);
                filter.Set("LanguageId", lan.Id);
            }

            string langId = filter["LanguageId"];
            int page = Convert.ToInt32(filter["page"]);
            int pageSize = Convert.ToInt32(filter["PageSize"]);
            string domainId;
            IQueryable<Content> totalList;

            if (user.IsSystemAccount)
            {
                totalList = (await contentRepository.GetAllAsync()).AsQueryable();
            }
            else
            {
                domainId = user.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;
                totalList = (await contentRepository.GetAllAsync()).AsQueryable().Where(c => c.AssociatedDomainId == domainId);
            }

            if (!string.IsNullOrWhiteSpace(filter["catId"]))
            {
                totalList = totalList.Where(c => c.ContentCategoryId.ToString() == filter["catId"]);
            }

            if (!string.IsNullOrWhiteSpace(filter["filter"]))
            {
                totalList = totalList.Where(c => c.TagKeywords.Contains(filter["filter"]) || c.Contents.Contains(filter["filter"]));
            }

            int totalCount = totalList.Count();
            totalList = totalList.Where(c => c.LanguageId.ToString() == langId);
            List<ContentViewModel> list = totalList.OrderByDescending(c => c.CreationDate)
                                                   .Skip((page - 1) * pageSize)
                                                   .Take(pageSize)
                                                   .Select(c => new ContentViewModel
                                                   {
                                                       Id = c.Id,
                                                       SeoDescription = c.SeoDescription,
                                                       LanguageName = c.LanguageName,
                                                       LanguageId = c.LanguageId,
                                                       Images = c.Images,
                                                       EndShowDate = c.EndShowDate,
                                                       ContentCategoryId = c.ContentCategoryId,
                                                       ContentCategoryName = c.ContentCategoryName,
                                                       ContentProviderName = c.ContentProviderName,
                                                       Description = c.Description,
                                                       SeoTitle = c.SeoTitle,
                                                       StartShowDate = c.StartShowDate,
                                                       SourceType = c.SourceType,
                                                       SubTitle = c.SubTitle,
                                                       TagKeywords = c.TagKeywords,
                                                       Title = c.Title,
                                                       UrlFriend = c.UrlFriend,
                                                       VisitCount = c.VisitCount,
                                                       IsDeleted = c.IsDeleted
                                                   })
                                                   .ToList();

            result.Items = list;
            result.CurrentPage = page;
            result.ItemsCount = totalCount;
            result.PageSize = pageSize;
            result.QueryString = queryString;
        }
        catch (Exception)
        {
            result.CurrentPage = 1;
            result.Items = [];
            result.ItemsCount = 0;
            result.PageSize = 10;
            result.QueryString = queryString;
        }

        return result;
    }

    public List<SelectListModel> GetAllImageTemplate()
    {
        List<SelectListModel> result = [];
        result.AddRange(from int i in Enum.GetValues(typeof(ImageTemplateType)) let name = Enum.GetName(typeof(ImageTemplateType), i) select new SelectListModel { Text = name, Value = i.ToString() });

        return result;
    }

    public List<SelectListModel> GetAllImageRatio()
    {
        List<SelectListModel> result = [];
        result.AddRange(from int i in Enum.GetValues(typeof(ImageRatio)) let name = Enum.GetName(typeof(ImageRatio), i) select new SelectListModel() { Text = name, Value = i.ToString() });

        return result;
    }

    public List<SelectListModel> GetAllSourceType()
    {
        List<SelectListModel> result = [];
        result.AddRange(from int i in Enum.GetValues(typeof(SourceType)) let name = Enum.GetName(typeof(SourceType), i) select new SelectListModel { Text = name, Value = i.ToString() });

        result.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });

        return result;
    }

    public async Task<Result> UpdateContent(ContentDto dto, CancellationToken cancellationToken)
    {
        Result result = new();
        Content content = ContentFetch(dto.Id).Result;

        if (content == null)
        {
            result.Message = ConstMessages.GeneralError;
            return result;
        }
        Content equivalentModel = mapper.Map(dto, content);

        if (!string.IsNullOrWhiteSpace(dto.PersianStartShowDate))
        {
            equivalentModel.StartShowDate = dto.PersianStartShowDate.Split(" ")[0].ToEnglishDate();
        }
        else if (dto.StartShowDate != null)
        {
            equivalentModel.StartShowDate = dto.StartShowDate.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.PersianEndShowDate))
        {
            equivalentModel.EndShowDate = dto.PersianEndShowDate.Split(" ")[0].ToEnglishDate();
        }
        else if (dto.EndShowDate != null)
        {
            equivalentModel.EndShowDate = dto.EndShowDate.Value;
        }

        if (httpContextAccessor.HttpContext != null)
        {
        }

        if (httpContextAccessor.HttpContext != null)
        {
        }

        Result<Content> updateResult = await contentRepository.UpdateAsync(equivalentModel, cancellationToken);

        if (updateResult.Succeeded)
        {
            result.Succeeded = true;
            result.Message = ConstMessages.SuccessfullyDone;
        }
        else
        {
            result.Message = ConstMessages.NotRecordChange;
        }

        return result;
    }

    public List<ContentGlance> GetContentInCategory(ProductOrContentType contentType, string contentCategoryId)
    {
        List<ContentGlance> lst = [];

        string domainName = GetCurrentDomainName();
        Domain domainEntity = domainRepository.FirstOrDefault(d => d.DomainName == $"https://{domainName}");

        FilterDefinitionBuilder<Content> builder = new();
        builder.Eq(nameof(Content.IsDeleted), false);
        builder.Eq(nameof(Content.ContentCategoryId), contentCategoryId);
        builder.Gte(nameof(Content.EndShowDate), DateTime.UtcNow);
        builder.Lte(nameof(Content.StartShowDate), DateTime.UtcNow);

        if (domainEntity == null)
        {
            return lst;
        }

        //??? should be uncommented in development mode
        builder.Eq(nameof(Content.AssociatedDomainId), domainEntity.Id);

        lst = contentType switch
        {
            ProductOrContentType.Newest => contentRepository.GetList(c => c.IsDeleted == false && c.ContentCategoryId == contentCategoryId)
                                                            .Select(c => new ContentGlance
                                                            {
                                                                TotalScore = c.TotalScore,
                                                                ScoredCount = c.ScoredCount,
                                                                VisitCount = c.VisitCount,
                                                                CategoryName = c.ContentCategoryName,
                                                                ContentCategoryId = c.ContentCategoryId,
                                                                Id = c.Id,
                                                                ContentProviderName = c.ContentProviderName,
                                                                Description = c.Description,
                                                                Images = c.Images,
                                                                SubTitle = c.SubTitle,
                                                                TagKeywords = c.TagKeywords,
                                                                Title = c.Title,
                                                                UrlFriend = c.UrlFriend,
                                                                ContentCode = c.ContentCode
                                                            })
                                                            .ToList() /*.Sort(Builders<DataLayer.Entities.General.Content.Content>.Sort.Descending(_ => _.StartShowDate)).Limit(count).ToList()*/,
            ProductOrContentType.MostPopular => contentRepository.GetList(c => c.IsDeleted == false && c.ContentCategoryId == contentCategoryId)
                                                                 .Select(c => new ContentGlance
                                                                 {
                                                                     TotalScore = c.TotalScore,
                                                                     ScoredCount = c.ScoredCount,
                                                                     VisitCount = c.VisitCount,
                                                                     CategoryName = c.ContentCategoryName,
                                                                     ContentCategoryId = c.ContentCategoryId,
                                                                     Id = c.Id,
                                                                     ContentProviderName = c.ContentProviderName,
                                                                     Images = c.Images,
                                                                     Description = c.Description,
                                                                     SubTitle = c.SubTitle,
                                                                     TagKeywords = c.TagKeywords,
                                                                     Title = c.Title,
                                                                     UrlFriend = c.UrlFriend,
                                                                     ContentCode = c.ContentCode
                                                                 })
                                                                 .ToList() /*.Sort(Builders<DataLayer.Entities.General.Content.Content>.Sort.Descending(_ => (float)_.TotalScore / _.ScoredCount)).Limit(count).ToList()*/,
            ProductOrContentType.MostVisited => contentRepository.GetList(c => c.IsDeleted == false && c.ContentCategoryId == contentCategoryId)
                                                                 .Select(c => new ContentGlance
                                                                 {
                                                                     TotalScore = c.TotalScore,
                                                                     ScoredCount = c.ScoredCount,
                                                                     VisitCount = c.VisitCount,
                                                                     CategoryName = c.ContentCategoryName,
                                                                     ContentCategoryId = c.ContentCategoryId,
                                                                     Id = c.Id,
                                                                     ContentProviderName = c.ContentProviderName,
                                                                     Images = c.Images,
                                                                     SubTitle = c.SubTitle,
                                                                     Description = c.Description,
                                                                     TagKeywords = c.TagKeywords,
                                                                     Title = c.Title,
                                                                     UrlFriend = c.UrlFriend,
                                                                     ContentCode = c.ContentCode
                                                                 })
                                                                 .ToList() /*.Sort(Builders<Entities.General.Content.Content>.Sort.Descending(_ => _.VisitCount)).Limit(count).ToList();*/,
            _ => lst
        };

        foreach (ContentGlance con in lst)
        {
            EntityRate r = Utilities.ConvertPopularityRate(con.TotalScore, con.ScoredCount);
            con.LikeRate = r.LikeRate;
            con.HalfLikeRate = r.HalfLikeRate;
            con.DisikeRate = r.DisikeRate;
        }

        return lst;
    }

    public List<ContentGlance> GetSpecialContent(int count,
                                                 ProductOrContentType contentType,
                                                 SelectionType selectionType,
                                                 string categoryId,
                                                 List<string> selectedIds = null,
                                                 bool isDevelopment = false,
                                                 string domainId = "")
    {
        List<ContentGlance> lst = [];

        Domain domainEntity = string.IsNullOrWhiteSpace(domainId) ? GetCurrentUserDomain() : domainRepository.FirstOrDefault(d => d.Id == domainId);

        List<Content> query = contentRepository.GetList(c => !c.IsDeleted);

        if (selectionType == SelectionType.CustomizedSelection && selectedIds is { Count: > 0 })
        {
            query = query.Where(c => selectedIds.Contains(c.Id)).ToList();
        }
        else
        {
            query = query.Where(c => c.StartShowDate <= DateTime.UtcNow && c.EndShowDate >= DateTime.UtcNow).ToList();

            if (selectionType == SelectionType.LatestFromProductOrContentTypeSelectedCategory)
            {
                query = query.Where(c => c.ContentCategoryId == categoryId).ToList();
            }
        }

        if (domainEntity != null)
        {
            query = query.Where(c => c.AssociatedDomainId == domainEntity.Id).ToList();

            switch (contentType)
            {
                case ProductOrContentType.Newest:
                    lst = query.OrderByDescending(c => c.StartShowDate)
                               .Take(count)
                               .Select(c => new ContentGlance
                               {
                                   TotalScore = c.TotalScore,
                                   ScoredCount = c.ScoredCount,
                                   VisitCount = c.VisitCount,
                                   CategoryName = c.ContentCategoryName,
                                   ContentCategoryId = c.ContentCategoryId,
                                   Id = c.Id,
                                   ContentProviderName = c.ContentProviderName,
                                   Images = c.Images,
                                   SubTitle = c.SubTitle,
                                   TagKeywords = c.TagKeywords,
                                   Title = c.Title,
                                   UrlFriend = c.UrlFriend,
                                   Description = c.Description,
                                   ContentCode = c.ContentCode
                               })
                               .ToList();

                    break;

                case ProductOrContentType.MostPopular:
                    lst = query.OrderByDescending(c => (float)c.TotalScore / c.ScoredCount)
                               .Take(count)
                               .Select(c => new ContentGlance
                               {
                                   TotalScore = c.TotalScore,
                                   ScoredCount = c.ScoredCount,
                                   VisitCount = c.VisitCount,
                                   CategoryName = c.ContentCategoryName,
                                   ContentCategoryId = c.ContentCategoryId,
                                   Id = c.Id,
                                   ContentProviderName = c.ContentProviderName,
                                   Images = c.Images,
                                   SubTitle = c.SubTitle,
                                   TagKeywords = c.TagKeywords,
                                   Title = c.Title,
                                   Description = c.Description,
                                   UrlFriend = c.UrlFriend,
                                   ContentCode = c.ContentCode
                               })
                               .ToList();

                    break;

                case ProductOrContentType.MostVisited:
                    lst = query.OrderByDescending(c => c.VisitCount)
                               .Take(count)
                               .Select(c => new ContentGlance
                               {
                                   TotalScore = c.TotalScore,
                                   ScoredCount = c.ScoredCount,
                                   VisitCount = c.VisitCount,
                                   CategoryName = c.ContentCategoryName,
                                   ContentCategoryId = c.ContentCategoryId,
                                   Id = c.Id,
                                   ContentProviderName = c.ContentProviderName,
                                   Images = c.Images,
                                   SubTitle = c.SubTitle,
                                   TagKeywords = c.TagKeywords,
                                   Description = c.Description,
                                   Title = c.Title,
                                   UrlFriend = c.UrlFriend,
                                   ContentCode = c.ContentCode
                               })
                               .ToList();

                    break;
            }

            foreach (ContentGlance con in lst)
            {
                EntityRate r = Utilities.ConvertPopularityRate(con.TotalScore, con.ScoredCount);
                con.LikeRate = r.LikeRate;
                con.HalfLikeRate = r.HalfLikeRate;
                con.DisikeRate = r.DisikeRate;
            }
        }

        return lst;
    }
    #endregion

    #region ICurrencyRepository
    public Result<Currency> FetchCurrency(string currencyId)
    {
        Result<Currency> result = new();

        try
        {
            Currency dbEntity = currencyRepository.FirstOrDefault(c => c.Id == currencyId);

            if (dbEntity == null)
            {
                result.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
            }

            result.Succeeded = true;
            result.Message = ConstMessages.SuccessfullyDone;
            result.ReturnValue = dbEntity;
        }
        catch (Exception)
        {
            result.Message = ConstMessages.ExceptionOccured;
        }

        return result;
    }

    public Result<Currency> GetDefaultCurrency()
    {
        Result<Currency> result = new();

        try
        {
            Task<ApplicationUser> currentUser = GetCurrentUser();

            Currency dbEntity = !string.IsNullOrEmpty(currentUser.Result.Profile.DefaultCurrencyId) ? currencyRepository.FirstOrDefault(c => c.Id == currentUser.Result.Profile.DefaultCurrencyId) : currencyRepository.FirstOrDefault(c => c.IsDefault);

            if (dbEntity == null)
            {
                result.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
            }

            result.Succeeded = true;
            result.Message = ConstMessages.SuccessfullyDone;
            result.ReturnValue = dbEntity;
        }
        catch (Exception)
        {
            result.Message = ConstMessages.ExceptionOccured;
        }

        return result;
    }

    public List<SelectListModel> GetAllActiveCurrency()
    {
        List<SelectListModel> result = currencyRepository.GetAll().AsQueryable().Where(c => c.IsActive).Select(c => new SelectListModel { Text = c.CurrencyName, Value = c.Id.ToString() }).ToList();
        result.Insert(0, new() { Value = "-1", Text = UtilityLanguage.GetString("AlertAndMessage_Choose") });

        return result;
    }

    public Currency GetCurrencyByItsPrefix(string prefix)
    {
        Currency result;

        try
        {
            result = currencyRepository.FirstOrDefault(c => c.Prefix == prefix);
        }
        catch (Exception)
        {
            result = null;
        }

        return result;
    }
    #endregion

    #region IMaduleRepository
    public List<SelectListModel> GetAllTransactionType()
    {
        List<SelectListModel> result = [];

        foreach (int i in Enum.GetValues(typeof(TransActionType)))
        {
            string name = Enum.GetName(typeof(TransActionType), i);
            SelectListModel obj = new() { Text = name, Value = i.ToString() };
            result.Add(obj);
        }

        result.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });

        return result;
    }

    public List<SelectListModel> GetAllLoadAnimationType()
    {
        List<SelectListModel> result = [];

        foreach (int i in Enum.GetValues(typeof(LoadAnimationType)))
        {
            string name = Enum.GetName(typeof(LoadAnimationType), i);
            SelectListModel obj = new() { Text = name, Value = i.ToString() };
            result.Add(obj);
        }

        result.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });

        return result;
    }
    #endregion

    #region IDomainRepository
    public async Task<Result> EditDomain(DomainModel dto, CancellationToken cancellationToken = default)
    {
        Result result = new();
        Domain entity = await domainRepository.FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);

        entity.LogoImage ??= new();

        entity.FavicoImage ??= new();

        if (entity != null)
        {
            Domain domain = mapper.Map(dto, entity);

            #region Prices
            domain.Prices = [];

            foreach (PriceDto price in dto.Prices.OrderBy(c => c.StartDate))
            {
                if (price.IsActive && string.IsNullOrWhiteSpace(price.EndDate))
                {
                    if (domain.Prices.Any(c => c.CurrencyId == price.CurrencyId && c.EndDate != null && c.IsActive))
                    {
                        Price exist = domain.Prices.FirstOrDefault(c => c.CurrencyId == price.CurrencyId && c.EndDate != null && c.IsActive);

                        if (exist != null)
                        {
                            exist.IsActive = false;
                            exist.EndDate = DateTime.UtcNow;
                        }
                    }
                }

                Price p = new()
                {
                    PriceId = !string.IsNullOrEmpty(price.PriceId) ? price.PriceId : Guid.NewGuid().ToString(),
                    CurrencyId = price.CurrencyId,
                    CurrencyName = price.CurrencyName,
                    IsActive = string.IsNullOrEmpty(price.PriceId) || price.IsActive,
                    Prefix = price.Prefix,
                    PriceValue = price.PriceValue,
                    StartDate = price.StartDate.Split(" ")[0].ToEnglishDate().ToUniversalTime(),
                    EndDate = !string.IsNullOrWhiteSpace(price.EndDate) ? price.EndDate.Split(" ")[0].ToEnglishDate().ToUniversalTime() : null
                };
                domain.Prices.Add(p);
            }
            #endregion

            domain.InvoiceNumberProcedure = (InvoiceNumberProcedure)Convert.ToInt32(dto.InvoiceNumberProcedure);
            domain.DefaultShippingTypeId = dto.DefaultShippingTypeId;

            Result<Domain> updateResult = await domainRepository.UpdateAsync(domain, cancellationToken);

            if (updateResult.Succeeded)
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
            result.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
        }

        return result;
    }

    public List<SelectListModel> GetAllActiveDomains()
    {
        List<SelectListModel> result = domainRepository.GetList(d => d.IsActive && !d.IsDeleted)
                                                       .Select(d => new SelectListModel { Text = d.DomainName, Value = d.Id.ToString() })
                                                       .ToList();
        result.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "" });

        return result;
    }

    public string GetCurrentDomainName()
    {
        Debug.Assert(httpContextAccessor.HttpContext != null, "httpContextAccessor.HttpContext != null");
        string domain = $"{httpContextAccessor.HttpContext.Request.Host}";

        return domain;
    }

    public Domain GetCurrentUserDomain()
    {
        Task<ApplicationUser> userDb = GetCurrentUser();
        Result<Domain> result = new();
        string? domainId = userDb.Result.Domains.FirstOrDefault(c => c.IsOwner)?.DomainId;

        if (string.IsNullOrEmpty(domainId))
        {
            domainId = userDb.Result.Domains.FirstOrDefault()?.DomainId;
        }
        Domain domain = FetchDomain(domainId).ReturnValue;

        return domain;
    }

    public bool HasLastId()
    {
        bool result = false;
        string domainName = GetCurrentDomainName();
        Domain domainEntity = domainRepository.Any(d => d.DomainName == domainName) ? domainRepository.FirstOrDefault(d => d.DomainName == domainName) : domainRepository.FirstOrDefault(d => d.IsDefault);

        if (basicDataRepository.Any(d => d.GroupKey.ToLower() == "lastid" && d.AssociatedDomainId == domainEntity.Id))
        {
            result = true;
        }

        return result;
    }

    public Result<Domain> FetchDefaultDomain()
    {
        Result<Domain> result = new();

        try
        {
            Domain dbEntity = domainRepository.FirstOrDefault(d => d.IsDefault);

            if (dbEntity != null)
            {
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
                result.ReturnValue = dbEntity;
            }
            else
            {
                result.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
                result.ReturnValue = new();
            }
        }
        catch (Exception)
        {
            result.Message = ConstMessages.ExceptionOccured;
        }

        return result;
    }

    public Result<Domain> FetchDomain(string domainId)
    {
        Result<Domain> result = new();

        try
        {
            Domain dbEntity = domainRepository.FirstOrDefault(d => d.Id == domainId);

            if (dbEntity == null)
            {
                result.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
            }
            else
            {
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
                result.ReturnValue = dbEntity;
            }
        }
        catch (Exception)
        {
            result.Message = ConstMessages.ExceptionOccured;
        }

        return result;
    }

    public Result<Domain> FetchDomainByName(string domainName, bool isDef)
    {
        Result<Domain> result = new();

        try
        {
            Domain dbEntity = domainRepository.FirstOrDefault(d => d.DomainName == domainName);

            if (dbEntity == null && isDef)
            {
                dbEntity = domainRepository.FirstOrDefault(d => d.IsDefault);
            }

            if (dbEntity != null)
            {
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
                result.ReturnValue = dbEntity;
            }
            else
            {
                result.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
                result.ReturnValue = new();
            }
        }
        catch (Exception)
        {
            result.Message = ConstMessages.ExceptionOccured;
        }

        return result;
    }
    #endregion

    #region ISliderRepository
    public Slider GetSlider(string id, string domainId)
    {
        Domain domainEntity = null;

        if (!string.IsNullOrEmpty(domainId))
        {
            domainEntity = domainRepository.FirstOrDefault(d => d.Id == domainId);
        }

        try
        {
            return !string.IsNullOrEmpty(domainId) ? sliderRepository.FirstOrDefault(s => s.Id == id && !s.IsDeleted && s.AssociatedDomainId == domainEntity.Id) : sliderRepository.FirstOrDefault(s => s.Id == id && !s.IsDeleted);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            throw;
        }
    }

    public async Task<bool> UpdateSlider(Slider model, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await sliderRepository.AnyAsync(s => s.Id == model.Id, cancellationToken))
            {
                return false;
            }

            {
                await sliderRepository.FirstOrDefaultAsync(s => s.Id == model.Id, cancellationToken);

                Result<Slider> result = await sliderRepository.UpdateAsync(c => c.Id == model.Id, m => m, model, cancellationToken);

                if (result.Succeeded)
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
    #endregion

    #region IUserRepository
    public List<SelectListModel> GetAddressTypes()
    {
        List<SelectListModel> result = [];
        result.AddRange(from int i in Enum.GetValues(typeof(AddressType)) let name = Enum.GetName(typeof(AddressType), i) select new SelectListModel { Text = name, Value = i.ToString() });

        result.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });

        return result;
    }

    public List<UserFavorites> GetUserFavoriteList(string userId, FavoriteType type)
    {
        List<UserFavorites> list = userRepository.FirstOrDefault(p => p.Favorites.All(c => c.CreatorUserId == userId && c.FavoriteType == type)).Favorites;

        return list;
    }

    public List<SelectListModel> GetAllProductType()
    {
        List<SelectListModel> result = [];
        result.AddRange(from int i in Enum.GetValues(typeof(ProductType)) let name = Enum.GetName(typeof(ProductType), i) select new SelectListModel { Text = UtilityLanguage.GetString($"EnumDesc_{name}"), Value = i.ToString() });

        return result;
    }

    public List<SelectListModel> GetAllDownloadLimitationType()
    {
        List<SelectListModel> result = [];
        result.AddRange(from int i in Enum.GetValues(typeof(DownloadLimitationType)) let name = Enum.GetName(typeof(DownloadLimitationType), i) select new SelectListModel { Text = UtilityLanguage.GetString($"EnumDesc_{name}"), Value = i.ToString() });

        result.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });

        return result;
    }
    #endregion

    #region IProductRepository
    public async Task<ProductOutputDto> ProductFetch(string productId)
    {
        ProductOutputDto result = new();
        string currentUserId = "";

        if (httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
        {
            currentUserId = GetCurrentUserId();
        }

        Product entity = await productRepository.FirstOrDefaultAsync(p => p.Id == productId);

        if (entity == null)
        {
            return result;
        }

        result = mapper.Map<ProductOutputDto>(entity);
        List<Comment> comments = await commentRepository.GetListAsync(c => c.ReferenceId == entity.Id && c.ReferenceType == ReferenceType.Product);

        if (!string.IsNullOrEmpty(currentUserId))
        {
            result.Comments = CreateNestedTreeComment(comments, currentUserId);
        }

        //TODO : 
        // var staticFileStorageURL = _configuration["StaticFilesPlace:APIURL"];

        return result;
    }

    private List<CommentVm> GetChildren(List<Comment> list, string currentCommentId, string currentUserId)
    {
        List<CommentVm> result;

        if (list.All(c => c.ParentId != currentCommentId))
        {
            result = [];
        }
        else
        {
            result = list.Where(c => c.ParentId == currentCommentId && c.IsApproved && !c.IsDeleted)
                         .Select(c => new CommentVm
                         {
                             Id = c.Id,
                             Content = c.Content,
                             CreationDate = c.CreationDate,
                             PersianCreationDate = c.CreationDate.ToLocalTime().ToPersianDdate(),
                             CreatorUserId = c.CreatorUserId,
                             CreatorUserName = c.CreatorUserName,
                             DislikeCount = c.DislikeCount,
                             ReferenceId = c.ReferenceId,
                             ReferenceType = c.ReferenceType,
                             LikeCount = c.LikeCount,
                             UserStatus = !string.IsNullOrEmpty(currentUserId)
                                                           ? httpContextAccessor.HttpContext?.Request.Cookies[$"{currentUserId}_cmt{c.Id}"] != null
                                                                 ? httpContextAccessor.HttpContext.Request.Cookies[$"{currentUserId}_cmt{c.Id}"] == "true" ? UserStatus.Like : UserStatus.Dislike
                                                                 : UserStatus.NoAction
                                                           : UserStatus.UnAuthorized,
                             Childrens = GetChildren(list, c.Id, currentUserId)
                         })
                         .ToList();
        }

        return result;
    }

    public ProductOutputDto EvaluateFinalPrice(string productId, List<Price> productPrices, List<string> productGroupIds, string defaultCurrencyId)
    {
        ProductOutputDto result = new();
        Price activePrice = productPrices.FirstOrDefault(p => p.IsActive && p.CurrencyId == defaultCurrencyId && p.StartDate <= DateTime.Now && (p.EndDate == null || p.EndDate.Value >= DateTime.Now));

        //check whether this product has any valid promotion
        Promotion promotionOnAll = null;
        Promotion promotionOnProductGroup = null;
        Promotion promotionOnThisProduct = null;
        Promotion newestPromotion = null;
        Domain domainEntity = domainRepository.Any(d => d.DomainName == GetCurrentDomainName()) ? domainRepository.FirstOrDefault(d => d.DomainName == GetCurrentDomainName()) : domainRepository.FirstOrDefault(d => d.IsDefault);

        List<Promotion> promotionList = [];

        if (promotionRepository.Any(p => p.PromotionType == PromotionType.All &&
                                         (p.EDate == null || p.EDate.Value >= DateTime.Now) &&
                                         p.SDate <= DateTime.Now &&
                                         p.AssociatedDomainId == domainEntity.Id &&
                                         p.IsActive &&
                                         !p.IsDeleted))
        {
            promotionOnAll = promotionRepository.FirstOrDefault(p => p.PromotionType == PromotionType.All &&
                                                                     (p.EDate == null || p.EDate.Value >= DateTime.Now) &&
                                                                     p.SDate <= DateTime.Now &&
                                                                     p.AssociatedDomainId == domainEntity.Id &&
                                                                     p.IsActive &&
                                                                     !p.IsDeleted);
        }

        if (promotionRepository.Any(p => p.PromotionType == PromotionType.Group &&
                                         p.SDate <= DateTime.Now &&
                                         (p.EDate == null || p.EDate.Value >= DateTime.Now) &&
                                         p.IsActive &&
                                         !p.IsDeleted &&
                                         p.AssociatedDomainId == domainEntity.Id &&
                                         p.Infoes.Any(a => productGroupIds.Contains(a.AffectedProductGroupId))))
        {
            promotionOnProductGroup = promotionRepository
                .FirstOrDefault(p => p.PromotionType == PromotionType.Group &&
                                     p.Infoes.Any(a => productGroupIds.Contains(a.AffectedProductGroupId)) &&
                                     p.AssociatedDomainId == domainEntity.Id &&
                                     p.IsActive &&
                                     !p.IsDeleted &&
                                     p.SDate <= DateTime.Now &&
                                     (p.EDate == null || p.EDate >= DateTime.Now));
        }

        if (promotionRepository.Any(p => p.PromotionType ==
                                         PromotionType.Product &&
                                         p.SDate <= DateTime.Now &&
                                         (p.EDate == null || p.EDate.Value >= DateTime.Now) &&
                                         p.Infoes.Any(a => a.AffectedProductId == productId)))
        {
            promotionOnThisProduct = promotionRepository.FirstOrDefault(p => p.PromotionType ==
                                                                             PromotionType.Product &&
                                                                             p.SDate <= DateTime.Now &&
                                                                             (p.EDate == null || p.EDate.Value >= DateTime.Now) &&
                                                                             p.Infoes.Any(a => a.AffectedProductId == productId));
        }

        if (promotionOnAll != null)
        {
            promotionList.Add(promotionOnAll);
        }

        if (promotionOnProductGroup != null)
        {
            promotionList.Add(promotionOnProductGroup);
        }

        if (promotionOnThisProduct != null)
        {
            promotionList.Add(promotionOnThisProduct);
        }

        decimal finalPrice = 0;

        foreach (Promotion t in promotionList)
        {
            if (newestPromotion == null)
            {
                if (t != null)
                {
                    newestPromotion = t;
                }
            }
            else
            {
                if (t.SDate > newestPromotion.SDate)
                {
                    newestPromotion = t;
                }
            }
        }

        if (newestPromotion != null)
        {
            switch (newestPromotion.DiscountType)
            {
                case DiscountType.Fixed:
                    if (newestPromotion.Value != null)
                    {
                        if (activePrice != null)
                        {
                            finalPrice = activePrice.PriceValue - newestPromotion.Value.Value;
                            result.OldPrice = activePrice.PriceValue;
                        }

                        result.DiscountType = newestPromotion.DiscountType;
                        result.DiscountValue = newestPromotion.Value;
                    }

                    break;

                case DiscountType.Percentage:
                    if (newestPromotion.Value != null)
                    {
                        if (activePrice != null)
                        {
                            decimal percentage = activePrice.PriceValue * newestPromotion.Value.Value / 100;
                            finalPrice = activePrice.PriceValue - percentage;
                        }
                    }

                    if (activePrice != null)
                    {
                        result.OldPrice = activePrice.PriceValue;
                    }

                    result.DiscountType = newestPromotion.DiscountType;
                    result.DiscountValue = newestPromotion.Value;

                    break;

                case DiscountType.Product:
                    if (activePrice != null)
                    {
                        finalPrice = activePrice.PriceValue;
                    }

                    Product giftProduct = productRepository.FirstOrDefault(p => p.Id == newestPromotion.PromotedProductId);
                    result.GiftProduct = mapper.Map<ProductOutputDto>(giftProduct);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            result.Promotion = newestPromotion;
        }

        if (activePrice != null)
        {
            result.PriceValWithPromotion = finalPrice == 0 ? activePrice.PriceValue : finalPrice;
        }

        return result;
    }

    public bool IsPublishOnMainDomain(string productId)
    {
        Product entity = productRepository.FirstOrDefault(p => p.Id == productId);

        return entity is { IsPublishedOnMainDomain: true };
    }

    public ProductOutputDto FetchByCode(string slugOrCode, Domain model, string userId)
    {
        ProductOutputDto result = new();
        Product productEntity = new();

        if (long.TryParse(slugOrCode, out long codeNumber))
        {
            if (!string.IsNullOrEmpty(model.Id))
            {
                productEntity = model.IsDefault ? productRepository.FirstOrDefault(p => ((p.AssociatedDomainId == model.Id && p.ProductCode == codeNumber) || (p.ProductCode == codeNumber && p.IsPublishedOnMainDomain)) && !p.IsDeleted) : productRepository.FirstOrDefault(p => p.AssociatedDomainId == model.Id && p.ProductCode == codeNumber && !p.IsDeleted);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(model.Id))
            {
                if (model.IsDefault)
                {
                    productEntity = productRepository.FirstOrDefault(p => ((p.AssociatedDomainId == model.Id && p.MultiLingualProperties.Any(a => a.UrlFriend == $"/product/{slugOrCode}")) ||
                                                                           (p.MultiLingualProperties.Any(a => a.UrlFriend == $"/product/{slugOrCode}") && p.IsPublishedOnMainDomain)) &&
                                                                          !p.IsDeleted);
                }
                else
                {
                    productEntity = productRepository.FirstOrDefault(p => p.AssociatedDomainId == model.Id && p.MultiLingualProperties.Any(a => a.UrlFriend == $"/product/{slugOrCode}") && !p.IsDeleted);
                }
            }
        }

        if (productEntity != null)
        {
            result = mapper.Map<ProductOutputDto>(productEntity);
            List<Comment> comments = commentRepository.GetList(c => c.ReferenceId == "p*" + productEntity.Id && c.ReferenceType == ReferenceType.Product);

            result.Images = result.Images.Where(i => i.ImageRatio == ImageRatio.Square).ToList();

            if (!string.IsNullOrEmpty(userId))
            {
                result.Comments = CreateNestedTreeComment(comments, userId);
            }

            result.MultiLingualProperties = productEntity.MultiLingualProperties;

            #region evaluate finalPrice
            ProductOutputDto res = EvaluateFinalPrice(productEntity.Id, productEntity.Prices, productEntity.GroupIds, model.DefaultCurrencyId);
            result.GiftProduct = res.GiftProduct;
            result.Promotion = res.Promotion;
            result.PriceValWithPromotion = res.PriceValWithPromotion;
            result.OldPrice = res.OldPrice;
            result.DiscountType = res.DiscountType;
            result.DiscountValue = res.DiscountValue;
            #endregion

            foreach (ProductSpecificationValue item in result.Specifications)
            {
                List<string> lst = item.Values.Split("|").ToList();
                int i = 1;

                foreach (SelectListModel obj in lst.Select(str => new SelectListModel { Text = str, Value = i.ToString() }))
                {
                    i += 1;
                    item.ValueList.Add(obj);
                }
            }

            EntityRate r = Utilities.ConvertPopularityRate(productEntity.TotalScore, productEntity.ScoredCount);
            result.LikeRate = r.LikeRate;
            result.DisikeRate = r.DisikeRate;
            result.HalfLikeRate = r.HalfLikeRate;
        }

        return result;
    }

    public string FetchIdByCode(long productCode)
    {
        Product entity = productRepository.FirstOrDefault(p => p.ProductCode == productCode);

        return entity != null ? entity.Id : "";
    }

    public async Task<Result> ImportFromExcel(List<ProductExcelImport> lst, CancellationToken cancellationToken)
    {
        Result result = new();
        string domainName = GetCurrentDomainName();
        ApplicationUser userDb = await GetCurrentUser(cancellationToken);
        Domain domainEntity = await domainRepository.FirstOrDefaultAsync(d => d.DomainName == "https://" + domainName, cancellationToken);
        Currency currencyEntity = await currencyRepository.FirstOrDefaultAsync(c => c.Id == domainEntity.DefaultCurrencyId, cancellationToken);
        ApplicationUser userEntity = await userRepository.FirstOrDefaultAsync(c => c.Id == userDb.Id, cancellationToken);

        try
        {
            foreach (ProductExcelImport pro in lst)
            {
                Product product = new();
                ProductUnit productUnitEntity = await productUnitRepository.FirstOrDefaultAsync(u => u.UnitNames.Any(a => a.LanguageId == domainEntity.DefaultLanguageId && a.Name == pro.ProductUnit), cancellationToken);

                product.Id = Guid.NewGuid().ToString();
                product.GroupIds = pro.GroupIds;
                product.ProductCode = pro.ProductCode;
                product.IsActive = true;
                product.MultiLingualProperties.Add(new()
                {
                    MultiLingualPropertyId = Guid.NewGuid().ToString(),
                    CurrencyId = domainEntity.DefaultCurrencyId,
                    CurrencyName = domainEntity.DefaultCurrencyName,
                    LanguageId = domainEntity.DefaultLanguageId,
                    LanguageName = domainEntity.DefaultLanguageName,
                    Name = pro.ProductName,
                    SeoDescription = pro.SeoDescription,
                    SeoTitle = pro.SeoTitle,
                    TagKeywords = pro.TagKeywords.Split(',').ToList(),
                    CurrencyPrefix = currencyEntity.Prefix,
                    CurrencySymbol = currencyEntity.Symbol
                });
                product.Prices.Add(new()
                {
                    CurrencyId = domainEntity.DefaultCurrencyId,
                    CurrencyName = domainEntity.DefaultCurrencyName,
                    StartDate = DateTime.Now,
                    PriceId = Guid.NewGuid().ToString(),
                    IsActive = true,
                    PriceValue = pro.Price,
                    Symbol = currencyEntity.Symbol
                });

                if (pro.ProductImage != null)
                {
                    product.Images.Add(new() { ImageId = pro.ProductImage.ImageId, Url = pro.ProductImage.Url });
                }

                product.UniqueCode = pro.UniqueCode;
                product.Inventory = [];
                product.ShowInLackOfInventory = pro.ShowInLackOfInventory;
                product.SellerUserId = GetCurrentUserId();
                product.SellerUserName = GetCurrentUser(cancellationToken).Result.UserName;
                product.Unit = productUnitEntity;
                product.IsPublishedOnMainDomain = pro.IsPublishOnMainDomain;

                product.CreationDate = DateTime.Now;

                if (httpContextAccessor.HttpContext != null)
                {
                    product.CreatorUserId = httpContextAccessor.HttpContext.User.Claims
                                                               .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                                                               ?.Value;
                    product.CreatorUserName = httpContextAccessor.HttpContext.User.Claims
                                                                 .FirstOrDefault(c => c.Type == ClaimTypes.Name)
                                                                 ?.Value;
                }

                product.AssociatedDomainId = userEntity.Domains.FirstOrDefault(c => c.IsOwner)?.DomainId;

                await productRepository.InsertAsync(product, cancellationToken);

                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Insert,
                    CollectionType = CollectionType.Excel,
                    Ip = GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = product.Id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
            }
        }
        catch (Exception)
        {
            result.Succeeded = false;
            result.Message = ConstMessages.ErrorInSaving;
        }
        finally
        {
            result.Succeeded = true;
            result.Message = ConstMessages.SuccessfullyDone;
        }

        return result;
    }

    public List<ProductOutputDto> GetSpecialProducts(int count, string currencyId, ProductOrContentType type, int skip = 0, string domainId = "")
    {
        // Get domain entity
        Domain domainEntity;

        if (string.IsNullOrWhiteSpace(domainId))
        {
            _ = "https://" + GetCurrentDomainName();
            domainEntity = GetCurrentUserDomain();
        }
        else
        {
            domainEntity = domainRepository.FirstOrDefault(d => d.Id == domainId);
        }

        // Get currency symbol
        string currencySymbol = currencyRepository.GetList(c => c.Id == currencyId)
                                                  .Select(c => c.Symbol)
                                                  .FirstOrDefault();

        // Initialize query for products
        List<Product> query = productRepository.GetList(p => p.IsActive && !p.IsDeleted);

        // Apply domain filter
        query = !domainEntity.IsDefault ? query.Where(p => p.AssociatedDomainId == domainEntity.Id).ToList() : query.Where(p => p.AssociatedDomainId == domainEntity.Id || p.IsPublishedOnMainDomain).ToList();

        // Project products to DTOs and sort/filter based on type

        List<ProductOutputDto> lst = type switch
        {
            ProductOrContentType.Newest => query.OrderByDescending(p => p.CreationDate)
                                                .Skip(skip)
                                                .Take(count)
                                                .Select(p => new ProductOutputDto
                                                {
                                                    GroupIds = p.GroupIds,
                                                    CurrencySymbol = currencySymbol,
                                                    Inventory = p.Inventory,
                                                    Images = p.Images,
                                                    MultiLingualProperties = p.MultiLingualProperties,
                                                    Prices = p.Prices,
                                                    ProductCode = p.ProductCode,
                                                    Id = p.Id,
                                                    Promotion = p.Promotion,
                                                    SaleCount = p.SaleCount,
                                                    UniqueCode = p.UniqueCode,
                                                    TotalScore = p.TotalScore,
                                                    ScoredCount = p.ScoredCount,
                                                    Unit = p.Unit,
                                                    VisitCount = p.VisitCount
                                                })
                                                .ToList(),
            ProductOrContentType.MostPopular => query.OrderByDescending(p => p.TotalScore)
                                                     .Skip(skip)
                                                     .Take(count)
                                                     .Select(p => new ProductOutputDto
                                                     {
                                                         GroupIds = p.GroupIds,
                                                         CurrencySymbol = currencySymbol,
                                                         Inventory = p.Inventory,
                                                         Images = p.Images,
                                                         MultiLingualProperties = p.MultiLingualProperties,
                                                         Prices = p.Prices,
                                                         ProductCode = p.ProductCode,
                                                         Id = p.Id,
                                                         Promotion = p.Promotion,
                                                         SaleCount = p.SaleCount,
                                                         UniqueCode = p.UniqueCode,
                                                         TotalScore = p.TotalScore,
                                                         ScoredCount = p.ScoredCount,
                                                         Unit = p.Unit,
                                                         VisitCount = p.VisitCount
                                                     })
                                                     .ToList(),
            ProductOrContentType.BestSale => query.OrderByDescending(p => p.SaleCount)
                                                  .Skip(skip)
                                                  .Take(count)
                                                  .Select(p => new ProductOutputDto
                                                  {
                                                      GroupIds = p.GroupIds,
                                                      CurrencySymbol = currencySymbol,
                                                      Inventory = p.Inventory,
                                                      Images = p.Images,
                                                      MultiLingualProperties = p.MultiLingualProperties,
                                                      Prices = p.Prices,
                                                      ProductCode = p.ProductCode,
                                                      Id = p.Id,
                                                      Promotion = p.Promotion,
                                                      SaleCount = p.SaleCount,
                                                      UniqueCode = p.UniqueCode,
                                                      TotalScore = p.TotalScore,
                                                      ScoredCount = p.ScoredCount,
                                                      Unit = p.Unit,
                                                      VisitCount = p.VisitCount
                                                  })
                                                  .ToList(),
            ProductOrContentType.MostVisited => query.OrderByDescending(p => p.VisitCount)
                                                     .Skip(skip)
                                                     .Take(count)
                                                     .Select(p => new ProductOutputDto
                                                     {
                                                         GroupIds = p.GroupIds,
                                                         CurrencySymbol = currencySymbol,
                                                         Inventory = p.Inventory,
                                                         Images = p.Images,
                                                         MultiLingualProperties = p.MultiLingualProperties,
                                                         Prices = p.Prices,
                                                         ProductCode = p.ProductCode,
                                                         Id = p.Id,
                                                         Promotion = p.Promotion,
                                                         SaleCount = p.SaleCount,
                                                         UniqueCode = p.UniqueCode,
                                                         TotalScore = p.TotalScore,
                                                         ScoredCount = p.ScoredCount,
                                                         Unit = p.Unit,
                                                         VisitCount = p.VisitCount
                                                     })
                                                     .ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        // Additional processing on the result list
        foreach (ProductOutputDto pro in lst)
        {
            ProductOutputDto res = EvaluateFinalPrice(pro.Id, pro.Prices, pro.GroupIds, currencyId);
            pro.GiftProduct = res.GiftProduct;
            pro.Promotion = res.Promotion;
            pro.PriceValWithPromotion = res.PriceValWithPromotion;
            pro.OldPrice = res.OldPrice;
            pro.DiscountType = res.DiscountType;
            pro.DiscountValue = res.DiscountValue;
            pro.MainImageUrl = pro.Images.FirstOrDefault(img => img.IsMain)?.Url ?? "";
            pro.MainAlt = pro.Images.FirstOrDefault(img => img.IsMain)?.Title ?? "";
            EntityRate r = Utilities.ConvertPopularityRate(pro.TotalScore ?? 0, pro.ScoredCount ?? 0);
            pro.LikeRate = r.LikeRate;
            pro.DisikeRate = r.DisikeRate;
            pro.HalfLikeRate = r.HalfLikeRate;
        }

        return lst;
    }

    public async Task<ModelOutputFilter> GetFilterList(string languageId, string domainId, string groupId = null)
    {
        ModelOutputFilter model = new() { Filters = [] };
        FilterDefinitionBuilder<Product> builder = new();
        List<List<string>> productSpecifications = [];
        List<string> commonSpecIds;

        FilterDefinition<Product> filterDef = builder.Eq(nameof(Product.IsActive), true);
        filterDef &= builder.Eq(nameof(Product.IsDeleted), false);

        //test
        //filterDef &= _builder.Eq(nameof(Entities.Shop.Product.Product.AssociatedDomainId), domainId);
        if (groupId != null)
        {
            filterDef &= builder.AnyIn(nameof(Product.GroupIds), new List<string> { groupId });
        }

        //Product product = new Product();
        //using IAsyncCursor<DataLayer.Entities.Shop.Product.Product> cursor = await _productRepository.GetAll()
        //    .Select(c => c.IsActive && !c.IsDeleted);

        //while (await cursor.MoveNextAsync())
        //{
        //    var specList = cursor.Current.Select(_ => _.Specifications.Select(a => a.SpecificationId).ToList()).ToList();
        //    ProductSpecifications.AddRange(specList);
        //}

        //find common specs between list intersection of alls
        try
        {
            commonSpecIds = productSpecifications
                            .Skip(1)
                            .Aggregate(
                                new HashSet<string>(productSpecifications.First()),
                                (h, e) =>
                                {
                                    h.IntersectWith(e);

                                    return h;
                                }
                            )
                            .ToList();

            foreach (string id in commonSpecIds)
            {
                ProductSpecification spec = await productSpecificationRepository.FirstOrDefaultAsync(s => s.Id == id);
                DynamicFilter obj = new()
                {
                    ControlType = spec.ControlType,
                    SpecificationId = spec.Id,
                    SpecificationName = spec.SpecificationNameValues.Any(p => p.LanguageId == languageId) ? spec.SpecificationNameValues.FirstOrDefault(p => p.LanguageId == languageId)?.Name : "",
                    PossibleValues = spec.ControlType == ControlType.CheckBoxList ? spec.SpecificationNameValues.FirstOrDefault(p => p.LanguageId == languageId)?.NameValues : []
                };
                model.Filters.Add(obj);
            }

            if (groupId != null)
            {
                model.MinPrice = (await productRepository.GetAllAsync())
                                                  .AsQueryable()
                                                  .Where(p => p.IsActive && !p.IsDeleted && p.AssociatedDomainId == domainId && p.GroupIds.Contains(groupId))
                                                  .Select(p => p.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow)))
                                                  .Any()
                                     ? (await productRepository.GetAllAsync())
                                       .AsQueryable()
                                       .Where(p => p.IsActive && !p.IsDeleted && p.AssociatedDomainId == domainId && p.GroupIds.Contains(groupId))
                                       .Select(p => p.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow)))
                                       .OrderBy(p => p.PriceValue)
                                       .FirstOrDefault()!
                                                        .PriceValue
                                     : 0;

                model.MaxPrice = (await productRepository.GetAllAsync())
                                                  .AsQueryable()
                                                  .Where(p => p.IsActive && !p.IsDeleted && p.AssociatedDomainId == domainId && p.GroupIds.Contains(groupId))
                                                  .Select(p => p.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow)))
                                                  .Any()
                                     ? (await productRepository.GetAllAsync())
                                       .AsQueryable()
                                       .Where(p => p.IsActive && !p.IsDeleted && p.AssociatedDomainId == domainId && p.GroupIds.Contains(groupId))
                                       .Select(p => p.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow)))
                                       .OrderByDescending(p => p.PriceValue)
                                       .FirstOrDefault()!
                                                        .PriceValue
                                     : 0;
            }
            else
            {
                model.MinPrice = (await productRepository.GetAllAsync())
                                                  .AsQueryable()
                                                  .Where(p => p.IsActive && !p.IsDeleted && p.AssociatedDomainId == domainId)
                                                  .Select(p => p.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow)))
                                                  .Any()
                                     ? (await productRepository.GetAllAsync())
                                       .AsQueryable()
                                       .Where(p => p.IsActive && !p.IsDeleted && p.AssociatedDomainId == domainId)
                                       .Select(p => p.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow)))
                                       .OrderBy(p => p.PriceValue)
                                       .FirstOrDefault()!
                                                        .PriceValue
                                     : 0;

                model.MaxPrice = (await productRepository.GetAllAsync())
                                                  .AsQueryable()
                                                  .Where(p => p.IsActive && !p.IsDeleted && p.AssociatedDomainId == domainId)
                                                  .Select(p => p.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow)))
                                                  .Any()
                                     ? (await productRepository.GetAllAsync())
                                       .AsQueryable()
                                       .Where(p => p.IsActive && !p.IsDeleted && p.AssociatedDomainId == domainId)
                                       .Select(p => p.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow)))
                                       .OrderByDescending(p => p.PriceValue)
                                       .FirstOrDefault()!
                                                        .PriceValue
                                     : 0;
            }

            //for testing 
            //model.MinPrice = 1000;
            //model.MaxPrice = 10000;
            decimal gap = model.MaxPrice - model.MinPrice;
            model.Step = Convert.ToInt32(gap / 10);
        }
        catch (Exception)
        {
            // ignored
        }

        return model;
    }

    public Task<PagedItems<ProductOutputDto>> GetFilteredProduct(int count,
                                                                 int skip,
                                                                 string currencyId,
                                                                 string languageId,
                                                                 string domainId)
    {
        PagedItems<ProductOutputDto> result = new();

        //var currencySymbol = _currencyRepository.FirstOrDefault(_ => _.Id == currencyId).Symbol;

        //try
        //{

        //    var totalCount = await _productRepository.GetList().CountDocumentsAsync();
        //    List<ProductOutputDTO> lst = new List<ProductOutputDTO>();
        //    switch (filter.ProductSortingType)
        //    {
        //        case Enums.ProductSortingType.Newest:
        //            lst = _productRepository
        //            .GetList(c => c.AssociatedDomainId == domainId && c.IsPublishedOnMainDomain == true && c.GroupIds == selectedFilter.GroupIds && c.Inventory.Count > 0)
        //            .Select(_ =>
        //                new ProductOutputDTO()
        //                {
        //                    GroupIds = _.GroupIds,
        //                    CurrencySymbol = currencySymbol,
        //                    Inventory = _.Inventory,
        //                    Images = _.Images,
        //                    MultiLingualProperties = _.MultiLingualProperties,
        //                    Prices = _.Prices,
        //                    ProductCode = _.ProductCode,
        //                    Id = _.Id,
        //                    Promotion = _.Promotion,
        //                    SaleCount = _.SaleCount,
        //                    UniqueCode = _.UniqueCode,
        //                    TotalScore = _.TotalScore,
        //                    ScoredCount = _.ScoredCount,
        //                    Unit = _.Unit,
        //                    VisitCount = _.VisitCount
        //                }).Sort(Builders<DataLayer.Entities.Shop.Product.Product>.Sort.Descending(_ => _.CreationDate)).Skip(skip).Limit(count).ToList();
        //            break;
        //        case Enums.ProductSortingType.MostVisited:
        //            lst = _productRepository
        //            .GetList(filterDef)
        //            .Select(_ =>
        //                new ProductOutputDTO()
        //                {
        //                    GroupIds = _.GroupIds,
        //                    CurrencySymbol = currencySymbol,
        //                    Inventory = _.Inventory,
        //                    Images = _.Images,
        //                    MultiLingualProperties = _.MultiLingualProperties,
        //                    Prices = _.Prices,
        //                    ProductCode = _.ProductCode,
        //                    Id = _.Id,
        //                    Promotion = _.Promotion,
        //                    SaleCount = _.SaleCount,
        //                    UniqueCode = _.UniqueCode,
        //                    TotalScore = _.TotalScore,
        //                    ScoredCount = _.ScoredCount,
        //                    Unit = _.Unit,
        //                    VisitCount = _.VisitCount
        //                }).Sort(Builders<DataLayer.Entities.Shop.Product.Product>.Sort.Descending(_ => _.VisitCount)).Skip(skip).Limit(count).ToList();
        //            break;
        //        case Enums.ProductSortingType.MostPopular:
        //            lst = _productRepository
        //            .GetList(filterDef)
        //            .Select(_ =>
        //                new ProductOutputDTO()
        //                {
        //                    GroupIds = _.GroupIds,
        //                    CurrencySymbol = currencySymbol,
        //                    Inventory = _.Inventory,
        //                    Images = _.Images,
        //                    MultiLingualProperties = _.MultiLingualProperties,
        //                    Prices = _.Prices,
        //                    ProductCode = _.ProductCode,
        //                    Id = _.Id,
        //                    Promotion = _.Promotion,
        //                    SaleCount = _.SaleCount,
        //                    UniqueCode = _.UniqueCode,
        //                    TotalScore = _.TotalScore,
        //                    ScoredCount = _.ScoredCount,
        //                    Unit = _.Unit,
        //                    VisitCount = _.VisitCount
        //                }).Sort(Builders<DataLayer.Entities.Shop.Product.Product>.Sort.Descending(_ => _.TotalScore)).Skip(skip).Limit(count).ToList();
        //            break;
        //        case Enums.ProductSortingType.BestSelling:
        //            lst = _productRepository
        //            .GetList(filterDef)
        //            .Select(_ =>
        //                new ProductOutputDTO()
        //                {
        //                    GroupIds = _.GroupIds,
        //                    CurrencySymbol = currencySymbol,
        //                    Inventory = _.Inventory,
        //                    Images = _.Images,
        //                    MultiLingualProperties = _.MultiLingualProperties,
        //                    Prices = _.Prices,
        //                    ProductCode = _.ProductCode,
        //                    Id = _.Id,
        //                    Promotion = _.Promotion,
        //                    SaleCount = _.SaleCount,
        //                    UniqueCode = _.UniqueCode,
        //                    TotalScore = _.TotalScore,
        //                    ScoredCount = _.ScoredCount,
        //                    Unit = _.Unit,
        //                    VisitCount = _.VisitCount
        //                }).Sort(Builders<DataLayer.Entities.Shop.Product.Product>.Sort.Descending(_ => _.SaleCount)).Skip(skip).Limit(count).ToList();
        //            break;
        //        default:
        //            break;
        //    }

        //    foreach (var pro in lst)
        //    {
        //        var res = EvaluateFinalPrice(pro.Id, pro.Prices, pro.GroupIds, currencyId);
        //        pro.GiftProduct = res.GiftProduct;
        //        pro.Promotion = res.Promotion;
        //        pro.PriceValWithPromotion = res.PriceValWithPromotion;
        //        pro.OldPrice = res.OldPrice;
        //        //testing
        //        // pro.OldPrice = ran.Next(0, 56000);

        //        pro.DiscountType = res.DiscountType;
        //        pro.DiscountValue = res.DiscountValue;
        //        pro.MainImageUrl = pro.Images.Any(_ => _.IsMain) ? pro.Images.FirstOrDefault(_ => _.IsMain).Url : "";
        //        pro.MainAlt = pro.Images.Any(_ => _.IsMain) ? (!string.IsNullOrWhiteSpace(pro.Images.FirstOrDefault(_ => _.IsMain).Title) ? pro.Images.FirstOrDefault(_ => _.IsMain).Title : "") : "";
        //        var r = DataLayer.Helpers.Utilities.ConvertPopularityRate(pro.TotalScore ?? 0, pro.ScoredCount ?? 0);
        //        pro.LikeRate = r.LikeRate;
        //        pro.DisikeRate = r.DisikeRate;
        //        pro.HalfLikeRate = r.HalfLikeRate;
        //    }
        //    result.Items = lst;
        //    result.CurrentPage = (skip / count) + 1;
        //    result.ItemsCount = totalCount;
        //    result.PageSize = count;
        //    result.QueryString = $"?page={result.CurrentPage}&pagesize={count}";

        //}
        //catch (Exception ex)
        //{
        //    result.Items = new List<ProductOutputDTO>();
        //    result.CurrentPage = 0;
        //    result.ItemsCount = 0;
        //    result.PageSize = count;
        //    result.QueryString = $"?page={result.CurrentPage}&pagesize={count}";
        //}
        return Task.FromResult(result);
    }
    #endregion

    #region IProductGroupRepository
    public ProductGroup ProductGroupFetch(string productGroupId)
    {
        ProductGroup entity = productGroupRepository.FirstOrDefault(g => g.Id == productGroupId);

        return entity;
    }

    public List<SelectListModel> GetAllActiveProductGroup(string langId)
    {
        List<SelectListModel> result;
        Task<ApplicationUser> userDb = GetCurrentUser();
        Domain domain = GetCurrentUserDomain();
        if (userDb.Result.IsSystemAccount)
        {
            result = productGroupRepository.GetList(c => c.IsActive && !c.IsDeleted)
                                           .Select(c => new SelectListModel
                                           {
                                               Text = c.MultiLingualProperties.Count(a => a.LanguageId == langId) != 0 ? c.MultiLingualProperties.FirstOrDefault(a => a.LanguageId == langId)?.Name : "",
                                               Value = c.Id.ToString()
                                           })
                                           .ToList();
        }
        else
        {
            //result = productGroupRepository.GetList(c => userDb.Result.Profile.Access.AccessibleProductGroupIds.Contains(c.Id))
            //                               .Select(c => new SelectListModel
            //                                            {
            //                                                Text = c.MultiLingualProperties.Count(a => a.LanguageId == langId) != 0 ? c.MultiLingualProperties.FirstOrDefault(a => a.LanguageId == langId)?.Name : "",
            //                                                Value = c.Id.ToString()
            //                                            })
            //                               .ToList();

            result = productGroupRepository.GetList(c => c.IsActive && !c.IsDeleted && c.AssociatedDomainId == domain.Id).Select(c => new SelectListModel
            {
                Text = c.MultiLingualProperties.Count(a => a.LanguageId == langId) != 0 ? c.MultiLingualProperties.FirstOrDefault(a => a.LanguageId == langId)?.Name : "",
                Value = c.Id.ToString()
            })
                                           .ToList();
        }

        return result;
    }
    #endregion

    #region IProductSpecGroupRepository
    public async Task<List<SelectListModel>> AllActiveSpecificationGroup(string langId, string domainId, CancellationToken cancellationToken = default)
    {
        List<SelectListModel> result = [];

        try
        {
            Task<ApplicationUser> userDb = GetCurrentUser(cancellationToken);

            if (userDb.Result.IsSystemAccount && !string.IsNullOrEmpty(domainId))
            {
                result = (await productSpecGroupRepository.GetListAsync(c => c.IsActive && !c.IsDeleted, cancellationToken))
                                                   .Select(c => new SelectListModel
                                                   {
                                                       Text = c.GroupNames.Count(a => a.LanguageId == langId) != 0 ? c.GroupNames.FirstOrDefault(a => a.LanguageId == langId)?.Name : "",
                                                       Value = c.Id.ToString()
                                                   })
                                                   .Where(item => !string.IsNullOrEmpty(item.Text))
                                                   .ToList();
            }
            else
            {
                string finalDomainId = !string.IsNullOrEmpty(domainId) ? domainId : userDb.Result.Domains.FirstOrDefault(a => a.IsOwner)?.DomainId;
                List<ProductSpecGroup> lst = (await productSpecGroupRepository.GetAllAsync(cancellationToken))
                                                                       .AsQueryable()
                                                                       .Where(c => c.IsActive && !c.IsDeleted && c.AssociatedDomainId == finalDomainId)
                                                                       .ToList();

                result = lst.Select(b => new SelectListModel { Text = b.GroupNames.Any(a => a.LanguageId == langId) ? b.GroupNames.FirstOrDefault(a => a.LanguageId == langId)?.Name : "", Value = b.Id.ToString() })
                            .Where(item => !string.IsNullOrEmpty(item.Text))
                            .ToList();
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return result;
    }

    public async Task<ProductSpecGroup> GroupSpecificationFetch(string productSpecificationGroupId)
    {
        ProductSpecGroup result;

        try
        {
            ProductSpecGroup group = await productSpecGroupRepository.FirstOrDefaultAsync(s => s.Id == productSpecificationGroupId);

            if (group != null)
            {
            }

            result = group;
        }
        catch (Exception)
        {
            result = null;
        }

        return result;
    }
    #endregion

    #region IShippingCartRepository
    private async Task<Result> InsertUserShoppingCart(string userId, CancellationToken cancellationToken = default)
    {
        Result result = new();
        ApplicationUser userEntity = await userRepository.FirstOrDefaultAsync(c => c.Id == userId, cancellationToken);
        Domain domainEntity = GetCurrentUserDomain();
        Language defLanguageEntity = await languageRepository.FirstOrDefaultAsync(c => c.Id == (!string.IsNullOrEmpty(userEntity.Profile.DefaultLanguageId) ? userEntity.Profile.DefaultLanguageId : domainEntity.DefaultLanguageId), cancellationToken);
        Currency defCurrencyEntity = await currencyRepository.FirstOrDefaultAsync(c => c.Id == (!string.IsNullOrEmpty(userEntity.Profile.DefaultCurrencyId) ? userEntity.Profile.DefaultCurrencyId : domainEntity.DefaultCurrencyId), cancellationToken);

        if (defCurrencyEntity == null)
        {
            return result;
        }

        ShoppingCart userCartModel = new()
        {
            CreationDate = DateTime.UtcNow,
            CreatorUserId = userId,
            CreatorUserName = GetCurrentUser(cancellationToken).Result.UserName,
            Id = Guid.NewGuid().ToString(),
            IsActive = true,
            IsDeleted = false,
            AssociatedDomainId = domainEntity.Id,
            ShoppingCartCulture = new()
            {
                CurrencyId = defCurrencyEntity.Id,
                CurrencyName = defCurrencyEntity.CurrencyName,
                CurrencyPrefix = defCurrencyEntity.Prefix,
                CurrencySymbol = defCurrencyEntity.Symbol,
                LanguageId = defLanguageEntity.Id,
                LanguageName = defLanguageEntity.LanguageName,
                LanguageSymbol = defLanguageEntity.Symbol
            }
        };

        try
        {
            await shoppingCartRepository.InsertAsync(userCartModel, cancellationToken);
            result.Message = ConstMessages.SuccessfullyDone;
            result.Succeeded = true;
        }
        catch (Exception)
        {
            result.Message = ConstMessages.GeneralError;
        }

        return result;
    }

    private async Task<Result<ShoppingCartDto>> FetchActiveUserShoppingCart(string userId, string domainId, CancellationToken cancellationToken = default)
    {
        Result<ShoppingCartDto> result = new() { ReturnValue = new() };
        ShoppingCartDto dto = new();

        if (!await shoppingCartRepository.AnyAsync(c => c.CreatorUserId == userId &&
                                                        !c.IsDeleted &&
                                                        c.IsActive &&
                                                        c.AssociatedDomainId == domainId,
                                                   cancellationToken))
        {
            await InsertUserShoppingCart(userId, cancellationToken);
        }

        ShoppingCart userCartEntity = await shoppingCartRepository.FirstOrDefaultAsync(c => c.CreatorUserId == userId &&
                                                                                            !c.IsDeleted &&
                                                                                            c.IsActive &&
                                                                                            c.AssociatedDomainId == domainId,
                                                                                       cancellationToken);

        //surely we have an active instance of ShoppingCart
        dto.ShoppingCartCulture = userCartEntity.ShoppingCartCulture;
        dto.Id = userCartEntity.Id;
        dto.DomainId = domainId;
        dto.OwnerId = userCartEntity.CreatorUserId;
        result.ReturnValue.Details = [];
        int rowNumber = 1;
        decimal finalPaymentPrice = 0;

        //each time we fetch shoppingCart data should be updated in it
        foreach (PurchasePerSeller sellerPurchase in userCartEntity.Details)
        {
            PurchasePerSellerDto obj = new() { SellerId = sellerPurchase.SellerId };
            decimal sellerFactor = 0;
            obj.SellerUserName = sellerPurchase.SellerUserName;
            obj.ShippingTypeId = sellerPurchase.ShippingTypeId;
            obj.ShippingExpense = sellerPurchase.ShippingExpense;

            //??? update shippingExpense if seller change it
            ApplicationUser sellerEntity = await userRepository.FirstOrDefaultAsync(c => c.Id == sellerPurchase.SellerId, cancellationToken);
            string domainOwner = sellerEntity.Domains.FirstOrDefault(c => c.IsOwner)?.DomainId;
            ShippingSetting shippingEntity = await shippingSettingRepository.FirstOrDefaultAsync(c => c.AssociatedDomainId == domainOwner, cancellationToken);

            if (shippingEntity == null)
            {
                continue;
            }

            {
                long fixedExpense = shippingEntity.AllowedShippingTypes.FirstOrDefault(c => c.HasFixedExpense)!.FixedExpenseValue;
                finalPaymentPrice += fixedExpense;

                sellerFactor += sellerPurchase.ShippingExpense;

                foreach (ShoppingCartDetail pro in sellerPurchase.Products.Where(d => !d.IsDeleted))
                {
                    string productId = pro.ProductId;
                    Product productEntity = await productRepository.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
                    InventoryDetail inventoryDetail = FindProductSpecValuesRecord(productId, pro.ProductSpecValues);
                    ShoppingCartDetailDto det = new();

                    if (productEntity is { IsActive: true, IsDeleted: false } && inventoryDetail is { Count: > 0 })
                    {
                        long activePriceValue = GetCurrentPrice(productEntity);
                        DiscountModel discountPerUnit = await GetCurrentDiscountPerUnit(productEntity, activePriceValue);
                        det = new()
                        {
                            RowNumber = rowNumber,
                            ShoppingCartDetailId = pro.Id,
                            ProductCode = productEntity.ProductCode,
                            CreationDate = DateTime.Now,
                            CreatorUserId = userCartEntity.CreatorUserId,
                            CreatorUserName = userCartEntity.CreatorUserName,
                            ProductId = productId,
                            ProductName = pro.ProductName,
                            OrderCount = pro.OrderCount,
                            PricePerUnit = activePriceValue,
                            ProductSpecValues = pro.ProductSpecValues,
                            DiscountPricePerUnit = discountPerUnit.DiscountPerUnit,
                            TotalAmountToPay = (activePriceValue - discountPerUnit.DiscountPerUnit) * pro.OrderCount,
                            ProductImage = productEntity.Images.Any(i => i.IsMain) ? productEntity.Images.FirstOrDefault(i => i.IsMain) : productEntity.Images.FirstOrDefault()
                        };
                        finalPaymentPrice += det.TotalAmountToPay;
                        sellerFactor += det.TotalAmountToPay;

                        #region changing in final amount per unit
                        decimal previousAmountPerUnit = pro.PricePerUnit - pro.DiscountPricePerUnit;
                        decimal currentAmountPerUnit = det.PricePerUnit - det.DiscountPricePerUnit;

                        if (previousAmountPerUnit != currentAmountPerUnit)
                        {
                            det.PreviousFinalPricePerUnit = previousAmountPerUnit;
                            decimal difference = currentAmountPerUnit >= previousAmountPerUnit ? currentAmountPerUnit - previousAmountPerUnit : previousAmountPerUnit - currentAmountPerUnit;

                            if (previousAmountPerUnit < currentAmountPerUnit)
                            {
                                det.Notifications.Add(UtilityLanguage.GetString("AlertAndMessage_ProductPriceIncrease")
                                                                    .Replace("[0]", $"{difference} {userCartEntity.ShoppingCartCulture.CurrencySymbol}"));
                            }
                            else
                            {
                                difference = previousAmountPerUnit - currentAmountPerUnit;
                                det.Notifications.Add(UtilityLanguage.GetString("AlertAndMessage_ProductPriceDecrease")
                                                                    .Replace("[0]", $"{difference} {userCartEntity.ShoppingCartCulture.CurrencySymbol}"));
                            }
                        }
                        #endregion changing in final amount pre unit

                        //check inventory if it is less than orderCount then change our order Count to our inventory
                        if (pro.OrderCount > inventoryDetail.Count)
                        {
                            det.OrderCount = inventoryDetail.Count;

                            det.Notifications.Add(UtilityLanguage.GetString("AlertAndMessage_ProductDecreaseInventory")
                                                                .Replace("[0]", productEntity.Inventory.ToString()));
                        }
                    }
                    else if (productEntity.IsDeleted || inventoryDetail.Count == 0)
                    {
                        det = new()
                        {
                            RowNumber = rowNumber,
                            ShoppingCartDetailId = pro.ShoppingCartDetailId,
                            CreationDate = DateTime.Now,
                            CreatorUserId = userCartEntity.CreatorUserId,
                            CreatorUserName = userCartEntity.CreatorUserName,
                            ProductId = productId,
                            ProductName = pro.ProductName,
                            OrderCount = 0,
                            PricePerUnit = 0,
                            DiscountPricePerUnit = 0,
                            TotalAmountToPay = 0,
                            ProductSpecValues = pro.ProductSpecValues,
                            PreviousOrderCount = pro.OrderCount,
                            PreviousFinalPricePerUnit = pro.PricePerUnit - pro.DiscountPricePerUnit
                        };
                        det.Notifications.Add(UtilityLanguage.GetString("AlertAndMessage_ProductNullCount"));
                    }

                    rowNumber += 1;
                    obj.Products.Add(det);
                }

                obj.TotalDetailsAmountWithShipping = sellerFactor;
                dto.Details.Add(obj);
            }
        }

        //then update current shopping cart in database
        userCartEntity.Details = [];

        foreach (PurchasePerSellerDto obj in dto.Details)
        {
            userCartEntity.Details.Add(new()
            {
                Products = mapper.Map<List<ShoppingCartDetail>>(obj.Products),
                SellerId = obj.SellerId,
                SellerUserName = obj.SellerUserName,
                ShippingExpense = Convert.ToInt64(obj.ShippingExpense),
                ShippingTypeId = obj.ShippingTypeId,
                TotalDetailsAmountWithShipping = Convert.ToInt64(obj.TotalDetailsAmountWithShipping)
            });
        }

        //userCartEntity.Details = _mapper.Map<List<PurchasePerSeller>>(dto.Details);
        dto.FinalPriceForPay = finalPaymentPrice;
        Result<ShoppingCart> updateResult = await shoppingCartRepository.UpdateAsync(userCartEntity, cancellationToken);

        if (updateResult.Succeeded)
        {
            result.Succeeded = true;
            result.Message = ConstMessages.SuccessfullyDone;
            result.ReturnValue = dto;
        }
        else
        {
            result.Message = ConstMessages.GeneralError;
        }

        return result;
    }

    public InventoryDetail FindProductSpecValuesRecord(string productId, List<SpecValue> specValues)
    {
        Product entity = productRepository.FirstOrDefault(p => p.Id == productId);
        InventoryDetail inventoryDetail = new();
        List<InventoryDetail> lst = entity.Inventory;

        foreach (SpecValue item in specValues)
        {
            lst = lst.Where(d => d.SpecValues.Any(a => a.SpecificationId == item.SpecificationId &&
                                                       a.SpecificationValue == item.SpecificationValue))
                     .ToList();
        }

        if (lst.Count > 0) //surely lst have just one
        {
            inventoryDetail = lst[0];
        }

        return inventoryDetail;
    }

    public long GetCurrentPrice(Product product)
    {
        Price availablePrice = product.Prices.Any(p => p.IsActive &&
                                                       p.StartDate <= DateTime.Now &&
                                                       (p.EndDate == null || p.EndDate >= DateTime.Now))
                                   ? product.Prices.FirstOrDefault(p => p.IsActive &&
                                                                        p.StartDate <= DateTime.Now &&
                                                                        (p.EndDate == null || p.EndDate >= DateTime.Now))
                                   : null;

        if (availablePrice != null)
        {
            return Convert.ToInt64(availablePrice.PriceValue);
        }

        return 0;
    }

    public async Task<DiscountModel> GetCurrentDiscountPerUnit(Product product,
                                                               long priceValue)
    {
        DiscountModel res = new();
        Promotion activePromotion = await GetActivePromotionOfThisProduct(product);
        res.DiscountPerUnit = 0;

        if (activePromotion != null)
        {
            switch (activePromotion.DiscountType)
            {
                case DiscountType.Fixed:
                    if (activePromotion.Value != null)
                    {
                        res.DiscountPerUnit = activePromotion.Value.Value;
                    }

                    break;

                case DiscountType.Percentage:
                    if (activePromotion.Value != null)
                    {
                        res.DiscountPerUnit = priceValue * activePromotion.Value.Value / 100;
                    }

                    break;

                case DiscountType.Product:
                    res.DiscountPerUnit = 0;
                    res.PraisedProductId = activePromotion.PromotedProductId;
                    res.PraisedProductCount = activePromotion.PromotedCountofUnit ?? 1;

                    break;
            }
        }

        return res;
    }

    private async Task<Promotion> GetActivePromotionOfThisProduct(Product product)
    {
        //maybe we have several promotions which is assign to product
        //promotion on all product promotion of productGroup
        //promotion of this product if it is so we should select latest promotion
        Promotion promotionOnAll = null;
        Promotion promotionOnThisProductGroup = null;
        Promotion promotionOnThisProduct = null;
        List<Promotion> promotionList = [];
        Promotion finalPromotion = null;

        Domain currentDomainEntity = GetCurrentUserDomain();
        ApplicationUser sellerUserEntity = await userRepository.FirstOrDefaultAsync(c => c.Id == product.SellerUserId);
        string searchDomainId;

        if (currentDomainEntity.IsDefault) //we should calculate the promotion of sellerDomain
        {
            if (sellerUserEntity != null)
            {
                searchDomainId = sellerUserEntity.IsSystemAccount ? currentDomainEntity.Id : product.AssociatedDomainId;
            }
            else
            {
                return null;
            }
        }
        else
        {
            //it isn't main domain then surely the seller is owner of domain, and it isn't system account
            searchDomainId = currentDomainEntity.Id;
        }

        try
        {
            promotionOnAll = await promotionRepository.AnyAsync(p => p.PromotionType == PromotionType.All &&
                                                                     p.AssociatedDomainId == searchDomainId &&
                                                                     p.IsActive &&
                                                                     !p.IsDeleted &&
                                                                     p.SDate <= DateTime.Now &&
                                                                     (p.EDate == null || p.EDate >= DateTime.Now))
                                 ? await promotionRepository.FirstOrDefaultAsync(p => p.PromotionType == PromotionType.All &&
                                                                                      p.AssociatedDomainId == searchDomainId &&
                                                                                      p.IsActive &&
                                                                                      !p.IsDeleted &&
                                                                                      p.SDate <= DateTime.Now &&
                                                                                      (p.EDate == null || p.EDate >= DateTime.Now))
                                 : null;

            List<string> groupIds = product.GroupIds;

            promotionOnThisProductGroup = await promotionRepository
                                              .FirstOrDefaultAsync(p => p.PromotionType == PromotionType.Group &&
                                                                        p.Infoes.Any(a => groupIds.Contains(a.AffectedProductGroupId)) &&
                                                                        p.AssociatedDomainId == searchDomainId &&
                                                                        p.IsActive &&
                                                                        !p.IsDeleted &&
                                                                        p.SDate <= DateTime.Now &&
                                                                        (p.EDate == null || p.EDate >= DateTime.Now));

            promotionOnThisProduct = product.Promotion is { IsActive: true, IsDeleted: false } &&
                                     product.Promotion.SDate <= DateTime.Now &&
                                     (product.Promotion.EDate == null || product.Promotion.EDate >= DateTime.Now)
                                         ? product.Promotion
                                         : null;
        }
        catch (Exception)
        {
            // ignored
        }

        if (promotionOnAll != null)
        {
            promotionList.Add(promotionOnAll);
        }

        if (promotionOnThisProductGroup != null)
        {
            promotionList.Add(promotionOnThisProductGroup);
        }

        if (promotionOnThisProduct != null)
        {
            promotionList.Add(promotionOnThisProduct);
        }

        if (promotionList.Any())
        {
            finalPromotion = promotionList.MaxBy(p => p.SDate);
        }

        return finalPromotion;
    }
    #endregion

    #region IPermissionRepository
    public async Task<List<PermissionTreeViewDto>> GetMenus(string currentUserId, string address)
    {
        List<PermissionTreeViewDto> result = [];

        try
        {
            ApplicationUser user = await userRepository.FirstOrDefaultAsync(c => c.Id == currentUserId);
            List<Permission> permissions = GetPermissionsOfUser(user);

            result = mapper.Map<List<PermissionTreeViewDto>>(permissions);

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
                List<string> accessUrl = [dto.ClientAddress];

                accessUrl.AddRange(dto.Urls.Select(u => u.ToLower()));

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

    private List<Permission> GetPermissionsOfUser(ApplicationUser user)
    {
        List<Permission> permissionList;

        try
        {
            // Fetch active permissions
            permissionList = permissionRepository.GetList(c => c.IsActive).ToList();

            if (!user.IsSystemAccount)
            {
                // Retrieve the user's role
                ApplicationRole role = roleRepository.FirstOrDefault(c => c.Id == user.UserRoleId);

                if (role == null)
                {
                    return [];
                }

                // Initialize an empty list for access permissions
                List<Permission> accessPermissions = [];

                // Filter permissions based on the role's PermissionIds
                foreach (Permission permission in permissionList.Where(p =>
                                                                           role.PermissionIds.Any(pId => pId.ToString().Trim().Equals(p.Id.ToString().Trim(), StringComparison.OrdinalIgnoreCase))))
                {
                    accessPermissions.Add(permission);
                    Remove(permission.Children);
                }

                return accessPermissions;

                // Local function to remove unauthorized child permissions
                void Remove(IList<Permission> children)
                {
                    for (int index = children.Count - 1; index >= 0; index--)
                    {
                        Permission child = children[index];

                        if (role.PermissionIds.Any(p => p.Equals(child.Id)))
                        {
                            Remove(child.Children); // Recursively process child permissions
                        }
                        else
                        {
                            children.RemoveAt(index);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception as needed
            Console.WriteLine($"An error occurred: {ex.Message}");
            return [];
        }

        return permissionList;
    }
    #endregion

    #region IMenuRepository
    public List<StoreMenuVm> StoreList(string domainId, string langId, bool isDesign)
    {
        List<StoreMenuVm> result = [];

        Result<Domain> domainEntity = FetchDomain(domainId);

        if (domainEntity == null)
        {
            return result;
        }

        string finalLangId = !string.IsNullOrWhiteSpace(langId) ? langId : domainEntity.ReturnValue.DefaultLanguageId;

        result = menuRepository.GetList(m => m.AssociatedDomainId == domainId && m.ParentId == null && !m.IsDeleted)
                               .Select(m => new StoreMenuVm
                               {
                                   MenuId = m.Id,
                                   MenuTitle = m.MenuTitles.Any(p => p.LanguageId == finalLangId)
                                                                ? m.MenuTitles.FirstOrDefault(p => p.LanguageId == finalLangId)
                                                                : m.MenuTitles.Any(p => p.LanguageId == domainEntity.ReturnValue.DefaultLanguageId)
                                                                    ? m.MenuTitles.FirstOrDefault(p => p.LanguageId == domainEntity.ReturnValue.DefaultLanguageId)
                                                                    : m.MenuTitles.FirstOrDefault(),
                                   Icon = m.Icon,
                                   MenuType = m.MenuType,
                                   Order = m.Order,
                                   Url = m.Url,
                                   SubId = m.SubId,
                                   SubName = m.SubName,
                                   SubGroupId = m.SubGroupId,
                                   SubGroupName = m.SubGroupName,
                                   MenuCode = m.MenuCode,
                                   Childrens = GetMenuChildren(m.Id, finalLangId, domainEntity.ReturnValue),
                                   IsFull = true
                               })
                               .ToList();

        //those menus which have isFull = true will be shown in UI
        if (!isDesign)
        {
            #region check isFull
            IEnumerable<StoreMenuVm> productGroupsMenu = result.Where(v => v.MenuType == MenuType.ProductGroup);
            IEnumerable<StoreMenuVm> productGroupMenuLeaves = productGroupsMenu.Where(v => !v.Childrens.Any());

            foreach (StoreMenuVm leaf in productGroupMenuLeaves)
            {
                long productsInGroupCounts = productRepository.GetCount(p => p.GroupIds.Contains(leaf.SubGroupId));

                if (productsInGroupCounts != 0)
                {
                    continue;
                }

                leaf.IsFull = false;
                StoreMenuVm tmp = leaf;

                while (tmp is { ParentId: not null })
                {
                    tmp = result.FirstOrDefault(v => v.MenuId == tmp.ParentId);

                    if (tmp != null)
                    {
                        tmp.IsFull = false;
                    }
                }

                if (tmp != null)
                {
                    tmp.IsFull = false;
                }
            }

            IEnumerable<StoreMenuVm> contentCategoryMenu = result.Where(v => v.MenuType == MenuType.CategoryContent);
            IEnumerable<StoreMenuVm> contentCategoryLeaves = contentCategoryMenu.Where(v => !v.Childrens.Any());

            foreach (StoreMenuVm leaf in contentCategoryLeaves)
            {
                long contentsInCategoryCounts = contentRepository.GetCount(c => c.ContentCategoryId == leaf.SubGroupId);

                if (contentsInCategoryCounts != 0)
                {
                    continue;
                }

                leaf.IsFull = false;
                StoreMenuVm tmp = leaf;

                while (tmp is { ParentId: not null })
                {
                    tmp = result.FirstOrDefault(v => v.MenuId == tmp.ParentId);

                    if (tmp != null)
                    {
                        tmp.IsFull = false;
                    }
                }

                if (tmp != null)
                {
                    tmp.IsFull = false;
                }
            }
            #endregion
        }

        return result;
    }

    private List<StoreMenuVm> GetMenuChildren(string menuId, string finalLangId, Domain domainEntity)
    {
        List<StoreMenuVm> result = [];
        Menu menuEntity = menuRepository.FirstOrDefault(m => m.Id == menuId);

        if (menuEntity == null)
        {
            return result;
        }

        {
            if (menuRepository.GetCount(m => m.ParentId == menuId) > 0)
            {
                result = menuRepository.GetList(m => m.AssociatedDomainId == domainEntity.Id && m.ParentId == menuId && !m.IsDeleted)
                                       .Select(m => new StoreMenuVm
                                       {
                                           MenuId = m.Id,
                                           MenuTitle = m.MenuTitles.Count(p => p.LanguageId == finalLangId) > 0 ? m.MenuTitles.FirstOrDefault(p => p.LanguageId == finalLangId) :
                                                                    m.MenuTitles.Count(p => p.LanguageId == domainEntity.DefaultLanguageId) > 0 ? m.MenuTitles.FirstOrDefault(p => p.LanguageId == domainEntity.DefaultLanguageId) :
                                                                    m.MenuTitles.FirstOrDefault(),
                                           Icon = m.Icon,
                                           MenuType = m.MenuType,
                                           Order = m.Order,
                                           Url = m.Url,
                                           SubId = m.SubId,
                                           SubName = m.SubName,
                                           SubGroupId = m.SubGroupId,
                                           SubGroupName = m.SubGroupName,
                                           MenuCode = m.MenuCode,
                                           Childrens = GetMenuChildren(m.Id, finalLangId, domainEntity)
                                       })
                                       .ToList();
            }
        }

        return result;
    }
    #endregion

    #region IShoppingCartRepository
    public async Task<int> LoadUserCartShopping(string userId)
    {
        Domain domainEntity = GetCurrentUserDomain();

        if (domainEntity == null)
        {
            return 0;
        }

        Result<ShoppingCartDto> res = await FetchActiveUserShoppingCart(userId, domainEntity.Id);
        ShoppingCart cartEntity = await shoppingCartRepository.FirstOrDefaultAsync(c => c.Id == res.ReturnValue.Id);

        return GetItemCountsInCart(cartEntity);

    }

    private int GetItemCountsInCart(ShoppingCart entity)
    {
        return entity.Details.Sum(item => item.Products.Count(a => !a.IsDeleted));
    }

    public async Task<Result<CartItemsCount>> AddOrChangeProductToUserCart(string productId, int orderCount, List<SpecValue> specValues, string shoppingCartDetailId = "", CancellationToken cancellationToken = default)
    {
        Result<CartItemsCount> result = new() { ReturnValue = new() };
        string langSymbol = CultureInfo.CurrentCulture.Name.ToLower();
        string languageId = (await languageRepository.FirstOrDefaultAsync(l => l.Symbol.ToLower() == langSymbol, cancellationToken)).Id;

        foreach (SpecValue spec in specValues)
        {
            ProductSpecification specEntity = await productSpecificationRepository.FirstOrDefaultAsync(s => s.Id == spec.SpecificationId, cancellationToken);
            spec.SpecificationName = specEntity.SpecificationNameValues.FirstOrDefault(p => p.LanguageId == languageId)?.Name;
        }

        Product productEntity = await productRepository.FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted, cancellationToken);
        string userId = GetCurrentUserId();
        ShoppingCart userCartEntity = await shoppingCartRepository.FirstOrDefaultAsync(c => c.CreatorUserId == userId && !c.IsDeleted && c.IsActive, cancellationToken);
        Domain domainEntity = GetCurrentUserDomain();

        if (userCartEntity == null)
        {
            Result res = await InsertUserShoppingCart(userId, cancellationToken);

            if (res.Succeeded)
            {
                userCartEntity = await shoppingCartRepository.FirstOrDefaultAsync(c => c.CreatorUserId == userId && !c.IsDeleted && c.AssociatedDomainId == domainEntity.Id && c.IsActive, cancellationToken);
            }
        }

        if (productEntity != null)
        {
            bool finalFlag = true;

            if (userCartEntity != null && (!string.IsNullOrWhiteSpace(shoppingCartDetailId) ||
                                           userCartEntity.Details.Any(s => s.Products.Any(a => a.ProductId == productId))))
            {
                PurchasePerSeller purchasePerSeller = userCartEntity.Details.FirstOrDefault(s => s.Products.Any(a => a.ProductId == productId));

                if (purchasePerSeller != null)
                {
                    foreach (ShoppingCartDetail pro in purchasePerSeller.Products.Where(a => a.ProductId == productId))
                    {
                        bool flag = !(from spec in pro.ProductSpecValues let modelSpec = specValues.FirstOrDefault(a => a.SpecificationId == spec.SpecificationId) where modelSpec == null || modelSpec.SpecificationValue != spec.SpecificationValue select spec).Any();

                        if (flag)
                        {
                            Result res = await ChangeProductCountInUserCart(userId, productId, orderCount, specValues, pro.ShoppingCartDetailId, cancellationToken);
                            result.Message = res.Message;
                            result.Succeeded = res.Succeeded;
                            finalFlag = false;
                        }
                    }
                }
            }

            if (finalFlag)
            {
                long currentPriceValue = GetCurrentPrice(productEntity);
                DiscountModel res = await GetCurrentDiscountPerUnit(productEntity, currentPriceValue);

                if (productEntity.Inventory != null && productEntity.Inventory.Sum(d => d.Count) > 0)
                {
                    InventoryDetail inventoryDetail = FindProductSpecValuesRecord(productId, specValues);
                    int finalOrderCnt = inventoryDetail.Count > orderCount ? orderCount : inventoryDetail.Count;
                    ShoppingCartDetail shopCartDetail = new()
                    {
                        ShoppingCartDetailId = Guid.NewGuid().ToString(),
                        ProductId = productEntity.Id,
                        ProductName = productEntity
                                                                          .MultiLingualProperties.FirstOrDefault(a => userCartEntity != null && a.LanguageId == userCartEntity.ShoppingCartCulture.LanguageId)
                                                                          ?.Name,
                        CreationDate = DateTime.Now,
                        CreatorUserId = userId,
                        AssociatedDomainId = domainEntity.Id,
                        CreatorUserName = GetCurrentUser(cancellationToken).Result.UserName,
                        DiscountPricePerUnit = res.DiscountPerUnit,
                        PricePerUnit = currentPriceValue,
                        IsActive = true,
                        ProductSpecValues = specValues,
                        OrderCount = finalOrderCnt,
                        SellerId = productEntity.SellerUserId,
                        TotalAmountToPay = (currentPriceValue - res.DiscountPerUnit) * finalOrderCnt
                    };

                    if (userCartEntity != null && userCartEntity.Details.Any(s => s.SellerId == productEntity.SellerUserId))
                    {
                        PurchasePerSeller row = userCartEntity.Details.FirstOrDefault(s => s.SellerId == productEntity.SellerUserId);

                        row?.Products.Add(shopCartDetail);
                    }
                    else
                    {
                        ApplicationUser sellerEntity = await userRepository.FirstOrDefaultAsync(c => c.Id == productEntity.SellerUserId, cancellationToken);
                        string? ownerDomainId = sellerEntity.Domains.Any(d => d.IsOwner) ? sellerEntity.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId : "";

                        Domain? sellerDomainEntity = null;

                        if (!string.IsNullOrWhiteSpace(ownerDomainId))
                        {
                            sellerDomainEntity = await domainRepository.FirstOrDefaultAsync(d => d.Id == ownerDomainId, cancellationToken);
                        }

                        Domain defaultDomain = await domainRepository.FirstOrDefaultAsync(d => d.IsDefault, cancellationToken);

                        string? relatedDomain = domainEntity.IsDefault ? domainEntity.Id : ownerDomainId;

                        ShippingSetting settingEntity = await shippingSettingRepository.FirstOrDefaultAsync(c => c.AssociatedDomainId == relatedDomain, cancellationToken);

                        int finalShippingTypeId = domainEntity.IsDefault ? domainEntity.DefaultShippingTypeId : sellerDomainEntity?.DefaultShippingTypeId ?? defaultDomain.DefaultShippingTypeId;
                        PurchasePerSeller purchase = new()
                        {
                            SellerId = productEntity.SellerUserId,
                            SellerUserName = sellerEntity.Profile.FullName,
                            Products = [],
                            ShippingTypeId = finalShippingTypeId,
                            ShippingExpense = settingEntity != null
                                                                               ? settingEntity.AllowedShippingTypes.FirstOrDefault(a => a.ShippingTypeId == finalShippingTypeId && a.HasFixedExpense)!.FixedExpenseValue
                                                                               : 0
                        };

                        purchase.Products.Add(shopCartDetail);

                        userCartEntity?.Details.Add(purchase);
                    }

                    Result<ShoppingCart> updateResult = await shoppingCartRepository.UpdateAsync(userCartEntity, cancellationToken);

                    if (updateResult.Succeeded)
                    {
                        result.Succeeded = true;
                        result.Message = ConstMessages.SuccessfullyDone;
                    }
                    else
                    {
                        result.Message = ConstMessages.GeneralError;
                    }
                }
                else
                {
                    result.Message = ConstMessages.LackOfInventoryOfProduct;
                }
            }

            if (userCartEntity != null)
            {
                result.ReturnValue.ItemsCount = GetItemCountsInCart(userCartEntity);
            }
        }
        else
        {
            result.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
        }

        return result;
    }

    private async Task<Result> ChangeProductCountInUserCart(string userId, string productId, int newCount, List<SpecValue> specValues, string shoppingCartDetailId, CancellationToken cancellationToken = default)
    {
        Result result = new();
        string domainName = GetCurrentDomainName();

        if (newCount != 0)
        {
            Domain domainEntity = await domainRepository.FirstOrDefaultAsync(d => d.DomainName == domainName, cancellationToken);
            ShoppingCart userCartEntity = await shoppingCartRepository.FirstOrDefaultAsync(c => c.CreatorUserId == userId && !c.IsDeleted && c.AssociatedDomainId == domainEntity.Id && c.IsActive, cancellationToken);

            Product productEntity = await productRepository.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
            InventoryDetail inventoryDetail = FindProductSpecValuesRecord(productId, specValues);

            if (userCartEntity == null)
            {
                Result res = await InsertUserShoppingCart(userId, cancellationToken);

                if (res.Succeeded)
                {
                    userCartEntity = await shoppingCartRepository.FirstOrDefaultAsync(c => c.CreatorUserId == userId && !c.IsDeleted && c.AssociatedDomainId == domainEntity.Id && c.IsActive, cancellationToken);
                }
            }

            //surely this product added before to shopping cart then it has this record
            PurchasePerSeller sellerObj = userCartEntity?.Details.FirstOrDefault(s => s.SellerId == productEntity.SellerUserId);

            if (sellerObj == null)
            {
                return result;
            }

            {
                ShoppingCartDetail productRow = sellerObj.Products.FirstOrDefault(d => d.ShoppingCartDetailId == shoppingCartDetailId);

                if (inventoryDetail.Count > 0 && inventoryDetail.Count >= newCount)
                {
                    if (productRow != null)
                    {
                        productRow.OrderCount = newCount;
                        long price = GetCurrentPrice(productEntity);
                        DiscountModel res = await GetCurrentDiscountPerUnit(productEntity, price);
                        productRow.DiscountPricePerUnit = res.DiscountPerUnit;
                        productRow.PricePerUnit = price;

                        Result<ShoppingCart> updateResult = await shoppingCartRepository.UpdateAsync(userCartEntity, cancellationToken);

                        if (updateResult.Succeeded)
                        {
                            result.Succeeded = true;
                            result.Message = ConstMessages.SuccessfullyDone;
                        }
                        else
                        {
                            result.Message = ConstMessages.GeneralError;
                        }
                    }
                    else
                    {
                        result.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
                    }
                }
                else
                {
                    result.Message = ConstMessages.LackOfInventoryOfProduct;
                }
            }
        }
        else //if new count == 0
        {
            result = await DeleteProductFromUserShoppingCart(userId, shoppingCartDetailId, cancellationToken);
        }

        return result;
    }

    private async Task<Result> DeleteProductFromUserShoppingCart(string userId, string shoppingCartDetailId, CancellationToken cancellationToken = default)
    {
        Result result = new();
        string domainName = GetCurrentDomainName();
        Domain domainEntity = await domainRepository.FirstOrDefaultAsync(d => d.DomainName == domainName, cancellationToken);
        ShoppingCart userCartEntity = await shoppingCartRepository.FirstOrDefaultAsync(c => c.CreatorUserId == userId && !c.IsDeleted && c.IsActive && c.AssociatedDomainId == domainEntity.Id, cancellationToken);

        //var productEntity = _productContext.ProductCollection.Find(_ => _.ProductId == productId).FirstOrDefault();
        if (userCartEntity != null)
        {
            PurchasePerSeller sellerObj = userCartEntity.Details.FirstOrDefault(s => s.Products.Any(a => a.ShoppingCartDetailId == shoppingCartDetailId));

            //var sellerObj = userCartEntity.Details.FirstOrDefault(_ => _.SellerId == productEntity.SellerUserId);
            int sellerIndex = userCartEntity.Details.IndexOf(sellerObj);

            if (sellerObj != null)
            {
                ShoppingCartDetail productRow = sellerObj.Products.FirstOrDefault(d => d.ShoppingCartDetailId == shoppingCartDetailId);
                sellerObj.Products.Remove(productRow);

                if (sellerObj.Products.Count == 0)
                {
                    userCartEntity.Details.Remove(sellerObj);

                    if (userCartEntity.Details.Count == 0)
                    {
                        userCartEntity.IsActive = false;
                        userCartEntity.IsDeleted = true;
                    }
                }
                else
                {
                    userCartEntity.Details[sellerIndex] = sellerObj;
                }

                Result<ShoppingCart> updateResult = await shoppingCartRepository.UpdateAsync(userCartEntity, cancellationToken);

                if (updateResult.Succeeded)
                {
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    result.Message = ConstMessages.GeneralError;
                }
            }
            else
            {
                result.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
            }
        }
        else
        {
            result.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
        }

        return result;
    }
    #endregion
}