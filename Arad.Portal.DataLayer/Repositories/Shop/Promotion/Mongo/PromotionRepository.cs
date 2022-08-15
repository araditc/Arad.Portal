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

namespace Arad.Portal.DataLayer.Repositories.Shop.Promotion.Mongo
{
    public class PromotionRepository : BaseRepository, IPromotionRepository
    {
        private readonly PromotionContext _context;
        private readonly ProductContext _productContext;
        private readonly IMapper _mapper;
        public PromotionRepository(IHttpContextAccessor httpContextAccessor,
                                   PromotionContext promotionContext,
                                   IWebHostEnvironment env,
                                   IMapper mapper,
                                   ProductContext productContext) : base(httpContextAccessor, env)
        {
            _context = promotionContext;
            _productContext = productContext;
            _mapper = mapper;
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
                    result.Message = ConstMessages.ObjectNotFound;
                }
            }
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
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
                    result.Message = ConstMessages.ObjectNotFound;
                }
            }
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
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
                result.Message = ConstMessages.ObjectNotFound;
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
                    list = list.Where(_ => _.Title.Contains(title)).Skip((page - 1) * pageSize)
                     .Take(pageSize).ToList();
                }
                if (!string.IsNullOrWhiteSpace(promotionTypeId))
                {
                    list = list.Where(_ => _.PromotionType == (PromotionType)Convert.ToInt32(promotionTypeId)).Skip((page - 1) * pageSize)
                      .Take(pageSize).ToList();
                }
                if (!string.IsNullOrWhiteSpace(discountTypeId))
                {
                    list = list.Where(_ => _.DiscountType == (DiscountType)Convert.ToInt32(discountTypeId)).Skip((page - 1) * pageSize)
                      .Take(pageSize).ToList();
                }
                if (!string.IsNullOrWhiteSpace(fromDate))
                {
                    list = list.Where(_ => _.SDate >= DateHelper.ToEnglishDate(fromDate).ToUniversalTime()).Skip((page - 1) * pageSize)
                      .Take(pageSize).ToList();
                }
                if (!string.IsNullOrWhiteSpace(toDate))
                {
                    list = list.Where(_ => _.EDate <= DateHelper.ToEnglishDate(toDate).ToUniversalTime()).Skip((page - 1) * pageSize)
                      .Take(pageSize).ToList();
                }
                if (!string.IsNullOrWhiteSpace(productId))
                {
                    list = list.Where(_ => _.Infoes.Any(_=>_.AffectedProductId == productId)).Skip((page - 1) * pageSize)
                     .Take(pageSize).ToList();
                }else if (!string.IsNullOrWhiteSpace(groupId))
                {
                    list = list.Where(_ => _.Infoes.Any(_=> _.AffectedProductGroupId == groupId)).Skip((page - 1) * pageSize)
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
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }

        public List<SelectListModel> GetActivePromotionsOfCurrentUser(string userId, PromotionType type)
        {
            var alltypes = _context.Collection.Find(_ => _.CreatorUserId == userId && _.SDate <= DateTime.UtcNow &&
            (_.EDate >= DateTime.UtcNow || _.EDate == null) && _.IsActive);
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
            
            result.PromotionTypeId = (int)entity.PromotionType;
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

        public List<SelectListModel> GetAllDiscountType()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(DiscountType)))
            {
                string name = Enum.GetName(typeof(DiscountType), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
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
    }
}
