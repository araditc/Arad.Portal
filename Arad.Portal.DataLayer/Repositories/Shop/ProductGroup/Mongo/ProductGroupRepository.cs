using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Arad.Portal.DataLayer.Models.Promotion;
using Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo;
using System.Web;
using System.Collections.Specialized;
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Repositories.General.Currency.Mongo;
using System.Globalization;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using MongoDB.Bson;
using Microsoft.AspNetCore.Hosting;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Repositories.Shop.ProductGroup.Mongo
{
    public class ProductGroupRepository : BaseRepository, IProductGroupRepository
    {
        private readonly ProductContext _productContext;
        private readonly LanguageContext _languageContext;
        private readonly IDomainRepository _domainRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
      
        public ProductGroupRepository(ProductContext context,
            LanguageContext languageContext, 
            IHttpContextAccessor httpContextAccessor,
            IDomainRepository domainRepository,
            IWebHostEnvironment env,
            IMapper mapper, UserManager<ApplicationUser> userManager):
            base(httpContextAccessor, env)
        {
            
            _productContext = context;
            _languageContext = languageContext;
            _userManager = userManager;
            _mapper = mapper;
            _domainRepository = domainRepository;
        }

        public async Task<Result> Add(ProductGroupDTO dto)
        {
            var result = new Result();
            var equallentModel = _mapper.Map<Entities.Shop.ProductGroup.ProductGroup>(dto);

            equallentModel.CreationDate = DateTime.Now;
            equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            try
            {
                equallentModel.ProductGroupId = Guid.NewGuid().ToString();
                equallentModel.IsActive = true;
                equallentModel.GroupImage = dto.GroupImage;
                await _productContext.ProductGroupCollection.InsertOneAsync(equallentModel);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }

            return result;
        }
        
        public async Task<Result> Delete(string productGroupId, string modificationReason)
        {
            var result = new Result();
            try
            {
                var allowDeletion = false;
                var check = _productContext.ProductCollection.AsQueryable().Any(_ => _.GroupIds.Contains(productGroupId));
                if(!check)
                {
                    allowDeletion = true;
                }
                if(allowDeletion)
                {
                    var groupEntity = _productContext.ProductGroupCollection.Find(_ => _.ProductGroupId == productGroupId).FirstOrDefault();
                    if (groupEntity != null)
                    {
                        groupEntity.IsDeleted = true;

                        #region Add Modification record
                        var currentModifications = groupEntity.Modifications;
                        var mod = GetCurrentModification(modificationReason);
                        currentModifications.Add(mod);
                        groupEntity.Modifications = currentModifications;
                        #endregion

                        var updateResult = await _productContext.ProductGroupCollection.ReplaceOneAsync(_ => _.ProductGroupId == productGroupId, groupEntity);
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
                        result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
                    }
                }else
                {
                    result.Message = ConstMessages.DeletedNotAllowedForDependencies;
                }
                
            }
            catch (Exception e)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }
            return result;
        }

        public ProductGroupDTO ProductGroupFetch(string productGroupId)
        {
            var result = new ProductGroupDTO();
            var entity = _productContext.ProductGroupCollection
                .Find(_ => _.ProductGroupId == productGroupId).FirstOrDefault();
            if(entity != null)
            {
                result = _mapper.Map<ProductGroupDTO>(entity);
                //result.ProductGroupId = entity.ProductGroupId;
                //result.ParentId = entity.ParentId;
                //result.MultiLingualProperties = entity.MultiLingualProperties;
                //result.Promotion = entity.Promotion;
            }
            return result;
        }

        public List<ProductGroupDTO> GetsDirectChildren(List<string> groupsWithProduct, string domainName, string productGroupId, int? count, int skip = 0)
        {
            var result = new List<ProductGroupDTO>();
            List<Entities.Shop.ProductGroup.ProductGroup> lst;
            try
            {
                if(count != null)
                {
                    lst = _productContext.ProductGroupCollection
                    .Find(_ => _.ParentId == productGroupId && !_.IsDeleted && _.IsActive && groupsWithProduct.Contains(_.ProductGroupId)).Skip(skip).Limit(count).ToList();
                }
                else
                {
                    lst = _productContext.ProductGroupCollection
                    .Find(_ => _.ParentId == productGroupId && !_.IsDeleted && _.IsActive && groupsWithProduct.Contains(_.ProductGroupId)).ToList();
                }
                result = _mapper.Map<List<ProductGroupDTO>>(lst);
            }
            catch (Exception e)
            {
            }
            return result;
        }

        public List<ProductGroupDTO> GetsParents()
        {
            var result = new List<ProductGroupDTO>();
            var lst= _productContext.ProductGroupCollection.AsQueryable()
               .Where(_ => !_.IsDeleted && _.ParentId == null)
               .ToList();
            result = _mapper.Map<List<ProductGroupDTO>>(lst);
            return result;
        }

        public bool GroupExistance(string productGroupId)
        {
            bool result = false;
            try
            {
                var exist = _productContext.ProductGroupCollection.AsQueryable()
                    .Any(_=>_.ProductGroupId == productGroupId);
                if (exist)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
               
            }
            return result;
        }

        public async Task<Result> AddPromotionToGroup(string productGroupId,
            PromotionDTO promotionDto, string modificationReason)
        {
            var result = new Result();
            var groupEntity = await _productContext.ProductGroupCollection
                .Find(_ => _.ProductGroupId == productGroupId).FirstOrDefaultAsync();
            if(groupEntity != null)
            {
                var promotion = _mapper.Map<Entities.Shop.Promotion.Promotion>(promotionDto);
                groupEntity.Promotion = promotion;

                #region add modification
                var modification = GetCurrentModification(modificationReason);
                groupEntity.Modifications.Add(modification);
                #endregion

                try
                {
                    var updateResult = await _productContext.ProductGroupCollection
                    .ReplaceOneAsync(_ => _.ProductGroupId == productGroupId, groupEntity);

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
                catch (Exception)
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

        public bool IsUniqueUrlFriend(string urlFriend, string groupId = "")
        {
            if (string.IsNullOrWhiteSpace(groupId)) //insert
            {
                return !_productContext.ProductGroupCollection.Find(_ => _.MultiLingualProperties.Any(a => a.UrlFriend == urlFriend)).Any();
            }
            else
            { //update
                FilterDefinitionBuilder<Entities.Shop.ProductGroup.ProductGroup> groupBuilder = new();
                FilterDefinitionBuilder<MultiLingualProperty> multiLingualBuilder = new();
                FilterDefinition<MultiLingualProperty> multiLingualFilterDefinition = multiLingualBuilder.Eq("UrlFriend", urlFriend);

                return !_productContext.ProductGroupCollection.Find(groupBuilder.ElemMatch("MultiLingualProperties", multiLingualFilterDefinition) & groupBuilder.Ne("ProductGroupId", groupId)).Any();
            }
        }

        public async Task<PagedItems<ProductGroupViewModel>> List(string queryString)
        {
            PagedItems<ProductGroupViewModel> result = new PagedItems<ProductGroupViewModel>();
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
                    var lan = _languageContext.Collection.Find(_ => _.IsDefault).FirstOrDefault();
                    filter.Set("LanguageId", lan.LanguageId);
                }
                if(string.IsNullOrWhiteSpace(filter["Name"]))
                {
                    filter.Set("Name", "");
                }
                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);
                var langId = filter["LanguageId"].ToString();
                var filterName = filter["Name"].ToString();
                long totalCount = await _productContext.ProductGroupCollection.Find(c => true).CountDocumentsAsync();
                var lst = _productContext.ProductGroupCollection.AsQueryable()
                   .Where(_=>_.MultiLingualProperties.Any(a => a.Name.Contains(filterName)) && _.MultiLingualProperties.Any(_=> _.LanguageId == langId))
                   .Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new ProductGroupViewModel()
                   {
                       ProductGroupId = _.ProductGroupId,
                       ParentId = _.ParentId,
                       IsDeleted = _.IsDeleted,
                       MultiLingualProperty = _.MultiLingualProperties.Any(a => a.LanguageId == langId) ?
                         _.MultiLingualProperties.First(a => a.LanguageId == langId) : _.MultiLingualProperties.First()
                   }).ToList();
                result.CurrentPage = page;
                result.Items = lst;
                result.ItemsCount = totalCount;
                result.PageSize = pageSize;
                result.QueryString = queryString;
            }
            catch (Exception ex)
            {
                result.CurrentPage = 1;
                result.Items = new List<ProductGroupViewModel>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<Result> Update(ProductGroupDTO dto)
        {
            var result = new Result();
            var entity = _productContext.ProductGroupCollection
                .Find(_ => _.ProductGroupId == dto.ProductGroupId).FirstOrDefault();
            if(entity != null)
            {
                entity.ParentId = dto.ParentId;
               

                #region Add Modification
                var currentModifications = entity.Modifications;
                var mod = GetCurrentModification(dto.ModificationReason);
                currentModifications.Add(mod);
                #endregion
               
                entity.Modifications = currentModifications;
                entity.MultiLingualProperties = dto.MultiLingualProperties;
                entity.GroupImage = dto.GroupImage;
               
                var updateResult = await _productContext.ProductGroupCollection
               .ReplaceOneAsync(_ => _.ProductGroupId == dto.ProductGroupId, entity);
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
            }else
            {
                result.Succeeded = false;
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            return result;
        }

        public async Task<Result> Restore(string id)
        {
            var result = new Result();
            var entity = _productContext.ProductGroupCollection
              .Find(_ => _.ProductGroupId == id).FirstOrDefault();
            if(entity != null)
            {
                entity.IsDeleted = false;
                var updateResult = await _productContext.ProductGroupCollection
                   .ReplaceOneAsync(_ => _.ProductGroupId == id, entity);
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
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
           
            return result;
        }
       
        public Entities.Shop.ProductGroup.ProductGroup FetchWholeProductGroup(string productGroupId)
        {
            Entities.Shop.ProductGroup.ProductGroup result;
            result = _productContext.ProductGroupCollection.Find(_ => _.ProductGroupId == productGroupId).FirstOrDefault();
            return result;
        }

        public async Task<List<SelectListModel>> GetAlActiveProductGroup(string langId, string currentUserId)
        {
            var result = new List<SelectListModel>();
            var dbUser = await _userManager.FindByIdAsync(currentUserId);
            if(dbUser != null)
            {
                if (dbUser.IsSystemAccount)
                {
                    result = _productContext.ProductGroupCollection.Find(_ => _.IsActive && !_.IsDeleted)
                   .Project(_ => new SelectListModel()
                   {
                       Text = _.MultiLingualProperties.Where(a => a.LanguageId == langId).Count() != 0 ?
                            _.MultiLingualProperties.FirstOrDefault(a => a.LanguageId == langId).Name : "",
                       Value = _.ProductGroupId
                   }).ToList();
                }
                else
                {
                    result = _productContext.ProductGroupCollection
                   .Find(_ => dbUser.Profile.Access.AccessibleProductGroupIds.Contains(_.ProductGroupId))
                   .Project(_ => new SelectListModel()
                   {
                       Text = _.MultiLingualProperties.Where(a => a.LanguageId == langId).Count() != 0 ?
                            _.MultiLingualProperties.FirstOrDefault(a => a.LanguageId == langId).Name : "",
                       Value = _.ProductGroupId
                   }).ToList();
                }
            }
           
           
            result.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "-1" });
            return result;
        }

        public ProductGroupDTO FetchBySlug(string slug, string domainName)
        {
            var result = new ProductGroupDTO();
            var urlFriend = $"{domainName}/category/{slug}";
            var groupEntity = _productContext.ProductGroupCollection
                .Find(_ => _.MultiLingualProperties.Any(a => a.UrlFriend == urlFriend)).FirstOrDefault();
            if(groupEntity != null)
            {
                result = _mapper.Map<ProductGroupDTO>(groupEntity);
            }
          
            return result;
        }

        //public CommonViewModel FetchByCode(long groupCode)
        //{
        //    var result = new CommonViewModel();

        //    var productEntity = _productContext.ProductGroupCollection
        //        .Find(_ => _.GroupCode == groupCode).FirstOrDefault();

        //    result.Groups = GetsDirectChildren(productEntity.ProductGroupId, 4, 0);
        //    //result.NGrpCountToSkip = 4;
        //    //result.NGrpCountToTake = 4;

        //    result.ProductList = GetLatestProductInThisGroup(productEntity.ProductGroupId, 4, 0);
        //    //result.NEntityCntToSkip = 4;
        //    //result.NEntityCntToTake = 4;
        //    return result;
        //}

        //public ProductGroupDTO FetchByCode(long groupCode)
        //{
        //    var result = new ProductGroupDTO();
        //    var groupEntity = _productContext.ProductGroupCollection
        //        .Find(_ => _.GroupCode == groupCode).FirstOrDefault();
        //    result = _mapper.Map<ProductGroupDTO>(groupEntity);
        //    return result;
        //}

        public List<ProductOutputDTO> GetLatestProductInThisGroup(string domainName, string productGroupId, int? count, int skip = 0)
        {
            var result = new List<ProductOutputDTO>();
            var list = _productContext.ProductCollection
                 .Find(_ => _.GroupIds.Contains(productGroupId) && !_.IsDeleted).SortByDescending(_ => _.CreationDate).Skip(skip).Limit(count).ToList();

            result = _mapper.Map<List<ProductOutputDTO>>(list);
            foreach (var pro in result)
            {
                var r = Helpers.Utilities.ConvertPopularityRate(pro.TotalScore ?? 0, pro.ScoredCount ?? 0);
                pro.LikeRate = r.LikeRate;
                pro.DisikeRate = r.DisikeRate;
                pro.HalfLikeRate = r.HalfLikeRate;
            }
            return result;
        }

        public string FetchByCode(string slugOrCode)
        {
            string result = "";
            long codeNumber;
            var entity = new Entities.Shop.ProductGroup.ProductGroup();
            if (long.TryParse(slugOrCode, out codeNumber))
            {
                entity = _productContext.ProductGroupCollection
                 .Find(_ => _.GroupCode == codeNumber).FirstOrDefault();
            }
            else
            {
                entity = _productContext.ProductGroupCollection
                    .Find(_ => _.MultiLingualProperties.Any(a => a.UrlFriend == $"/group/{slugOrCode}")).FirstOrDefault();
            }

            if(entity != null)
            {
                result = entity.ProductGroupId;
            }
            return result;
        }

        public async Task<long> GetDircetChildrenCount(string domainName,string productGroupId, List<string> groupListWithProducts)
        {
            var currentDomain = _domainRepository.FetchByName(domainName, false);

            var count = await  _productContext.ProductGroupCollection
                    .Find(_ => _.ParentId == productGroupId
                           && groupListWithProducts.Contains(_.ProductGroupId)).CountDocumentsAsync();
            return count;
        }

        public long GetProductsInGroupCounts(string domainName, string productGroupId)
        {
            long count;
            var currentDomain = _domainRepository.FetchByName(domainName, false);
            var defDomain = _domainRepository.GetDefaultDomain();
            if(currentDomain.ReturnValue.DomainId != defDomain.ReturnValue.DomainId)
            {
                count = _productContext.ProductCollection
                    .Find(_ =>_.GroupIds.Contains(productGroupId) &&  _.AssociatedDomainId == currentDomain.ReturnValue.DomainId).CountDocuments();
            }else
            {
                count = _productContext.ProductCollection
                    .Find(_ => _.GroupIds.Contains(productGroupId) && (_.AssociatedDomainId == currentDomain.ReturnValue.DomainId || _.IsPublishedOnMainDomain)).CountDocuments();
            }
            return count;
        }

        public async Task<List<string>> AllGroupIdsWhichEndInProducts(string domainName)
        {
            var currentDomain = _domainRepository.FetchByName(domainName, false);
            var defDomain = _domainRepository.GetDefaultDomain();

            FilterDefinitionBuilder<Entities.Shop.Product.Product> builder = new();
            FilterDefinition<Entities.Shop.Product.Product> filterDef;
            //var filter = new BsonDocument("AssociatedDomainId", currentDomain.ReturnValue.DomainId);
            filterDef = builder.Eq(nameof(Entities.Shop.Product.Product.AssociatedDomainId), currentDomain.ReturnValue.DomainId);
            if (currentDomain.ReturnValue.DomainId == defDomain.ReturnValue.DomainId)
            {
                filterDef = builder.And(filterDef, builder.Eq(nameof(Entities.Shop.Product.Product.IsPublishedOnMainDomain), true));
            }
            //TODO : doesnt work
            var cursor = await _productContext.ProductCollection.DistinctAsync<string>("GroupIds", filterDef);
            var lst = await cursor.ToListAsync();
            
            //lst.Add("85b8f7f8-3381-44bf-9429-3c3dcd5988c2");
            //lst.Add("9c27b561-3b95-4e73-a955-0571937d767a");
            //lst.Add("01ec3195-5498-4e6f-a4c0-9d14ddeced63");
            //lst.Add("7ca0c66f-2a89-45b9-8d77-a3c7592e7788");
            var finalList = new List<string>();
            foreach (var groupId in lst)
            {
                finalList.Add(groupId);
                var entity = _productContext.ProductGroupCollection
                             .Find(_ => _.ProductGroupId == groupId).FirstOrDefault();
                if(entity != null)
                {
                    var tmp = entity;
                    while (tmp.ParentId != null)
                    {
                        finalList.Add(tmp.ParentId);
                        tmp = _productContext.ProductGroupCollection
                             .Find(_ => _.ProductGroupId == tmp.ParentId).Any() ?
                             _productContext.ProductGroupCollection
                             .Find(_ => _.ProductGroupId == tmp.ParentId).FirstOrDefault() : null;
                    }
                }
               
            }
            //testing part
            var groups = _productContext.ProductGroupCollection.AsQueryable().Select(_ => _.ProductGroupId).ToList();
            finalList = groups;
            return finalList;
        }

        public List<SelectListModel> GetSubGroups(string languageId,string domainId, string parentId = null)
        {
            var result = new List<SelectListModel>();
            if(!string.IsNullOrWhiteSpace(parentId))
            {
                result = _productContext.ProductGroupCollection
                     .Find(_ => _.IsActive && !_.IsDeleted).Project(_ => new SelectListModel()
                     {
                         Value = _.ProductGroupId,
                         Text = _.MultiLingualProperties.Any(_=>_.LanguageId == languageId) ?
                          _.MultiLingualProperties.FirstOrDefault(_=>_.LanguageId == languageId).Name : _.MultiLingualProperties.FirstOrDefault().Name

                     }).ToList();
            }
            else
            {
                result = _productContext.ProductGroupCollection
                   .Find(_ => _.IsActive && !_.IsDeleted && _.ParentId == parentId).Project(_ => new SelectListModel()
                   {
                       Value = _.ProductGroupId,
                       Text = _.MultiLingualProperties.Any(_ => _.LanguageId == languageId) ? 
                        _.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == languageId).Name : _.MultiLingualProperties.FirstOrDefault().Name

                   }).ToList();
            }
            return result;
        }
        public async Task<ModelOutputFilter> GetFilterList(string languageId, string domainId, string groupId = null)
        {
            var model = new ModelOutputFilter();
            model.Filters = new List<DynamicFilter>();
             FilterDefinitionBuilder<Entities.Shop.Product.Product> _builder = new();
             List<List<string>> ProductSpecifications = new List<List<string>>();
            List<string> commonSpecIds = new List<string>();
         
            FilterDefinition<Entities.Shop.Product.Product> filterDef;

            filterDef = _builder.Eq(nameof(Entities.Shop.Product.Product.IsActive), true);
            filterDef &= _builder.Eq(nameof(Entities.Shop.Product.Product.IsDeleted), false);
            //test
            //filterDef &= _builder.Eq(nameof(Entities.Shop.Product.Product.AssociatedDomainId), domainId);
            if (groupId != null)
            {
                filterDef &= _builder.AnyIn(nameof(Entities.Shop.Product.Product.GroupIds), new List<string>() { groupId });
            }

            using IAsyncCursor<Entities.Shop.Product.Product> cursor = await _productContext.ProductCollection.WithReadPreference(ReadPreference.Secondary)
                .FindAsync(filterDef, new()
                {
                    AllowDiskUse = true,
                    BatchSize = 2000,
                    Projection =
                   Builders<Entities.Shop.Product.Product>.Projection.Expression(x =>
                   new Entities.Shop.Product.Product { Specifications = x.Specifications, ProductId = x.ProductId, GroupIds = x.GroupIds })
                });

            while (await cursor.MoveNextAsync())
            {
                var specList = cursor.Current.Select(_ => _.Specifications.Select(a => a.SpecificationId).ToList()).ToList();
                ProductSpecifications.AddRange(specList);
            }

            //find common specs between list intersection of alls
            try
            {
                commonSpecIds = ProductSpecifications
                      .Skip(1)
                      .Aggregate(
                          new HashSet<string>(ProductSpecifications.First()),
                          (h, e) => { h.IntersectWith(e); return h; }
                      ).ToList();

                foreach (var id in commonSpecIds)
                {
                    var spec = _productContext.SpecificationCollection.Find(_ => _.ProductSpecificationId == id).FirstOrDefault();
                    var obj = new DynamicFilter()
                    {
                        ControlType = spec.ControlType,
                        SpecificationId = spec.ProductSpecificationId,
                        SpecificationName = spec.SpecificationNameValues.Any(_=>_.LanguageId == languageId) ? spec.SpecificationNameValues.FirstOrDefault(_=>_.LanguageId == languageId).Name : "",
                        PossibleValues = spec.ControlType == Entities.Shop.ProductSpecification.ControlType.CheckBoxList ?
                             spec.SpecificationNameValues.FirstOrDefault(_ => _.LanguageId == languageId).NameValues : new List<string>()
                    };
                    model.Filters.Add(obj);
                }
                if (groupId != null)
                {
                    model.MinPrice = _productContext.ProductCollection.AsQueryable()
                               .Where(_ => _.IsActive && !_.IsDeleted && _.AssociatedDomainId == domainId && _.GroupIds.Contains(groupId))
                               .Select(_ => _.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow))).Any() ?
                               _productContext.ProductCollection.AsQueryable()
                               .Where(_ => _.IsActive && !_.IsDeleted && _.AssociatedDomainId == domainId && _.GroupIds.Contains(groupId))
                               .Select(_ => _.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow))).OrderBy(_=>_.PriceValue).FirstOrDefault().PriceValue : 0;

                    model.MaxPrice = _productContext.ProductCollection.AsQueryable()
                              .Where(_ => _.IsActive && !_.IsDeleted && _.AssociatedDomainId == domainId && _.GroupIds.Contains(groupId))
                              .Select(_ => _.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow))).Any() ?
                              _productContext.ProductCollection.AsQueryable()
                              .Where(_ => _.IsActive && !_.IsDeleted && _.AssociatedDomainId == domainId && _.GroupIds.Contains(groupId))
                              .Select(_ => _.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow))).OrderByDescending(_ => _.PriceValue).FirstOrDefault().PriceValue : 0;

                }else
                {
                    model.MinPrice = _productContext.ProductCollection.AsQueryable()
                              .Where(_ => _.IsActive && !_.IsDeleted && _.AssociatedDomainId == domainId )
                              .Select(_ => _.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow))).Any() ?
                              _productContext.ProductCollection.AsQueryable()
                              .Where(_ => _.IsActive && !_.IsDeleted && _.AssociatedDomainId == domainId )
                              .Select(_ => _.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow))).OrderBy(_ => _.PriceValue).FirstOrDefault().PriceValue : 0;

                    model.MaxPrice = _productContext.ProductCollection.AsQueryable()
                              .Where(_ => _.IsActive && !_.IsDeleted && _.AssociatedDomainId == domainId )
                              .Select(_ => _.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow))).Any() ?
                              _productContext.ProductCollection.AsQueryable()
                              .Where(_ => _.IsActive && !_.IsDeleted && _.AssociatedDomainId == domainId )
                              .Select(_ => _.Prices.First(a => a.IsActive && a.StartDate <= DateTime.UtcNow && (a.EndDate == null || a.EndDate >= DateTime.UtcNow))).OrderByDescending(_ => _.PriceValue).FirstOrDefault().PriceValue : 0;

                }
                //for testing 
                //model.MinPrice = 1000;
                //model.MaxPrice = 10000;
                var gap = (model.MaxPrice - model.MinPrice);
                model.Step = Convert.ToInt32(gap / 10);

            }
            catch (Exception)
            {
                commonSpecIds = new List<string>();
            }

            return model;
        }

        public List<SelectListModel> GetProductSortingType()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(ProductSortingType)))
            {
                string name = Enum.GetName(typeof(ProductSortingType), i);
                var obj = new SelectListModel()
                {
                    Text = name.GetDescription(),
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel() { Text = Language.GetString("Choose"), Value = "-1" });
            return result;
        }
    }
}
