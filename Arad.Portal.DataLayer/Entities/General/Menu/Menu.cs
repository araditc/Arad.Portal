using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.Menu
{
    /// <summary>
    /// this entity save menues in the store or blog part
    /// </summary>
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
        /// LanguageId, Name and UrlFriend will be filled here
        /// </summary>
        public List<MultiLingualProperty> MenuTitles { get; set; }

        /// <summary>
        /// reperesent the source of menu
        /// </summary>
        public MenuType MenuType { get; set; }

        /// <summary>
        /// order of menues in one level 
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// the Id of parnet menu it can be null if it is in first level
        /// </summary>
        public string ParentId { get; set; }
        /// <summary>
        /// the name of parent menu it can be null if it is in first level
        /// </summary>
        public string ParentName { get; set; }
       
        public string Icon { get; set; }
        /// <summary>
        /// the address of menu
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// if it has not urlfriend in th specific language and its menuType is one of these items : {ProductGroup, Product, CategoryContent, Content} then menuCode can be one of these items : {groupCode, ProductCode, CategoryCode, ContentCode}
        /// </summary>
        public long  MenuCode { get; set; }

        /// <summary>
        /// if menutype is productGroup then subgroupId is productgroupId,
        /// if menutype is product then subId is productId and SubGroupId is productGroupId
        /// if menuType is contentCategory then subGroupId is contentcategoryId 
        /// and if it is content then subId is contentId and SubGroupId is contentCategoryId
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
