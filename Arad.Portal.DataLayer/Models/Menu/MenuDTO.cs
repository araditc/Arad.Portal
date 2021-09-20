using Arad.Portal.DataLayer.Entities.General.Menu;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Menu
{
    public class MenuDTO
    {
        public MenuDTO()
        {
            MenuTitles = new();
        }
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

        public string CreatorUserName { get; set; }

        public string CreatorUserId { get; set; }
    }
}
