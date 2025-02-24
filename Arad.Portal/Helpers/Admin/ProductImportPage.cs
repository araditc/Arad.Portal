using Microsoft.AspNetCore.Http;

using System.Collections.Generic;

namespace Arad.Portal.Helpers.Admin;

public class ProductImportPage
{
    public ProductImportPage()
    {
        ProductGroupIds = new();
    }
    public List<string> ProductGroupIds { get; init; }
    public IFormFile ProductImages { get; init; } = null;

    public IFormFile ProductsExcelFile { get; init; } = null;
}