using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class GeneralSearchResult
    {
        public string EntityId { get; set; }

        public string EntityName { get; set; }

        public long EntityCode { get; set; }

        public string ImageUrl { get; set; }

        /// <summary>
        /// if isProduct = false then it is an instance of content
        /// </summary>
        public bool IsProduct { get; set; }
    }
}
