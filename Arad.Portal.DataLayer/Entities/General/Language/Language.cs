using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.Language
{
    public class Language : BaseEntity
    {

        public string LanguageId { get; set; }

        public string LanguageName { get; set; }

        public string Symbol { get; set; }

        public Direction Direction { get; set; }
    }

    public enum Direction
    {
        ltr,
        rtl
    }
}
