using Arad.Portal.DataLayer.Contracts.Shop.Promotion;
using Arad.Portal.DataLayer.Models.Promotion;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using AutoMapper;
using System.Collections.Specialized;
using System.Web;
using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using Arad.Portal.GeneralLibrary.Utilities;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Arad.Portal.DataLayer.Models.User;

namespace Arad.Portal.DataLayer.Repositories.Shop.Promotion.Mongo
{
    public class PromotionRepository : BaseRepository, IPromotionRepository
    {
        private readonly PromotionContext _context;
        private readonly ProductContext _productContext;
        private readonly DomainContext _domainContext;
        private readonly IMapper _mapper;
        public PromotionRepository(IHttpContextAccessor httpContextAccessor,
                                   PromotionContext promotionContext,
                                   IWebHostEnvironment env,
                                   IMapper mapper,
                                   DomainContext domainContext,
                                   ProductContext productContext) : base(httpContextAccessor, env)
        {
            _context = promotionContext;
            _productContext = productContext;
            _mapper = mapper;
            _domainContext = domainContext;
        }

        public async Task<Result> AssignPromotionToProduct(string promotionId, string productId)
        {
            var result = new Result();
            var promotionEntity = _context.Collection
                .Find(_ => _.PromotionId == promotionId &&
                _.SDate <= DateTime.UtcNow && _.EDate == null)
                .FirstOrDefault();
            if(promotionEntity != null)
            {
                var productEntity = _productContext.ProductCollection
                    .Find(_ => _.ProductId == productId).FirstOrDefault();
                if(productEntity != null)
                {
                    productEntity.Promotion = promotionEntity;
                    var updateResult = await _productContext.ProductCollection
                        .ReplaceOneAsync(_=>_.ProductId == productId, productEntity);
                    if(updateResult.IsAcknowledged)
                    {
                        result.Succeeded = true;
                        result.Message = ConstMessages.SuccessfullyDone;
                    }
                    else
                    {
                        result.Message = ConstMessages.GeneralError;
                    }
                }else
                {
                    result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
                }
            }
            else
            {
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            return result;
        }

        public async Task<Result> AssingPromotionToProductGroup(string promotionId, string ProductGroupId)
        {
            var result = new Result();
            var promotionEntity = _context.Collection
                .Find(_ => _.PromotionId == promotionId &&
                _.SDate <= DateTime.UtcNow && _.EDate == null)
                .FirstOrDefault();
            if (promotionEntity != null)
            {
                var productGroupEntity = _productContext.ProductGroupCollection
                    .Find(_ => _.ProductGroupId == ProductGroupId).FirstOrDefault();
                if (productGroupEntity != null)
                {
                    productGroupEntity.Promotion = promotionEntity;
                    var updateResult = await _productContext.ProductGroupCollection
                        .ReplaceOneAsync(_ => _.ProductGroupId == ProductGroupId, productGroupEntity);
                    if (updateResult.IsAcknowledged)
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
                    result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
                }
            }
            else
            {
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            return result;
        }

        public async Task<Result> DeletePromotion(string promotionId, string modificationReason)
        {
            var result = new Result();
            var promotionEntity = _context.Collection
                .Find(_ => _.PromotionId == promotionId).FirstOrDefault();
            //bool isValid = (promotionEntity.EndDate != null && promotionEntity.EndDate.Value >= DateTime.UtcNow)
            //     || promotionEntity.EndDate == null;
           
                bool allowDeletion;

                var check = _productContext.ProductCollection
                    .AsQueryable().Any(_ => _.Promotion.PromotionId == promotionId);
                check &= _productContext.ProductGroupCollection
                    .AsQueryable().Any(_=>_.Promotion.PromotionId == promotionId);
                if (check)
                    allowDeletion = false;
                else
                    allowDeletion = true;

                if(allowDeletion)
                {
                    promotionEntity.IsDeleted = true;
                    #region add modification
                    var mod = GetCurrentModification(modificationReason);
                    promotionEntity.Modifications.Add(mod);
                    #endregion
                    var updateResult = await _context.Collection
                        .ReplaceOneAsync(_ => _.PromotionId == promotionId, promotionEntity);
                    if (updateResult.IsAcknowledged)
                    {
                        result.Succeeded = true;
                        result.Message = ConstMessages.SuccessfullyDone;
                    }
                    else
                    {
                        result.Message = ConstMessages.GeneralError;
                    }
                }else
                {
                    result.Message = ConstMessages.DeletedNotAllowedForDependencies;
                }
           
            return result;
        }

        public async Task<Result> InsertPromotion(PromotionDTO dto)
        {
            var result = new Result();
            try
            {
            var equallentModel = _mapper.Map<Entities.Shop.Promotion.Promotion>(dto);
            equallentModel.PromotionId = Guid.NewGuid().ToString();
            equallentModel.CreationDate = DateTime.Now;
            equallentModel.CreatorUserId = GetUserId();
            equallentModel.CreatorUserName = GetUserName();

            if(!dto.AsUserCoupon)
            equallentModel.PromotionType = (PromotionType)dto.PromotionTypeId;

            equallentModel.DiscountType = (DiscountType)dto.DiscountTypeId;
            equallentModel.SDate = dto.PersianStartDate.ToEnglishDate();
            equallentModel.EDate =dto.PersianEndDate.ToEnglishDate();

                await _context.Collection.InsertOneAsync(equallentModel);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;

            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.GeneralError;
            }
            return result;
        }
        public async Task<Result> UpdatePromotion(PromotionDTO dto)
        {
            var result = new Result();
            var oldEntity = _context.Collection
                .Find(_ => _.PromotionId == dto.PromotionId).FirstOrDefault();
            if(oldEntity != null)
            {
                var equallentModel = _mapper.Map<Entities.Shop.Promotion.Promotion>(dto);
                equallentModel.SDate = DateHelper.ToEnglishDate(dto.PersianStartDate);
                if(!string.IsNullOrWhiteSpace(dto.PersianEndDate))
                equallentModel.EDate = DateHelper.ToEnglishDate(dto.PersianEndDate);
                var modifications = oldEntity.Modifications;
                var newModification = GetCurrentModification(dto.ModificationReason);
                modifications.Add(newModification);
                equallentModel.Modifications = modifications;
                equallentModel.CreationDate = oldEntity.CreationDate;
                equallentModel.CreatorUserId = oldEntity.CreatorUserId;
                equallentModel.CreatorUserName = oldEntity.CreatorUserName;

                var updateResult = await _context.Collection
                    .ReplaceOneAsync(_ => _.PromotionId == dto.PromotionId, equallentModel);
                if (updateResult.IsAcknowledged)
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
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            return result;
        }

        public async Task<PagedItems<PromotionDTO>> ListPromotions(string queryString)
        {
            PagedItems<PromotionDTO> result = new PagedItems<PromotionDTO>()
            {
             CurrentPage = 1,
             ItemsCount = 0,
             PageSize = 10,
             QueryString = queryString
            };
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
                var userId = filter["userId"];
                var promotionTypeId = filter["promotionTypeId"];
                var groupId = filter["groupId"];
                var productId = filter["productId"];
                var discountTypeId = filter["discountTypeId"];
                var title = filter["title"];
                var fromDate = filter["fDate"];
                var toDate = filter["tDate"];

                List<Entities.Shop.Promotion.Promotion> list;
                long totalCount = await _context.Collection.Find(c => true).CountDocumentsAsync();
                if(userId == Guid.Empty.ToString())//show all promotions cause the user is system account
                {
                    list = _context.Collection.AsQueryable().Skip((page - 1) * pageSize)
                          .Take(pageSize).ToList();
                }else
                {
                    list = _context.Collection.AsQueryable().Where(_=>_.CreatorUserId == userId).Skip((page - 1) * pageSize)
                      .Take(pageSize).ToList();
                }
                if(!string.IsNullOrWhiteSpace(title))
                {
                    list = list.Where(_ => _.Title.Contains(title)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(promotionTypeId))
                {
                    list = list.Where(_ => _.PromotionType == (PromotionType)Convert.ToInt32(promotionTypeId)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(discountTypeId))
                {
                    list = list.Where(_ => _.DiscountType == (DiscountType)Convert.ToInt32(discountTypeId)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(fromDate))
                {
                    list = list.Where(_ => _.SDate >= DateHelper.ToEnglishDate(fromDate).ToUniversalTime()).ToList();
                }
                if (!string.IsNullOrWhiteSpace(toDate))
                {
                    list = list.Where(_ => _.EDate <= DateHelper.ToEnglishDate(toDate).ToUniversalTime()).ToList();
                }
                if (!string.IsNullOrWhiteSpace(productId))
                {
                    list = list.Where(_ => _.Infoes.Any(_=>_.AffectedProductId == productId)).ToList();
                }else if (!string.IsNullOrWhiteSpace(groupId))
                {
                    list = list.Where(_ => _.Infoes.Any(_=> _.AffectedProductGroupId == groupId)).ToList();
                }
                list = list.OrderByDescending(_ => _.CreationDate).ToList();
                if(list.Count > pageSize)
                {
                    list = list.Skip((page - 1) * pageSize)
                     .Take(pageSize).ToList();
                }

                result.Items = _mapper.Map<List<PromotionDTO>>(list);
                foreach (var item in result.Items)
                {
                    foreach (var desc in item.Infoes)
                    {
                        if(!string.IsNullOrWhiteSpace(desc.AffectedProductName))
                        {
                            item.ProductNamesConcat += desc.AffectedProductName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.CurrentPage = 1;
                result.ItemsCount = 0;
                result.PageSize = 10;
            }
            return result;
        }

        public async Task<Result> SetPromotionExpirationDate(string promotionId, DateTime? dateTime)
        {
            var result = new Result();
            var promotionEntity = _context.Collection
                .Find(_ => _.PromotionId == promotionId).FirstOrDefault();
            if(promotionEntity != null)
            {
                promotionEntity.EDate = dateTime == null ? DateTime.Now : dateTime.Value;
                var updateResult = await _context.Collection
                        .ReplaceOneAsync(_ => _.PromotionId == promotionId, promotionEntity);
                if (updateResult.IsAcknowledged)
                {
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    result.Message = ConstMessages.GeneralError;
                }
            }else
            {
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            return result;
        }

        public List<SelectListModel> GetActivePromotionsOfCurrentUser(string userId, PromotionType type)
        {
            var alltypes = _context.Collection.Find(_ => _.CreatorUserId == userId && _.SDate <= DateTime.UtcNow &&
            (_.EDate >= DateTime.UtcNow || _.EDate == null) && _.IsActive && !_.AsUserCoupon);
            var res = alltypes.ToList().Where(_=>_.PromotionType == type).Select(_ => new SelectListModel() 
            { 
                Text = _.Title,
                Value = _.PromotionId
            }).ToList();
            return res;
        }

        public PromotionDTO FetchPromotion(string id)
        {
            var result = new PromotionDTO();
            var entity = _context.Collection.Find(_ => _.PromotionId == id).FirstOrDefault();
            if(entity != null)
            {
                result = _mapper.Map<PromotionDTO>(entity);
            }
            result.PersianStartDate = DateHelper.ToPersianDdate(entity.SDate.ToLocalTime());
            if(entity.EDate != null)
            {
                result.PersianEndDate = DateHelper.ToPersianDdate(entity.EDate.Value.ToLocalTime());
            }
            if(entity.PromotionType != null)
            {
                result.PromotionTypeId = (int)entity.PromotionType;
            }
            result.DiscountTypeId = (int)entity.DiscountType;
            return result;
        }

        public List<SelectListModel> GetAllPromotionType()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(PromotionType)))
            {
                string name = Enum.GetName(typeof(PromotionType), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel() {Text = Language.GetString("Choose"), Value = "-1" });
            return result;
        }

        public List<SelectListModel> GetAllDiscountType(bool asCoupon)
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(DiscountType)))
            {
                if(!asCoupon)
                {
                    string name = Enum.GetName(typeof(DiscountType), i);
                    var obj = new SelectListModel()
                    {
                        Text = name,
                        Value = i.ToString()
                    };
                    result.Add(obj);
                }else if(i != 2)
                {
                    string name = Enum.GetName(typeof(DiscountType), i);
                    var obj = new SelectListModel()
                    {
                        Text = name,
                        Value = i.ToString()
                    };
                    result.Add(obj);
                }
                
            }
            result.Insert(0, new SelectListModel() { Text = Language.GetString("Choose"), Value = "-1" });
            return result;
        }

        public async Task<Result> Restore(string promotionId)
        {
            var result = new Result();
            var entity = _context.Collection
              .Find(_ => _.PromotionId == promotionId).FirstOrDefault();
            entity.IsDeleted = false;
            var updateResult = await _context.Collection
               .ReplaceOneAsync(_ => _.PromotionId == promotionId, entity);
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
            return result;
        }

        public List<SelectListModel> GetAvailableCouponsOfDomain(string domainName)
        {
            var domainEntity = _domainContext.Collection.Find(_ => _.DomainName == domainName).FirstOrDefault();
            var res = new List<SelectListModel>();
            res = _context.Collection.Find(_ => _.AssociatedDomainId == domainEntity.DomainId && _.SDate <= DateTime.UtcNow &&
            (_.EDate >= DateTime.UtcNow || _.EDate == null) && _.IsActive && _.AsUserCoupon)
                .Project(_=> new SelectListModel()
                    {
                    Text = _.CouponCode,
                    Value = _.PromotionId
                }).ToList();
            res.Insert(0, new SelectListModel() { Value = "-1", Text = Language.GetString("Choose") });
            return res;
        }

        public Result CheckCouponCodeUniqueness(string couponCode, string domainId)
        {
            var res = new Result();
            var isExist = _context.Collection.Find(_ => _.AssociatedDomainId == domainId && _.CouponCode == couponCode).Any();
            res.Succeeded = !isExist;
            return res;
        }

        public UserCouponDTO FetchUserCoupon(string userCouponId)
        {
            var result = new UserCouponDTO();
            var entity = _context.UserCouponCollection
                .Find(_ => _.UserCouponId == userCouponId).FirstOrDefault();
            
            if(entity != null)
            {
                result.UserCouponId = userCouponId;
                result.CouponCode = entity.CouponCode;
                result.PromotionId = entity.PromotionId;
                result.UserIds = entity.UserIds;

                var promotion = _context.Collection
                     .Find(_ => _.PromotionId == entity.PromotionId).FirstOrDefault();

                if(promotion != null)
                {
                    result.PromotionName = promotion.Title;
                    result.DiscountType = promotion.DiscountType;
                    result.StartDate = promotion.SDate;
                    result.EndDate = promotion.EDate.Value;
                    result.Value = promotion.Value.Value;
                }
            }
            return result;
        }

        public async Task<Result> InsertUserCoupon(UserCouponDTO dto)
        {
            var result = new Result();
            try
            {
                var model = _mapper.Map<UserCoupon>(dto);
                model.CreationDate = DateTime.Now;
                model.CreatorUserId = GetUserId();
                var promotion = _context.Collection.Find(_ => _.PromotionId == dto.PromotionId).FirstOrDefault();
                model.CouponCode = promotion.CouponCode;
                model.CreatorUserName = GetUserName();
                model.IsActive = true;
                model.UserCouponId = Guid.NewGuid().ToString();
                await _context.UserCouponCollection.InsertOneAsync(model);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;

            }
            catch (Exception)
            {
                result.Message = ConstMessages.GeneralError;
            }
            return result;
        }

        public async Task<Result> UpdateUserCoupon(UserCouponDTO dto)
        {
            var result = new Result();
            var model = _mapper.Map<UserCoupon>(dto);
            var updateRes =await _context.UserCouponCollection.ReplaceOneAsync(_ => _.UserCouponId == model.UserCouponId, model);
            if(updateRes.IsAcknowledged)
            {
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            else
            {
                result.Message = ConstMessages.GeneralError;
            }
            return result;
        }

        public async Task<PagedItems<UserCouponDTO>> ListUserCoupon(string queryString)
        {
            PagedItems<UserCouponDTO> result = new PagedItems<UserCouponDTO>()
            {
                CurrentPage = 1,
                ItemsCount = 0,
                PageSize = 10,
                QueryString = queryString
            };
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
                //surely querystring contains domainId
                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);
                var domainId = filter["domainId"];
                string promotionId = string.Empty;
                if(string.IsNullOrWhiteSpace(filter["promotionId"]))
                {
                    promotionId = filter["promotionId"];
                }
                List<Entities.Shop.Promotion.UserCoupon> list;
                long totalCount = await _context.UserCouponCollection.Find(_=>_.AssociatedDomainId == domainId && _.PromotionId == promotionId).CountDocumentsAsync();

                list = _context.UserCouponCollection.Find(_ => _.AssociatedDomainId == domainId).ToList();
                
                if (!string.IsNullOrWhiteSpace(promotionId))
                {
                    list = list.Where(_ => _.PromotionId == promotionId).ToList();
                }
               if(list.Count > 0 )
                {
                    list = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                }
                foreach (var item in list)
                {
                    var obj = _mapper.Map<UserCouponDTO>(item);
                    var promotion = _context.Collection.Find(_ => _.PromotionId == item.PromotionId).FirstOrDefault();
                    obj.DiscountType = promotion.DiscountType;
                    obj.Value = promotion.Value;
                    obj.StartDate = promotion.SDate;
                    obj.CouponCode = promotion.CouponCode;
                    obj.EndDate = promotion.EDate;
                    result.Items.Add(obj);
                }
            }
            catch (Exception ex)
            {
                result.CurrentPage = 1;
                result.ItemsCount = 0;
                result.PageSize = 10;
            }
            return result;
        }

        public async Task<Result> DeleteUserCoupon(string userCouponId)
        {
            var result = new Result();
            var userCoupon = _context.UserCouponCollection
                .Find(_ => _.UserCouponId == userCouponId).FirstOrDefault();

            userCoupon.IsDeleted = true;
            var updateRes = await _context.UserCouponCollection.ReplaceOneAsync(_ => _.UserCouponId == userCouponId, userCoupon);
            if(updateRes.IsAcknowledged)
            {
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }else
            {
                result.Message = ConstMessages.ExceptionOccured;
            }
            return result;
        }

        public Result<NewVal> CheckCode(string userId, string code, string domainId, long price)
        {
            var result = new Result<NewVal>();
            result.ReturnValue = new NewVal();
            var entity = _context.UserCouponCollection
                .Find(_ => _.UserIds.Contains(userId) && _.AssociatedDomainId == domainId && _.CouponCode == code).Any() ? _context.UserCouponCollection
                .Find(_ => _.UserIds.Contains(userId) && _.AssociatedDomainId == domainId && _.CouponCode == code).FirstOrDefault() : null ;

            if(entity != null)
            {
                var promotion = _context.Collection.Find(_ => _.PromotionId == entity.PromotionId).FirstOrDefault();
                if(promotion != null && promotion.IsActive && !promotion.IsDeleted && promotion.SDate <= DateTime.UtcNow && promotion.EDate >= DateTime.UtcNow)
                {
                    result.Succeeded = true;
                    if(promotion.DiscountType == DiscountType.Fixed)
                    {
                        result.ReturnValue.Price = price - promotion.Value.Value;
                    }else
                    if(promotion.DiscountType == DiscountType.Percentage)
                    {
                        result.ReturnValue.Price = (price - (promotion.Value.Value * price / 100));
                    }
                }
            }
            return result;
        }

        public async Task<Result> RemoveUserFromUserCoupon(string couponCode, string userId, string domainId)
        {
            var res = new Result();
            var entity = _context.UserCouponCollection.Find(_ => _.CouponCode == couponCode && _.AssociatedDomainId == domainId).FirstOrDefault();
            if(entity != null)
            {
                entity.UserIds.Remove(userId);
                var updateRes = await _context.UserCouponCollection.ReplaceOneAsync(_ => _.UserCouponId == entity.UserCouponId, entity);
                if(updateRes.IsAcknowledged)
                {
                    res.Succeeded = true;
                    res.Message = Language.GetString("AlertAndMessage_OperationDoneSuccessfully");
                }else
                {
                    res.Message = ConstMessages.ExceptionOccured;
                }
            }
            else
            {
                res.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            return res;
        }

        public async Task<Result<NewVal>> RevertCodeForUser(string userId, string code, string domainId, long currentPriceWithCouponCode)
        {
            var res = new Result<NewVal>();
            res.ReturnValue = new NewVal();
            var entity = _context.UserCouponCollection.Find(_ => _.CouponCode == code && _.AssociatedDomainId == domainId).FirstOrDefault();
            if(entity != null)
            {
                var promotion = _context.Collection.Find(_ => _.PromotionId == entity.PromotionId).FirstOrDefault();
                if(promotion != null)
                {
                    if (promotion.DiscountType == DiscountType.Fixed)
                    {
                        res.ReturnValue.Price = currentPriceWithCouponCode + promotion.Value.Value;
                    }
                    else
                    if (promotion.DiscountType == DiscountType.Percentage)
                    {
                        long priceWithoutCode = 0;
                        var val = promotion.Value.Value / 100;
                        currentPriceWithCouponCode = priceWithoutCode - (val * priceWithoutCode);
                        priceWithoutCode = currentPriceWithCouponCode / (1 - val);
                        res.ReturnValue.Price = priceWithoutCode;
                    }
                    entity.UserIds.Add(userId);
                    var updateRes = await _context.UserCouponCollection.ReplaceOneAsync(_ => _.UserCouponId == entity.UserCouponId, entity);
                    if(updateRes.IsAcknowledged)
                    {
                        res.Succeeded = true;
                        res.Message = Language.GetString("AlertAndMessage_OperationDoneSuccessfully");
                    }
                    else
                    {
                        res.Succeeded = false;
                        res.Message = ConstMessages.ErrorInSaving;
                    }
                }
                else
                {
                    res.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
                }
            }else
            {
                res.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            return res;
        }
    }
}
