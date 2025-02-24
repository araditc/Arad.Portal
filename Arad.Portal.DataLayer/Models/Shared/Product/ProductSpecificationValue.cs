using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Models.Shared.Product;

public class ProductSpecificationValue
{
    public ProductSpecificationValue()
    {
        ValueList = new List<SelectListModel>();
    }

    public string LanguageId { get; set; }
    public string SpecificationId { get; set; }
    public string SpecificationGroupId { get; set; }
    public string LanguageName { get; set; }
    public string SpecificationGroupName { get; set; }
    public string SpecificationName { get; set; }
    /// <summary>
    /// all available values for this specification on this product
    ///  | seperated values
    /// </summary>
    public string Values { get; set; }

    public List<SelectListModel> ValueList { get; set; }
}