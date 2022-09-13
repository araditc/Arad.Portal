﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Entities.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class PageViewModel
    {
        public PageViewModel()
        {
            Filters = new List<DynamicFilter>();
        }
        public int CurrentPage { get; set; }
        public long ItemsCount { get; set; }
        public int PageSize { get; set; }
        public string Navigation { get; set; }
        public string QueryParams { get; set; }
        public List<DynamicFilter> Filters { get; set; }
    }
  
    public class Pagination : ViewComponent
    {
        public IViewComponentResult Invoke(PageViewModel model)
        {
            return View("Default", model);
        }
    }
}
