using Arad.Portal.DataLayer.Entities.Shop.Transaction;

using System;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.Models.Shared.Transaction;

public class TransactionGlanceAdminView
{
    public string UserId { get; init; }

    public string TransactionId { get; init; }

    public string MainInvoiceNumber { get; init; }

    public string UserName { get; set; }

    public string UserFullName { get; init; }

    public DateTime RegisteredDate { get; set; }

    public DateTime PaymentDate { get; init; }

    public decimal TotalAmount { get; init; }

    public int OrderItemCount { get; set; }

    public PaymentStage PaymentStage { get; init; }

    public OrderStatus? OrderStatus { get; init; }
}