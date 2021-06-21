using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.Shop.ProductGroup.Mongo
{
    public class ProductGroupRepository : BaseRepository, IProductGroupRepository
    {
        private readonly ProductGroupContext _context;
        private readonly ProductContext _productContext;
      
        public ProductGroupRepository(ProductGroupContext context,
            ProductContext productContext,
            IHttpContextAccessor httpContextAccessor):
            base(httpContextAccessor)
        {
            _context = context;
            _productContext = productContext;
        }
        public bool Add(ProductGroupDTO productGroup)
        {
            throw new NotImplementedException();
        }

        public Task<List<ProductGroupDTO>> AllProductGroups()
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryOperationResult> Delete(string id, string modificationReason)
        {
            throw new NotImplementedException();
        }

        public ProductGroupDTO GetById(string productGroupId)
        {
            throw new NotImplementedException();
        }

        public List<ProductGroupDTO> GetsDirectChildren(string productGroupId)
        {
            throw new NotImplementedException();
        }

        public List<ProductGroupDTO> GetsParents()
        {
            throw new NotImplementedException();
        }

        public bool GroupExistance(string productGroupId)
        {
            throw new NotImplementedException();
        }

        public List<ProductGroupDTO> List(string parentId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(ProductGroupDTO dto)
        {
            throw new NotImplementedException();
        }
    }
}
