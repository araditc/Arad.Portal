using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{
    public class ProductImportPage
    {
        public IFormFile ProductImages { get; set; } = null;

        public IFormFile ProductsExcelFile { get; set; } = null;
    }
}
