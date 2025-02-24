using Arad.Portal.DataLayer.Entities.General.Content;
using Arad.Portal.DataLayer.Models.Shared;

using System;
using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Content;

public class ContentViewModel
{
    public string Id { get; init; }

    public string ContentCategoryId { get; set; }

    public string ContentCategoryName { get; init; }

    public long ContentCode { get; set; }

    public string Title { get; init; }

    public string SubTitle { get; init; }

    public string LanguageId { get; set; }

    public string LanguageName { get; init; }

    public string Description { get; set; }

    public string SeoTitle { get; init; }

    public string SeoDescription { get; init; }

    public string UrlFriend { get; init; }

    public List<Image> Images { get; init; }

    public DateTime StartShowDate { get; set; }

    public string PersianStartShowDate { get; set; }

    public DateTime EndShowDate { get; set; }

    public string PersianEndShowDate { get; set; }

    public int VisitCount { get; set; }

    public List<string> TagKeywords { get; set; }

    //public int PopularityRate { get; set; }

    public SourceType SourceType { get; init; }

    public string ContentProviderName { get; init; }

    public bool IsDeleted { get; init; }

    public long? TotalScore { get; set; }

    public int? ScoredCount { get; set; }

    public int LikeRate { get; set; }

    public bool HalfLikeRate { get; set; }

    public int DisikeRate { get; set; }
}