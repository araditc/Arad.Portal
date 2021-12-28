using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.UI.Shop.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Scheduling
{
    public class CacheCleanerService : HostedService
    {
        private readonly SharedRuntimeData _provider;

        Timer _timer;
        public CacheCleanerService(SharedRuntimeData provider)
        {
            _provider = provider; 
        }
        protected  override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(Task.Run(_provider.DeleteAllUnusedData(),cancellationToken), null, 1000, 20 * 1000 * 60);
            while (!cancellationToken.IsCancellationRequested)
            {
              
                await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
            }
        }

        private void DeleteAll(object state, CancellationToken cancellationToken)
        {
            if(!can)
            _provider.DeleteAllUnusedData();
        }
    }
}
