using Arad.Portal.DataLayer.Models.Shared.DesignStructure;

namespace Arad.Portal.Models.UI;

public class LayoutModel
{
    public bool HasCustomizedHeader { get; set; }
    public PageHeaderPart HeaderPart { get; set; } = new();
    public bool HasCustomizedFooter { get; set; }
    public PageFooterPart FooterPart { get; set; } = new();
    public bool IsMultiLingual { get; set; }
    public bool IsShop { get; set; }
}