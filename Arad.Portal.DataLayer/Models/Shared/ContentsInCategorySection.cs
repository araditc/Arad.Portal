namespace Arad.Portal.DataLayer.Models.Shared;

public class ContentsInCategorySection
{
    public int CountToTake { get; set; }

    public int CountToSkip { get; set; }

    public string ContentCategoryId { get; set; }

    public string DefaultLanguageId { get; set; }

    public int TotalCount { get; set; }
}