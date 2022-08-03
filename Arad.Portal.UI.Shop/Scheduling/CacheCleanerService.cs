using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.UI.Shop.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Scheduling
{
    public class CacheCleanerService
    {
        private readonly SharedRuntimeData _provider;

        Timer _timer;
        public CacheCleanerService(SharedRuntimeData provider)
        {
            _provider = provider;
           
           
        }

        public void startTimer()
        {
            TimerCallback cb = new(DictionaryReview);
            _timer = new Timer(cb, null, 1000, 20 * 1000 * 60);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
       
        private  void DictionaryReview(object state)
        {
          _provider.DeleteAllUnusedData();
        }
    }
}
