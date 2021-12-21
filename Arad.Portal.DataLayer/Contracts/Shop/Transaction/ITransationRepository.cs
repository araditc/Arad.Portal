using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Entities.Shop.Transaction;

namespace Arad.Portal.DataLayer.Contracts.Shop.Transaction
{
    public interface ITransationRepository
    {
        Task InsertTransaction(Entities.Shop.Transaction.Transaction transaction);
    }
}
