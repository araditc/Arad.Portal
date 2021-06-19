using Arad.Portal.DataLayer.Entities.General.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Language
{
    public class LanguageDTO
    {
        public string LanguageId { get; set; }

        public string LanguageName { get; set; }

        public string Symbol { get; set; }

        public Direction Direction { get; set; }

        public string ModificationReason { get; set; }
    }

}
