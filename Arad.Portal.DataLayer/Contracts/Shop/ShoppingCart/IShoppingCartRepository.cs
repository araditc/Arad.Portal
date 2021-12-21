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
        Task<Result<CartItemsCount>> AddOrChangeProductToUserCart(string productId, int orderCount);

        Task<Result> InsertUserShoppingCart(string userId);

        bool AddProductToCart(string productId);

        Task<Result> ChangeProductCountInUserCart(string userId, string productId, int newCount);

        Task<Result<ShoppingCartDTO>> FetchActiveUserShoppingCart(string userId, string domainId);

        Task<Result<Entities.Shop.ShoppingCart.ShoppingCart>> FetchUserShoppingCart(string userCartId);

        Task<Result> DeleteProductFromUserShoppingCart(string userId, string productId);

        Task<Result> DeleteWholeUserShoppingCart(string userId);

        /// <summary>
        /// the return type Of rResult is UserCartId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Result<string> FindCurrentUserShoppingCart(string userId, string domainId);

        
    }
}
