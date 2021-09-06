using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Content
{
    public class ContentViewModel
    {
        public string ContentId { get; set; }

        public string CategoryContentId { get; set; }

        public string CategoryContentName { get; set; }

        public string Title { get; set; }

        public string SubTitle { get; set; }

        public string LanguageId { get; set; }

        public string LanguageName { get; set; }

        public string Description { get; set; }

        public string SeoTitle { get; set; }

        public string SeoDescription { get; set; }

        public string UrlFriend { get; set; }

        public string FileLogo { get; set; }

        public List<Shared.Image> Images { get; set; }

        public DateTime StartShowDate { get; set; }

        public string PersianStartShowDate { get; set; }

        public DateTime EndShowDate { get; set; }

        public string PersianEndShowDate { get; set; }

        public int VisitCount { get; set; }

        public List<string> TagKeywords { get; set; }

        public int PopularityRate { get; set; }

        public Entities.General.Content.SourceType SourceType { get; set; }

        public string ContentProviderName { get; set; }
    }
}

