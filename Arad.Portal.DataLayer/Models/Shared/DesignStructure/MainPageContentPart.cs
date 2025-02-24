using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Models.Shared.DesignStructure;

public class MainPageContentPart
{
    public BGType? BGType { get; set; } = new ();

    public BgImage BgImage { get; set; } = new();
    public string CodeColor { get; set; }
    public string PriorFixedContent { get; set; }
    public List<RowContent> RowContents { get; set; } = [];
    public string LatterFixedContent { get; set; }
}