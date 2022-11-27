using Arad.Portal.DataLayer.Contracts.General.BasicData;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Entities.General.BasicData;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{
    public class CodeGenerator
    {
        private readonly IBasicDataRepository _basicDataRepository;
        private readonly IDomainRepository _domainRepository;
        public static ConcurrentDictionary<long, long> dict = new ConcurrentDictionary<long, long>();
        private readonly IHttpContextAccessor _accessor;
        public CodeGenerator(IBasicDataRepository basicDataRepository, IHttpContextAccessor accessor, IDomainRepository domainRepository)
        {
            _basicDataRepository = basicDataRepository;
            _accessor = accessor;
            _domainRepository = domainRepository;
        }
       
        public long GetNewId()
        {
            bool added = false;
            var lst = _basicDataRepository.GetList("LastId", false);
            if(lst != null && lst.Count > 0 )
            {
                long newId = long.Parse(lst[0].Value) + 1;
                while (!added)
                {
                    added = dict.TryAdd(newId, newId);
                    newId += 1;
                }
                return newId - 1;
            }
            else
            {
                var domainName = _accessor.HttpContext.Request.Host.ToString();
                var domain = _domainRepository.FetchByName(domainName, false).ReturnValue;
                var basicData = new BasicData()
                {
                    AssociatedDomainId = domain.DomainId,
                    BasicDataId = Guid.NewGuid().ToString(),
                    GroupKey = "LastId",
                    Value = "0",
                    Text = "0"
                };
                _basicDataRepository.InsertNewRecord(basicData);
                return 1;
            }
            
            
        }
        public bool SaveToDB(long lastId)
        {
            long item = 0;
            var res =  _basicDataRepository.SaveLastId(lastId);
            dict.TryRemove(lastId,out item);
            return res;
        }

        //public static void ClearOldId(object state)
        //{
        //    foreach (var item in dict)
        //    {
        //        long outItem;
        //        dict.TryGetValue(item.Key, out outItem);
        //        if ((DateTime.Now - new DateTime(outItem)).TotalMinutes > Startup.interval)
        //        {
        //            dict.TryRemove(item.Key, out outItem);
        //        }
        //    }
        //    Log.Information($"ClearOldId at: {DateTime.Now}");
        //}
    }
}
