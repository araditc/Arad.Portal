using Arad.Portal.DataLayer.Entities.Shop.Order;
using Arad.Portal.DataLayer.Models.Order;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.Shop.Order
{
    public interface IOrderRepository
    {
        Task<RepositoryOperationResult> InsertNewOrder(string userId, OrderStatus orderStatus);

        Task<List<OrderDetail>> GetOrderDetails(string orderId);

        Task<OrderDTO> FetchOrder(string orderId);

        Task<PagedItems<OrderDTO>> GetAllOrdersOfUser(string queryString);
    }
}
