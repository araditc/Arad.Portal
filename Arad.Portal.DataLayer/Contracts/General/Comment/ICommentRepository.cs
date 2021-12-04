using Arad.Portal.DataLayer.Models.Comment;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.Comment
{
    public interface ICommentRepository
    {
        Task<RepositoryOperationResult<Entities.General.Comment.Comment>> Add(CommentDTO dto);
        Task<PagedItems<CommentViewModel>> List(string queryString);
        Task<CommentDTO> CommentFetch(string commentId);
        Task<RepositoryOperationResult> Update(CommentDTO dto);
        Task<RepositoryOperationResult> Delete(string commentId, string modificationReason);
        Task<RepositoryOperationResult> Restore(string commentId);
        List<SelectListModel> GetAllReferenceType();
        Task<RepositoryOperationResult> ChangeApproval(string commentId, bool isApproved);
        Task<RepositoryOperationResult<Entities.General.Comment.Comment>> AddLikeDislike(string commentId, bool isLike);
       
    }
}
