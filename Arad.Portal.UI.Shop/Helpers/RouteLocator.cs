using Arad.Portal.DataLayer.Contracts.General.Content;
using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Contracts.General.Menu;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Helpers
{
    public class RouteLocator : IRouteLocator
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IProductGroupRepository _groupRepository;
        private readonly IProductRepository _productRepository;
        private readonly IContentCategoryRepository _contentCategoryRepository;
        private readonly IContentRepository _contentRepository;

        public RouteLocator(IMenuRepository menuRepository, IProductGroupRepository groupRepository,
                            IProductRepository productRepository, IContentCategoryRepository contentCategoryRepository,
                            IContentRepository contentRepository)
        {
            _menuRepository = menuRepository;
            _groupRepository = groupRepository;
            _productRepository = productRepository;
            _contentCategoryRepository = contentCategoryRepository;
            _contentRepository = contentRepository;
        }
        public string GetContentCategoryId(string categoryCode)
        {
            throw new NotImplementedException();
        }

        public string GetContentId(string contentCode)
        {
            throw new NotImplementedException();
        }

        public string GetProductGroupId(string groupCode)
        {
            throw new NotImplementedException();
        }

        public string GetProductId(string productCode)
        {
            throw new NotImplementedException();
        }
    }
}
