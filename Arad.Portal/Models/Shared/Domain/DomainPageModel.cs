using Arad.Portal.DataLayer.Models.Shared.DesignStructure;

using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.Models.Shared.Domain;

public class DomainPageModel
{
    public DomainPageModel()
    {
        HeaderPart = new();
        MainPageContainerPart = new();
        FooterPart = new();
    }
    public string DomainId { get; set; }
    public bool IsAnchor { get; set; }
    public string LanguageId { get; set; }

    public PageHeaderPart HeaderPart { get; set; }

    public MainPageContentPart MainPageContainerPart { get; set; }

    public PageFooterPart FooterPart { get; set; }

    public PageType PageType { get; set; }

}