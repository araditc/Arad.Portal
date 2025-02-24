using Arad.Portal.DataLayer.Models.Shared.Product;

using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Product;

public class BasketModel
{
    public BasketModel()
    {
        SpecVals = new();
    }
    public string Code { get; set; }

    public int Count { get; set; }

    public string CartDetailId { get; set; }

    public List<SpecValue> SpecVals { get; set; }
}