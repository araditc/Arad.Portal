using Arad.Portal.DataLayer.Entities.Shop.Transaction;

namespace Arad.Portal.Models.Shared.Transaction;

public class OrderChangeStatus
{
    public string TransactionId { get; set; }

    public OrderStatus OrderStatus { get; set; }
}