using Arad.Portal.DataLayer.Contracts.General.Comment;
using Arad.Portal.DataLayer.Models.Comment;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class CommentController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        public CommentController(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;     
        }
        

        [HttpPost]
        public async Task<IActionResult> SubmitComment([FromBody] AddComment model)
        {
            JsonResult result;
            var dto = new CommentDTO();
            var loginStatus = HttpContext.User.Identity.IsAuthenticated;
            if (!loginStatus)
            {
                var url = Url.Action("Login", "Account", new { area = "" });
                result = Json(new { status = "auth", data = url });
            }
            else
            {
                //var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                //var dbUser = await _userManager.FindByIdAsync(userId);
                if (model.ReferenceId.StartsWith("p*"))
                {
                    dto.ReferenceType = DataLayer.Entities.General.Comment.ReferenceType.Product;
                }
                else if (model.ReferenceId.StartsWith("c*"))
                {
                    dto.ReferenceType = DataLayer.Entities.General.Comment.ReferenceType.Content;
                }
                dto.Content = model.Content;
                dto.ParentCommentId = model.ParentId;
                dto.ReferenceId = model.ReferenceId.Substring(2);

                RepositoryOperationResult saveResult = await _commentRepository.Add(dto);
                result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                : new { Status = "Error", saveResult.Message });
            }

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> AddLikeDisLike([FromHeader] string commentId, [FromHeader] bool isLike)
        {
            JsonResult result;
            //set cookie for related user
            var res = await _commentRepository.AddLikeDislike(commentId, isLike);
            var userId = HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value;
            HttpContext.Response.Cookies.Append($"{userId}_cmt{commentId}", isLike.ToString());

            result = Json(res.Succeeded ? new { Status = "Success", IsLike = isLike }
                : new { Status = "Error", IsLike = isLike });
            return result;
        }


     
    }
}
