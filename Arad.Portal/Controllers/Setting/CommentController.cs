using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Comment;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Content;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using AutoMapper;
using Arad.Portal.DataLayer.Entities.General.Comment;
using System.Threading;
using Arad.Portal.Models.UI;
using Arad.Portal.Models.Shared.Content;
using Arad.Portal.Controllers.Base;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared.Comment;

namespace Arad.Portal.Controllers.Setting;

public class CommentController(
    ICommentRepository commentRepository,
    IContentRepository contentRepository,
    IProductRepository productRepository,
    IDomainRepository domainRepository,
    IUserRepository userRepository,
    ILanguageRepository languageRepository,
    IHttpContextAccessor accessor,
    IMapper mapper,
    ControllerHelper controllerHelper)
    : BaseController(accessor, domainRepository, languageRepository)
{

    [HttpPost]
    public async ValueTask<IActionResult> SubmitComment([FromBody] AddComment model, CancellationToken cancellationToken)
    {
        string? lanIcon = HttpContext.Request.Path.Value?.Split("/")[1];
        if (User.Identity is { IsAuthenticated: true })
        {
            JsonResult result;
            CommentDto dto = new() { Id = Guid.NewGuid().ToString(), CreationDate = DateTime.Now };

            string refId = model.ReferenceId;
            dto.CreatorUserId = controllerHelper.GetCurrentUserId();
            dto.CreatorUserName = controllerHelper.GetCurrentUser(cancellationToken).Result.UserName;
            bool loginStatus = HttpContext.User.Identity is { IsAuthenticated: true };
            if (!loginStatus)
            {
                string? url = Url.Action("Login", "Account", new { area = "" });
                result = Json(new { status = "auth", data = url });
            }
            else
            {
                dto.Content = model.Content;
                dto.ParentId = model.ParentId;
                dto.ReferenceId = model.ReferenceId;
                if (refId.StartsWith("p*"))
                {
                    dto.ReferenceType = ReferenceType.Product;
                }
                else if (refId.StartsWith("c*"))
                {
                    dto.ReferenceType = ReferenceType.Content;
                }


                Domain domainEntity = controllerHelper.GetCurrentUserDomain();
                dto.AssociatedDomainId = domainEntity.Id;
                Comment commentModel = mapper.Map<Comment>(dto);
                Result<Comment> saveResult = await commentRepository.InsertAsync(commentModel, cancellationToken);
                saveResult.ReturnValue = commentModel;
                result = Json(saveResult.Succeeded ? new
                                                     {
                                                         Status = "Success",
                                                         Message = UtilityLanguage.GetString("AlertAndMessage_SubmitComment"),
                                                         username = User.GetUserName(),
                                                         date = saveResult.ReturnValue.CreationDate.ToPersianDdate(),
                                                         commentid = saveResult.ReturnValue.Id,
                                                         content = saveResult.ReturnValue.Content,
                                                         refid = saveResult.ReturnValue.ReferenceId
                                                     } : new { Status = "Error", saveResult.Message });


            }

            return result;
        }
        else
        {
            return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + $"/{lanIcon}/Account/Login");
        }
    }

    [HttpGet]
    public async ValueTask<IActionResult> AddToFavList(string code, string name, CancellationToken cancellationToken)
    {
        string lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
        string entityId = "";
        string url = "";
        string domainName = DomainName;
        Result<Domain> domainResult = controllerHelper.FetchDomainByName(domainName, false);
        if (User != null && User.Identity.IsAuthenticated)
        {
            string userId = User.GetUserId();


            FavoriteType type;
            if (name.ToLower() == "product")
            {
                type = FavoriteType.Product;
                entityId = controllerHelper.FetchIdByCode(Convert.ToInt64(code));
                url = $"/product/{code}";
            }
            else
            {
                type = FavoriteType.Content;
                entityId = controllerHelper.FetchIdByCode(Convert.ToInt64(code));
                url = $"/blog/{code}";
            }
            Result finalRes = new();
            try
            {
                UserFavorites obj = new()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        CreationDate = DateTime.Now,
                                        CreatorUserId = userId,
                                        AssociatedDomainId = domainResult.ReturnValue.Id,
                                        EntityId = entityId,
                                        FavoriteType = type,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Url = url
                                    };
                Task<ApplicationUser> user = controllerHelper.GetCurrentUser(cancellationToken);
                user.Result.Favorites ??= [];
                user.Result.Favorites.Add(obj);
                await userRepository.UpdateAsync(c => c.Id == user.Result.Id, m => m.Favorites, user.Result.Favorites, cancellationToken);
                finalRes.Succeeded = true;
            }
            catch (Exception)
            {
                finalRes.Succeeded = false;
            }
            if (finalRes.Succeeded)
            {
                return
                    Json(new
                         {
                             status = "Succeed"
                         });
            }

            return Json(new { status = "error", Message = UtilityLanguage.GetString(ConstMessages.InternalServerErrorMessage) });

        }
        return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + $"/{lanIcon}/Account/Login?returnUrl={lanIcon}/Product/{code}");
    }

    [HttpPost]
    public async ValueTask<IActionResult> Rate([FromBody] RateModel model, CancellationToken cancellationToken)
    {
        string userId = User.GetUserId();
        string lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
        string prevRate = "";
        long code = 0;
        Result<DataLayer.Entities.General.Content.Content> finalRes = new();
        Result<DataLayer.Entities.Shop.Product.Product> finalRes2 = new();
        if (User != null && User.Identity.IsAuthenticated)
        {
            try
            {
                string cookieName = "";

                cookieName = model.IsContent ? $"{userId}_cc{model.EntityId}" : $"{userId}_pp{model.EntityId}";

                if (!model.IsNew)//the user has rated before
                {
                    prevRate = HttpContext.Request.Cookies[cookieName];
                }
                int preS = !string.IsNullOrWhiteSpace(prevRate) ? Convert.ToInt32(prevRate) : 0;


                if (model.IsContent)
                {
                    finalRes = await contentRepository.UpdateAsync(c => c.Id == model.EntityId, m => m.ScoredCount, model.Score, cancellationToken);

                    code = Convert.ToInt64((await contentRepository.FirstOrDefaultAsync(c => c.Id == model.EntityId, cancellationToken)).ContentCode);
                }
                else
                {
                    finalRes2 = await productRepository.UpdateAsync(c => c.Id == model.EntityId, m => m.ScoredCount, model.Score, cancellationToken);

                    code = Convert.ToInt64((await productRepository.FirstOrDefaultAsync(c => c.Id == model.EntityId, cancellationToken)).ProductCode);
                }
                if (finalRes.Succeeded)
                {
                    //set its related cookie
                    CookieOptions option = new() { Expires = DateTime.Now.AddYears(1) };
                    Response.Cookies.Append(cookieName, model.Score.ToString(), option);


                    return
                        Json(new
                             {
                                 //status = "Succeed",
                                 //like = finalRes.ReturnValue.LikeRate,
                                 //dislike = finalRes.ReturnValue.DisikeRate,
                                 //half = finalRes.ReturnValue.HalfLikeRate
                             });
                }
                else
                {
                    return Json(new { status = "error", message = UtilityLanguage.GetString(ConstMessages.InternalServerErrorMessage) });
                }
            }
            catch (Exception)
            {
                return Json(new { status = "error", message = UtilityLanguage.GetString(ConstMessages.InternalServerErrorMessage) });
            }
        }
        else if (model.IsContent)
        {
            return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + $"/{lanIcon}/Account/Login?returnUrl={lanIcon}/blog/{code}");
        }
        else
        {
            return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + $"/{lanIcon}/Account/Login?returnUrl={lanIcon}/Product/{code}");
        }
    }

    [HttpGet]
    public async ValueTask<IActionResult> RemoveFromFav([FromQuery] string pkey, CancellationToken cancellationToken)
    {
        Result res = new();
        Task<ApplicationUser> userDb = controllerHelper.GetCurrentUser(cancellationToken);
        UserFavorites userFavorite = userDb.Result.Favorites.Find(c => c.Id == pkey);
        userDb.Result.Favorites.Remove(userFavorite);
        Result<ApplicationUser> delRes = await userRepository.UpdateAsync(u => u.Id == userDb.Result.Id, m => m.Favorites, userDb.Result.Favorites, cancellationToken);
        if (delRes.Succeeded)
        {
            res.Succeeded = true;
        }
        if (res.Succeeded)
        {
            return Json(new
                        {
                            status = "Succeed"
                        });
        }
        else
        {
            return Json(new { status = "error", Message = UtilityLanguage.GetString(ConstMessages.InternalServerErrorMessage) });
        }
    }

    [HttpPost]
    public async ValueTask<IActionResult> AddLikeDisLike([FromQuery] string commentId, [FromQuery] bool isLike)
    {
        string lanIcon = HttpContext.Request.Path.Value.Split("/")[1];

        if (!User.Identity.IsAuthenticated)
        {
            return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + $"/{lanIcon}/Account/Login");
        }

        //set cookie for related user
        // var res = await _commentRepository.AddLikeDislike(commentId, isLike);
        string userId = User.GetUserId();
        HttpContext.Response.Cookies.Append($"{userId}_cmt{commentId}", isLike.ToString());

        JsonResult result = Json(new { Status = "Success", IsLike = isLike });
        return result;
    }

}