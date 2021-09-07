using Arad.Portal.DataLayer.Models.Comment;
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

        public string FileLogo { get; set; }

        public List<Models.Shared.Image> Images { get; set; }

        public DateTime StartShowDate { get; set; }

        public DateTime EndShowDate { get; set; }

        public int VisitCount { get; set; }

        public List<string> TagKeywords { get; set; }

        public int PopularityRate { get; set; }

        public List<Comment> Comments { get; set; }

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
