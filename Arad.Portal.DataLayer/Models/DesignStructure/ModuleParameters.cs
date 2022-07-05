using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using Arad.Portal.GeneralLibrary.CustomAttributes;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Models.DesignStructure
{
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
        public string CatId { get; set; }

        [BsonIgnoreIfNull]
        public List<string> SelectedIds { get; set; }

        [BsonIgnoreIfNull]
        public string DomainId { get; set; }

        [BsonIgnoreIfNull]
        public bool IsShop { get; set; }

        [BsonIgnoreIfNull]
        public string LanguageId { get; set; }

        [BsonIgnoreIfNull]
        public string SliderId { get; set; }

        [BsonIgnoreIfNull]
        public Entities.General.SliderModule.TransActionType LoadAnimation { get; set; }

        [BsonIgnoreIfNull]
        public LoadAnimationType LoadAnimationType { get; set; }


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
}
