using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Models.Shared.DesignStructure;

public class PageDesignContent
{
    public PageDesignContent()
    {
        HeaderPart = new();
        MainPageContainerPart = new();
        FooterPart = new();
    }
    public string LanguageId { get; set; }

    public string LanguageName { get; set; }

    public GlobalStyle GlobalCustomContent { get; set; }

    public PageHeaderPart HeaderPart { get; set; }

    public MainPageContentPart MainPageContainerPart { get; set; }

    public PageFooterPart FooterPart { get; set; }

}



