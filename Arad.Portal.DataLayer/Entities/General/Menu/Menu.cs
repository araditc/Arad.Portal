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
        /// LanguageId and Name will be filled here
        /// </summary>
        public List<MultiLingualProperty> MenuTitles { get; set; }

        public MenuType MenuType { get; set; }

        public int Order { get; set; }

        public string ParentId { get; set; }

        public string ParentName { get; set; }

        public string Icon { get; set; }

        public string Url { get; set; }

        /// <summary>
        /// based on menutype fill if menutype is productGroup then subgroupId is productgroupId if menutype is product then subId is productId and SubGroupId is productGroupId
        /// if menuType is contentCategory then subGroupId is content categoryId and if its content then subId is contentId and SubGroupId is contentCategoryId
        /// </summary>
        public string SubId { get; set; }

        public string SubName { get; set; }

        public string SubGroupId { get; set; }

        public string SubGroupName { get; set; }
    }


    public enum MenuType
    {
        ProductGroup,//0
        Product,//1
        CategoryContent,//2
        Content, //3
        DirectLink,//4
        Module//5
    }
}
