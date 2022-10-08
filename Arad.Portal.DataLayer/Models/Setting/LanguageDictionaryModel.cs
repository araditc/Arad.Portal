using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Setting
{
    public class LanguageDictionaryModel
    {
        public string LanguageId { get; set; }

        public IFormFile LanguageUploadFile { get; set; }
    }
}
