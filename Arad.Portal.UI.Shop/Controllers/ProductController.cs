using Arad.Portal.DataLayer.Contracts.General.Comment;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Comment;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILanguageRepository _lanRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductController(IProductRepository productRepository, IHttpContextAccessor accessor,
            UserManager<ApplicationUser> userManager,
            ILanguageRepository lanRepository, IDomainRepository domainRepository, ICommentRepository commentRepository)
        {
            _productRepository = productRepository;
            _accessor = accessor;
            _lanRepository = lanRepository;
            _domainRepository = domainRepository;
            _commentRepository = commentRepository;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Route("{language}/product/{**slug}")]
        public IActionResult Details(long slug)
        {
            var domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            var domainEntity = _domainRepository.FetchByName(domainName);
            var lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
            var entity = _productRepository.FetchByCode(slug, domainEntity.ReturnValue);
            var lanId = _lanRepository.FetchBySymbol(lanIcon);
            ViewBag.CurCurrencyId = domainEntity.ReturnValue.DefaultCurrencyId;
            ViewBag.CurLanguageId = lanId;
            return View(entity);
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

    }
}

