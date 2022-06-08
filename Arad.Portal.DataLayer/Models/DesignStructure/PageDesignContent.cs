using Arad.Portal.DataLayer.Models.DesignStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.DesignStructure
{
    public class PageDesignContent
    {
        public PageDesignContent()
        {
            HeaderPart = new();
            MainPageContainerPart = new();
            FooterPart = new();
        }
        public string LanguageId { get; set; }

        public PageHeaderPart HeaderPart { get; set; }

        public MainPageContentPart MainPageContainerPart { get; set; }

        public PageFooterPart FooterPart { get; set; }
    }
}
