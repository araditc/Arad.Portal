using Enyim.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Helpers
{
    public class EnyimMemcachedMethods
    {
        private readonly IMemcachedClient _cache;
        public EnyimMemcachedMethods(IMemcachedClient cache)
        {
            _cache = cache; 
        }
    }
}
