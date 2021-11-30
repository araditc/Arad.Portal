using Arad.Portal.DataLayer.Contracts.General.Comment;
using Arad.Portal.DataLayer.Entities.General.Comment;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Comment;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.Utilities;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Route("/Products/[action]/{id?}")]
    [Route("/Contents/[action]/{id?}")]
    public class CommentController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IPermissionView _permissionViewManager;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CommentController(ICommentRepository commentRepository, IWebHostEnvironment webHostEnvironment, 
            IPermissionView permissionView, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _commentRepository = commentRepository;
            _webHostEnvironment = webHostEnvironment;
            _permissionViewManager = permissionView;
            _configuration = configuration;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IActionResult> List()
        {
            PagedItems<CommentViewModel> result = new PagedItems<CommentViewModel>();
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            var referenceSource = Request.Path.ToString().Split("/")[1];
            ViewBag.Permissions = dicKey;

            if (referenceSource == "ProductComments")
            {
                ViewBag.Title = Language.GetString("Menu_ProductComments");
                ViewBag.Name = Language.GetString("Menu_Product");
                ViewBag.lbl = Language.GetString("tbl_ProductName");
            }
            else if (referenceSource == "ContentComments")
            {
                ViewBag.Title = Language.GetString("Menu_ContentComments");
                ViewBag.Name = Language.GetString("Menu_Content");
                ViewBag.lbl = Language.GetString("ContentTitle");
            }
            try
            {
                var queryString = "";
                if(!string.IsNullOrEmpty(Request.QueryString.ToString()))
                {
                    queryString = Request.QueryString.ToString();
                    queryString += $"&refType={(referenceSource == "ProductComments" ? ReferenceType.Product : ReferenceType.Content)}";
                }
                else
                {
                    queryString = $"?refType={(referenceSource == "ProductComments" ? ReferenceType.Product : ReferenceType.Content)}";
                }
                result = await _commentRepository.List(Request.QueryString.ToString());
                var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                ViewBag.ReferenceTypes = _commentRepository.GetAllReferenceType();

            }
            catch (Exception ex)
            {
            }
            return View(result);
        }

        public async Task<IActionResult> AddEdit(string id = "")
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var referenceSource = Request.Path.ToString().Split("/")[1];

            if(referenceSource == "ProductComments")
            {
                ViewBag.Title = Language.GetString("Menu_ProductComments");
                ViewBag.Name= Language.GetString("Menu_Product");
                ViewBag.lbl = Language.GetString("tbl_ProductName");
            }
            else if(referenceSource == "ContentComments")
            {
                ViewBag.Title = Language.GetString("Menu_ContentComments");
                ViewBag.Name = Language.GetString("Menu_Content");
                ViewBag.lbl = Language.GetString("ContentTitle");
            }

            var model = new CommentDTO();
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = await _commentRepository.CommentFetch(id);
            }
           
            ViewBag.ReferenceTypes = _commentRepository.GetAllReferenceType();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            RepositoryOperationResult opResult = await _commentRepository.Delete(id, $"delete the comment by userId={userId} and userName={userName} in date={DateTime.UtcNow.ToPersianDdate()}");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }

        
        [HttpGet]
        public async Task<IActionResult> ApproveComment(string commentId, bool isApproved)
        {
            JsonResult result;
            RepositoryOperationResult saveResult = await _commentRepository.ChangeApproval(commentId, isApproved);
            result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
            : new { Status = "Error", saveResult.Message });
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] CommentDTO dto)
        {
            JsonResult result;
            if (!ModelState.IsValid)
            {
                var errors = new List<AjaxValidationErrorModel>();
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel
                    { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                RepositoryOperationResult saveResult = await _commentRepository.Update(dto);
                result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                : new { Status = "Error", saveResult.Message });
            }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CommentDTO dto)
        {
            JsonResult result;
            if (!ModelState.IsValid)
            {
                var errors = new List<AjaxValidationErrorModel>();

                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors
                        .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
               
                RepositoryOperationResult saveResult = await _commentRepository.Add(dto);
                result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                : new { Status = "Error", saveResult.Message });
            }
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> Restore(string id)
        {
            JsonResult result;
            try
            {
                var res = await _commentRepository.Restore(id);
                if (res.Succeeded)
                {
                    result = new JsonResult(new
                    {
                        Status = "success",
                        Message = Language.GetString("AlertAndMessage_EditionDoneSuccessfully")
                    });
                }
                else
                {
                    result = new JsonResult(new
                    {
                        Status = "error",
                        Message = Language.GetString("AlertAndMessage_TryLator")
                    });
                }
            }
            catch (Exception ex)
            {
                result = new JsonResult(new
                {
                    Status = "error",
                    Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_TryLator")
                });
            }
            return result;
        }
    }
}
