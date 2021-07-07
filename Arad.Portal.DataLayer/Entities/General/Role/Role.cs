using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.General.Role
{
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
        public List<string> PermissionIds { get; set; }

    }
}
