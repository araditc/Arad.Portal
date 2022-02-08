using Arad.Portal.GeneralLibrary.CustomAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class Image
    {
        public string ImageId { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public bool IsMain { get; set; }
        public ImageRatio ImageRatio { get; set; } 
        public int ImageRatioId { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }

    public enum ImageRatio
    {
        [Description("1*1")]
        Square = 1,
        [Description("2*1")]
        TwoToOne = 2,
        [Description("4*1")]
        FourToOne = 4
    }
}
