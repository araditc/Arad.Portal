namespace Arad.Portal.Models.Admin;

public class TemplateDesign
{
    public string DomainId { get; init; }

    public string LanguageId { get; init; }

    public string LangSymbol { get; set; }

    public string HeaderContent { get; init; }

    public string ContainerContent { get; init; }

    public string FooterContent { get; init; }

    public bool IsMultiLinguals { get; init; }

    public bool IsShop { get; init; }
}