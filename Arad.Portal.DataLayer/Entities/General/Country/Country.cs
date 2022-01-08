using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.Country
{
    public class Country : BaseEntity
    {
        public Country()
        {
            States = new();
        }
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string Id { get; set; }

        public string Name { get; set; }

        public string SortName { get; set; }

        public List<State.State> States { get; set; }
    }
}
