using Arad.Portal.DataLayer.Contracts.General.Comment;
using Arad.Portal.DataLayer.Models.Comment;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Entities.General.Comment;
using Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Content.Mongo;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;

namespace Arad.Portal.DataLayer.Repositories.General.Comment.Mongo
{
    public class CommentRepository: BaseRepository, ICommentRepository
    {
        private readonly IMapper _mapper;
        private readonly CommentContext _commentContext;
        private readonly ProductContext _productContext;
        private readonly ContentContext _contentContext;
        private readonly DomainContext _domainContext;
       
        private readonly UserManager<ApplicationUser> _userManager;
        public CommentRepository(IHttpContextAccessor httpContextAccessor,
            IMapper mapper, CommentContext commentContext,
            IWebHostEnvironment env,
            ProductContext productContext, ContentContext contentContext, UserManager<ApplicationUser> userManager, DomainContext domainContext)
            :base(httpContextAccessor, env)
        {
            _mapper = mapper;
            _commentContext = commentContext;
            _productContext = productContext;
            _contentContext = contentContext;
            _userManager = userManager;
            _domainContext = domainContext;
        }

        public async Task<Result<Entities.General.Comment.Comment>> Add(CommentDTO dto)
        {
            var result = new Result<Entities.General.Comment.Comment>();
            try
            {
                var equallentModel = _mapper.Map<Entities.General.Comment.Comment>(dto);
                equallentModel.IsActive = true;
               
                
                await _commentContext.Collection.InsertOneAsync(equallentModel);
                var comment = _commentContext.Collection.Find(_ => _.CommentId == equallentModel.CommentId).FirstOrDefault();
                result.ReturnValue = comment;
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;

            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.InternalServerErrorMessage;
            }
            return result;
        }

        public async Task<Result<Entities.General.Comment.Comment>> AddLikeDislike(string commentId, bool isLike)
        {
            var result = new Result<Entities.General.Comment.Comment>();
            bool finalRes = false;
            ReplaceOneResult res;
            var cmtEntity = _commentContext.Collection.Find(_ => _.CommentId == commentId).FirstOrDefault();
            if(cmtEntity != null)
            {
                if (isLike)
                {
                    cmtEntity.LikeCount += 1;
                }
                else
                {
                    cmtEntity.DislikeCount += 1;
                }

                var commentEntityUpdate = await _commentContext.Collection.ReplaceOneAsync(_ => _.CommentId == commentId, cmtEntity);
                if (commentEntityUpdate.IsAcknowledged)
                {
                    switch (cmtEntity.ReferenceType)
                    {
                        case ReferenceType.Product:
                            var proEntity = _productContext.ProductCollection.Find(_ => _.ProductId == cmtEntity.ReferenceId).FirstOrDefault();
                            if(proEntity != null)
                            {
                                var cmt = proEntity.Comments.Where(_ => _.CommentId == commentId).FirstOrDefault();
                                if(cmt != null)
                                {
                                    if (isLike)
                                    {
                                        cmt.LikeCount += 1;
                                    }
                                    else
                                    {
                                        cmt.DislikeCount += 1;
                                    }
                                    
                                    res = await _productContext.ProductCollection
                                       .ReplaceOneAsync(_ => _.ProductId == cmtEntity.ReferenceId, proEntity);
                                    if (res.IsAcknowledged)
                                    {
                                        finalRes = await UpdateComment(cmtEntity);
                                    }
                                }
                            }
                            break;
                        case ReferenceType.Content:
                            var contentEntity = _contentContext.Collection.Find(_ => _.ContentId == cmtEntity.ReferenceId).FirstOrDefault();
                            if(contentEntity != null)
                            {
                                var cmnt = contentEntity.Comments.Where(_ => _.CommentId == commentId).FirstOrDefault();
                                if(cmnt != null)
                                {
                                    if (isLike)
                                    {
                                        cmnt.LikeCount += 1;
                                    }
                                    else
                                    {
                                        cmnt.DislikeCount += 1;
                                    }
                                   
                                    res = await _contentContext.Collection
                                       .ReplaceOneAsync(_ => _.ContentId == cmtEntity.ReferenceId, contentEntity);
                                    if (res.IsAcknowledged)
                                    {
                                        finalRes = await UpdateComment(cmtEntity);
                                    }
                                }
                            }
                           
                            break;
                        default:
                            break;
                    }
                }

                if (finalRes)
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

        private async Task<bool> UpdateComment(Entities.General.Comment.Comment comment)
        {
            var res = await _commentContext.Collection.ReplaceOneAsync(_ => _.CommentId == comment.CommentId, comment);
            return res.IsAcknowledged;
        }
        public async Task<Result> ChangeApproval(string commentId, bool isApproved)
        {
            var result = new Result();
            var entity = _commentContext.Collection.Find(_ => _.CommentId == commentId).FirstOrDefault();
            if(entity != null)
            {
                entity.IsApproved = isApproved;
                var updateResult = await _commentContext.Collection
                    .ReplaceOneAsync(_ => _.CommentId == commentId, entity);
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

        public async Task<CommentDTO> CommentFetch(string commentId)
        {
            var result = new CommentDTO();
            var entity = await _commentContext.Collection
                .Find(_ => _.CommentId == commentId && !_.IsDeleted).FirstOrDefaultAsync();

            if (entity != null)
            {
                result = _mapper.Map<CommentDTO>(entity);
            }
            return result;
        }

        public async Task<Result> Delete(string commentId, string modificationReason)
        {
            var result = new Result();
            var entity = await _commentContext.Collection.Find(_ => _.CommentId == commentId).FirstOrDefaultAsync();
            if (entity != null)
            {
                entity.IsDeleted = true;
                #region Add modification
                var mod = GetCurrentModification(modificationReason);
                entity.Modifications.Add(mod);
                #endregion
                var updateResult = await _commentContext.Collection
                    .ReplaceOneAsync(_ => _.CommentId == commentId, entity);
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

        public List<SelectListModel> GetAllReferenceType()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(ReferenceType)))
            {
                string name = Enum.GetName(typeof(ReferenceType), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            //result.Insert(0, new SelectListModel() { Text = GeneralLibrary.Utilities.Language.GetString("Choose"), Value = "-1" });
            return result;
        }

        public async Task<PagedItems<CommentViewModel>> List(string queryString)
        {
            PagedItems<CommentViewModel> result = new PagedItems<CommentViewModel>();
             var userId  = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            string langId = string.Empty;
            var dbUser = await _userManager.FindByIdAsync(userId);
            if(dbUser != null)
            {
                 langId = dbUser.Profile.DefaultLanguageId;
            }else
            {
                var domainEntity = _domainContext.Collection.Find(_ => _.DomainName == GetCurrentDomainName()).FirstOrDefault();
                langId = domainEntity.DefaultLanguageId;
            }
           
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

                ReferenceType referenceType;
                Enum.TryParse<ReferenceType>(filter["refType"].ToString(), true, out referenceType);
                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);
                long totalCount = 0;
                IQueryable<Entities.General.Comment.Comment> totalList = null;
                if(dbUser.IsSystemAccount)
                {
                    totalCount = await _commentContext.Collection.Find(_ => _.ReferenceType == referenceType).CountDocumentsAsync();
                    totalList = _commentContext.Collection.AsQueryable().Where(_ => _.ReferenceType == referenceType);
                }else
                {
                    totalCount = await _commentContext.Collection.Find(_ => _.ReferenceType == referenceType && _.AssociatedDomainId == dbUser.Domains.FirstOrDefault(a=> a.IsOwner).DomainId).CountDocumentsAsync();
                    totalList = _commentContext.Collection.AsQueryable().Where(_ => _.ReferenceType == referenceType &&  _.AssociatedDomainId == dbUser.Domains.FirstOrDefault(a => a.IsOwner).DomainId);
                }
                
                //if (!string.IsNullOrWhiteSpace(filter["userId"]))
                //{
                //    totalList = totalList.Where(_ => _.CreatorUserId == filter["userId"]);
                //}
                if (!string.IsNullOrWhiteSpace(filter["fDate"]))
                {
                    totalList = totalList
                        .Where(_ => _.CreationDate >= filter["fDate"].ToString().ToEnglishDate().ToUniversalTime());
                }
                if (!string.IsNullOrWhiteSpace(filter["tDate"]))
                {
                    totalList = totalList
                        .Where(_ => _.CreationDate <= filter["tDate"].ToString().ToEnglishDate().ToUniversalTime());
                }
                if (!string.IsNullOrWhiteSpace(filter["domainId"]))
                {
                    totalList = totalList
                       .Where(_ => _.AssociatedDomainId == filter["domainId"].ToString());
                }
                var temp = totalList.ToList();
                var list = totalList.OrderByDescending(_=>_.CreationDate).Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new CommentViewModel()
                   {
                       CommentId = _.CommentId,
                       Content = _.Content,
                      // PersianCreationDate = _.CreationDate.ToPersianDdate(),
                       CreatorUserId = _.CreatorUserId,
                       CreationDate = _.CreationDate,
                       CreatorUserName = _.CreatorUserName,
                       IsApproved = _.IsApproved,
                       LikeCount = _.LikeCount,
                       DislikeCount = _.DislikeCount,
                       ParentCommentId = _.ParentId,
                       ReferenceId = _.ReferenceId,
                       ReferenceType = _.ReferenceType,
                       IsDeleted = _.IsDeleted,
                       AssociatedDomainId = _.AssociatedDomainId
                       
                   }).ToList();
                foreach (var item in list)
                {
                    item.ParentCommentContent = _commentContext.Collection.Find(_ => _.CommentId == item.ParentCommentId).Any() ?
                        _commentContext.Collection.Find(_ => _.CommentId == item.ParentCommentId).FirstOrDefault().Content : "";
                    if(item.ReferenceType == ReferenceType.Product)
                    {
                      if(_productContext.ProductCollection.Find(_ => _.ProductId == item.ReferenceId).Any())
                        {
                            var productEntity = _productContext.ProductCollection.Find(_ => _.ProductId == item.ReferenceId).FirstOrDefault();
                            if(productEntity != null)
                            {
                                item.ReferenceTitle = productEntity.MultiLingualProperties.Any(_ => _.LanguageId == langId) ?
                                    productEntity.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == langId).Name : "";
                            }
                            
                        }
                        
                    }
                    else if(item.ReferenceType == ReferenceType.Content)
                    {
                        item.ReferenceTitle = _contentContext.Collection.Find(_ => _.ContentId == item.ReferenceId).Any() ?
                            _contentContext.Collection.Find(_ => _.ContentId == item.ReferenceId).FirstOrDefault().Title : "";
                    }
                    item.PersianCreationDate = item.CreationDate.Value.ToPersianDdate();
                    item.domainName = _domainContext.Collection.Find(_ => _.DomainId == item.AssociatedDomainId).FirstOrDefault().DomainName;

                }
                result.Items = list;
                result.CurrentPage = page;
                result.ItemsCount = totalCount;
                result.PageSize = pageSize;
                result.QueryString = queryString;

            }
            catch (Exception ex)
            {
                result.CurrentPage = 1;
                result.Items = new List<CommentViewModel>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<Result> Restore(string commentId)
        {
            var result = new Result();
            var entity = _commentContext.Collection
              .Find(_ => _.CommentId == commentId).FirstOrDefault();
            if(entity != null)
            {
                entity.IsDeleted = false;
                var updateResult = await _commentContext.Collection
                   .ReplaceOneAsync(_ => _.CommentId == commentId, entity);
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
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
           
            return result;
        }

        public async Task<Result> Update(CommentDTO dto)
        {
            var result = new Result();

            var equallentModel = _mapper.Map<Entities.General.Comment.Comment>(dto);
            var userName = _httpContextAccessor.HttpContext.User.Claims
                   .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            var userId = _httpContextAccessor.HttpContext.User.Claims
                   .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            equallentModel.CreatorUserName = userName;
            #region add modification
            var mod = GetCurrentModification($"update this Comment by its owner:'{userId}' and userName:'{userName}' in date:'{DateTime.Now.ToPersianLetDateTime()}'");
            equallentModel.Modifications.Add(mod);
            #endregion

            var updateResult = await _commentContext.Collection
                .ReplaceOneAsync(_ => _.CommentId == dto.CommentId, equallentModel);

            if (updateResult.IsAcknowledged)
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
    }
}
