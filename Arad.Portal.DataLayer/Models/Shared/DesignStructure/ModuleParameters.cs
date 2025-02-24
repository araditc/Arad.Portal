using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using Arad.Portal.GeneralLibrary.CustomAttributes;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

using Arad.Portal.DataLayer.Models.Shared.FAQ;
using Arad.Portal.DataLayer.Models.Shared.ProgressBar;

namespace Arad.Portal.DataLayer.Models.Shared.DesignStructure;

public class ModuleParameters
{
    [BsonIgnoreIfNull]
    public int? Count { get; set; }

    [BsonIgnoreIfNull]
    public ProductOrContentType? ProductOrContentType { get; set; }

    [BsonIgnoreIfNull]
    public ProductTemplateDesign? ProductTemplateDesign { get; set; }

    [BsonIgnoreIfNull]
    public ContentTemplateDesign? ContentTemplateDesign { get; set; }

    [BsonIgnoreIfNull]
    public ImageSliderTemplateDesign? ImageSliderTemplateDesign { get; set; }

    [BsonIgnoreIfNull]
    public AdvertisementTemplateDesign? AdvertisementTemplateDesign { get; set; }

    [BsonIgnoreIfNull]
    public SelectionType? SelectionType { get; set; }

    [BsonIgnoreIfNull]
    public List<string> SelectedIds { get; set; }

    [BsonIgnoreIfNull]
    public string CatId { get; set; }

    [BsonIgnoreIfNull]
    public string DomainId { get; set; }

    [BsonIgnoreIfNull]
    public bool IsShop { get; set; }

    [BsonIgnoreIfNull]
    public string LanguageId { get; set; }

    [BsonIgnoreIfNull]
    public string SliderId { get; set; }

    [BsonIgnoreIfNull]
    public Enums.SliderType SliderType { get; set; }

    [BsonIgnoreIfNull]
    public Entities.General.SliderModule.TransActionType LoadAnimation { get; set; }

    [BsonIgnoreIfNull]
    public LoadAnimationType LoadAnimationType { get; set; }


    [BsonIgnoreIfNull]
    public string Keyword { get; set; }

    #region FAQ
    [BsonIgnoreIfNull]
    public FAQViewModel FAQViewModel { get; set; } = new FAQViewModel();
    #endregion

    #region ProgressBar
    [BsonIgnoreIfNull]
    public ProgressBarViewModel ProgressBarViewModel { get; set; } = new ProgressBarViewModel();
    #endregion

    #region IconBlocks
    [BsonIgnoreIfNull]
    public List<IconBlock.IconBlock> IconBlocks { get; set; } = [];
    #endregion

    #region Counter
    public List<Counter.Counter> Counters { get; set; } = [];
    #endregion

    #region ContactUs
    public List<ContactUs.ContactUs> ContactUsList { get; set; } = [];
    #endregion

    #region PictureWithLabel
    public List<PictureWithLabel.PictureWithLabel> PictureWithLabels { get; set; } = [];
    #endregion

    #region SocialMedia
    public List<SocialMedia.SocialMedia> SocialMedias { get; set; } = [];
    #endregion
}

public enum SelectionType
{
    [CustomDescription("EnumDesc_LatestFromProductOrContentTypeInAllCategories")]
    LatestFromProductOrContentTypeInAllCategories = 1,

    [CustomDescription("EnumDesc_LatestFromProductOrContentTypeSelectedCategory")]
    LatestFromProductOrContentTypeSelectedCategory = 2,

    [CustomDescription("EnumDesc_CustomizedSelection")]
    CustomizedSelection = 3
}

public enum LoadAnimationType
{
    [CustomDescription("EnumDesc_LoadAnimationTypeInner")]
    InnerElements,

    [CustomDescription("EnumDesc_LoadAnimationTypeOuter")]
    OuterElement
}