﻿using Arad.Portal.DataLayer.Entities.General.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class CommentVM
    {
        public CommentVM()
        {
            Childrens = new();
        }
        public string CommentId { get; set; }

        public string Content { get; set; }

        public int LikeCount { get; set; }

        public int DislikeCount { get; set; }

        public string ReferenceId { get; set; }

        public string CreatorUserId { get; set; }

        public ReferenceType ReferenceType { get; set; }

        public string CreatorUserName { get; set; }

        public userStatus userStatus { get; set; }

        public DateTime CreationDate { get; set; }

        public string PersianCreationDate { get; set; }

        public List<CommentVM> Childrens { get; set; }
    }

    public enum userStatus
    {
        Like,
        Dislike,
        NoAction,
        UnAuthorized
    }
}
