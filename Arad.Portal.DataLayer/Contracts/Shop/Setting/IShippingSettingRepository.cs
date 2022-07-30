using Arad.Portal.DataLayer.Entities.Shop.Setting;
using Arad.Portal.DataLayer.Models.Setting;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.Shop.Setting
{
    public interface IShippingSettingRepository
    {
        Task<PagedItems<ShippingSettingDTO>> List(string queryString);

        ShippingSettingDTO FetchById(string shippingSettingId);

        ShippingSetting FetchShippingSettingOfDomain(string domainId); 

        Task<Result> AddShippingSetting(ShippingSettingDTO dto);

        Task<Result> EditShippingSetting(ShippingSettingDTO dto);


    }
}
