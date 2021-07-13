using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.CustomAttributes;

namespace Arad.Portal.DataLayer.Models.Attachment
{
    public class Attachment
    {
        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string ContentType { get; set; }

        public FileType FileType { get; set; }
    }

    public enum FileType
    {
      
        [CustomDescription("TextAndPlaceholder_Pdf")]
        Pdf,

        [CustomDescription("TextAndPlaceholder_Excel")]
        Excel,

        [CustomDescription("TextAndPlaceholder_Word")]
        Word,

        [CustomDescription("TextAndPlaceholder_Text")]
        Text,

        [CustomDescription("TextAndPlaceholder_Pic")]
        Pic,

        [CustomDescription("TextAndPlaceholder_PowerPoint")]
        PowerPoint,

        [CustomDescription("TextAndPlaceholder_Rar")]
        Rar,

        [CustomDescription("TextAndPlaceholder_Zip")]
        Zip
    }
}
