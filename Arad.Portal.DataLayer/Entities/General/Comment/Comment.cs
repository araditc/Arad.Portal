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
        /// <summary>
        /// primary key of entity
        /// </summary>
        public string CommentId { get; set; }
        /// <summary>
        /// the main content of comment  
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// the primary key id of its parent
        /// </summary>
        public string ParentId { get; set; }
        /// <summary>
        /// whether it is approved by admin to show or not
        /// </summary>
        public bool IsApproved { get; set; }
        /// <summary>
        /// the userid who comment
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// how many like it get
        /// </summary>
        public int LikeCount { get; set; }
        /// <summary>
        /// how many dislike it get
        /// </summary>
        public int DislikeCount { get; set; }
        /// <summary>
        /// whether it is a comment for profuct or for Content entity
        /// </summary>
        public ReferenceType ReferenceType { get; set; }

        /// <summary>
        /// the primary key of the entity which this comment belong to
        /// </summary>
        public string ReferenceId { get; set; }
    }
    public enum ReferenceType
    {
        Product,
        Content,
        //etc
    }
}
