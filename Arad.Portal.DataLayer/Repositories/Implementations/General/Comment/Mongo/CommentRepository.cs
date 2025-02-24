using Arad.Portal.DataLayer.Repositories.Interfaces.General.Comment;
using Arad.Portal.DataLayer.Repositories.Implementations.Abstractions;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

namespace Arad.Portal.DataLayer.Repositories.Implementations.General.Comment.Mongo;

public class CommentRepository : Repository<Entities.General.Comment.Comment>, ICommentRepository
{
    public CommentRepository(IMongoDbContext dbContext) : base(dbContext)
    {
    }
}