using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class AddComment
    {
        public string Content { get; set; }

        public string ParentId { get; set; }

        public string ReferenceId { get; set; }
    }
}
