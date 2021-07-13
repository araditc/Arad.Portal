using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Permission
{
    public class PermissionListView
    {
        public PermissionListView()
        {
            pers = new List<PermissionListView>();
        }
        public string Title { get; set; }
        public string Id { get; set; }
        public bool IsSelected { get; set; }
        public bool IsActive { get; set; }
        public List<PermissionListView> pers { get; set; }
    }
}
