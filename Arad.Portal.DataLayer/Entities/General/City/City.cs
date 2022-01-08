using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.City
{
    public class City
    {
       
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string StateId { get; set; }
        public string StateName { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }



    }
}
