using Arad.Portal.DataLayer.Entities.General.Content;
using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Content;

public class ContentDto
{
    public string Id { get; init; }

    public string ContentCategoryId { get; init; }

    public string ContentCategoryName { get; init; }

    public string Title { get; init; }

    public string SubTitle { get; init; }

    public string LanguageId { get; init; }

    public string LanguageName { get; init; }

    public string Description { get; init; }

    public string SeoTitle { get; init; }

    public string SeoDescription { get; init; }

    public string UrlFriend { get; set; }
    /// <summary>
    /// main content from textEditor
    /// </summary>
    public string Contents { get; init; }
    public long ContentCode { get; set; }
    /// <summary>
    /// Ismain = true is for fileLogo
    /// </summary>
    public List<Image> Images { get; init; } = new();

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? StartShowDate { get; init; }
    public string PersianStartShowDate { get; set; }
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? EndShowDate { get; init; }
    public string PersianEndShowDate { get; set; }
    public int? VisitCount { get; init; }
    public List<string> TagKeywords { get; init; } = new();

    //public int? PopularityRate { get; set; }
    public bool IsCommentBoxShowing { get; init; }
    public bool IsSidebarContentsShowing { get; init; }
    public bool IsSliderShowing { get; init; }
    public int? SidebarContentCount { get; init; }
    public bool IsRateBoxShowing { get; init; }
    public long TotalScore { get; init; }
    public int ScoredCount { get; init; }
    public int LikeRate { get; set; }
    public bool HalfLikeRate { get; set; }
    public int DisikeRate { get; set; }
    public List<CommentVm> Comments { get; set; } = new();

    public SourceType? SourceType { get; init; }
    public string SourceTypeId { get; init; }
    public string ContentProviderName { get; init; }
    public string AssociatedDomainId { get; set; }

    public List<Image>? ContentRandomImages { get; set; }

}