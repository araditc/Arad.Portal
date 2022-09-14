using Arad.Portal.DataLayer.Entities.Shop.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Models.Transaction
{
    public class TransactionDTO
    {
        public TransactionDTO()
        {
            Details = new();
        }
        public string TransactionId { get; set; }
        public string MainInvoiceNumber { get; set; }

        public string ShoppingCartId { get; set; }

        /// <summary>
        /// based on culture its english or persian or others on Ui
        /// </summary>
        public DateTime? PaymentDate { get; set; }

        /// <summary>
        /// creationDate based on culture its english or persian or others on Ui
        /// </summary>
        public DateTime RegisteredDate { get; set; }
        public string UserId { get; set; }

        public int OrderItemsCount { get; set; }

        public decimal FinalPriceToPay { get; set; }

        public PaymentStage PaymentStage { get; set; }

        public OrderStatus? OrderStatus { get; set; }

        public List<TransactionDetail> Details { get; set; }
    }
    

    public class TransactionDetail
    {
        public TransactionDetail()
        {
            Products = new();
        }
        public string SellerId { get; set; }

        public string SellerUserName { get; set; }

        public List<ProductOrderDetail> Products { get; set; }

        public decimal TotalDetailsAmountToPayWithShipping { get; set; }
    }

    public class ProductOrderDetail
    {
        public string ProductId { get; set; }

        public long ProductCode { get; set; }

        public string ProductName { get; set; }

        public int OrderCount { get; set; }

        public bool IsDownloadable { get; set; }

        /// <summary>
        /// price with discount
        /// </summary>
        public decimal PriceWithDiscountPerUnit { get; set; }
        
    }
}
