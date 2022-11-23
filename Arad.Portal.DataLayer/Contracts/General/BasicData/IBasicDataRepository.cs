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
        List<BasicDataModel> GetList(string groupKey, bool withChooseItem, bool isDomain = true);

        bool HasLastID();

        bool SaveLastId(long id);

        Task<Result> InsertNewRecord(Entities.General.BasicData.BasicData model);

        bool HasShippingType();

        Task<Result> DeleteGroupKeyListInDomain(string domainId, string groupKey);

        
    }
}
