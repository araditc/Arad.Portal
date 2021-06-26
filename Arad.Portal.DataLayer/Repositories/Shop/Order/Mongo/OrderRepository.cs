using Arad.Portal.DataLayer.Contracts.Shop.Order;
using Arad.Portal.DataLayer.Models.Order;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Shop.ShoppingCart.Mongo;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Specialized;
using System.Web;
using Arad.Portal.DataLayer.Entities.Shop.Order;

namespace Arad.Portal.DataLayer.Repositories.Shop.Order.Mongo
{
    public class OrderRepository : BaseRepository, IOrderRepository
    {
        private readonly OrderContext _context;
        private readonly ShoppingCartContext _shoppingCartContext;
        private readonly IMapper _mapper;
        public OrderRepository(OrderContext context,
                               ShoppingCartContext shoppingCartContext,
                               IHttpContextAccessor httpContextAccessor,
                               IMapper mapper) : base(httpContextAccessor)
        {
            _context = context;
            _mapper = mapper ;
            _shoppingCartContext = shoppingCartContext;
        }

        public async Task<OrderDTO> FetchOrder(string orderId)
        {
            var result = new OrderDTO();
            var OrderEntity = await  _context.Collection
                .Find(_ => _.OrderId == orderId).FirstOrDefaultAsync();
            if(OrderEntity != null)
            {
                result = _mapper.Map<OrderDTO>(OrderEntity);
            }
            return result;
        }

        public async Task<PagedItems<OrderDTO>> GetAllOrdersOfUser(string queryString)
        {
            PagedItems<OrderDTO> result = new PagedItems<OrderDTO>()
            {
              CurrentPage = 1,
              ItemsCount = 0,
              PageSize = 10,
              QueryString = queryString
            };
            try
            {
                NameValueCollection filter = HttpUtility.ParseQueryString(queryString);
                //id = userId
                if (!string.IsNullOrWhiteSpace(filter["id"]))
                {
                    if (string.IsNullOrWhiteSpace(filter["CurrentPage"]))
                    {
                        filter.Set("CurrentPage", "1");
                    }

                    if (string.IsNullOrWhiteSpace(filter["PageSize"]))
                    {
                        filter.Set("PageSize", "10");
                    }

                    var page = Convert.ToInt32(filter["CurrentPage"]);
                    var pageSize = Convert.ToInt32(filter["PageSize"]);
                    var userId = Convert.ToInt32(filter["id"]);

                    long totalCount = await _context.Collection.Find(c => true).CountDocumentsAsync();
                    var list = _context.Collection.AsQueryable().Skip((page - 1) * pageSize)
                       .Take(pageSize).ToList();

                    result.Items = _mapper.Map<List<OrderDTO>>(list);
                    result.CurrentPage = page;
                    result.ItemsCount = totalCount;
                    result.PageSize = pageSize;
                    result.QueryString = queryString;
                }
            }
            catch (Exception ex)
            {
               
            }
            return result;
        }

        public async Task<List<OrderDetail>> GetOrderDetails(string orderId)
        {
            var result = new List<OrderDetail>();
            var orderEntity = await _context.Collection
                .Find(_ => _.OrderId == orderId).FirstOrDefaultAsync();
            if(orderEntity != null)
            {
                result = orderEntity.Details;
            }
            return result;
        }

        public async Task<RepositoryOperationResult> InsertNewOrder(string currentUserId,
            OrderStatus orderStatus )
        {
            var result = new RepositoryOperationResult();
            //currentUserId should be equall to return value of GetUserId()
            decimal totalOrderAmount = 0;
            var userCartEntity = await _shoppingCartContext.Collection
                .Find(_ => _.CreatorUserId == currentUserId && !_.IsDeleted).FirstOrDefaultAsync();
            if(userCartEntity != null)
            {
                var orderObject = new Entities.Shop.Order.Order()
                {
                    OrderId = Guid.NewGuid().ToString(),
                    CreationDate = DateTime.UtcNow,
                    CreatorUserId = currentUserId,
                    CreatorUserName = GetUserName(),
                    OrderDate = DateTime.UtcNow,
                    OrderUserId = currentUserId,
                    OrderStatus = orderStatus
                };

                foreach (var item in userCartEntity.Details)
                {
                    var amount = (item.PricePerUnit - item.DiscountPricePerUnit) * item.OrderCount;
                    orderObject.Details.Add(new OrderDetail()
                    {
                        DiscountPerUnit = item.DiscountPricePerUnit,
                        OrderCount = item.OrderCount,
                        PricePerUnit = item.PricePerUnit,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        CurrencyId = item.CurrencyId,
                        LanguageId = item.LanguageId,
                        LanguageName = item.languageName,
                        ProductAmountToPay = amount
                    });
                    totalOrderAmount += amount;
                }
                orderObject.TotalAmountOfOrder = totalOrderAmount;
                try
                {
                    await _context.Collection.InsertOneAsync(orderObject);
                    result.Message = ConstMessages.SuccessfullyDone;
                    result.Succeeded = true;
                   
                }
                catch (Exception)
                {
                    result.Message = ConstMessages.GeneralError;
                }
            }
            else
            {
                result.Message = ConstMessages.ObjectNotFound;
            }
            return result;
        }
    }
}
