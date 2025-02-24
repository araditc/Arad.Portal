using Microsoft.AspNetCore.Http;

namespace Arad.Portal.DataLayer.Models.Shared.PictureWithLabel
{
    public class PictureWithLabel
    {
        public Image Image { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public string SubTitle { get; set; }
    }
}
