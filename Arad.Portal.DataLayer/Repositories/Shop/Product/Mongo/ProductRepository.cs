using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo
{
    public class ProductRepository : BaseRepository, IProductRepository
    {
        public ProductRepository(IHttpContextAccessor  httpContextAccessor)
            : base(httpContextAccessor)
        {

        }
    }
}
