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
        /// htmlContent which contains []  and will be replaced by a modules or combinations of Module
        /// </summary>
        public string HtmlContent { get; set; }
    }
}
