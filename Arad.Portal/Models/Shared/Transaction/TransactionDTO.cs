using Arad.Portal.DataLayer.Entities.Shop.Transaction;

using System;
using System.Collections.Generic;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.Models.Shared.Transaction;

public class TransactionDto
{
    public TransactionDto()
    {
        Details = new();
    }
    public string TransactionId { get; init; }
    public string MainInvoiceNumber { get; init; }

    public string ShoppingCartId { get; init; }

    /// <summary>
    /// based on culture its english or persian or others on Ui
    /// </summary>
    public DateTime? PaymentDate { get; init; }

    /// <summary>
    /// creationDate based on culture its english or persian or others on Ui
    /// </summary>
    public DateTime RegisteredDate { get; init; }
    public string UserId { get; init; }

    public int OrderItemsCount { get; init; }

    public decimal FinalPriceToPay { get; init; }

    public PaymentStage PaymentStage { get; init; }

    public OrderStatus? OrderStatus { get; init; }



    public override int GetHashCode()
    {
        return HashCode.Combine(OrderStatus);
    }

    public List<TransactionDetail> Details { get; init; }
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