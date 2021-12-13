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
        Task<Result> AddProductToUserCart(ShoppingCartProductDTO productDto);

        Result InsertUserShoppingCart(string userId);



        Task<Result> ChangeProductCountInUserCart(string userId, string productId, int newCount);

        Task<Result<ShoppingCartDTO>> FetchActiveUserShoppingCart(string userId);

        Task<Result> DeleteProductFromUserShoppingCart(string userId, string productId);

        Task<Result> DeleteWholeUserShoppingCart(string userId);

        /// <summary>
        /// the return type Of repositoryOperationResult is UserCartId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Result<string> FindCurrentUserShoppingCart(string userId);

        
    }
}
