using Arad.Portal.DataLayer.Entities.General.Service;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.Services
{
    public interface IProviderRepository
    {
        List<SelectListModel> GetProvidersPerType(ProviderType type);

        void InsertOne(Provider entity);
    }
}
