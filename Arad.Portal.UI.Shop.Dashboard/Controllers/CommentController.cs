﻿using Arad.Portal.DataLayer.Contracts.General.Comment;
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
using Microsoft.AspNetCore.Authorization;
using Arad.Portal.DataLayer.Contracts.General.Domain;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    

    public class CommentController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDomainRepository _domainRepository;
        public CommentController(ICommentRepository commentRepository, IWebHostEnvironment webHostEnvironment, 
            IConfiguration configuration,IDomainRepository domainrepository,
            UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _commentRepository = commentRepository;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _userManager = userManager;
            _domainRepository = domainrepository;
            _httpContextAccessor = httpContextAccessor;
        }

        [Route("/ProductComments/List")]
        [Route("/ContentComments/List")]
        public async Task<IActionResult> List()
        {
            PagedItems<CommentViewModel> result = new PagedItems<CommentViewModel>();
            var referenceSource = Request.Path.ToString().Split("/")[1];
           

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
                result = await _commentRepository.List(queryString);
                var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userDb = await _userManager.FindByIdAsync(currentUserId);
                ViewBag.IsSysAcc = userDb.IsSystemAccount;
                if(userDb.IsSystemAccount)
                {
                    ViewBag.Domains = _domainRepository.GetAllActiveDomains();
                }
                ViewBag.ReferenceTypes = _commentRepository.GetAllReferenceType();

            }
            catch (Exception ex)
            {
            }
            return View(result);
        }


        [Route("/ProductComments/AddEdit/{id?}")]
        [Route("/ContentComments/AddEdit/{id?}")]
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
        [Route("/ProductComments/Delete/{id?}")]
        [Route("/ContentComments/Delete/{id?}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            Result opResult = await _commentRepository.Delete(id, $"delete the comment by userId={userId} and userName={userName} in date={DateTime.UtcNow.ToPersianDdate()}");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }



        [Route("/ProductComments/ApproveComment")]
        [Route("/ContentComments/ApproveComment")]
        public async Task<IActionResult> ApproveComment(string commentId, bool isApproved = true)
        {
            JsonResult result;
            Result saveResult = await _commentRepository.ChangeApproval(commentId, isApproved);
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
                Result saveResult = await _commentRepository.Update(dto);
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
               
                Result<Comment> saveResult = await _commentRepository.Add(dto);
                result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                : new { Status = "Error", saveResult.Message });
            }
            return result;
        }

        [HttpGet]
        [Route("/ProductComments/Restore/{id?}")]
        [Route("/ContentComments/Restore/{id?}")]
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
