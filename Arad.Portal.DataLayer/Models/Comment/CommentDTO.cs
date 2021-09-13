using Arad.Portal.DataLayer.Entities.General.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Comment
{
    public class CommentDTO
    {
        public string CommentId { get; set; }

        public string Content { get; set; }

        public string ParentCommentId { get; set; }

        public string ParentCommentContent { get; set; }

        public bool IsApproved { get; set; }

        public int LikeCount { get; set; }

        public int DislikeCount { get; set; }

        public string CreatorUserId { get; set; }

        public string CreatorUserName { get; set; }

        public DateTime CreationDate { get; set; }

        public ReferenceType ReferenceType { get; set; }

        public string ReferenceId { get; set; }

        public string ReferenceTitle { get; set; }
    }
}
