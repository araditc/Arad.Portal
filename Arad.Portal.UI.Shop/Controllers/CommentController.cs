using Arad.Portal.DataLayer.Contracts.General.Comment;
using Arad.Portal.DataLayer.Models.Comment;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.General.Content.Mongo;
using Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Microsoft.AspNetCore.Authorization;
using Arad.Portal.DataLayer.Contracts.General.Domain;

namespace Arad.Portal.UI.Shop.Controllers
{
   [Authorize(Policy = "Role")]
    public class CommentController : BaseController
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ProductContext _productContext;
        private readonly ContentContext _contentContex;
        private readonly IProductRepository _productRepository;
       
        public CommentController(ICommentRepository commentRepository,
                                 ContentContext contentContext,
                                 ProductContext productContext,
                                 IDomainRepository domainRepository,
                                 IProductRepository productRepository,
                                 IHttpContextAccessor accessor):base(accessor, domainRepository)
        {
            _commentRepository = commentRepository;
            _productContext = productContext;
            _contentContex = contentContext;
            _productRepository = productRepository;
        }
        

        [HttpPost]
        public async Task<IActionResult> SubmitComment([FromBody] AddComment model)
        {
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            if (User.Identity.IsAuthenticated)
            {
                JsonResult result;
                var dto = new CommentDTO();
                bool isAddAllow = false;
                dto.CommentId = Guid.NewGuid().ToString();
                dto.CreationDate = DateTime.Now;
                string refId = model.ReferenceId;
                dto.CreatorUserId = HttpContext.User.Claims
                        .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                dto.CreatorUserName = HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
                var loginStatus = HttpContext.User.Identity.IsAuthenticated;
                if (!loginStatus)
                {
                    var url = Url.Action("Login", "Account", new { area = "" });
                    result = Json(new { status = "auth", data = url });
                }
                else
                {
                    dto.Content = model.Content;
                    dto.ParentCommentId = model.ParentId;
                    dto.ReferenceId = model.ReferenceId.Substring(2);
                    if (refId.StartsWith("p*"))
                    {
                        dto.ReferenceType = DataLayer.Entities.General.Comment.ReferenceType.Product;
                        var pro = _productContext.ProductCollection.Find(_ => _.ProductId == dto.ReferenceId).FirstOrDefault();
                        pro.Comments.Add(new DataLayer.Entities.General.Comment.Comment()
                        {
                            CommentId = dto.CommentId,
                            ReferenceId = dto.ReferenceId,
                            ReferenceType = dto.ReferenceType,
                            ParentId = dto.ParentCommentId,
                            Content = dto.Content,
                            CreationDate = dto.CreationDate,
                            CreatorUserId = dto.CreatorUserId,
                            CreatorUserName = dto.CreatorUserName,
                            IsActive = true
                        });
                        var updateResult = await _productContext.ProductCollection
                       .ReplaceOneAsync(_ => _.ProductId == dto.ReferenceId, pro);
                        if (updateResult.IsAcknowledged)
                        {
                            isAddAllow = true;
                        }

                    }
                    else if (refId.StartsWith("c*"))
                    {
                        dto.ReferenceType = DataLayer.Entities.General.Comment.ReferenceType.Content;
                        var content = _contentContex.Collection.Find(_ => _.ContentId == dto.ReferenceId).FirstOrDefault();
                        content.Comments.Add(new DataLayer.Entities.General.Comment.Comment()
                        {
                            CommentId = dto.CommentId,
                            ReferenceId = dto.ReferenceId,
                            ReferenceType = dto.ReferenceType,
                            ParentId = dto.ParentCommentId,
                            Content = dto.Content,
                            CreationDate = dto.CreationDate,
                            CreatorUserId = dto.CreatorUserId,
                            CreatorUserName = dto.CreatorUserName,
                            IsActive = true
                        });
                        var updateResult = await _contentContex.Collection
                       .ReplaceOneAsync(_ => _.ContentId == dto.ReferenceId, content);
                        if (updateResult.IsAcknowledged)
                        {
                            isAddAllow = true;
                        }
                    }

                    if (isAddAllow)
                    {
                        Result<DataLayer.Entities.General.Comment.Comment> saveResult = await _commentRepository.Add(dto);
                        result = Json(saveResult.Succeeded ? new
                        {
                            Status = "Success",
                            Message = Language.GetString("AlertAndMessage_SubmitComment"),
                            username = HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Name).Value,
                            date = Arad.Portal.GeneralLibrary.Utilities.DateHelper.ToPersianDdate(saveResult.ReturnValue.CreationDate),
                            commentid = saveResult.ReturnValue.CommentId,
                            content = saveResult.ReturnValue.Content,
                            refid = saveResult.ReturnValue.ReferenceId
                        }
                    : new { Status = "Error", saveResult.Message });
                    }
                    else
                    {
                        result = Json(new { Status = "Error", Message = Language.GetString("AlertAndMessage_InsertError") });
                    }
                }

                return result;
            }else
            {
                return Redirect($"/{lanIcon}/Account/Login");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RatingProduct([FromBody] RateProduct model)
        {
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            if (User.Identity.IsAuthenticated)
            {

                var userId = HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value;
                string prevRate = "";
                var userProductRateCookieName = $"{userId}_p{model.ProductId}";
                if (!model.IsNew)//the user has rated before
                {
                    prevRate = HttpContext.Request.Cookies[userProductRateCookieName];
                }
                int preS = !string.IsNullOrWhiteSpace(prevRate) ? Convert.ToInt32(prevRate) : 0;

                var res = await _productRepository.RateProduct(model.ProductId, model.Score,
                        model.IsNew, preS);
                if (res.Succeeded)
                {
                    //set its related cookie
                    return
                        Json(new
                        {
                            status = "Succeed",
                            like = res.ReturnValue.LikeRate,
                            dislike = res.ReturnValue.DisikeRate,
                            half = res.ReturnValue.HalfLikeRate
                        });
                }
                else
                {
                    return Json(new { status = "error" });
                }
            }
            return Redirect($"{lanIcon}/Account/Login");
        }

        [HttpPost]
        public async Task<IActionResult> AddLikeDisLike([FromQuery] string commentId, [FromQuery] bool isLike)
        {
            JsonResult result;
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            if (User.Identity.IsAuthenticated)
            {
                //set cookie for related user
                var res = await _commentRepository.AddLikeDislike(commentId, isLike);
                var userId = HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value;
                HttpContext.Response.Cookies.Append($"{userId}_cmt{commentId}", isLike.ToString());

                result = Json(res.Succeeded ? new { Status = "Success", IsLike = isLike }
                    : new { Status = "Error", IsLike = isLike });
                return result;
            }
                return Redirect($"{lanIcon}/Account/Login");
        }


     
    }
}
