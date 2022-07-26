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
        //public string Title { get; set; }
        //public List<string> Routes { get; set; }
        //public string ClientAddress { get; set; }
        //public Enums.PermissionType Type { get; set; }
        //public Enums.PermissionMethod Method { get; set; }
        //public string MenuIdOfModule { get; set; }
        ///// <summary>
        ///// ParentMenu
        ///// </summary>
        //public string ParentMenuId { get; set; }
        //public double Priority { get; set; }
        //public string Icon { get; set; }
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
