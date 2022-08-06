using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.General.Role
{
    /// <summary>
    /// each user who have access to admini part should have one role and based on the role permissions the admin menues would be constructed for accessability
    /// </summary>
    public class Role : BaseEntity
    {
        public Role()
        {
            PermissionIds = new();
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string RoleId { get; set; }
        public string RoleName { get; set; }

        /// <summary>
        /// all PermissionId which this role contains
        /// </summary>
        public List<string> PermissionIds { get; set; }

    }
}
