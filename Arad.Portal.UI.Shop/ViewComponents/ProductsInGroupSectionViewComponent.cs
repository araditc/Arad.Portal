using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class ProductsInGroupSectionViewComponent : ViewComponent
    {
        private readonly IProductGroupRepository _groupRepository;
        private readonly IHttpContextAccessor _accessor;
        public ProductsInGroupSectionViewComponent(IProductGroupRepository groupRepository, IHttpContextAccessor accessor)
        {
            _accessor = accessor;
            _groupRepository = groupRepository;
        }

        public IViewComponentResult Invoke()
        {


            return View();
        }
    }
}
