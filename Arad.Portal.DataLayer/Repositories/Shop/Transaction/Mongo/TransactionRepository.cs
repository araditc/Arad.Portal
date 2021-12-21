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
        private readonly TransactionContext _context;
        public TransactionRepository(IHttpContextAccessor httpContextAccessor,
            TransactionContext context)
            : base(httpContextAccessor)
        {
            _context = context;
        }
        public async Task InsertTransaction(Entities.Shop.Transaction.Transaction transaction)
        {
            await _context.Collection.InsertOneAsync(transaction);
        }
    }
}
