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
        /// <summary>
        /// Id of content category which this content belongs to
        /// </summary>
        public string ContentCategoryId { get; set; }
        /// <summary>
        /// name of content category which this content belongs to
        /// </summary>
        public string ContentCategoryName { get; set; }
        
        public string Title { get; set; }
        /// <summary>
        /// subtitle is a short description about the content some modules use this field 
        /// </summary>
        public string SubTitle { get; set; }

        public string LanguageId { get; set; }

        public string LanguageName { get; set; }

        public string Description { get; set; }
        /// <summary>
        /// this field is use for ranking content in search engins like google
        /// </summary>
        public string SeoTitle { get; set; }


        /// <summary>
        /// this field is use for ranking content in search engins like google
        /// </summary>
        public string SeoDescription { get; set; }


        /// <summary>
        /// admin can design prefered url for this content but if this field left empty we use blog/CondentCode to access this content
        /// </summary>
        public string UrlFriend { get; set; }


        /// <summary>
        /// main content which comes from textEditor and can containes images too
        /// </summary>
        public string Contents { get; set; }

        /// <summary>
        /// use for accessing the content via blog/ContentCode if urlFriend is empty
        /// </summary>
        public long ContentCode { get; set; }

        /// <summary>
        /// all images which uses fileUploader set here and the one which ismain is equal true is for ContentLogo
        /// </summary>
        public List<Models.Shared.Image> Images { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartShowDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EndShowDate { get; set; }
        /// <summary>
        /// how many visitors open this content from site
        /// </summary>
        public int VisitCount { get; set; }


        /// <summary>
        ///this field is keywords which search engines find in their searches
        /// </summary>
        public List<string> TagKeywords { get; set; }


        /// <summary>
        /// the sum of all scores which this content gets from visitors each score has (0 to 5 ) range
        /// </summary>
        public long TotalScore { get; set; }


        /// <summary>
        /// how many peaple has rate this content the sume of all them
        /// </summary>
        public int ScoredCount { get; set; }
        /// <summary>
        ///is comment on this content enables or not
        /// </summary>
        public bool IsCommentBoxShowing { get; set; }

        /// <summary>
        /// there is a sidebarContent beside the detail Page of content this field enable or diabled it
        /// that side bar has links to other contents
        /// </summary>
        public bool IsSidebarContentsShowing { get; set; }

        /// <summary>
        /// if admin set any Images for this content and set this fied to true images will be shown in slider on top of page content if there isnt any image or this field is false the slider wont be shown there
        /// </summary>
        public bool IsSliderShowing { get; set; }


        /// <summary>
        /// if admin set to have sidebarContent beside the page IsSidebarContentsShowing = true then here can set how many content have links in that sidebar
        /// </summary>
        public int? SidebarContentCount { get; set; }

        /// <summary>
        /// it determine whether this paeg content has a box whic user can rate this content or not
        /// </summary>
        public bool IsRateBoxShowing { get; set; }


        /// <summary>
        /// if commenting is enable on this page content IsCommentBoxShowing = true then this is the comments which visitors put on this content
        /// </summary>
        public List<Comment.Comment> Comments { get; set; }


        /// <summary>
        /// it determin whether this content extracted from other place or any user prepare this content or the content produce by admin of site itself
        /// </summary>
        public SourceType SourceType { get; set; }

        /// <summary>
        /// if the sourceType is SiteAdmin Or User then it is the name of producer and if its is CopyFromOtherSource the link of source site will be put here
        /// </summary>
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
