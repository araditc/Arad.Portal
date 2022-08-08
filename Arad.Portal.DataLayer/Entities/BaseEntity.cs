using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Entities.Shop.Product;

namespace Arad.Portal.DataLayer.Entities
{
    public class BaseEntity
    {
        public BaseEntity()
        {
            Modifications = new ();
        }

        #region Properties
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreationDate { get; set; }

        public string CreatorUserId { get; set; }

        public string CreatorUserName { get; set; }

        public List<Modification> Modifications { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsActive { get; set; }

        public string AssociatedDomainId { get; set; }
        #endregion

        
    }

    public class Modification
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime DateTime { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }

        /// <summary>
        /// the reason of updating this entity written by application for logging
        /// </summary>
        public string ModificationReason { get; set; }
    }
}
