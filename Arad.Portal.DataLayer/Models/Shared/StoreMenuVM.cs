using Arad.Portal.DataLayer.Entities.General.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class StoreMenuVM
    {
        public StoreMenuVM()
        {
            Childrens = new();
            IsFull = true;
            MenuTitle = new MultiLingualProperty();
        }
        public string MenuId { get; set; }
        /// <summary>
        /// LanguageId and Title will be filled here
        /// </summary>
        public MultiLingualProperty MenuTitle { get; set; }
        public MenuType MenuType { get; set; }
        public int Order { get; set; }
        public string ParentId { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }

        public string SubId { get; set; }

        public string SubName { get; set; }

        public string SubGroupId { get; set; }

        public string SubGroupName { get; set; }
        public bool IsFull { get; set; }
        public List<StoreMenuVM> Childrens { get; set; }
    }
}
