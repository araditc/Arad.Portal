using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Content
{
    public class ContentGlance
    {
        public ContentGlance()
        {
            Images = new();
            TagKeywords = new();
        }
        public string ContentId { get; set; }
        public string ContentCategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<Shared.Image> Images { get; set; }
        public string DesiredImageUrl { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public List<string> TagKeywords { get; set; }
        public long ContentCode { get; set; }
        public long TotalScore { get; set; }
        public int ScoredCount { get; set; }
        public int VisitCount { get; set; }
        public int LikeRate { get; set; }
        public bool HalfLikeRate { get; set; }
        public int DisikeRate { get; set; }
        public string ContentProviderName { get; set; }
    }
}
