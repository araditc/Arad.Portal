using Arad.Portal.DataLayer.Contracts.Shop.Transaction;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.Shop.Transaction.Mongo
{
    public class TransactionRepository : BaseRepository, ITransationRepository
    {
        public TransactionRepository(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {

        }
    }
}
