using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.DesignStructure
{
    public class Template : BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string TemplateId { get; set; }
        /// <summary>
        /// htmlContent which contains [] or {} and will be replaced by modules
        /// </summary>
        public string HtmlContent { get; set; }
    }
}
