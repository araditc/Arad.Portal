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
using Arad.Portal.DataLayer.Models.Transaction;
using Arad.Portal.DataLayer.Entities.Shop.Transaction;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo;
using System.Collections.Specialized;
using System.Web;

namespace Arad.Portal.DataLayer.Repositories.Shop.Transaction.Mongo
{
    public class TransactionRepository : BaseRepository, ITransactionRepository
    {
        private readonly TransactionContext _context;
        private readonly DomainContext _domainContext;
        private readonly ProductContext _productContext;
       
        public TransactionRepository(IHttpContextAccessor httpContextAccessor,
            DomainContext domainContext,
            IWebHostEnvironment env,
            ProductContext productContext,
            TransactionContext context)
            : base(httpContextAccessor, env)
        {
            _context = context;
            _domainContext = domainContext;
            _productContext = productContext;
        }

        private long  GetProductCode(string productId)
        {
            long result = 0;
            var entity = _productContext.ProductCollection.Find(_ => _.ProductId == productId).FirstOrDefault();
            if (entity != null)
            {
                result = entity.ProductCode;
            }
            return result;
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

        public List<TransactionDTO> GetUserOrderHistory(string userId)
        {
            var result = new List<TransactionDTO>();
            FilterDefinitionBuilder<Entities.Shop.Transaction.Transaction> transBuilder = new();
            FilterDefinitionBuilder<CustomerData> customerBuilder = new();
            FilterDefinition<Entities.Shop.Transaction.Transaction> transFilterDef = null;
            FilterDefinition<CustomerData> customerFilterDef = null;
            customerFilterDef = customerBuilder.Eq(nameof(CustomerData.UserId), userId);
            transFilterDef = transBuilder.ElemMatch("CustomerData", customerFilterDef);
            var lst = _context.Collection.Find(transFilterDef).ToList();
            foreach (var item in lst)
            {
                var dto = new TransactionDTO();
                dto.TransactionId = item.TransactionId;
                dto.UserId = userId;
                dto.MainInvoiceNumber = item.MainInvoiceNumber;
                dto.FinalPriceToPay = item.FinalPriceToPay;
                dto.PaymentDate = DateTime.Parse(item.AdditionalData.FirstOrDefault(_ => _.Key == "CreationDate").Value);
                dto.RegisteredDate = item.CreationDate;
                dto.OrderStatus = item.OrderStatus;
                dto.PaymentStage = item.BasicData.Stage;
                int itemCount = 0;
                foreach (var sub in item.SubInvoices)
                {
                    var detail = new TransactionDetail();
                    detail.SellerId = sub.ParchasePerSeller.SellerId;
                    detail.SellerUserName = sub.ParchasePerSeller.SellerUserName;
                    detail.TotalDetailsAmountToPayWithShipping = sub.ParchasePerSeller.TotalDetailsAmountWithShipping;
                    foreach (var pro in sub.ParchasePerSeller.Products)
                    {
                        var obj = new ProductOrderDetail();

                        obj.ProductId = pro.ProductId;
                        obj.ProductCode = this.GetProductCode(pro.ProductId);
                        obj.ProductName = pro.ProductName;
                        obj.OrderCount = pro.OrderCount;
                        obj.PriceWithDiscountPerUnit = pro.PricePerUnit - pro.DiscountPricePerUnit;

                        detail.Products.Add(obj);
                        itemCount += 1;
                    }
                    dto.Details.Add(detail);
                }
                dto.OrderItemsCount = itemCount;
                result.Add(dto);
            }

            return result;
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

        public async Task<PagedItems<TransactionGlanceAdminView>> GetSiteAdminTransactionList(string queryString)
        {
            var result = new PagedItems<TransactionGlanceAdminView>();
            FilterDefinitionBuilder<Entities.Shop.Transaction.Transaction> traBuilder = new();
            FilterDefinition<Entities.Shop.Transaction.Transaction> transFilterDef = null;

            try
            {
                NameValueCollection filter = HttpUtility.ParseQueryString(queryString);

                if (string.IsNullOrWhiteSpace(filter["page"]))
                {
                    filter.Set("page", "1");
                }

                if (string.IsNullOrWhiteSpace(filter["PageSize"]))
                {
                    filter.Set("PageSize", "20");
                }
                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);
                long totalCount = 0;
                if(!string.IsNullOrWhiteSpace(filter["domainId"]))
                {
                    totalCount = await _context.Collection.Find(_ => _.AssociatedDomainId == filter["domainId"]).CountDocumentsAsync();
                }else
                {
                    totalCount = await _context.Collection.Find(_ => true).CountDocumentsAsync();
                }

                if (!string.IsNullOrWhiteSpace(filter["domainId"]))
                {
                    transFilterDef = traBuilder.Eq(nameof(Entities.Shop.Transaction.Transaction.AssociatedDomainId), filter["domainId"]);
                }
                else
                {
                    transFilterDef = traBuilder.Empty;
                }
                var list = _context.Collection.Find(transFilterDef)
                    .Project(_=> new TransactionGlanceAdminView
                    {
                        MainInvoiceNumber =_.MainInvoiceNumber,
                        OrderStatus = _.OrderStatus,
                        PaymentDate = DateTime.Parse(_.AdditionalData.FirstOrDefault(_ => _.Key == "CreationDate").Value),
                        PaymentStage = _.BasicData.Stage,
                        RegisteredDate = _.CreationDate,
                        TotalAmount = _.FinalPriceToPay,
                        TransactionId = _.TransactionId,
                        UserId = _.CustomerData.UserId,
                        UserFullName = _.CustomerData.UserFullName,
                        UserName = _.CustomerData.UserName,
                       // OrderItemCount = _.SubInvoices.Sum(a=> a.ParchasePerSeller.Products.Count())
                    } ).Sort(Builders<Entities.Shop.Transaction.Transaction>.Sort.Descending(a => DateTime.Parse(a.AdditionalData.FirstOrDefault(_ => _.Key == "CreationDate").Value))).Skip((page - 1) * pageSize).Limit(pageSize).ToList();
                
                result.Items = list;
                result.CurrentPage = page;
                result.ItemsCount = totalCount;
                result.PageSize = pageSize;
                result.QueryString = queryString;

            }
            catch (Exception)
            {
                result.CurrentPage = 1;
                result.Items = new List<TransactionGlanceAdminView>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public List<SelectListModel> GetAllOrderStatusType()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(OrderStatus)))
            {
                string name = Enum.GetName(typeof(OrderStatus), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel() { Text = Language.GetString("Choose"), Value = "-1" });
            return result;
        }

        public List<SelectListModel> GetAllPaymentStageList()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(PaymentStage)))
            {
                string name = Enum.GetName(typeof(PaymentStage), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel() { Text = Language.GetString("Choose"), Value = "-1" });
            return result;
        }

        public async Task<bool> ChangeOrderStatus(string transactionId, OrderStatus status)
        {
            var entity = _context.Collection.Find(_ => _.TransactionId == transactionId).FirstOrDefault();
            if(entity != null)
            {
                entity.OrderStatus = status;
                var updateRes = await _context.Collection.ReplaceOneAsync(_ => _.TransactionId == transactionId, entity);
                if(updateRes.IsAcknowledged)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
