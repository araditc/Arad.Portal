using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.Menu
{
    public class Menu : BaseEntity
    {
        public Menu()
        {
            MenuTitles = new();
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string MenuId { get; set; }

        /// <summary>
        /// LanguageId and Title will be filled here
        /// </summary>
        public List<MultiLingualProperty> MenuTitles { get; set; }

        public MenuType MenuType { get; set; }

        public int Order { get; set; }

        public string ParentId { get; set; }

        public string Icon { get; set; }

        public string Url { get; set; }
    }


    public enum MenuType
    {
        Content, 
        CategoryContent,
        ProductGroup,
        Product,
        DirectLink,
        Module
    }
}
