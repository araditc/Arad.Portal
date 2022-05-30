using Arad.Portal.DataLayer.Entities.General.SliderModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.SlideModule
{
    public class SlidesListGridView
    {
        public string SliderId { get; set; }
        public string Id { get; set; }
        public string ImageUrl { get; set; }
        public string ColoredBackground { get; set; }
        public string VideoUrl { get; set; }
        public ImageFit ImageFit { get; set; }
        public TransActionType TransActionType { get; set; }
        public string Link { get; set; }
        public Target Target { get; set; }
        public string PersianStartDate { get; set; }
        public string PersianExpireDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? ExpireDate { get; set; }
        public bool IsActive { get; set; }
        public string Title { get; set; }
        public string Alt { get; set; }
    }
}
