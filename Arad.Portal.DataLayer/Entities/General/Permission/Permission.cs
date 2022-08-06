using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.Permission
{
    /// <summary>
    /// this entity is used in admin part for creating menues of admin and accessibility of user based on his role
    /// each role has several permission that user who have that role can access all permissions of that role
    /// these permissions are nested and can have deep up to 3 levels here the permission entity whill be filled with our permissions.json which cover all available links 
    /// but if any new links in admin constructed its url should be added to this file for access and this access should be added to role too
    /// </summary>
    public class Permission : BaseEntity
    {
        public Permission()
        {
            Children = new();
            Actions = new();
        }
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string PermissionId { get; set; }

        public string Title { get; set; }

        public short LevelNo { get; set; }

        public bool IsUI { get; set; }

        public string ParentId { get; set; }

        public short Priority { get; set; }

        public string Icon { get; set; }

        public List<string> Urls { get; set; }

        public string ClientAddress { get; set; }

        public List<Permission> Children { get; set; }

        public List<Action> Actions { get; set; }
    }

    public class Action
    {
        public string PermissionId { get; set; }

        public string Title { get; set; }

        public string ClientAddress { get; set; }

        public List<string> Urls { get; set; }
    }

}
