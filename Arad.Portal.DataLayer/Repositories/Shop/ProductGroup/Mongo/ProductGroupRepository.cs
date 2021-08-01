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

namespace Arad.Portal.DataLayer.Repositories.Shop.ProductGroup.Mongo
{
    public class ProductGroupRepository : BaseRepository, IProductGroupRepository
    {
        private readonly ProductContext _productContext;
        private readonly IMapper _mapper;
      
        public ProductGroupRepository(ProductContext context,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper):
            base(httpContextAccessor)
        {
            
            _productContext = context;
            _mapper = mapper;
        }

        public async Task<RepositoryOperationResult> Add(ProductGroupDTO dto)
        {
            var result = new RepositoryOperationResult();
            var equallentModel = _mapper.Map<Entities.Shop.ProductGroup.ProductGroup>(dto);
            equallentModel.CreationDate = DateTime.Now;
            equallentModel.CreatorUserId = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            equallentModel.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            equallentModel.ProductGroupId = Guid.NewGuid().ToString();
            try
            {
                equallentModel.ProductGroupId = Guid.NewGuid().ToString();
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

        public async Task<List<ProductGroupDTO>> AllProductGroups()
        {
            var result = new List<ProductGroupDTO>();
            var lst = await _productContext.ProductGroupCollection.AsQueryable().Where(_ => !_.IsDeleted).ToListAsync();
            result = _mapper.Map<List<ProductGroupDTO>>(lst);
            return result;
        }

        public async Task<RepositoryOperationResult> Delete(string productGroupId, string modificationReason)
        {
            var result = new RepositoryOperationResult();
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
                    var groupEntity = _productContext.ProductGroupCollection.Find(_ => _.ProductGroupId == productGroupId).First();
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
                        result.Message = ConstMessages.ObjectNotFound;
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
            var entity = _productContext.ProductGroupCollection.Find(_ => _.ProductGroupId == productGroupId);
            result = _mapper.Map<ProductGroupDTO>(entity);
            return result;
        }

        public List<ProductGroupDTO> GetsDirectChildren(string productGroupId)
        {
            var result = new List<ProductGroupDTO>();
            try
            {
                var lst = _productContext.ProductGroupCollection.AsQueryable()
                    .Where(_ => _.ParentId == productGroupId && !_.IsDeleted).ToList();

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

        public async Task<RepositoryOperationResult> AddPromotionToGroup(string productGroupId,
            PromotionDTO promotionDto, string modificationReason)
        {
            var result = new RepositoryOperationResult();
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
                result.Message = ConstMessages.ObjectNotFound;

            }
            return result;
        }
        //public Task<RepositoryOperationResult> Update(ProductGroupDTO dto)
        //{
        //    try
        //    {
        //        var exist = new List<ProductGroup>();

        //        if (dto.MultiLingualProperties.Count > 0)
        //        {
        //            exist = _context.Collection.AsQueryable()
        //                .Where(_ => m.UrlFriendGroup == group.UrlFriendGroup && m.IsDeleted != 1).ToList();

        //            if (exist.Any())
        //            {
        //                if (exist.Count > 1)
        //                {
        //                    return false;
        //                }

        //                if (exist.Count == 1)
        //                {
        //                    if (exist[0].Id != group.Id)
        //                    {
        //                        return false;
        //                    }
        //                }
        //            }

        //            group.UrlFriendGroup = group.UrlFriendGroup.Replace(" ", "-");
        //        }

        //        var filter = Builders<ProductGroup>.Filter.Eq("_id", new BsonObjectId(ObjectId.Parse(group.Id)));
        //        var update = Builders<ProductGroup>.Update.Set("Title", group.Title)
        //            .Set("MenuId", group.MenuId)
        //            .Set("UrlFriendGroup", group.UrlFriendGroup);

        //        var resultUpdate = await _context.collection.UpdateOneAsync(filter, update);
        //        var ack = resultUpdate.IsAcknowledged;

        //        if (ack)
        //        {
        //            //update باکس های صفحه اول شاپ

        //            var setting = _settingsContext.collection.AsQueryable().FirstOrDefault();

        //            if (setting != null)
        //            {
        //                foreach (var box in setting.BoxHomePics)
        //                {
        //                    if (box.ProductGroupId == group.Id)
        //                    {
        //                        box.Title = group.Title;
        //                    }
        //                }

        //                var resultSett =
        //                    await _settingsContext.collection.ReplaceOneAsync(c => c.Id == setting.Id, setting);
        //            }

        //            return true;
        //        }

        //        return false;
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Error(e.ToString());
        //        return false;
        //    }
        //}


    }
}
