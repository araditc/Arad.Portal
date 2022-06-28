using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.Content
{
    public class Content : BaseEntity
    {
        public Content()
        {
            Images = new();
            Comments = new();
            TagKeywords = new();
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string ContentId { get; set; }

        public string ContentCategoryId { get; set; }

        public string ContentCategoryName { get; set; }

        public string Title { get; set; }

        public string SubTitle { get; set; }

        public string LanguageId { get; set; }

        public string LanguageName { get; set; }

        public string Description { get; set; }

        public string SeoTitle { get; set; }

        public string SeoDescription { get; set; }

        public string UrlFriend { get; set; }
        /// <summary>
        /// main content from textEditor
        /// </summary>
        public string Contents { get; set; }
        public long ContentCode { get; set; }
        /// <summary>
        /// Ismain = true is for fileLogo
        /// </summary>
        public List<Models.Shared.Image> Images { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartShowDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EndShowDate { get; set; }

        public int VisitCount { get; set; }

        public List<string> TagKeywords { get; set; }

        //public int PopularityRate { get; set; }
        public long TotalScore { get; set; }

        public int ScoredCount { get; set; }

        public bool IsCommentBoxShowing { get; set; }
        public bool IsSidebarContentsShowing { get; set; }
        public bool IsSliderShowing { get; set; }
        public int? SidebarContentCount { get; set; }
        public bool IsRateBoxShowing { get; set; }

        public List<Comment.Comment> Comments { get; set; }

        public SourceType SourceType { get; set; }

        public string ContentProviderName { get; set; }

    }

    public enum SourceType
    {
        //superAdmin of site
        SiteAdmin,
        //all users can generate content
        Users,
        //copy this content from other website
        CopyFromOtherSource
    }
}
