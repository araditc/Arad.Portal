using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class CommentSectionViewComponent : ViewComponent
    {
        private readonly IProductRepository _productRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILanguageRepository _lanRepository;
        private readonly IDomainRepository _domainRepository;
        public CommentSectionViewComponent(IProductRepository proRepository,IHttpContextAccessor accessor,
            IDomainRepository domainRepository, ILanguageRepository languageRepository)
        {
            _productRepository = proRepository;
            _lanRepository = languageRepository;
            _domainRepository = domainRepository;
            _accessor = accessor;
        }

        public  IViewComponentResult InvokeAsync(CommentVM comment)
        {
            return View(comment);
        }
    }
}
