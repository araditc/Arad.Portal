using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Comment
{
    public class Comment
    {

        public string CommentId { get; set; }

        public string Content { get; set; }

        public string ParentId { get; set; }

        public string CreationUserId { get; set; }

        public string UserName { get; set; }


        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreationDate { get; set; }

        public bool IsApproved { get; set; }

        public int LikeCount { get; set; }

        public int UnlikeCount { get; set; }
    }
}
