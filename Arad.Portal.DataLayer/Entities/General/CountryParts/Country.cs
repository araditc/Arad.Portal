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

        /// <summary>
        /// Alpha-2 code
        /// </summary>
        public string SortName { get; set; }
       
        public List<State> States { get; set; }
    }

    public class State 
    {
        public State()
        {
            Cities = new();
        }
       
        public string Id { get; set; }

        public string Name { get; set; }

        public string CountryId { get; set; }

        public string CountryName { get; set; }

        public List<City> Cities { get; set; }
    }

    public class City
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string StateId { get; set; }
        public string StateName { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
    }
}
