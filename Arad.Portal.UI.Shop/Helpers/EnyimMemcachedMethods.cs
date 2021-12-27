using Arad.Portal.DataLayer.Contracts.Shop.Transaction;
using Enyim.Caching;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Helpers
{
    public class EnyimMemcachedMethods<T> 
    {
        private readonly IMemcachedClient _cache;
        private readonly ITransactionRepository _transactionRepository;
        public EnyimMemcachedMethods(IMemcachedClient cache,
            ITransactionRepository transationRepository)
        {
            _cache = cache;
            _transactionRepository = transationRepository;
        }

        public  bool AddToCache(string cachedKey,T model, int cachedSecond)
        {
            bool result = false;
            T input;
            try
            {
                if(!_cache.TryGet(cachedKey, out input))
                {
                    result = _cache.Add(cachedKey, model, cachedSecond);
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public T GetFromCache(string cachedKey)
        {
            T t = _cache.Get<T>(cachedKey);

            return t;
        }
    }
}
