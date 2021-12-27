using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Entities.Shop.Transaction;

namespace Arad.Portal.DataLayer.Contracts.Shop.Transaction
{
    public interface ITransactionRepository
    {
        Task InsertTransaction(Entities.Shop.Transaction.Transaction transaction);

        Entities.Shop.Transaction.Transaction FetchById(string transactionId);

        Entities.Shop.Transaction.Transaction FetchByIdentifierToken(string identifierToken);
    }
}
