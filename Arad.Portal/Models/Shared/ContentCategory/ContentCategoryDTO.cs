using Arad.Portal.DataLayer.Entities.General.ContentCategory;
using Arad.Portal.DataLayer.Models.Shared;

using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.ContentCategory;

public class ContentCategoryDto
{
    public ContentCategoryDto()
    {
        CategoryNames = [];
    }
    public string Id { get; init; }

    public string? ParentCategoryId { get; init; }
    /// <summary>
    /// name and languageId
    /// </summary>
    public List<MultiLingualProperty> CategoryNames { get; init; }

    public ContentCategoryType CategoryType { get; init; }

    public int CategoryTypeId { get; init; }

    public string AssociatedDomainId { get; init; }

    public long CategoryCode { get; init; }

    public bool IsDeleted { get; init; }

    public List<Image> ContentPhotosSlider { get; set; }
}