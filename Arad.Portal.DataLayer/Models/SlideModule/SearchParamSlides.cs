using Arad.Portal.GeneralLibrary.CustomAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.SlideModule
{
    public class SearchParamSlides
    {
        public string SliderId { get; set; }
        public int PageSize { get; set; }

        [Required]
        [CustomErrorMessage("AlertAndMessage_FieldEssential")]
        public int CurrentPage { get; set; }
    }
}
