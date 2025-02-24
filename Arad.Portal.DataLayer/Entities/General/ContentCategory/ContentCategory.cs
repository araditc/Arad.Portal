using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Models.Shared;

using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.General.ContentCategory;

public class ContentCategory :BaseEntity
{
    public ContentCategory()
    {
        CategoryNames = new();
        ContentPhotosSlider = new();
    }

    public string ParentCategoryId { get; set; }
    /// <summary>
    /// MultiLingualProperty is an object which we store all field of entities that has different value in languages(string type and list of string )
    /// here name and languageId and UrlFriend will be filled
    /// </summary>
    public List<MultiLingualProperty> CategoryNames { get; set; }


    /// <summary>
    /// if this content  doesnt have url friend in this language then it can be accessed by /caregory/{CategoryCode}
    /// </summary>
    public long CategoryCode { get; set; }

    public ContentCategoryType CategoryType { get; set; }

    public List<Image> ContentPhotosSlider { get; set; }
}

public enum ContentCategoryType
{
    News = 0,
    Blog = 1,
    //etc...
}