namespace Arad.Portal.Models.UI;

public class ProductsInGroupSection
{
    public int CountToTake { get; init; }

    public int CountToSkip { get; init; }

    public string ProductGroupId { get; init; }

    public string DefaultLanguageId { get; init; }

    public int TotalCount { get; set; }
}