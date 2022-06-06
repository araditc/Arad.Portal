using Arad.Portal.DataLayer.Entities.General.SliderModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.SlideModule
{
    public class SlideDTO : Slide
    {

        public string PersianStartShowDate { get; set; }

        public string PersianEndShowDate { get; set; }
    }
}
