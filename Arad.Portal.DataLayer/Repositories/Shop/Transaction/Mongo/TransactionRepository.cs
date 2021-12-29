using Arad.Portal.DataLayer.Contracts.Shop.Transaction;
using Arad.Portal.DataLayer.Models.Shared;
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

        public TransactionItems CreateTransactionItemsModel(string transactionId)
        {
            var model = new TransactionItems();
            var entity = _context.Collection
               .Find(_ => _.TransactionId == transactionId).FirstOrDefault();
            foreach (var seller in entity.SubInvoices)
            {
                foreach (var pro in seller.ParchasePerSeller.Products)
                {
                    var obj = new ProductOrder()
                    {
                        ProductId = pro.ProductId,
                        OrderCount = pro.OrderCount
                    };
                    model.Orders.Add(obj);
                }
            }
            model.CreatedDate = DateTime.Now;

            return model;
        }

        public Entities.Shop.Transaction.Transaction FetchById(string transactionId)
        {
            var entity = _context.Collection
                .Find(_ => _.TransactionId == transactionId).FirstOrDefault();

            return entity;
        }

        public Entities.Shop.Transaction.Transaction FetchByIdentifierToken(string reservationNumber)
        {
            var entity = _context.Collection
                .Find(_ => _.BasicData.ReservationNumber == reservationNumber).FirstOrDefault();

            return entity;
        }

        public async Task InsertTransaction(Entities.Shop.Transaction.Transaction transaction)
        {
            await _context.Collection.InsertOneAsync(transaction);
        }

        public async Task UpdateTransaction(Entities.Shop.Transaction.Transaction transaction)
        {
            var entity
                = _context.Collection.Find(_ => _.TransactionId== transaction.TransactionId);
            if (entity != null)
            {
                await _context.Collection.ReplaceOneAsync(_ => _.TransactionId == transaction.TransactionId, transaction);
            }
        }
    }
}
