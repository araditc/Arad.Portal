using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Helpers
{
    public interface IRouteLocator
    {
        string GetProductGroupId(string groupCode);
        string GetProductId(string productCode);
        string GetContentCategoryId(string categoryCode);
        string GetContentId(string contentCode);
    }
}
