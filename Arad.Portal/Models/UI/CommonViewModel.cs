using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Shared.Product;
using Arad.Portal.DataLayer.Models.Shared.ProductGroup;
using Arad.Portal.Models.Shared.Content;
using Arad.Portal.Models.Shared.ContentCategory;
using System.Collections.Generic;

namespace Arad.Portal.Models.UI;

public class CommonViewModel
{
    public CommonViewModel()
    {
        NotFound = false;
    }
    public List<ProductGroupDto> Groups { get; init; }
    public List<ProductOutputDto> ProductList { get; init; }
    public List<ContentCategoryDto> Categories { get; set; }

    public List<ContentViewModel> ContentList { get; set; }
    public List<ContentViewModel> BlogList { get; init; }
    public ProductOutputDto ProductDetail { get; init; }
    public ContentDto ContentDetail { get; init; }
    public GroupSection GroupSection { get; set; }
    public ProductsInGroupSection ProductInGroupSection { get; set; }
    public CategorySection CategorySection { get; set; }
    public ContentsInCategorySection ContentsInCategorySection { get; set; }

    public bool NotFound { get; init; }
}