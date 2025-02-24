using Arad.Portal.DataLayer.Entities.General.Comment;

using System;
using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Models.Shared;

public class CommentVm
{
    public CommentVm()
    {
        Childrens = new();
    }
    public string Id { get; set; }

    public string Content { get; set; }

    public int LikeCount { get; set; }

    public int DislikeCount { get; set; }

    public string ReferenceId { get; set; }

    public string CreatorUserId { get; set; }

    public ReferenceType ReferenceType { get; set; }

    public string CreatorUserName { get; set; }

    public UserStatus UserStatus { get; set; }

    public DateTime CreationDate { get; set; }

    public string PersianCreationDate { get; set; }

    public List<CommentVm> Childrens { get; set; }
}

public enum UserStatus
{
    Like,
    Dislike,
    NoAction,
    UnAuthorized
}