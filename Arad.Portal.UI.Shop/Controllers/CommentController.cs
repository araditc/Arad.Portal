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
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Contracts.General.User;
using DocumentFormat.OpenXml.Math;
using Arad.Portal.DataLayer.Contracts.General.Content;
using Arad.Portal.DataLayer.Models.Content;
using DocumentFormat.OpenXml.Office2013.PowerPoint.Roaming;

namespace Arad.Portal.UI.Shop.Controllers
{
  
    public class CommentController : BaseController
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IDomainRepository _domainRepository;
       
        private readonly UserManager<ApplicationUser> _userManager;
       
        public CommentController(ICommentRepository commentRepository,
                                 IContentRepository contentRepository,
                                 IProductRepository productRepository,
                                 IDomainRepository domainRepository,
                                 UserManager<ApplicationUser> userManager,
                                 IUserRepository userRepository,
                                 IHttpContextAccessor accessor):base(accessor, domainRepository)
        {
            _commentRepository = commentRepository;
            _contentRepository = contentRepository;
            _productRepository = productRepository;
            _userManager = userManager;
            _userRepository = userRepository;
            _domainRepository = domainRepository;
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
                    dto.ParentId = model.ParentId;
                    dto.ReferenceId = model.ReferenceId.Substring(2);
                    if (refId.StartsWith("p*"))
                    {
                        dto.ReferenceType = DataLayer.Entities.General.Comment.ReferenceType.Product;
                        var pro = _productRepository.FetchProductForComment(dto.ReferenceId);
                        pro.Comments.Add(new DataLayer.Entities.General.Comment.Comment()
                        {
                            CommentId = dto.CommentId,
                            ReferenceId = dto.ReferenceId,
                            ReferenceType = dto.ReferenceType,
                            ParentId = dto.ParentId,
                            Content = dto.Content,
                            CreationDate = dto.CreationDate,
                            CreatorUserId = dto.CreatorUserId,
                            CreatorUserName = dto.CreatorUserName,
                            IsActive = true
                        });
                        var res = await _productRepository.UpdateProductEntity(pro);
                        if (res.Succeeded)
                        {
                            isAddAllow = true;
                        }

                    }
                    else if (refId.StartsWith("c*"))
                    {
                        dto.ReferenceType = DataLayer.Entities.General.Comment.ReferenceType.Content;
                        var content = await _contentRepository.ContentSelect(dto.ReferenceId);
                        content.Comments.Add(new DataLayer.Entities.General.Comment.Comment()
                        {
                            CommentId = dto.CommentId,
                            ReferenceId = dto.ReferenceId,
                            ReferenceType = dto.ReferenceType,
                            ParentId = dto.ParentId,
                            Content = dto.Content,
                            CreationDate = dto.CreationDate,
                            CreatorUserId = dto.CreatorUserId,
                            CreatorUserName = dto.CreatorUserName,
                            IsActive = true
                        });
                        var res = await _contentRepository.UpdateContentEntity(content);
                        if (res.Succeeded)
                        {
                            isAddAllow = true;
                        }
                    }

                    if (isAddAllow)
                    {
                        var domainName = HttpContext.Request.Host.ToString();
                        var domainEntity = _domainRepository.FetchByName(domainName, false).ReturnValue;
                        dto.AssociatedDomainId = domainEntity.DomainId;
                        Result<DataLayer.Entities.General.Comment.Comment> saveResult = await _commentRepository.Add(dto);
                        result = Json(saveResult.Succeeded ? new
                        {
                            Status = "Success",
                            Message = Language.GetString("AlertAndMessage_SubmitComment"),
                            username = User.GetUserName(),
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

        [HttpGet]
        public  IActionResult AddToFavList(string code, string name)
        {
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            string entityId = string.Empty;
            string url = string.Empty;
            var domainName = base.DomainName;
            var domainResult = _domainRepository.FetchByName(domainName, false);
            if (User != null && User.Identity.IsAuthenticated)
            {
                var userId = User.GetUserId();
               
               
                FavoriteType type;
                if(name.ToLower() == "product")
                {
                    type = FavoriteType.Product;
                    entityId = _productRepository.FetchIdByCode(Convert.ToInt64(code));
                    url = $"/product/{code}";
                }
                else
                {
                    type = FavoriteType.Content;
                    entityId = _contentRepository.FetchIdByCode(Convert.ToInt64(code));
                    url = $"/blog/{code}";
                }
                var finalRes = _userRepository.AddToUserFavoriteList(userId, type, entityId, url, domainResult.ReturnValue.DomainId);
                if (finalRes.Succeeded)
                {
                    return
                        Json(new
                        {
                            status = "Succeed"
                        });
                }
                else
                {
                    return Json(new { status = "error", Message = Language.GetString(ConstMessages.InternalServerErrorMessage) });
                }

            }
            return Redirect($"{lanIcon}/Account/Login?returnUrl={lanIcon}/Product/{code}");
        }



        [HttpPost]
        public async Task<IActionResult> Rate([FromBody] RateModel model)
        {
            var userId = User.GetUserId();
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            string prevRate = "";
            long code = 0;
            string cookieName = "";
            Result<EntityRate> finalRes;
            if (User != null && User.Identity.IsAuthenticated)
            {
                try
                {
                    if (model.IsContent)
                    {
                        cookieName = $"{userId}_cc{model.EntityId}";
                    }
                    else
                    {
                        cookieName = $"{userId}_pp{model.EntityId}";
                    }

                    if (!model.IsNew)//the user has rated before
                    {
                        prevRate = HttpContext.Request.Cookies[cookieName];
                    }
                    int preS = !string.IsNullOrWhiteSpace(prevRate) ? Convert.ToInt32(prevRate) : 0;


                    if (model.IsContent)
                    {
                        finalRes = await _contentRepository.RateContent(model.EntityId, model.Score,
                           model.IsNew, preS);

                        code = _contentRepository.GetContentCode(model.EntityId);
                    }
                    else
                    {
                        finalRes = await _productRepository.RateProduct(model.EntityId, model.Score,
                               model.IsNew, preS);

                        code = _productRepository.GetProductCode(model.EntityId);
                    }
                    if (finalRes.Succeeded)
                    {
                        //set its related cookie
                        CookieOptions option = new CookieOptions();
                        option.Expires = DateTime.Now.AddYears(1);
                        Response.Cookies.Append(cookieName, model.Score.ToString(), option);


                        return
                            Json(new
                            {
                                status = "Succeed",
                                like = finalRes.ReturnValue.LikeRate,
                                dislike = finalRes.ReturnValue.DisikeRate,
                                half = finalRes.ReturnValue.HalfLikeRate
                            });
                    }
                    else
                    {
                        return Json(new { status = "error", message = Language.GetString(ConstMessages.InternalServerErrorMessage) });
                    }
                }
                catch (Exception)
                {
                    return Json(new { status = "error", message = Language.GetString(ConstMessages.InternalServerErrorMessage) });
                }
            }else if (model.IsContent)
            {
                return Redirect($"{lanIcon}/Account/Login?returnUrl={lanIcon}/blog/{code}");
            }
            else
            {
                return Redirect($"{lanIcon}/Account/Login?returnUrl={lanIcon}/Product/{code}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> RemoveFromFav([FromQuery] string pkey)
        {
            var res = await _userRepository.RemoveToUserFavouriteList(pkey);
            if (res.Succeeded)
            {
                return
                    Json(new
                    {
                        status = "Succeed"
                    });
            }
            else
            {
                return Json(new { status = "error", Message = Language.GetString(ConstMessages.InternalServerErrorMessage) });
            }

        }


        //[HttpPost]
        //public async Task<IActionResult> RatingProduct([FromBody] RateProduct model)
        //{
        //    var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
        //    var productCode = _productRepository.GetProductCode(model.ProductId);
        //    if (User != null && User.Identity.IsAuthenticated)
        //    {

        //        var userId = User.GetUserId();
        //        string prevRate = "";
        //        var userProductRateCookieName = $"{userId}_p{model.ProductId}";
        //        if (!model.IsNew)//the user has rated before
        //        {
        //            prevRate = HttpContext.Request.Cookies[userProductRateCookieName];
        //        }
        //        int preS = !string.IsNullOrWhiteSpace(prevRate) ? Convert.ToInt32(prevRate) : 0;

        //        var res = await _productRepository.RateProduct(model.ProductId, model.Score,
        //                model.IsNew, preS);
        //        if (res.Succeeded)
        //        {
        //            //set its related cookie
        //            return
        //                Json(new
        //                {
        //                    status = "Succeed",
        //                    like = res.ReturnValue.LikeRate,
        //                    dislike = res.ReturnValue.DisikeRate,
        //                    half = res.ReturnValue.HalfLikeRate
        //                });
        //        }
        //        else
        //        {
        //            return Json(new { status = "error" });
        //        }
        //    }
        //    return Redirect($"{lanIcon}/Account/Login?returnUrl={lanIcon}/Product/{productCode}");
        //}

        [HttpPost]
        public async Task<IActionResult> AddLikeDisLike([FromQuery] string commentId, [FromQuery] bool isLike)
        {
            JsonResult result;
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            if (User.Identity.IsAuthenticated)
            {
                //set cookie for related user
                var res = await _commentRepository.AddLikeDislike(commentId, isLike);
                var userId = User.GetUserId();
                HttpContext.Response.Cookies.Append($"{userId}_cmt{commentId}", isLike.ToString());

                result = Json(res.Succeeded ? new { Status = "Success", IsLike = isLike }
                    : new { Status = "Error", IsLike = isLike });
                return result;
            }
                return Redirect($"{lanIcon}/Account/Login");
        }


     
    }
}
