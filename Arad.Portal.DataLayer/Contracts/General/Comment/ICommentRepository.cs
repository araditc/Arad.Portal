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
        Task<Result<Entities.General.Comment.Comment>> Add(CommentDTO dto);
        Task<PagedItems<CommentViewModel>> List(string queryString);
        Task<CommentDTO> CommentFetch(string commentId);
        Task<Result> Update(CommentDTO dto);
        Task<Result> Delete(string commentId, string modificationReason);
        Task<Result> Restore(string commentId);
        List<SelectListModel> GetAllReferenceType();
        Task<Result> ChangeApproval(string commentId, bool isApproved);
        Task<Result<Entities.General.Comment.Comment>> AddLikeDislike(string commentId, bool isLike);
       
    }
}
