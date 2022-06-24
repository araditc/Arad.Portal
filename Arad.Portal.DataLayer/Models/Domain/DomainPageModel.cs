using Arad.Portal.DataLayer.Models.DesignStructure;
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
            HeaderPart = new();
            MainPageContainerPart = new();
            FooterPart = new();
        }
        public string DomainId { get; set; }

        public string LanguageId { get; set; }

        public PageHeaderPart HeaderPart { get; set; }

        public MainPageContentPart MainPageContainerPart { get; set; }

        public PageFooterPart FooterPart { get; set; }

        public bool IsMultiLinguals { get; set; }

        public bool IsShop { get; set; }
    }
}
