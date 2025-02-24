using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using Arad.Portal.GeneralLibrary.Utilities;
using Microsoft.AspNetCore.Authorization;

using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Promotion;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using System.Threading;
using System.Collections.Specialized;
using System.Web;
using System.Globalization;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductGroup;
using Serilog;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.Promotion;
using Arad.Portal.Models.Shared.Language;
using Arad.Portal.Models.Shared.User;
using Arad.Portal.Helpers.Shared;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Arad.Portal.Areas.Admin.Controllers.Setting;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class PromotionController(
    IPromotionRepository promotionRepository,
    ILanguageRepository lanRepository,
    IProductRepository productRepository,
    IModificationRepository modificationRepository,
    UserManager<ApplicationUser> userManager,
    IMapper mapper,
    ILogger logger,
    ControllerHelper controllerHelper,
    IProductGroupRepository productGroupRepository)
    : Controller
{

    [HttpGet]
    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        PagedItems<PromotionDto> promotionDtoList = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        ViewBag.IsSysAcc = userDb.IsSystemAccount;
        DataLayer.Entities.General.Language.Language defLang = controllerHelper.GetDefaultLanguage();
        List<SelectListModel> slmProduct = (await productRepository.GetListAsync(p => p.SellerUserId == userDb.Id && p.IsActive, cancellationToken))
                                           .Select(p => new SelectListModel
                                           {
                                               Text = p.MultiLingualProperties.Count(a => a.LanguageId == defLang.Id) != 0 ?
                                                                       p.MultiLingualProperties.FirstOrDefault(a => a.LanguageId == defLang.Id)?.Name : "",
                                               Value = p.Id.ToString()
                                           }).ToList();
        slmProduct.Insert(0, new() { Text = UtilityLanguage.GetString("AlertAndMessage_Choose"), Value = "-1" });
        ViewBag.CurrentSellerProductList = slmProduct;

        try
        {
            string q;
            if (!string.IsNullOrWhiteSpace(Request.QueryString.ToString()))
            {
                q = Request.QueryString + "&";
            }
            else
            {
                q = "?";
            }
            q += userDb.IsSystemAccount ? $"userId={Guid.Empty}" : $"userId={userDb.Id}";


            promotionDtoList = new()
            {
                CurrentPage = 1,
                ItemsCount = 0,
                PageSize = 10,
                QueryString = q
            };
            try
            {
                NameValueCollection filter = HttpUtility.ParseQueryString(q);

                if (string.IsNullOrWhiteSpace(filter["page"]))
                {
                    filter.Set("page", "1");
                }
                if (string.IsNullOrWhiteSpace(filter["PageSize"]))
                {
                    filter.Set("PageSize", "20");
                }
                int page = Convert.ToInt32(filter["page"]);
                int pageSize = Convert.ToInt32(filter["PageSize"]);
                string? userId = filter["userId"];
                string? promotionTypeId = filter["promotionTypeId"];
                string? groupId = filter["groupId"];
                string? productId = filter["productId"];
                string? discountTypeId = filter["discountTypeId"];
                string? title = filter["title"];
                string? fromDate = filter["fDate"];
                string? toDate = filter["tDate"];

                List<Promotion> promotionList;
              
                if (userId == Guid.Empty.ToString())
                {
                    promotionList = await promotionRepository.GetAllAsync(cancellationToken);
                }
                else
                {
                    promotionList = (await promotionRepository.GetListAsync(p => p.CreatorUserId.ToString() == userId, cancellationToken)).AsQueryable().ToList();
                }
                if (!string.IsNullOrWhiteSpace(title))
                {
                    promotionList = promotionList.Where(p => p.Title.Contains(title)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(promotionTypeId))
                {
                    promotionList = promotionList.Where(p => p.PromotionType == (PromotionType)Convert.ToInt32(promotionTypeId)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(discountTypeId))
                {
                    promotionList = promotionList.Where(p => p.DiscountType == (DiscountType)Convert.ToInt32(discountTypeId)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(fromDate))
                {
                    promotionList = promotionList.Where(p => p.SDate >= fromDate.Split(" ")[0].ToEnglishDate().ToUniversalTime()).ToList();
                }
                if (!string.IsNullOrWhiteSpace(toDate))
                {
                    promotionList = promotionList.Where(p => p.EDate <= toDate.Split(" ")[0].ToEnglishDate().ToUniversalTime()).ToList();
                }
                if (!string.IsNullOrWhiteSpace(productId))
                {
                    promotionList = promotionList.Where(p => p.Infoes.Any(i => i.AffectedProductId.ToString() == productId)).ToList();
                }
                else if (!string.IsNullOrWhiteSpace(groupId))
                {
                    promotionList = promotionList.Where(p => p.Infoes.Any(i => i.AffectedProductGroupId.ToString() == groupId)).ToList();
                }
                promotionList = promotionList.OrderByDescending(p => p.CreationDate).ToList();
                if (promotionList.Count > pageSize)
                {
                    promotionList = promotionList.Skip((page - 1) * pageSize)
                                                 .Take(pageSize).ToList();
                }

                promotionDtoList.Items = mapper.Map<List<PromotionDto>>(promotionList);
                foreach (PromotionDto item in promotionDtoList.Items)
                {
                    foreach (PromotionInfo desc in item.Infoes.Where(desc => !string.IsNullOrWhiteSpace(desc.AffectedProductName)))
                    {
                        item.ProductNamesConcat += desc.AffectedProductName;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(PromotionController)}/{nameof(List)}");
                promotionDtoList.CurrentPage = 1;
                promotionDtoList.ItemsCount = 0;
                promotionDtoList.PageSize = 10;
            }
            ViewBag.DefCurrencyId = controllerHelper.GetDefaultCurrency().ReturnValue.Id;

            ViewBag.CurrencyList = controllerHelper.GetAllActiveCurrency();

            List<SelectListModel> slmPromotionTypes = [];
            slmPromotionTypes.AddRange(from int i in Enum.GetValues(typeof(PromotionType)) let name = Enum.GetName(typeof(PromotionType), i) select new SelectListModel { Text = name, Value = i.ToString() });

            slmPromotionTypes.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });
            ViewBag.PromotionTypes = slmPromotionTypes;

            ViewBag.DiscountTypes = controllerHelper.GetAllDiscountType(false);
            ViewBag.Domains = controllerHelper.GetAllActiveDomains();
            ViewBag.ProductGroupList = controllerHelper.GetAllActiveProductGroup(defLang.Id);

            IList<ApplicationUser> vendorList = await userManager.GetUsersForClaimAsync(new("Vendor", "True"));
            ViewBag.Vendors = vendorList.ToList().Select(u => new SelectListModel
            {
                Text = u.Profile.FullName,
                Value = u.Id.ToString()
            }).ToList();


        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(PromotionController)}/{nameof(List)}");
        }
        return View(promotionDtoList);
    }

    [HttpGet]
    public async ValueTask<IActionResult> UserCouponsList(CancellationToken cancellationToken)
    {
        PagedItems<UserCouponDto> listUserCouponDto = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        ViewBag.IsSysAcc = userDb.IsSystemAccount;
        Domain domainEntity = controllerHelper.GetCurrentUserDomain();
        try
        {
            string q;
            if (!string.IsNullOrWhiteSpace(Request.QueryString.ToString()))
            {
                q = Request.QueryString + "&";
            }
            else
            {
                q = "?";
            }
            string domain = userDb.IsSystemAccount ? "" : domainEntity.Id;
            q += $"domainId={domain}";
            listUserCouponDto = new()
            {
                CurrentPage = 1,
                ItemsCount = 0,
                PageSize = 10,
                QueryString = q
            };
            NameValueCollection filter = HttpUtility.ParseQueryString(q);

            if (string.IsNullOrWhiteSpace(filter["page"]))
            {
                filter.Set("page", "1");
            }
            if (string.IsNullOrWhiteSpace(filter["PageSize"]))
            {
                filter.Set("PageSize", "20");
            }
            //surely querystring contains domainId
            int page = Convert.ToInt32(filter["page"]);
            int pageSize = Convert.ToInt32(filter["PageSize"]);
            string domainId = filter["domainId"]!;
            string promotionId = "";
            if (string.IsNullOrWhiteSpace(filter["promotionId"]))
            {
                promotionId = filter["promotionId"]!;
            }
            List<UserCoupon> userCouponList = [];

            if (!string.IsNullOrWhiteSpace(domainId))
            {
                await promotionRepository.GetCountAsync(p => p.AssociatedDomainId.ToString() == domainId, cancellationToken);
                userCouponList = (await promotionRepository.FirstOrDefaultAsync(p => p.AssociatedDomainId.ToString() == domainId, cancellationToken)).UserCoupons;
            }
            else
            {
                await promotionRepository.GetCountAsync(_ => true, cancellationToken);
                Promotion promotion = await promotionRepository.FirstOrDefaultAsync(c => c.AsUserCoupon == true, cancellationToken);
                if (promotion != null)
                {
                    userCouponList = promotion.UserCoupons;
                }

            }

            if (!string.IsNullOrWhiteSpace(promotionId))
            {
                userCouponList = userCouponList.Where(c => c.PromotionId.ToString() == promotionId).ToList();
            }
            if (userCouponList != null)
            {
                if (userCouponList.Count > 0)
                {
                    userCouponList = userCouponList.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                }
                foreach (UserCoupon item in userCouponList)
                {
                    UserCouponDto obj = mapper.Map<UserCouponDto>(item);
                    Promotion promotion = await promotionRepository.FirstOrDefaultAsync(p => p.Id == item.PromotionId, cancellationToken);

                    if (promotion == null)
                    {
                        continue;
                    }

                    obj.DiscountType = promotion.DiscountType;
                    obj.Value = promotion.Value;
                    obj.StartDate = promotion.SDate;
                    obj.CouponCode = promotion.CouponCode;
                    obj.EndDate = promotion.EDate;
                    listUserCouponDto.Items.Add(obj);
                }
            }
            List<SelectListModel> res;
            if (!string.IsNullOrWhiteSpace(domainEntity.DomainName))
            {
                res = (await promotionRepository.GetListAsync(p => p.AssociatedDomainId == domainEntity.Id && p.SDate <= DateTime.UtcNow &&
                                                                   (p.EDate >= DateTime.UtcNow || p.EDate == null) && p.IsActive && p.AsUserCoupon,
                                                              cancellationToken))
                                          .Select(p => new SelectListModel
                                          {
                                              Text = p.CouponCode,
                                              Value = p.Id.ToString()
                                          }).ToList();
            }
            else
            {
                res = (await promotionRepository.GetListAsync(p => p.SDate <= DateTime.UtcNow &&
                                                                   (p.EDate >= DateTime.UtcNow || p.EDate == null) && p.IsActive && p.AsUserCoupon,
                                                              cancellationToken))
                                          .Select(p => new SelectListModel
                                          {
                                              Text = p.CouponCode,
                                              Value = p.Id.ToString()
                                          }).ToList();
            }

            res.Insert(0, new() { Value = "-1", Text = UtilityLanguage.GetString("Choose") });
            ViewBag.PromotionList = res;
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(PromotionController)}/{nameof(UserCouponsList)}");

        }
        return View(listUserCouponDto);
    }

    public async ValueTask<IActionResult> AddEdit(string id, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        PromotionDto promotionDto = new();
        LanguageDto? languageDto = null;
        DataLayer.Entities.General.Language.Language language;
        if (!string.IsNullOrEmpty(userDb.Profile.DefaultLanguageId))
        {
            language = await lanRepository.FirstOrDefaultAsync(l => l.Id == userDb.Profile.DefaultLanguageId, cancellationToken);
        }
        else
        {
            language = await lanRepository.FirstOrDefaultAsync(l => l.IsDefault, cancellationToken);
        }
        if (language != null)
        {
            languageDto = mapper.Map<LanguageDto>(language);
        }

        ViewBag.IsSysAcc = userDb.IsSystemAccount;
        if (!userDb.IsSystemAccount)
        {
            List<SelectListModel> lst = (await productRepository.GetListAsync(p => p.SellerUserId == userDb.Id && p.IsActive, cancellationToken))
                                        .Select(p => new SelectListModel
                                        {
                                            Text = p.MultiLingualProperties.Count(a => languageDto != null && a.LanguageId == languageDto.Id) != 0 ?
                                                                    p.MultiLingualProperties.FirstOrDefault(a => languageDto != null && a.LanguageId == languageDto.Id)?.Name : "",
                                            Value = p.Id.ToString()
                                        }).ToList();
            lst.Insert(0, new() { Text = UtilityLanguage.GetString("AlertAndMessage_Choose"), Value = "-1" });
            ViewBag.CurrentSellerProductList = lst;
        }
        else
        {
            ViewBag.Domains = controllerHelper.GetAllActiveDomains();
        }

        string? promotionDtoAssociatedDomainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;

        if (promotionDtoAssociatedDomainId != null)
        {
            promotionDto.AssociatedDomainId = promotionDtoAssociatedDomainId;
        }

        if (!string.IsNullOrEmpty(id))
        {
            Promotion promotionEntity = await promotionRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (promotionEntity != null)
            {
                promotionDto = mapper.Map<PromotionDto>(promotionEntity);
            }

            if (promotionEntity != null)
            {
                promotionDto.PersianStartDate = promotionEntity.SDate.ToLocalTime().ToPersianDdate();

                if (promotionEntity.EDate != null)
                {
                    promotionDto.PersianEndDate = promotionEntity.EDate.Value.ToLocalTime().ToPersianDdate();
                }

                if (promotionEntity.PromotionType != null)
                {
                    promotionDto.PromotionTypeId = (int)promotionEntity.PromotionType;
                }

                promotionDto.DiscountTypeId = (int)promotionEntity.DiscountType;
            }
        }
        ViewBag.DefCurrencyId = controllerHelper.GetDefaultCurrency();
        ViewBag.CurrencyList = controllerHelper.GetAllActiveCurrency();
        ViewBag.DiscountTypes = controllerHelper.GetAllDiscountType(false);


        List<SelectListModel> slmPromotionTypes = [];
        slmPromotionTypes.AddRange(from int i in Enum.GetValues(typeof(PromotionType)) let name = Enum.GetName(typeof(PromotionType), i) select new SelectListModel { Text = name, Value = i.ToString() });

        slmPromotionTypes.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });
        ViewBag.PromotionTypes = slmPromotionTypes;

        if (language != null)
        {
            ViewBag.ProductGroupList = controllerHelper.GetAllActiveProductGroup(language.Id);
            ViewBag.LangId = language.Id;
        }

        IQueryable<ApplicationUser> vendorList = userManager.Users.Where(c => c.IsVendor);
        ViewBag.Vendors = vendorList.ToList().Select(u => new SelectListModel
        {
            Text = u.Profile.FullName,
            Value = u.Id.ToString()
        });


        return View(promotionDto);
    }

    [HttpPost]
    public async ValueTask<IActionResult> Add([FromBody] PromotionDto dto, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        Result<Promotion> saveResult = new();
        List<AjaxValidationErrorModel> errors = [];
        try
        {
            if (dto.AsUserCoupon)
            {
                Result res = new();
                bool isExist = await promotionRepository.AnyAsync(p => p.AssociatedDomainId == dto.AssociatedDomainId && p.CouponCode == dto.CouponCode, cancellationToken);
                res.Succeeded = !isExist;
                if (!res.Succeeded)
                {
                    AjaxValidationErrorModel obj = new() { Key = "CouponCode", ErrorMessage = UtilityLanguage.GetString("AlertAndMessage_DuplicateCode") };
                    errors.Add(obj);
                }
            }
            if (!ModelState.IsValid)
            {

                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry? modelStateVal = ModelState[modelStateKey];

                    if (modelStateVal != null)
                    {
                        errors.AddRange(modelStateVal.Errors
                                                     .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                    }
                }
                Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else if (errors.Count > 0)
            {
                Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                Promotion model = mapper.Map<Promotion>(dto);
                model.Id = Guid.NewGuid().ToString();
                model.CreationDate = DateTime.Now;
                model.CreatorUserId = controllerHelper.GetCurrentUserId();
                model.CreatorUserName = controllerHelper.GetCurrentUser(cancellationToken).Result.UserName;
                model.IsActive = true;
                if (dto is { AsUserCoupon: false, PromotionTypeId: not null })
                {
                    model.PromotionType = (PromotionType)dto.PromotionTypeId;
                }

                model.DiscountType = (DiscountType)dto.DiscountTypeId;
                if (CultureInfo.CurrentCulture.Name.ToLower() == "fa-ir")
                {
                    model.SDate = dto.PersianStartDate.ToEnglishDate();
                    model.EDate = dto.PersianEndDate.ToEnglishDate();
                }
                saveResult = await promotionRepository.InsertAsync(model, cancellationToken);
                if (saveResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Insert,
                        CollectionType = CollectionType.Promotion,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = model.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    saveResult.Message = ConstMessages.SuccessfullyDone;
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, adding promotion with {dto.Id} id and {dto.Title} done successfully");
                }
                else
                {
                    saveResult.Message = ConstMessages.ErrorInSaving;
                }

            }
        }
        catch (Exception e)
        {
            saveResult.Succeeded = false;
            saveResult.Message = ConstMessages.ExceptionOccured;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(PromotionController)}/{nameof(Add)}");
        }
        JsonResult result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                                     : new { Status = "Error", saveResult.Message });
        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> AddUserCoupon([FromBody] UserCouponDto dto, CancellationToken cancellationToken)
    {
        Result<Promotion> upResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            Domain domainEntity = controllerHelper.GetCurrentUserDomain();
            dto.AssociatedDomainId = domainEntity.Id;
            UserCoupon model = mapper.Map<UserCoupon>(dto);
            model.CreationDate = DateTime.Now;
            model.CreatorUserId = userDb.Id;
            Promotion promotion = await promotionRepository.FirstOrDefaultAsync(p => p.Id == dto.PromotionId, cancellationToken);
            model.CouponCode = promotion.CouponCode;
            model.CreatorUserName = userDb.UserName;
            model.IsActive = true;
            model.Id = Guid.NewGuid().ToString();
            promotion.UserCoupons ??= [];
            promotion.UserCoupons.Add(model);
            upResult = await promotionRepository.UpdateAsync(promotion, cancellationToken);
            if (upResult.Succeeded)
            {
                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Insert,
                    CollectionType = CollectionType.Promotion,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = promotion.Id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},adding user coupon with {dto.Id} id and {dto.CouponCode} done successfully");
                upResult.Message = ConstMessages.SuccessfullyDone;
            }
            else
            {
                upResult.Message = ConstMessages.ErrorInSaving;
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(PromotionController)}/{nameof(AddUserCoupon)}");
            upResult.Message = ConstMessages.ExceptionOccured;
            upResult.Succeeded = false;
        }

        JsonResult result = Json(upResult.Succeeded ? new { Status = "Success", upResult.Message }
                                     : new { Status = "Error", upResult.Message });

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> AssignPromotionToUserAddEdit(string id, CancellationToken cancellationToken)
    {
        Domain domainName = controllerHelper.GetCurrentUserDomain();
        UserCouponDto dto = new();
        if (!string.IsNullOrEmpty(id))
        {
            List<Promotion> promotions = await promotionRepository.GetAllAsync(cancellationToken);
            UserCoupon? foundUserCoupon = null;

            foreach (Promotion promotion in promotions)
            {
                foundUserCoupon = promotion.UserCoupons.FirstOrDefault(c => c.Id == id);

                if (foundUserCoupon == null)
                {
                    continue;
                }

                break;
            }

            if (foundUserCoupon != null)
            {
                dto = mapper.Map<UserCouponDto>(foundUserCoupon);
                {
                    dto.Id = id;
                    dto.CouponCode = foundUserCoupon.CouponCode;
                    dto.PromotionId = foundUserCoupon.PromotionId;
                    dto.UserIds = foundUserCoupon.UserIds;

                    Promotion promotion = await promotionRepository.FirstOrDefaultAsync(p => p.Id == foundUserCoupon.PromotionId, cancellationToken);

                    if (promotion != null)
                    {
                        dto.PromotionName = promotion.Title;
                        dto.DiscountType = promotion.DiscountType;
                        dto.StartDate = promotion.SDate;

                        if (promotion.EDate != null)
                        {
                            dto.EndDate = promotion.EDate.Value;
                        }

                        if (promotion.Value != null)
                        {
                            dto.Value = promotion.Value.Value;
                        }
                    }
                }
            }

        }
        Result<Domain> domainEntity = controllerHelper.FetchDomainByName(domainName.DomainName, false);
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        List<SelectListModel> res;
        if (!string.IsNullOrWhiteSpace(domainName.DomainName))
        {
            res = (await promotionRepository.GetListAsync(p => p.AssociatedDomainId == domainEntity.ReturnValue.Id && p.SDate <= DateTime.UtcNow &&
                                                               (p.EDate >= DateTime.UtcNow || p.EDate == null) && p.IsActive && p.AsUserCoupon,
                                                          cancellationToken))
                                      .Select(p => new SelectListModel
                                      {
                                          Text = p.CouponCode,
                                          Value = p.Id.ToString()
                                      }).ToList();
        }
        else
        {
            res = (await promotionRepository.GetListAsync(p => p.SDate <= DateTime.UtcNow &&
                                                               (p.EDate >= DateTime.UtcNow || p.EDate == null) && p.IsActive && p.AsUserCoupon,
                                                          cancellationToken))
                                      .Select(p => new SelectListModel
                                      {
                                          Text = p.CouponCode,
                                          Value = p.Id.ToString()
                                      }).ToList();
        }

        res.Insert(0, new() { Value = "-1", Text = UtilityLanguage.GetString("Choose") });
        ViewBag.PromotionList = res;
        if (userDb is { IsSystemAccount: true })
        {
            ViewBag.DomainUsers = userManager.Users.Where(u => u.IsActive && !u.IsDeleted)
                                              .Select(u => new SelectListModel
                                              {
                                                  Text = u.Profile.FullName,
                                                  Value = u.Id.ToString()
                                              }).ToList();
        }
        else
        {
            ViewBag.DomainUsers = userManager.Users
                                              .Where(u => u.IsActive && !u.IsDeleted && u.Domains.Any(a => a.DomainId == domainEntity.ReturnValue.Id) && u.Id != userDb.Id)
                                              .Select(u => new SelectListModel
                                              {
                                                  Text = u.Profile.FullName,
                                                  Value = u.Id.ToString()
                                              }).ToList();
        }

        return View(dto);
    }

    [HttpGet]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        Result<Promotion> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            Promotion promotion = await promotionRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (promotion != null)
            {
                opResult = await promotionRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);
                if (opResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Restore,
                        CollectionType = CollectionType.Promotion,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = promotion.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, restoring promotion with {id} id and {promotion.Title} done successfully");
                    opResult.Message = ConstMessages.SuccessfullyDone;
                    result = new(new
                    {
                        Status = "success",
                        Message = UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully")
                    });
                }
                else
                {
                    opResult.Message = ConstMessages.ErrorInSaving;
                    result = new(new
                    {
                        Status = "error",
                        Message = UtilityLanguage.GetString("AlertAndMessage_TryLater")
                    });
                }
            }
            else
            {
                result = new(new
                {
                    Status = "error",
                    Message = UtilityLanguage.GetString("AlertAndMessage_EntityNotFound")
                });
                opResult.Message = ConstMessages.ObjectNotFound;
            }

        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(PromotionController)}/{nameof(Restore)}");
            opResult.Message = ConstMessages.ExceptionOccured;
            result = new(new
            {
                Status = "error",
                Message = UtilityLanguage.GetString("AlertAndMessage_TryLater")
            });
        }
        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Edit([FromBody] PromotionDto dto, CancellationToken cancellationToken)
    {
        Result<Promotion> saveResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {

            if (!ModelState.IsValid)
            {
                List<AjaxValidationErrorModel> errors = [];

                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry? modelStateVal = ModelState[modelStateKey];

                    if (modelStateVal != null)
                    {
                        errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                    }
                }

                return Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                Promotion entity = await promotionRepository.FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken);
                PromotionDto promotionDto = mapper.Map<PromotionDto>(entity);

                if (entity == null)
                {
                    return Json(new { Status = "ModelError" });

                }

                promotionDto.PersianStartDate = entity.SDate.ToLocalTime().ToPersianDdate();

                if (entity.EDate != null)
                {
                    promotionDto.PersianEndDate = entity.EDate.Value.ToLocalTime().ToPersianDdate();
                }

                if (entity.PromotionType != null)
                {
                    promotionDto.PromotionTypeId = (int)entity.PromotionType;
                }

                promotionDto.DiscountTypeId = (int)entity.DiscountType;

                Promotion promotion = mapper.Map(dto, entity);

                if (CultureInfo.CurrentCulture.Name.ToLower() == "fa-ir")
                {
                    promotion.SDate = dto.PersianStartDate.Split(" ")[0].ToEnglishDate();
                }

                if (CultureInfo.CurrentCulture.Name.ToLower() == "fa-ir" && !string.IsNullOrWhiteSpace(dto.PersianEndDate.Split(" ")[0]))
                {
                    promotion.EDate = dto.PersianEndDate.ToEnglishDate();
                }

                if (dto.PromotionTypeId != null)
                {
                    promotion.PromotionType = (PromotionType)dto.PromotionTypeId;
                }

                saveResult = await promotionRepository.UpdateAsync(promotion, cancellationToken);

                if (saveResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Update,
                        CollectionType = CollectionType.Promotion,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = promotion.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},editing promotion with {dto.Id} id and {dto.Title} done successfully");
                    saveResult.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    saveResult.Message = ConstMessages.ErrorInSaving;
                }
            }
        }
        catch (Exception e)
        {
            saveResult.Succeeded = false;
            saveResult.Message = ConstMessages.ExceptionOccured;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(PromotionController)}/{nameof(Edit)}");
        }

        JsonResult result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                                     : new { Status = "Error", saveResult.Message });
        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> EditUserCoupon([FromBody] UserCouponDto dto, CancellationToken cancellationToken)
    {
        Result<Promotion> saveResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            Promotion promotion = await promotionRepository.FirstOrDefaultAsync(p => p.UserCoupons.Any(c => c.Id == dto.Id), cancellationToken);
            if (promotion == null)
            {
                return RedirectToAction("PageOrItemNotFound", "Account");
            }

            UserCoupon? entity = promotion.UserCoupons.FirstOrDefault(c => c.Id == dto.Id);
            if (entity == null)
            {
                return RedirectToAction("PageOrItemNotFound", "Account");
            }

            // Update the properties of the existing entity
            entity.UserIds = dto.UserIds;
            entity.PromotionId = dto.PromotionId;
            entity.CouponCode = dto.CouponCode;

            // Update the promotion properties that are relevant
            promotion.Title = dto.PromotionName;
            promotion.DiscountType = dto.DiscountType ?? promotion.DiscountType;
            promotion.SDate = dto.StartDate ?? promotion.SDate;
            promotion.EDate = dto.EndDate ?? promotion.EDate;
            promotion.Value = dto.Value ?? promotion.Value;

            saveResult = await promotionRepository.UpdateAsync(promotion, cancellationToken);
            if (saveResult.Succeeded)
            {
                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Update,
                    CollectionType = CollectionType.Promotion,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = promotion.Id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, editing user coupon with {dto.Id} id and {dto.CouponCode} done successfully");
                saveResult.Message = ConstMessages.SuccessfullyDone;
            }
            else
            {
                saveResult.Message = ConstMessages.ErrorInSaving;
            }
        }
        catch (Exception e)
        {
            saveResult.Message = ConstMessages.ExceptionOccured;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(PromotionController)}/{nameof(EditUserCoupon)}");
        }

        return Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                        : new { Status = "Error", saveResult.Message });
    }

    [HttpGet]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        Result<Promotion> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            Promotion promotion = await promotionRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (promotion == null)
            {
                opResult.Succeeded = false;
                opResult.Message = ConstMessages.ObjectNotFound;
            }
            else
            {
                opResult = await promotionRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, true, cancellationToken);
                if (opResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Delete,
                        CollectionType = CollectionType.Promotion,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = promotion.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},adding product specification group with {id} id and {promotion.Title} done successfully");
                    opResult.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    opResult.Message = ConstMessages.ErrorInSaving;
                }

            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(PromotionController)}/{nameof(Delete)}");
            opResult.Succeeded = false;
            opResult.Message = ConstMessages.ExceptionOccured;
        }

        return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }

    [HttpGet]
    public async ValueTask<IActionResult> DeleteUserCoupon(string id, CancellationToken cancellationToken)
    {
        Result<Promotion> upResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            Promotion promotion = await promotionRepository.FirstOrDefaultAsync(c => c.UserCoupons.Any(uc => uc.Id == id), cancellationToken);
            if (promotion == null)
            {
                upResult.Succeeded = false;
                upResult.Message = ConstMessages.ObjectNotFound;
            }
            else
            {
                UserCoupon? userCoupon = promotion.UserCoupons.FirstOrDefault(c => c.Id == id);
                promotion.UserCoupons.Remove(userCoupon);
                upResult = await promotionRepository.UpdateAsync(promotion, cancellationToken);
                if (upResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Delete,
                        CollectionType = CollectionType.Promotion,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = promotion.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    upResult.Succeeded = true;
                    upResult.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    upResult.Message = ConstMessages.ErrorInSaving;
                }
            }
        }
        catch (Exception)
        {
            upResult.Succeeded = false;
            upResult.Message = ConstMessages.ExceptionOccured;
        }
        return Json(upResult.Succeeded ? new { Status = "Success", upResult.Message }
                        : new { Status = "Error", upResult.Message });
    }

    [HttpGet]
    public async ValueTask<IActionResult> GetFilteredProduct(string productGroupId, string vendorId, CancellationToken cancellationToken)
    {
        JsonResult result;
        //List<SelectListModel> lst = new List<SelectListModel>();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        DataLayer.Entities.General.Language.Language defaultLanguage = controllerHelper.GetDefaultLanguage();
        if (vendorId == "0")
        {
            if (!userDb.IsSystemAccount)
            {
                vendorId = userDb.Id;
            }
        }
        if (userDb.IsSystemAccount)
        {
            vendorId = "-1";
        }
        if (userDb.IsSystemAccount)
        {
            userDb.Id = Guid.Empty.ToString();
        }

        List<SelectListModel> selectLists;

        if (userDb.Id == Guid.Empty.ToString())//systemAccount
        {
            selectLists = (await productRepository.GetListAsync(p => p.GroupIds.Contains(productGroupId) && (vendorId == "-1" || p.SellerUserId.ToString() == vendorId) && p.IsActive, cancellationToken))
                                           .Select(p => new SelectListModel
                                           {
                                               Text = p.MultiLingualProperties.Any(a => a.LanguageId == defaultLanguage.Id) ?
                                                                       p.MultiLingualProperties.FirstOrDefault(a => a.LanguageId == defaultLanguage.Id)?.Name : "",
                                               Value = p.Id.ToString()
                                           }).ToList();
        }
        else
        {
            selectLists = (await productRepository.GetListAsync(p => p.GroupIds.Contains(productGroupId) && (vendorId == "-1" || p.SellerUserId.ToString() == vendorId)
                                                                                                        && p.IsActive && p.CreatorUserId == userDb.Id,
                                                                cancellationToken))
                                           .Select(p => new SelectListModel
                                           {
                                               Text = p.MultiLingualProperties.Any(a => a.LanguageId == defaultLanguage.Id) ?
                                                                       p.MultiLingualProperties.FirstOrDefault(a => a.LanguageId == defaultLanguage.Id)?.Name : "",
                                               Value = p.Id.ToString()
                                           }).ToList();
        }
        if (selectLists.Count > 0)
        {
            result = new(new { Status = "success", Data = selectLists });
        }
        else
        {
            result = new(new { Status = "error", Message = UtilityLanguage.GetString(ConstMessages.ObjectNotFound) });
        }
        return result;

    }

    [HttpGet]
    public IActionResult GetRelatedDiscountType(bool asUserCoupon)
    {
        JsonResult result;
        List<SelectListModel> list = controllerHelper.GetAllDiscountType(asUserCoupon);
        if (list.Any())
        {
            result = new(new { Status = "success", Data = list });
        }
        else
        {
            result = new(new { Status = "error", Message = UtilityLanguage.GetString(ConstMessages.ObjectNotFound) });
        }
        return result;

    }

    [HttpGet]
    public async ValueTask<IActionResult> GetFilteredProductGroup(string vendorId, CancellationToken cancellationToken)
    {
        JsonResult result;
        List<SelectListModel> list = [];

        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        if (vendorId == "0")
        {
            if (!userDb.IsSystemAccount)
            {
                vendorId = userDb.Id;
            }
        }
        if (userDb.IsSystemAccount)
        {
            vendorId = "-1";
        }
        DataLayer.Entities.General.Language.Language defaultLanguage = controllerHelper.GetDefaultLanguage();
        if (vendorId != "-1")
        {
            List<List<string>> lst = (await productRepository.GetListAsync(p => p.SellerUserId == vendorId && p.IsActive, cancellationToken))
                                                      .Select(p => p.GroupIds).ToList();
            List<string> finalList = [];
            finalList.AddRange(lst.SelectMany(item => item));

            finalList = finalList.Distinct().ToList();
            list.AddRange(finalList.Select(item => new SelectListModel
            {
                Text = productGroupRepository.FirstOrDefault(g => g.Id == item)
                                                                                    .MultiLingualProperties.Any(p => p.LanguageId == defaultLanguage.Id)
                                                                  ? productGroupRepository.FirstOrDefault(g => g.Id == item)
                                                                                          .MultiLingualProperties.FirstOrDefault(p => p.LanguageId == defaultLanguage.Id)
                                                                                          ?.Name
                                                                  : productGroupRepository.FirstOrDefault(g => g.Id == item)
                                                                                          .MultiLingualProperties.FirstOrDefault()
                                                                                          ?.Name,
                Value = item
            }));
        }
        else
        {
            list = (await productGroupRepository.GetListAsync(g => g.IsActive, cancellationToken))
                                          .Select(g => new SelectListModel
                                          {
                                              Text = g.MultiLingualProperties.Any(p => p.LanguageId == defaultLanguage.Id) ? g.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == defaultLanguage.Id)?.Name :
                                                                      g.MultiLingualProperties.FirstOrDefault()?.Name,
                                              Value = g.Id.ToString()
                                          }).ToList();

        }
        if (list.Count > 0)
        {
            result = new(new { Status = "success", Data = list });
        }
        else
        {
            result = new(new { Status = "error", Message = UtilityLanguage.GetString(ConstMessages.ObjectNotFound) });
        }
        return result;
    }
}