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

        public Direction Direction { get; set; }
    }

    public enum Direction
    {
        ltr,
        rtl
    }
}
