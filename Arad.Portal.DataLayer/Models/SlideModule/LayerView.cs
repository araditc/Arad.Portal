using Arad.Portal.DataLayer.Entities.General.SliderModule;
using Arad.Portal.GeneralLibrary.CustomAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Attribute = Arad.Portal.DataLayer.Entities.General.SliderModule.Attribute;

namespace Arad.Portal.DataLayer.Models.SlideModule
{
    public class LayerView
    {
        public string SliderId { get; set; }
        public string SlideId { get; set; }
        public string LayerId { get; set; }
        public string Id { get; set; }
        [Required]
        [ErrorMessage("AlertAndMessage_FieldEssential")]
        public LayerType Type { get; set; }
        [Required]
        [ErrorMessage("AlertAndMessage_FieldEssential")]
        public string Content { get; set; }
        public string Link { get; set; }
        public Target Target { get; set; }
        public Position Position { get; set; }
        public TransActionType TransActionType { get; set; }
        public Style Styles { get; set; }
        public Attribute Attributes { get; set; }
        public int IsDeleted { get; set; }
        public bool IsActive { get; set; }
    }
}
