using Arad.Portal.DataLayer.Models.Shared.DesignStructure;

namespace Arad.Portal.Models.Shared.DesignStructure;

public class RowSelectedColumnsModel
{
    public RowSelectedColumnsModel()
    {
        RowData = null;
    }
    public int Count { get; set; }
    public int ColumnWidth { get; set; }
    public string RowNumber { get; set; }
    public string DomainId { get; set; }
    public string RowGuid { get; set; }

    public RowContent RowData { get; set; }
}