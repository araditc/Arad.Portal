using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.Language
{
    public class Language : BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string LanguageId { get; set; }

        public string LanguageName { get; set; }

        public string Symbol { get; set; }

        /// <summary>
        /// whether this language is rtl or ltr
        /// </summary>
        public Direction Direction { get; set; }

        /// <summary>
        /// one language in the language document
        /// </summary>
        public bool IsDefault { get; set; }
    }

    public enum Direction
    {
        ltr,
        rtl
    }
}
