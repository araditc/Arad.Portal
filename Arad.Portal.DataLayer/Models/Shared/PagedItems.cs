using System;
using System.Collections.Generic;
using System.Text;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class PagedItems<T> where T : class
    {
        public PagedItems()
        {
            Items = new List<T>();
        }
        public int CurrentPage { get; set; }
        public long ItemsCount { get; set; }
        public List<T> Items { get; set; }
        public int PageSize { get; set; }

        public string  Navigation { get; set; }
        public string QueryString { get; set; }
    }
}
