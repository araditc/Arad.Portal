using Arad.Portal.DataLayer.Entities.General.ContentCategory;
using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.Models.Shared.ContentCategory;

public class ContentCategoryViewModel
{
    public string ContentCategoryId { get; init; }

    public string ParentCategoryId { get; set; }
    /// <summary>
    /// name and languageId
    /// </summary>
    public MultiLingualProperty CategoryName { get; init; }

    public ContentCategoryType CategoryType { get; init; }

    public bool IsDeleted { get; init; }
}