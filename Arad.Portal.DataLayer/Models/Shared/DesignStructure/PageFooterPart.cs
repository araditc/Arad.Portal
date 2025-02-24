using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Models.Shared.DesignStructure;

public class PageFooterPart
{
    public PageFooterPart()
    {
        CustomizedContent = new();
    }
    public BGType? BGType { get; set; }
    public BgImage BgImage { get; set; }
    public string CodeColor { get; set; }
    public string PriorFixedContent { get; set; }
    public List<RowContent> CustomizedContent { get; set; }
    public string LatterFixedContent { get; set; }
}