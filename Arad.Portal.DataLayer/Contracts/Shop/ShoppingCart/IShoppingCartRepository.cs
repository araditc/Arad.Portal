using Arad.Portal.DataLayer.Models.Product;
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
        Task<Result<CartItemsCount>> AddOrChangeProductToUserCart(string productId, int orderCount, List<SpecValue> specValues);

        Task<Result> InsertUserShoppingCart(string userId);

        bool AddProductToCart(string productId);

        int GetItemCountsInCart(Entities.Shop.ShoppingCart.ShoppingCart entity);

        Task<Result> ChangeProductCountInUserCart(string userId, string productId, int newCount, List<SpecValue> specValues);

        Entities.Shop.ShoppingCart.ShoppingCart FetchShoppingCart(string shoppingCartId);

        Task<Result> Reorder(string shoppingCartId);

        Task<int> LoadUserCartShopping(string userId);

        Task<Result<ShoppingCartDTO>> FetchActiveUserShoppingCart(string userId, string domainId);

        Task<Result<Entities.Shop.ShoppingCart.ShoppingCart>> FetchUserShoppingCart(string userCartId);

        Task<Result> DeleteProductFromUserShoppingCart(string userId, string productId);

        Task<Result> DeleteWholeUserShoppingCart(string userId);

        Task<Result> DeleteShoppingCartItem(string shoppingCartId, string shoppingCartDetailId);
        /// <summary>
        /// checking the price and inventory of products 
        /// </summary>
        /// <param name="userCartId"></param>
        /// <returns></returns>
        bool UserCartShoppingValidation(string userCartId);

        /// <summary>
        /// the return type Of Result is UserCartId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Result<string> FindCurrentUserShoppingCart(string userId, string domainId);

        Task<Result> SubtractUserCartOrderCntFromInventory(Entities.Shop.ShoppingCart.ShoppingCart shoppingCart);

        Task<Result> ChangePriceWithCouponCode(string shoppingCartId, string code, long oldPrice,  long newPrice);

        
    }
}
