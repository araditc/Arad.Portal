using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{
    public interface IRouteLocator
    {
        string GetProductGroupId(string groupCode);
        string GetProductId(string productCode);
        string GetContentCategoryId(string categoryCode);
        string GetContentId(string contentCode);
        //string GetPageId(string urlFriend);
    }
}
