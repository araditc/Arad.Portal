using Arad.Portal.DataLayer.Entities.General.Comment;

using System;

namespace Arad.Portal.Models.Shared.Comment;

public class CommentViewModel
{
    public string CommentId { get; init; }

    public string Content { get; init; }

    public string ParentCommentId { get; init; }

    public string ParentCommentContent { get; set; }

    public bool IsApproved { get; init; }

    public int LikeCount { get; init; }

    public int DislikeCount { get; init; }

    public string CreatorUserId { get; set; }

    public string CreatorUserName { get; init; }

    public DateTime? CreationDate { get; init; }

    public string PersianCreationDate { get; set; }

    public ReferenceType ReferenceType { get; init; }

    public string ReferenceId { get; init; }

    public string ReferenceTitle { get; set; }

    public bool IsDeleted { get; init; }

    public string AssociatedDomainId { get; init; }

    public string DomainName { get; set; }
}