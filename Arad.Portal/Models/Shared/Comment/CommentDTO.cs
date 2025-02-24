using Arad.Portal.DataLayer.Entities.General.Comment;

using System;

namespace Arad.Portal.Models.Shared.Comment;

public class CommentDto
{
    public string Id { get; set; }

    public string Content { get; set; }

    public string ParentId { get; set; }

    public string ParentCommentContent { get; init; }

    public bool IsApproved { get; init; }

    public int LikeCount { get; init; }

    public int DislikeCount { get; init; }

    public string CreatorUserId { get; set; }

    public string CreatorUserName { get; set; }

    public DateTime CreationDate { get; set; }

    public ReferenceType ReferenceType { get; set; }

    public string ReferenceId { get; set; }

    public string AssociatedDomainId { get; set; }

    public string ReferenceTitle { get; init; }
}