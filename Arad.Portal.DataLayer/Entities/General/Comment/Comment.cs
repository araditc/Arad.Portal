using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.Comment
{
    public class Comment : BaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string CommentId { get; set; }

        public string Content { get; set; }

        public string ParentId { get; set; }

        public string CreationUserId { get; set; }
        
        public bool IsApproved { get; set; }

        public int LikeCount { get; set; }

        public int UnlikeCount { get; set; }

        public ReferenceType ReferenceType { get; set; }

        public string ReferenceId { get; set; }
    }
    public enum ReferenceType
    {
        Product,
        Content,
        //etc
    }
}
