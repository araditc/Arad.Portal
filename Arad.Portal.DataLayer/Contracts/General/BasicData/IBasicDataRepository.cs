using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.BasicData
{
    public interface IBasicDataRepository
    {
        List<BasicDataModel> GetList(string groupKey);

        bool HasLastID();

        bool SaveLastId(long id);

        List<BasicDataModel> GetListPerDomain(string groupKey);

        bool HasShippingType();

        string GetDomainName();
    }
}
