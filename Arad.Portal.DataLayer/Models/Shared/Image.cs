using System.Collections.Generic;
using System.ComponentModel;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Models.Shared;

public class Image
{
    public string ImageId { get; set; }
    public string Url { get; set; }
    public string FileName { get; set; }
    public string Title { get; set; }
    public bool IsMain { get; set; }
    public ImageRatio ImageRatio { get; set; }
    public List<ImageTemplateType> ImageTemplateTypes { get; set; }
    public int ImageRatioId { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
    public string Counter { get; set; }
}

public enum ImageRatio
{
    [Description("1*1")]
    Square = 1,
    [Description("2*1")]
    TwoToOne = 2,
    [Description("4*1")]
    FourToOne = 4,
    [Description("5*1")]
    FiveToOne = 5,
    [Description("6*1")]
    SixToOne = 6,
    [Description("7*1")]
    SevenToOne = 7

}