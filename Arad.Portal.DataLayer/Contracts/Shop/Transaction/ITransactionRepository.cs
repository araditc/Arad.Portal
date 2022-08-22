using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Entities.Shop.Transaction;
using Arad.Portal.DataLayer.Models.Shared;
using static Arad.Portal.DataLayer.Models.Shared.Enums;
using Arad.Portal.DataLayer.Models.Transaction;

namespace Arad.Portal.DataLayer.Contracts.Shop.Transaction
{
    public interface ITransactionRepository
    {
        Task InsertTransaction(Entities.Shop.Transaction.Transaction transaction);

        Entities.Shop.Transaction.Transaction FetchById(string transactionId);

        Entities.Shop.Transaction.Transaction FetchByIdentifierToken(string identifierToken);

        TransactionItems CreateTransactionItemsModel(string transactionId);

        List<TransactionDTO> GetUserOrderHistory(string userId);

        Task<PagedItems<TransactionGlanceAdminView>> GetSiteAdminTransactionList(string queryString);

        bool IsRefNumberUnique(string referenceNumber, PspType pspType);

        Task UpdateTransaction(Entities.Shop.Transaction.Transaction transaction);

        Task<bool> ChangeOrderStatus(string transactionId, OrderStatus status);

        Task<Result> RollBackPayingTransaction();

        List<SelectListModel> GetAllOrderStatusType();

        List<SelectListModel> GetAllPaymentStageList();

        Task<Result> AddToDownloadLimitation(Entities.Shop.Transaction.Transaction transaction);
    }
}
