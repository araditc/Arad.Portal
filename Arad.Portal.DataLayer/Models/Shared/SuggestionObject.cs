using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class SuggestionObject
    {
        public string Phrase { get; set; }

        public bool IsProduct { get; set; }

        public string UrlParameter { get; set; }
    }
}
