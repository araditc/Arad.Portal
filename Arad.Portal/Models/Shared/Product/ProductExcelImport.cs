using Arad.Portal.DataLayer.Models.Shared;

using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Product;

public class ProductExcelImport
{
    public ProductExcelImport()
    {
        GroupIds = new();
    }
    public List<string> GroupIds { get; set; }
    public string ProductName { get; set; }

    //public int Inventory { get; set; }

    /// <summary>
    /// this prop with be generated in dashboard with codegeneraor.getNewId()
    /// </summary>
    public int ProductCode { get; set; }
    public string ProductUnit { get; set; }
    public bool IsPublishOnMainDomain { get; set; }
    public bool ShowInLackOfInventory { get; set; }
    public string UniqueCode { get; set; }
    public long Price { get; set; }
    public string SeoTitle { get; set; }
    public string SeoDescription { get; set; }
    public string TagKeywords { get; set; }

    public Image ProductImage { get; set; }

}