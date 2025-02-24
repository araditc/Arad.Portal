using System.Collections.Generic;

using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.Models.UI;

public class CategorySection
{
    public CategorySection()
    {
        CategoriesWithContent = new();
    }
    public int CountToTake { get; set; }

    public int CountToSkip { get; set; }

    public string ContentCategoryId { get; set; }

    public string DefaultLanguageId { get; set; }

    public long TotalCount { get; set; }

    public List<string> CategoriesWithContent { get; set; }

    public List<Image>? Images { get; set; }
}