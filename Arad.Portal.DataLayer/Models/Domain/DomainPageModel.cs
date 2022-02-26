using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Models.Domain
{
    public class DomainPageModel
    {
        public DomainPageModel()
        {
            ModuleParams = new();
        }
        public string DomainId { get; set; }

        public string TemplateId { get; set; }

        public PageType PageType { get; set; }

        public List<KeyVal> ParamsValue { get; set; }

        public List<ModuleParams> ModuleParams { get; set; }
    }
}
