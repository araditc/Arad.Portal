using Arad.Portal.DataLayer.Entities.General.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class SpecificationGroup
    {
        public string SpecificationGroupId { get; set; }
        public string GroupName { get; set; }
        public string LanguageId { get; set; }
        public string LanguageName { get; set; }
    }
}
