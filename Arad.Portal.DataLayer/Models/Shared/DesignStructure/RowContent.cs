using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Models.Shared.DesignStructure;

public class RowContent
{
    public int RowNumber { get; set; }

    public string RowGuid { get; set; }

    public BGType? BGType { get; set; }

    public BgImage BgImage { get; set; }

    public string BgCodeColor { get; set; }

    public string ExtraClassNames { get; set; }

    public string InlineStyles { get; set; }

    public int? Order { get; set; }

    public string EnumColsId { get; set; }

    public List<ColContent> ColsContent { get; set; } = [];
}