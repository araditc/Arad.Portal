using Arad.Portal.DataLayer.Models.Shared;

using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Content;

public class ContentGlance
{
    public ContentGlance()
    {
        Images = new();
        TagKeywords = new();
    }
    public string Id { get; set; }
    public string ContentCategoryId { get; set; }
    public string CategoryName { get; set; }
    public List<Image> Images { get; init; }
    public string DesiredImageUrl { get; set; }
    public string UrlFriend { get; init; }
    public string ImageTitle { get; set; }
    public string Title { get; init; }
    public string SubTitle { get; init; }

    public string Description { get; set; }
    public List<string> TagKeywords { get; set; }
    public long ContentCode { get; init; }
    public long TotalScore { get; init; }
    public int ScoredCount { get; init; }
    public int VisitCount { get; set; }
    public int LikeRate { get; set; }
    public bool HalfLikeRate { get; set; }
    public int DisikeRate { get; set; }
    public string ContentProviderName { get; init; }
}