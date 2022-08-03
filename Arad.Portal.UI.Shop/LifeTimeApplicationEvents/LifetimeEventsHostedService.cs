using Arad.Portal.DataLayer.Contracts.Shop.Transaction;
using Arad.Portal.UI.Shop.Helpers;
using DnsClient.Internal;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.LifeTimeApplicationEvents
{
    public class LifetimeEventsHostedService : IHostedService
    {
        //order of events being executed :
        //StartAsync is being called
        //OnStarted has been called.
        //OnStopping has been called.
        //StopAsync is being called
        //OnStopped has been called.
       
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly ITransactionRepository _transactionRepository;
        private readonly SharedRuntimeData _sharedRuntimeData;
        public LifetimeEventsHostedService(IHostApplicationLifetime applifetime, ITransactionRepository transactionRepository, SharedRuntimeData sharedRuntimeData)
        {
            
            _appLifetime = applifetime;
            _transactionRepository = transactionRepository;
            _sharedRuntimeData = sharedRuntimeData;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
           
            _appLifetime.ApplicationStopping.Register(OnStopping);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {

            //System.IO.File.AppendAllText("/test/aaaaaa.txt","StopAsync is being called");
            return Task.CompletedTask;
        }

        private void OnStopping()
        {
            _sharedRuntimeData.DeleteAllDataWithRoleBack();
            _transactionRepository.RollBackPayingTransaction();
        }

    }
}
