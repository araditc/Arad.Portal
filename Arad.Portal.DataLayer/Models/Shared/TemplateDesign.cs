using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class TemplateDesign
    {
        public string DomainId { get; set; }

        public string  LanguageId { get; set; }

        public string LangSymbol { get; set; }

        public string HeaderContent { get; set; }

        public string ContainerContent { get; set; }

        public string FooterContent { get; set; }

        public bool IsMultiLinguals { get; set; }

        public bool IsShop { get; set; }
    }
}
