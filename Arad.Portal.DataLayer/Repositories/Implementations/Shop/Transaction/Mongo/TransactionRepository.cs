using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Transaction;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.Shop.Transaction.Mongo;

public class TransactionRepository : Repository<Entities.Shop.Transaction.Transaction>, ITransactionRepository
{
    public TransactionRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}