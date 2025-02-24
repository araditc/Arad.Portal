using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Models.Shared.DesignStructure;

public class PageHeaderPart
{
    public BGType? BGType { get; set; } = new();

    public BgImage BgImage { get; set; } = new();
    public string CodeColor { get; set; }
    public string PriorFixedContent { get; set; }

    public List<RowContent> CustomizedContent { get; set; } = [];

    public string LatterFixedContent { get; set; }
}

public enum BGType
{
    Color = 1,
    Image = 2
}