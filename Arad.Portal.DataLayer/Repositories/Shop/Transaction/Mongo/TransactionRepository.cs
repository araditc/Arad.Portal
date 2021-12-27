using Arad.Portal.DataLayer.Contracts.Shop.Transaction;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.Shop.Transaction.Mongo
{
    public class TransactionRepository : BaseRepository, ITransactionRepository
    {
        private readonly TransactionContext _context;
        public TransactionRepository(IHttpContextAccessor httpContextAccessor,
            TransactionContext context)
            : base(httpContextAccessor)
        {
            _context = context;
        }

        public Entities.Shop.Transaction.Transaction FetchById(string transactionId)
        {
            var entity = _context.Collection
                .Find(_ => _.TransactionId == transactionId).FirstOrDefault();

            return entity;
        }

        public Entities.Shop.Transaction.Transaction FetchByIdentifierToken(string identifierToken)
        {
            var entity = _context.Collection
                .Find(_ => _.BasicData.InternalTokenIdentifier == identifierToken).FirstOrDefault();
            return entity;
        }

        public async Task InsertTransaction(Entities.Shop.Transaction.Transaction transaction)
        {
            await _context.Collection.InsertOneAsync(transaction);
        }
    }
}
