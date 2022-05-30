using Arad.Portal.DataLayer.Entities.General.SliderModule;
using Arad.Portal.GeneralLibrary.CustomAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.SlideModule
{
    public class SlideView
    {
        public string Id { get; set; }
        public string SliderId { get; set; }
        [Required]
        [CustomErrorMessage("AlertAndMessage_FieldEssential")]
        public string ImageUrl { get; set; }
        public string ColoredBackground { get; set; }
        public string VideoUrl { get; set; }
        public ImageFit ImageFit { get; set; }
        public TransActionType TransActionType { get; set; }
        public string Link { get; set; }
        public Target Target { get; set; }
        [Required]
        [CustomErrorMessage("AlertAndMessage_FieldEssential")]
        public DateTime? StartDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public bool IsActive { get; set; }
        [Required]
        [CustomErrorMessage("AlertAndMessage_FieldEssential")]
        public string Title { get; set; }
        public string Alt { get; set; }
    }
}
