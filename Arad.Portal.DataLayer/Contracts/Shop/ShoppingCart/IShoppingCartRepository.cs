using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.Shop.ShoppingCart
{
    public interface IShoppingCartRepository
    {
        Task<RepositoryOperationResult> AddProductToUserCart(ShoppingCartProductDTO productDto);

        RepositoryOperationResult InsertUserShoppingCart(string userId);

        Task<RepositoryOperationResult> ChangeProductCountInUserCart(string userId, string productId, int newCount);

        Task<RepositoryOperationResult> RefreshUserShoppingCart(string userId);

        Task<RepositoryOperationResult> DeleteProductFromUserShoppingCart(string userId, string productId);

        Task<RepositoryOperationResult> DeleteWholeUserShoppingCart(string userId);

        /// <summary>
        /// the return type Of repositoryOperationResult is UserCartId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        RepositoryOperationResult<string> FindCurrentUserShoppingCart(string userId);

        
    }
}
