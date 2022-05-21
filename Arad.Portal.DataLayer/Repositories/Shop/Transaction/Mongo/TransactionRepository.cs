using Arad.Portal.DataLayer.Contracts.Shop.Transaction;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;
using Microsoft.AspNetCore.Hosting;

namespace Arad.Portal.DataLayer.Repositories.Shop.Transaction.Mongo
{
    public class TransactionRepository : BaseRepository, ITransactionRepository
    {
        private readonly TransactionContext _context;
        private readonly DomainContext _domainContext;
       
        public TransactionRepository(IHttpContextAccessor httpContextAccessor,
            DomainContext domainContext,
            IWebHostEnvironment env,
            TransactionContext context)
            : base(httpContextAccessor, env)
        {
            _context = context;
            _domainContext = domainContext;
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

        public bool IsRefNumberUnique(string referenceNumber, PspType pspType)
        {
            var result = false;
            var domainName = base.GetCurrentDomainName();
            var domainEntity = _domainContext.Collection.Find(_ => _.DomainName == domainName).FirstOrDefault();
            if(!_context.Collection.Find(_=>_.BasicData.ReferenceId == referenceNumber 
                                           && _.BasicData.PspType == pspType
                                           && _.AssociatedDomainId == domainEntity.DomainId).Any())
            {
                result = true;
            }
            return result;
        }

        public async Task<Result> RollBackPayingTransaction()
        {
            var result = new Result();
            var list = new List<PaymentStage>() { PaymentStage.Initialized, PaymentStage.GenerateToken, PaymentStage.RedirectToIPG, PaymentStage.DoneButNotConfirmed };
            FilterDefinitionBuilder<Entities.Shop.Transaction.Transaction> filterDefinition = new(); 
            var filter = filterDefinition.In(_ => _.BasicData.Stage, list);


            UpdateDefinition<Entities.Shop.Transaction.Transaction> updateDefinition = 
                Builders<Entities.Shop.Transaction.Transaction>.Update.Set(x => x.BasicData.Stage, PaymentStage.ForcedToCancelledBySystem);
            var entities = _context.Collection.Find(_ => _.BasicData.Stage == PaymentStage.Initialized
            || _.BasicData.Stage == PaymentStage.GenerateToken
            || _.BasicData.Stage == PaymentStage.RedirectToIPG
            || _.BasicData.Stage == PaymentStage.DoneButNotConfirmed);

            var updateResult = await _context.Collection.UpdateManyAsync(filter, updateDefinition);
            if (updateResult.IsAcknowledged)
            {
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            else
            {
                result.Message = ConstMessages.GeneralError;
            }
            return result;
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
